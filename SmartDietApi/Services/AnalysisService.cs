using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartDietApi.Data;
using SmartDietApi.DTOs.Analysis;
using SmartDietApi.Entities;

namespace SmartDietApi.Services;

public class AnalysisService
{
    private readonly AppDbContext _db;
    private readonly MlClientService _mlClient;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Approximate daily RDA values for micronutrients
    private static readonly Dictionary<string, (double Rda, string Unit)> RdaMap = new()
    {
        ["Sodium"] = (2300, "mg"),
        ["Potassium"] = (3500, "mg"),
        ["Calcium"] = (1000, "mg"),
        ["Iron"] = (18, "mg"),
        ["Vitamin A"] = (900, "mcg"),
        ["Vitamin C"] = (90, "mg"),
        ["Vitamin D"] = (20, "mcg"),
        ["Vitamin B12"] = (2.4, "mcg"),
        ["Zinc"] = (11, "mg"),
        ["Magnesium"] = (400, "mg"),
        ["Phosphorus"] = (700, "mg"),
    };

    public AnalysisService(
        AppDbContext db,
        MlClientService mlClient,
        IWebHostEnvironment env,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _mlClient = mlClient;
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AnalysisResultDto> UploadAndAnalyzeAsync(string userId, IFormFile image)
    {
        // Save image
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";
        var filename = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, filename);

        await using (var fs = File.Create(filePath))
            await image.CopyToAsync(fs);

        var imageUrl = $"/uploads/{filename}";

        // Call ML service
        await using var imageStream = image.OpenReadStream();
        var mlResult = await _mlClient.PredictAsync(imageStream, image.FileName);

        // Map to food items
        var foodItems = mlResult.FoodItems.Select(fi => new FoodItemDto
        {
            Name = fi.Name.Replace("_", " "),
            Confidence = fi.Confidence,
            PortionGrams = fi.PortionGrams,
            Calories = fi.Nutrients.Calories,
            Nutrients = new NutrientsDto
            {
                Calories = fi.Nutrients.Calories,
                Carbohydrates = fi.Nutrients.Carbohydrates,
                Protein = fi.Nutrients.Protein,
                TotalFat = fi.Nutrients.TotalFat,
                DietaryFiber = fi.Nutrients.DietaryFiber,
                TotalSugars = fi.Nutrients.TotalSugars,
                SaturatedFat = fi.Nutrients.SaturatedFat,
                Cholesterol = fi.Nutrients.Cholesterol,
                Sodium = fi.Nutrients.Sodium,
                Potassium = fi.Nutrients.Potassium,
                Calcium = fi.Nutrients.Calcium,
                Iron = fi.Nutrients.Iron,
                VitaminA = fi.Nutrients.VitaminA,
                VitaminC = fi.Nutrients.VitaminC,
                VitaminD = fi.Nutrients.VitaminD,
                VitaminB12 = fi.Nutrients.VitaminB12,
                Zinc = fi.Nutrients.Zinc,
                Magnesium = fi.Nutrients.Magnesium,
                Phosphorus = fi.Nutrients.Phosphorus,
            }
        }).ToList();

        // Aggregate macros
        var macro = new MacronutrientsDto
        {
            Carbs = foodItems.Sum(f => f.Nutrients.Carbohydrates),
            Protein = foodItems.Sum(f => f.Nutrients.Protein),
            Fat = foodItems.Sum(f => f.Nutrients.TotalFat),
            Fiber = foodItems.Sum(f => f.Nutrients.DietaryFiber),
        };

        // Build micronutrients
        var micros = BuildMicronutrients(foodItems);

        var mealType = DetectMealType(DateTime.UtcNow);

        var analysis = new MealAnalysis
        {
            UserId = userId,
            ImageUrl = imageUrl,
            MealType = mealType,
            FoodItemsJson = JsonSerializer.Serialize(foodItems),
            TotalCalories = mlResult.TotalCalories,
            MacroCarbs = macro.Carbs,
            MacroProtein = macro.Protein,
            MacroFat = macro.Fat,
            MacroFiber = macro.Fiber,
            MicronutrientsJson = JsonSerializer.Serialize(micros),
        };

        _db.MealAnalyses.Add(analysis);
        await _db.SaveChangesAsync();

        return ToDto(analysis, foodItems, macro, micros);
    }

