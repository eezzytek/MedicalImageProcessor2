using System.Security.Claims;
using MedicalImageProcessor.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supabase;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly Supabase.Client _supabase;

    public DebugController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetRawResults()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                     ?? User.FindFirst("sub")?.Value;

        var response = await _supabase
            .From<DetectionResultRecord>()
            .Where(x => x.UserId == userId)
            .Get();

        // Повертаємо ВСІ поля як є
        var raw = response.Models.Select(r => new
        {
            r.Id,
            r.UserId,
            r.ImageUrl,
            r.HasBrainTumor,
            r.BrainTumorConfidence,
            r.HasFracture,
            r.FractureConfidence,
            r.CreatedAt
        });

        return Ok(raw);
    }
}