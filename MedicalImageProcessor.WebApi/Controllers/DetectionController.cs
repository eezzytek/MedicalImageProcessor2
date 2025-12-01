using MedicalImageProcessor.Application.DTOs;
using MedicalImageProcessor.Application.Services;
using MedicalImageProcessor.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalImageProcessor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // ФІКС: Додаємо JWT auth
    public class DetectionController : ControllerBase
    {
        private readonly ImageDetectionService _service;

        public DetectionController(ImageDetectionService service) => _service = service;

        [HttpPost("detect")]
        public async Task<ActionResult<DetectionResponseDto>> Detect(IFormFile imageFile, [FromQuery] string modelType = "tumor")  // ФІКС: Param для вибору моделі
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            // ФІКС: Валідація modelType
            if (modelType != "tumor" && modelType != "fracture")
            {
                return BadRequest("modelType must be 'tumor' or 'fracture'.");
            }

            using var ms = new MemoryStream();
            await imageFile.CopyToAsync(ms);
            var requestDto = new DetectionRequestDto
            {
                ImageData = ms.ToArray(),
                Format = Path.GetExtension(imageFile.FileName)?.TrimStart('.') ?? "png"
            };

            if (!IsValidImage(requestDto)) return BadRequest("Invalid image format.");

            var input = new ImageInput { ImageData = requestDto.ImageData, Format = requestDto.Format, ImageId = Guid.NewGuid().ToString() };
            var result = await _service.ProcessAndDetectAsync(input, modelType);  // ФІКС: Передаємо modelType в сервіс

            return Ok(new DetectionResponseDto
            {
                HasBrainTumor = result.HasBrainTumor,
                BrainTumorConfidence = result.BrainTumorConfidence,
                HasFracture = result.HasFracture,
                FractureConfidence = result.FractureConfidence
            });
        }

        private bool IsValidImage(DetectionRequestDto dto)
        {
            // Простий чек: розмір < 10MB, формат OK
            return dto.ImageData.Length > 0 && dto.ImageData.Length < 10 * 1024 * 1024 &&
                   (dto.Format == "png" || dto.Format == "jpg" || dto.Format == "jpeg" || dto.Format == "dicom");
        }
    }
}