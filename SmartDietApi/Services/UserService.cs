using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartDietApi.Data;
using SmartDietApi.DTOs.User;
using SmartDietApi.Entities;

namespace SmartDietApi.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserDto> GetProfileAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        return AuthService.MapToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(string userId, UserUpdateRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (request.Name is not null) user.Name = request.Name;
        if (request.Avatar is not null) user.Avatar = request.Avatar;
        if (request.DateOfBirth is not null) user.DateOfBirth = request.DateOfBirth;
        if (request.Gender is not null) user.Gender = request.Gender;
        if (request.Height.HasValue) user.Height = request.Height.Value;
        if (request.Weight.HasValue) user.Weight = request.Weight.Value;
        if (request.ActivityLevel is not null) user.ActivityLevel = request.ActivityLevel;
        if (request.DietaryPreferences is not null)
            user.DietaryPreferencesJson = JsonSerializer.Serialize(request.DietaryPreferences);
        if (request.Allergies is not null)
            user.AllergiesJson = JsonSerializer.Serialize(request.Allergies);
        if (request.CalorieTarget.HasValue) user.CalorieTarget = request.CalorieTarget.Value;
        if (request.MacroTargets is not null)
        {
            user.MacroCarbsTarget = request.MacroTargets.Carbs;
            user.MacroProteinTarget = request.MacroTargets.Protein;
            user.MacroFatTarget = request.MacroTargets.Fat;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return AuthService.MapToDto(user);
    }
}

public class UserUpdateRequest
{
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? ActivityLevel { get; set; }
    public List<string>? DietaryPreferences { get; set; }
    public List<string>? Allergies { get; set; }
    public double? CalorieTarget { get; set; }
    public MacroTargetsDto? MacroTargets { get; set; }
}
