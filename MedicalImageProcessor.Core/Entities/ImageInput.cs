namespace MedicalImageProcessor.Core.Entities
{
    public class ImageInput
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string Format { get; set; } = "png";
        public string ImageId { get; set; } = Guid.NewGuid().ToString();
    }
}