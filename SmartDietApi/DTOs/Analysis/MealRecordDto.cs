namespace SmartDietApi.DTOs.Analysis;

public class MealRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public List<string> FoodItems { get; set; } = new();
    public double TotalCalories { get; set; }
}
