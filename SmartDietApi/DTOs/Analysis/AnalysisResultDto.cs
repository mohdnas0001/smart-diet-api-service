namespace SmartDietApi.DTOs.Analysis;

public class AnalysisResultDto
{
    public string Id { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public List<FoodItemDto> FoodItems { get; set; } = new();
    public double TotalCalories { get; set; }
    public MacronutrientsDto Macronutrients { get; set; } = new();
    public List<MicronutrientDto> Micronutrients { get; set; } = new();
}

public class FoodItemDto
{
    public string Name { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public double PortionGrams { get; set; }
    public double Calories { get; set; }
    public NutrientsDto Nutrients { get; set; } = new();
}

public class NutrientsDto
{
    public double Calories { get; set; }
    public double Carbohydrates { get; set; }
    public double Protein { get; set; }
    public double TotalFat { get; set; }
    public double DietaryFiber { get; set; }
    public double TotalSugars { get; set; }
    public double SaturatedFat { get; set; }
    public double Cholesterol { get; set; }
    public double Sodium { get; set; }
    public double Potassium { get; set; }
    public double Calcium { get; set; }
    public double Iron { get; set; }
    public double VitaminA { get; set; }
    public double VitaminC { get; set; }
    public double VitaminD { get; set; }
    public double VitaminB12 { get; set; }
    public double Zinc { get; set; }
    public double Magnesium { get; set; }
    public double Phosphorus { get; set; }
}

public class MacronutrientsDto
{
    public double Carbs { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Fiber { get; set; }
}

public class MicronutrientDto
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double RdaPercentage { get; set; }
}
