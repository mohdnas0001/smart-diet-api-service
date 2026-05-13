using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartDietApi.Data;
using SmartDietApi.DTOs.Auth;
using SmartDietApi.DTOs.User;
using SmartDietApi.Entities;

namespace SmartDietApi.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        if (request.Profile is { } profile)
        {
            user.DateOfBirth = profile.DateOfBirth;
            user.Gender = profile.Gender;
            user.Height = profile.Height;
            user.Weight = profile.Weight;
            user.ActivityLevel = profile.ActivityLevel;
            user.DietaryPreferencesJson = JsonSerializer.Serialize(profile.DietaryPreferences);
            user.AllergiesJson = JsonSerializer.Serialize(profile.Allergies);
            user.CalorieTarget = profile.CalorieTarget > 0 ? profile.CalorieTarget : 2000;
            user.MacroCarbsTarget = profile.MacroTargets.Carbs > 0 ? profile.MacroTargets.Carbs : 250;
            user.MacroProteinTarget = profile.MacroTargets.Protein > 0 ? profile.MacroTargets.Protein : 75;
            user.MacroFatTarget = profile.MacroTargets.Fat > 0 ? profile.MacroTargets.Fat : 65;
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower())
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task LogoutAsync(string userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.IsRevoked = true;

        await _db.SaveChangesAsync();
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken)
    {
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");

        return GenerateAccessToken(stored.User);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = MapToDto(user)
        };
    }

    private string GenerateAccessToken(User user)
    {
        var secret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(string userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        });

        await _db.SaveChangesAsync();
        return token;
    }

    public static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Height = user.Height,
            Weight = user.Weight,
            ActivityLevel = user.ActivityLevel,
            DietaryPreferences = JsonSerializer.Deserialize<List<string>>(user.DietaryPreferencesJson) ?? new(),
            Allergies = JsonSerializer.Deserialize<List<string>>(user.AllergiesJson) ?? new(),
            CalorieTarget = user.CalorieTarget,
            MacroTargets = new DTOs.User.MacroTargetsDto
            {
                Carbs = user.MacroCarbsTarget,
                Protein = user.MacroProteinTarget,
                Fat = user.MacroFatTarget,
            }
        };
    }
}
