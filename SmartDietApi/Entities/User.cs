using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDietApi.Entities;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public string DateOfBirth { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Gender { get; set; } = "other";

    public double Height { get; set; }

    public double Weight { get; set; }

    [MaxLength(50)]
    public string ActivityLevel { get; set; } = "sedentary";

    // Stored as JSON strings
    public string DietaryPreferencesJson { get; set; } = "[]";

    public string AllergiesJson { get; set; } = "[]";

    public double CalorieTarget { get; set; } = 2000;

    public double MacroCarbsTarget { get; set; } = 250;
    public double MacroProteinTarget { get; set; } = 75;
    public double MacroFatTarget { get; set; } = 65;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<MealAnalysis> MealAnalyses { get; set; } = new List<MealAnalysis>();
}
