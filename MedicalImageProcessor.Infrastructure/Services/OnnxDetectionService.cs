using MedicalImageProcessor.Core.Entities;
using MedicalImageProcessor.Core.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;

namespace MedicalImageProcessor.Infrastructure.Services
{
    public class OnnxDetectionService : IDetectionService
    {
        private readonly InferenceSession _brainTumorSession;
        private readonly InferenceSession _fractureSession;

        public OnnxDetectionService()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var brainPath = Path.Combine(currentDirectory, "Models", "brain_tumor.onnx");
            var fracturePath = Path.Combine(currentDirectory, "Models", "bone_fracture.onnx");

            Console.WriteLine($"=== ДЕБАГ МОДЕЛЕЙ ===");
            Console.WriteLine($"Поточна директорія: {currentDirectory}");
            Console.WriteLine($"Шлях до brain_tumor.onnx: {brainPath} — існує? {File.Exists(brainPath)}");
            Console.WriteLine($"Шлях до BoneFracture.onnx: {fracturePath} — існує? {File.Exists(fracturePath)}");

            if (!File.Exists(brainPath) || !File.Exists(fracturePath))
                throw new FileNotFoundException("Файли моделей не знайдено. Перевір bin/Debug/net9.0/models/");

            _brainTumorSession = new InferenceSession(brainPath);
            _fractureSession = new InferenceSession(fracturePath);
        }

        public async Task<DetectionResult> DetectAsync(byte[] preprocessedImage, CancellationToken ct = default)
        {
            if (preprocessedImage == null || preprocessedImage.Length == 0)
                throw new ArgumentException("Preprocessed image data is empty.", nameof(preprocessedImage));

            // Для brain: 224x224 classification
            var brainTensor = await CreateTensorFromBytesAsync(preprocessedImage, 224, true, ct);  // true = normalize
            var brainConfidence = RunClassificationModel(_brainTumorSession, brainTensor, "input");

            // Для fracture: 416x416 detection (YOLO)
            var fractureTensor = await CreateTensorFromBytesAsync(preprocessedImage, 416, true, ct);
            var fractureConfidence = RunDetectionModel(_fractureSession, fractureTensor, "images");  // 'images' для YOLO

            return new DetectionResult
            {
                HasBrainTumor = brainConfidence > 0.5f,
                BrainTumorConfidence = brainConfidence,
                HasFracture = fractureConfidence > 0.5f,
                FractureConfidence = fractureConfidence,
                ImageId = string.Empty
            };
        }

        private async Task<DenseTensor<float>> CreateTensorFromBytesAsync(byte[] imageBytes, int size, bool normalize, CancellationToken ct)
        {
            await using var stream = new MemoryStream(imageBytes);
            using var image = await Image.LoadAsync<Rgba32>(stream, ct);

            // Resize до size (для YOLO 416, для classification 224)
            image.Mutate<Rgba32>(x => x.Resize(size, size));

            var tensor = new DenseTensor<float>(new[] { 1, 3, size, size });

            float[] mean = { 0.485f, 0.456f, 0.406f };
            float[] std = { 0.229f, 0.224f, 0.225f };

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        var pixel = row[x];
                        float r = pixel.R / 255f;
                        float g = pixel.G / 255f;
                        float b = pixel.B / 255f;

                        if (normalize)
                        {
                            r = (r - mean[0]) / std[0];
                            g = (g - mean[1]) / std[1];
                            b = (b - mean[2]) / std[2];
                        }

                        tensor[0, 0, y, x] = r;
                        tensor[0, 1, y, x] = g;
                        tensor[0, 2, y, x] = b;
                    }
                }
            });

            return tensor;
        }

        // Для classification (brain tumor): single logit → sigmoid
        private float RunClassificationModel(InferenceSession session, DenseTensor<float> inputTensor, string inputName)
        {
            try
            {
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputTensor) };
                using var outputs = session.Run(inputs);
                var outputTensor = outputs.First().AsTensor<float>();
                var rawOutput = outputTensor.ToArray()[1];  // [0] = no-tumor, [1] = tumor (multi-class 2)
                return Sigmoid(rawOutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка classification: {ex.Message}");
                return 0f;
            }
        }

        // Для detection (YOLO fracture): parse max conf для fracture класів (2-6 з yaml)
        private float RunDetectionModel(InferenceSession session, DenseTensor<float> inputTensor, string inputName)
        {
            try
            {
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputTensor) };
                using var outputs = session.Run(inputs);
                var outputTensor = outputs.First().AsTensor<float>();  // [1, 84, 8400] (YOLOv8n на 416: ~8400 anchors)

                float maxConf = 0f;
                int numDets = outputTensor.Dimensions[2];  // 8400
                for (int d = 0; d < numDets; d++)
                {
                    float conf = outputTensor[0, 4, d];  // Conf (5-й елемент: xywh + conf)
                    if (conf < 0.25f) continue;  // NMS threshold з тренування

                    // Класи 5-11 (0-6 в yaml, але YOLO offset)
                    for (int cls = 5; cls < 12; cls++)  // 2-6 fracture класи (elbow, fingers, forearm, humerus, shoulder, wrist)
                    {
                        float classConf = outputTensor[0, cls, d];
                        float totalConf = conf * Sigmoid(classConf);  // Conf * class prob
                        if (totalConf > maxConf)
                            maxConf = totalConf;
                    }
                }
                return maxConf;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка detection: {ex.Message}");
                return 0f;
            }
        }

        private static float Sigmoid(float x)
        {
            if (x < -709f) return 0f;
            if (x > 709f) return 1f;
            return 1f / (1f + MathF.Exp(-x));
        }
        
        public async Task<DetectionResult> DetectTumorAsync(byte[] preprocessedImage, CancellationToken ct = default)
        {
            var tensor = await CreateTensorFromBytesAsync(preprocessedImage, 224, true, ct);  // 224 for tumor
            var confidence = RunClassificationModel(_brainTumorSession, tensor, "input");
            return new DetectionResult
            {
                HasBrainTumor = confidence > 0.5f,
                BrainTumorConfidence = confidence,
                HasFracture = false,
                FractureConfidence = 0f
            };
        }

        public async Task<DetectionResult> DetectFractureAsync(byte[] preprocessedImage, CancellationToken ct = default)
        {
            var tensor = await CreateTensorFromBytesAsync(preprocessedImage, 416, false, ct);  // 416 for YOLO, без нормалізації (YOLO очікує [0,1])
            var confidence = RunDetectionModel(_fractureSession, tensor, "images");
            return new DetectionResult
            {
                HasFracture = confidence > 0.5f,
                FractureConfidence = confidence,
                HasBrainTumor = false,
                BrainTumorConfidence = 0f
            };
        }
    }
}