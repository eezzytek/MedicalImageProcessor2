namespace MedicalImageProcessor.Core.Entities
{
    public class DetectionResult
    {
        public bool HasBrainTumor { get; set; }
        public float BrainTumorConfidence { get; set; }
        public bool HasFracture { get; set; }
        public float FractureConfidence { get; set; }
        public string ImageId { get; set; } = string.Empty;
    }
}