using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDietApi.DTOs.User;
using SmartDietApi.Services;

namespace SmartDietApi.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    private string GetUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
     ?? User.FindFirst("sub")?.Value
     ?? throw new UnauthorizedAccessException();

    /// <summary>Get the authenticated user's profile.</summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetProfile()
    {
        var dto = await _userService.GetProfileAsync(GetUserId());
        return Ok(dto);
    }

    /// <summary>Update the authenticated user's profile.</summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateRequest request)
    {
        var dto = await _userService.UpdateProfileAsync(GetUserId(), request);
        return Ok(dto);
    }
}
