using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Infrastructure.Identity;

namespace TalentoPlus.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        if (!context.Departments.Any())
        {
            context.Departments.AddRange(
                new Department { Id = Guid.NewGuid(), Name = "Talento Humano" },
                new Department { Id = Guid.NewGuid(), Name = "Tecnología" },
                new Department { Id = Guid.NewGuid(), Name = "Finanzas" }
            );
            await context.SaveChangesAsync();
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@talentoplus.com";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!";
        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Document = "ADMIN"
            };
            await userManager.CreateAsync(user, adminPassword);
        }
    }
}
