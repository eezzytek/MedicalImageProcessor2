using FluentAssertions;
using MedicalImageProcessor.Application.Services;
using MedicalImageProcessor.Core.Entities;
using MedicalImageProcessor.Core.Interfaces;
using Moq;
using Xunit;

namespace MedicalImageProcessor.Tests.UnitTests.Services
{
    public class ImageDetectionServiceTests
    {
        [Fact]
        public async Task ProcessAndDetectAsync_ShouldReturnDetectionResult()
        {
            // Arrange
            var mockProcessor = new Mock<IImageProcessor>();
            mockProcessor.Setup(p => p.PreprocessAsync(It.IsAny<ImageInput>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            var mockDetector = new Mock<IDetectionService>();
            mockDetector.Setup(d => d.DetectAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DetectionResult { HasBrainTumor = true, BrainTumorConfidence = 0.8f });

            var service = new ImageDetectionService(mockProcessor.Object, mockDetector.Object, Mock.Of<ILogger<ImageDetectionService>>());

            var input = new ImageInput { ImageData = new byte[] { 0 }, ImageId = "test" };

            // Act
            var result = await service.ProcessAndDetectAsync(input);

            // Assert
            result.HasBrainTumor.Should().BeTrue();
            result.BrainTumorConfidence.Should().Be(0.8f);
        }
    }
}
