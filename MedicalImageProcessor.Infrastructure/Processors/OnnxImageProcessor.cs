using MedicalImageProcessor.Core.Entities;
using MedicalImageProcessor.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing; 
using System.IO;

namespace MedicalImageProcessor.Infrastructure.Processors
{
    public class OnnxImageProcessor : IImageProcessor
    {
        public async Task<byte[]> PreprocessAsync(ImageInput input, CancellationToken ct = default)
        {
            if (input.ImageData == null || input.ImageData.Length == 0)
            {
                throw new ArgumentException("Image data is empty.", nameof(input));
            }

            // Конвертувати byte[] в Stream для LoadAsync
            await using var inputStream = new MemoryStream(input.ImageData);
            
            // Завантажити зображення з Stream
            using var image = await Image.LoadAsync<Rgba32>(inputStream, ct);
            
            // Resize до 224x224 (використовуємо Mutate для стандартних операцій)
            image.Mutate<Rgba32>(x => x.Resize(224, 224));

            // Нормалізація та серіалізація
            var normalizedBytes = NormalizeAndSerialize(image);

            return normalizedBytes;
        }

        private byte[] NormalizeAndSerialize(Image<Rgba32> image)
        {
            var mean = new[] { 0.485f, 0.456f, 0.406f };
            var std = new[] { 0.229f, 0.224f, 0.225f };

            // Клон для мутації (щоб не змінювати оригінал)
            using var processedImage = image.Clone();

            // ФІКС: ProcessPixelRows викликаємо безпосередньо на зображенні (не в Mutate!)
            processedImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        ref Rgba32 pixel = ref row[x];
                        // Нормалізувати канали R,G,B (A лишаємо без змін)
                        var r = Math.Clamp((pixel.R / 255f - mean[0]) / std[0], 0f, 1f) * 255f;
                        var g = Math.Clamp((pixel.G / 255f - mean[1]) / std[1], 0f, 1f) * 255f;
                        var b = Math.Clamp((pixel.B / 255f - mean[2]) / std[2], 0f, 1f) * 255f;
                        row[x] = new Rgba32((byte)r, (byte)g, (byte)b, pixel.A);
                    }
                }
            });

            // Зберегти як byte[] (PNG для простоти; для ONNX — конвертуйте в float[] tensor)
            using var outputMs = new MemoryStream();
            processedImage.SaveAsPng(outputMs);
            return outputMs.ToArray();
        }
    }
}