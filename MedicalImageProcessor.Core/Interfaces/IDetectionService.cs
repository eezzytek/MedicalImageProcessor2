using MedicalImageProcessor.Core.Entities;

namespace MedicalImageProcessor.Core.Interfaces
{
    public interface IDetectionService
    {
        Task<DetectionResult> DetectAsync(byte[] preprocessedImage, CancellationToken ct = default);
        Task<DetectionResult> DetectTumorAsync(byte[] preprocessedImage, CancellationToken ct = default);  // Новий
        Task<DetectionResult> DetectFractureAsync(byte[] preprocessedImage, CancellationToken ct = default);  // Новий
    }
}