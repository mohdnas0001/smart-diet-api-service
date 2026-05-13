using Microsoft.EntityFrameworkCore;
using SmartDietApi.Entities;

namespace SmartDietApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MealAnalysis> MealAnalyses => Set<MealAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasMany(u => u.RefreshTokens)
                  .WithOne(rt => rt.User)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(u => u.MealAnalyses)
                  .WithOne(ma => ma.User)
                  .HasForeignKey(ma => ma.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(rt => rt.Token).IsUnique();
        });

        modelBuilder.Entity<MealAnalysis>(entity =>
        {
            entity.HasIndex(ma => new { ma.UserId, ma.CreatedAt });
        });
    }
}
