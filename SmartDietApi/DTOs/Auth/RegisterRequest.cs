using System.ComponentModel.DataAnnotations;

namespace SmartDietApi.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public ProfileInfo? Profile { get; set; }
}

public class ProfileInfo
{
    public string DateOfBirth { get; set; } = string.Empty;
    public string Gender { get; set; } = "other";
    public double Height { get; set; }
    public double Weight { get; set; }
    public string ActivityLevel { get; set; } = "sedentary";
    public List<string> DietaryPreferences { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public double CalorieTarget { get; set; } = 2000;
    public MacroTargetsDto MacroTargets { get; set; } = new();
}

public class MacroTargetsDto
{
    public double Carbs { get; set; } = 250;
    public double Protein { get; set; } = 75;
    public double Fat { get; set; } = 65;
}