    public async Task<AnalysisResultDto> GetAnalysisAsync(string userId, string analysisId)
    {
        var analysis = await _db.MealAnalyses
            .FirstOrDefaultAsync(a => a.Id == analysisId && a.UserId == userId)
            ?? throw new KeyNotFoundException("Analysis not found.");

        return ToDto(analysis);
    }

    public async Task<List<MealRecordDto>> GetHistoryAsync(string userId, int page, int limit)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var records = await _db.MealAnalyses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return records.Select(r =>
        {
            var foodItems = JsonSerializer.Deserialize<List<FoodItemDto>>(r.FoodItemsJson) ?? new();
            return new MealRecordDto
            {
                Id = r.Id,
                ImageUrl = r.ImageUrl,
                CreatedAt = r.CreatedAt.ToString("o"),
                MealType = r.MealType,
                FoodItems = foodItems.Select(f => f.Name).ToList(),
                TotalCalories = r.TotalCalories,
            };
        }).ToList();
    }

    private static string DetectMealType(DateTime utcNow)
    {
        var hour = utcNow.Hour;
        return hour switch
        {
            >= 6 and <= 10 => "breakfast",
            >= 11 and <= 15 => "lunch",
            >= 16 and <= 19 => "dinner",
            _ => "snack"
        };
    }

    private static List<MicronutrientDto> BuildMicronutrients(List<FoodItemDto> items)
    {
        var totals = new Dictionary<string, double>
        {
            ["Sodium"] = items.Sum(f => f.Nutrients.Sodium),
            ["Potassium"] = items.Sum(f => f.Nutrients.Potassium),
            ["Calcium"] = items.Sum(f => f.Nutrients.Calcium),
            ["Iron"] = items.Sum(f => f.Nutrients.Iron),
            ["Vitamin A"] = items.Sum(f => f.Nutrients.VitaminA),
            ["Vitamin C"] = items.Sum(f => f.Nutrients.VitaminC),
            ["Vitamin D"] = items.Sum(f => f.Nutrients.VitaminD),
            ["Vitamin B12"] = items.Sum(f => f.Nutrients.VitaminB12),
            ["Zinc"] = items.Sum(f => f.Nutrients.Zinc),
            ["Magnesium"] = items.Sum(f => f.Nutrients.Magnesium),
            ["Phosphorus"] = items.Sum(f => f.Nutrients.Phosphorus),
        };

        return totals.Select(kv =>
        {
            var (rda, unit) = RdaMap.TryGetValue(kv.Key, out var r) ? r : (100, "mg");
            return new MicronutrientDto
            {
                Name = kv.Key,
                Value = Math.Round(kv.Value, 2),
                Unit = unit,
                RdaPercentage = rda > 0 ? Math.Round(kv.Value / rda * 100, 1) : 0,
            };
        }).ToList();
    }

    private static AnalysisResultDto ToDto(MealAnalysis a,
        List<FoodItemDto>? foodItems = null,
        MacronutrientsDto? macro = null,
        List<MicronutrientDto>? micros = null)
    {
        foodItems ??= JsonSerializer.Deserialize<List<FoodItemDto>>(a.FoodItemsJson) ?? new();
        macro ??= new MacronutrientsDto
        {
            Carbs = a.MacroCarbs,
            Protein = a.MacroProtein,
            Fat = a.MacroFat,
            Fiber = a.MacroFiber,
        };
        micros ??= JsonSerializer.Deserialize<List<MicronutrientDto>>(a.MicronutrientsJson) ?? new();

        return new AnalysisResultDto
        {
            Id = a.Id,
            ImageUrl = a.ImageUrl,
            CreatedAt = a.CreatedAt.ToString("o"),
            MealType = a.MealType,
            FoodItems = foodItems,
            TotalCalories = a.TotalCalories,
            Macronutrients = macro,
            Micronutrients = micros,
        };
    }
}
