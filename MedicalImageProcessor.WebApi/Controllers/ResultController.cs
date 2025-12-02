// Controllers/ResultsController.cs

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using MedicalImageProcessor.WebApi.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly Supabase.Client _supabase;

    public ResultsController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUserResults()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var response = await _supabase
                .From<DetectionResultRecord>()
                .Where(x => x.UserId == userId)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            var results = response.Models.Select(r => new
            {
                imageId = Path.GetFileNameWithoutExtension(r.ImageUrl?.Split('/').Last() ?? "unknown"),
                r.ImageUrl,
                r.HasBrainTumor,
                r.BrainTumorConfidence,
                r.HasFracture,
                r.FractureConfidence,
                r.CreatedAt
            }).ToList();

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}