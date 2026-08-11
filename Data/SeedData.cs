using CosturaProducao.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync("Owner"))
            await roles.CreateAsync(new IdentityRole("Owner"));

        var email = configuration["SeedOwner:Email"];
        var password = configuration["SeedOwner:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var owner = await users.FindByEmailAsync(email);
        if (owner is null)
        {
            owner = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = "Dono" };
            var result = await users.CreateAsync(owner, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
        if (!await users.IsInRoleAsync(owner, "Owner")) await users.AddToRoleAsync(owner, "Owner");
    }
}