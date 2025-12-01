using System.ComponentModel.DataAnnotations;

namespace MedicalImageProcessor.Application.DTOs
{
    public class DetectionRequestDto
    {
        [Required]
        [MaxLength(10 * 1024 * 1024)]  // Макс. 10MB для зображення
        public byte[] ImageData { get; set; } = Array.Empty<byte>();  

        public string Format { get; set; } = "png";  
    }
}