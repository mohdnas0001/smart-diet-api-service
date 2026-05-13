using System.ComponentModel.DataAnnotations;

namespace SmartDietApi.Entities;

public class MealAnalysis
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(20)]
    public string MealType { get; set; } = "snack";

    // Full analysis result stored as JSON
    public string FoodItemsJson { get; set; } = "[]";

    public double TotalCalories { get; set; }

    public double MacroCarbs { get; set; }
    public double MacroProtein { get; set; }
    public double MacroFat { get; set; }
    public double MacroFiber { get; set; }

    // Micronutrients stored as JSON
    public string MicronutrientsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
