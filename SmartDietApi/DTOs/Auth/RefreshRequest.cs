using System.ComponentModel.DataAnnotations;

namespace SmartDietApi.DTOs.Auth;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
