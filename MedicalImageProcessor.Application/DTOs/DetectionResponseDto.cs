namespace MedicalImageProcessor.Application.DTOs
{
    public class DetectionResponseDto
    {
        public bool HasBrainTumor { get; set; }
        public float BrainTumorConfidence { get; set; }
        public bool HasFracture { get; set; }
        public float FractureConfidence { get; set; }
    }
}