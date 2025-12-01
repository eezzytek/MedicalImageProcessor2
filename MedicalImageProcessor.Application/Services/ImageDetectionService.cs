using MedicalImageProcessor.Core.Entities;
using MedicalImageProcessor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalImageProcessor.Application.Services
{
    public class ImageDetectionService
    {
        private readonly IImageProcessor _processor;
        private readonly IDetectionService _detector;
        private readonly ILogger<ImageDetectionService> _logger;

        public ImageDetectionService(IImageProcessor processor, IDetectionService detector, ILogger<ImageDetectionService> logger)
        {
            _processor = processor;
            _detector = detector;
            _logger = logger;
        }

        public async Task<DetectionResult> ProcessAndDetectAsync(ImageInput input, string modelType = "tumor", CancellationToken ct = default)
        {
            _logger.LogInformation("Starting processing for image {ImageId} with model {ModelType}", input.ImageId, modelType);

            var preprocessed = await _processor.PreprocessAsync(input, ct);
            DetectionResult result = new() { ImageId = input.ImageId };

            if (modelType == "tumor")
            {
                var tumorResult = await _detector.DetectTumorAsync(preprocessed, ct);  // ФІКС: Async метод
                result.HasBrainTumor = tumorResult.HasBrainTumor;
                result.BrainTumorConfidence = tumorResult.BrainTumorConfidence;
                result.HasFracture = false;
                result.FractureConfidence = 0f;
            }
            else if (modelType == "fracture")
            {
                var fractureResult = await _detector.DetectFractureAsync(preprocessed, ct);  // ФІКС: Async метод
                result.HasFracture = fractureResult.HasFracture;
                result.FractureConfidence = fractureResult.FractureConfidence;
                result.HasBrainTumor = false;
                result.BrainTumorConfidence = 0f;
            }
            else
            {
                _logger.LogWarning("Invalid modelType {ModelType}, defaulting to tumor", modelType);
                var tumorResult = await _detector.DetectTumorAsync(preprocessed, ct);
                result.HasBrainTumor = tumorResult.HasBrainTumor;
                result.BrainTumorConfidence = tumorResult.BrainTumorConfidence;
                result.HasFracture = false;
                result.FractureConfidence = 0f;
            }

            _logger.LogInformation("Detection complete for {ImageId}: Tumor {TumorConf}, Fracture {FractureConf}", input.ImageId, result.BrainTumorConfidence, result.FractureConfidence);

            return result;
        }
    }
}