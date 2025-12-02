// Models/DetectionResultRecord.cs
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MedicalImageProcessor.WebApi.Models;

[Table("detection_results")]
public class DetectionResultRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public string? UserId { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("model_type")]
    public string? ModelType { get; set; }

    [Column("has_brain_tumor")]
    public bool HasBrainTumor { get; set; }

    [Column("brain_tumor_confidence")]
    public float BrainTumorConfidence { get; set; }

    [Column("has_fracture")]
    public bool HasFracture { get; set; }

    [Column("fracture_confidence")]
    public float FractureConfidence { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}