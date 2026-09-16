using JobApplicationTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationTracker.Data;

public static class UserSeeder
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        IConfiguration configuration,
        IPasswordHasher<AppUser> passwordHasher)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var username = configuration["SeedUser:Username"];
        var password = configuration["SeedUser:Password"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Seed user credentials are not configured.");
        }

        var user = new AppUser
        {
            Username = username.Trim(),
            NormalizedUsername = username.Trim().ToUpperInvariant()
        };

        user.PasswordHash =
            passwordHasher.HashPassword(user, password);

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
