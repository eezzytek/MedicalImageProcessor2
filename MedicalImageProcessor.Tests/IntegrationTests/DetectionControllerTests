using FluentAssertions;
using MedicalImageProcessor.WebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MedicalImageProcessor.Tests.IntegrationTests
{
    public class DetectionControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public DetectionControllerTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task Detect_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
            content.Add(fileContent, "Image", "test.png");

            // Act
            var response = await client.PostAsync("/api/Detection/detect", content);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
