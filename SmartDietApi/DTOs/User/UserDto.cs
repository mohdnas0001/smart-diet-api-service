namespace SmartDietApi.DTOs.User;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string DateOfBirth { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public double Height { get; set; }
    public double Weight { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
    public List<string> DietaryPreferences { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public double CalorieTarget { get; set; }
    public MacroTargetsDto MacroTargets { get; set; } = new();
}

public class MacroTargetsDto
{
    public double Carbs { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
}
