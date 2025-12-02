using System.Security.Claims;
using MedicalImageProcessor.Application.DTOs;
using MedicalImageProcessor.Application.Services;
using MedicalImageProcessor.Core.Entities;
using MedicalImageProcessor.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace MedicalImageProcessor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DetectionController : ControllerBase
    {
        private readonly ImageDetectionService _service;
        private readonly Supabase.Client _supabase;

        public DetectionController(ImageDetectionService service, Supabase.Client supabase)
        {
            _service = service;
            _supabase = supabase;
        }

        [HttpPost("detect")]
        public async Task<ActionResult<DetectionResponseDto>> Detect(
            IFormFile imageFile,
            [FromQuery] string modelType = "tumor")
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("No image file provided.");

            if (modelType != "tumor" && modelType != "fracture")
                return BadRequest("modelType must be 'tumor' or 'fracture'.");

            using var ms = new MemoryStream();
            await imageFile.CopyToAsync(ms);

            var requestDto = new DetectionRequestDto
            {
                ImageData = ms.ToArray(),
                Format = Path.GetExtension(imageFile.FileName)?.TrimStart('.')?.ToLower() ?? "png"
            };

            if (!IsValidImage(requestDto))
                return BadRequest("Invalid image format or size.");

            var input = new ImageInput
            {
                ImageData = requestDto.ImageData,
                Format = requestDto.Format,
                ImageId = Guid.NewGuid().ToString()
            };

            var result = await _service.ProcessAndDetectAsync(input, modelType);

            // === SUPABASE: Зберігаємо результат ===
            try
            {
                var fileId = $"{Guid.NewGuid()}_{imageFile.FileName}";
                var storage = _supabase.Storage.From("medical-images");

                await storage.Upload(requestDto.ImageData, fileId, new Supabase.Storage.FileOptions
                {
                    ContentType = imageFile.ContentType,
                    Upsert = false
                });

                var publicUrl = storage.GetPublicUrl(fileId);

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value
                             ?? "anonymous";
                
                var record = new DetectionResultRecord
                {
                    UserId = userId,
                    ImageUrl = publicUrl,
                    ModelType = modelType,
                    HasBrainTumor = result.HasBrainTumor,
                    BrainTumorConfidence = result.BrainTumorConfidence,
                    HasFracture = result.HasFracture,
                    FractureConfidence = result.FractureConfidence
                };

                // ПРАВИЛЬНИЙ ВИКЛИК Insert
                var response = await _supabase
                    .From<DetectionResultRecord>()
                    .Insert(record);

                Console.WriteLine($"[SUPABASE] Saved: {response.Models.FirstOrDefault()?.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SUPABASE] Error: {ex.Message}");
                // Не падаємо — просто логимо
            }

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
            var allowed = new[] { "png", "jpg", "jpeg", "dicom" };
            return dto.ImageData.Length > 0
                && dto.ImageData.Length < 15 * 1024 * 1024
                && allowed.Contains(dto.Format);
        }
    }
}