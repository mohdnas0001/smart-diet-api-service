using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDietApi.DTOs.Analysis;
using SmartDietApi.Services;

namespace SmartDietApi.Controllers;

[ApiController]
[Route("api/analysis")]
[Authorize]
public class AnalysisController : ControllerBase
{
    private readonly AnalysisService _analysisService;

    public AnalysisController(AnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    private string GetUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
     ?? User.FindFirst("sub")?.Value
     ?? throw new UnauthorizedAccessException();

    /// <summary>Upload a food image and receive nutritional analysis.</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AnalysisResultDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Upload(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest(new { message = "Image file is required." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(image.ContentType.ToLowerInvariant()))
            return BadRequest(new { message = "Only image files are allowed (jpeg, png, webp, gif)." });

        var result = await _analysisService.UploadAndAnalyzeAsync(GetUserId(), image);
        return Ok(result);
    }

    /// <summary>Get a specific analysis result by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnalysisResultDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAnalysis(string id)
    {
        try
        {
            var result = await _analysisService.GetAnalysisAsync(GetUserId(), id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Get the meal analysis history for the current user.</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<MealRecordDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var records = await _analysisService.GetHistoryAsync(GetUserId(), page, limit);
        return Ok(records);
    }
}
