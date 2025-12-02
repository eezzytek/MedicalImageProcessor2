// Models/UserRecord.cs
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MedicalImageProcessor.WebApi.Models;

[Table("users")]
public class UserRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("username")]
    public string Username { get; set; } = null!;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}