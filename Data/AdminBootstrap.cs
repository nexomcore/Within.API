using Microsoft.EntityFrameworkCore;
using WithinAPI.Domain;
using WithinAPI.Services;

namespace WithinAPI.Data;

public static class AdminBootstrap
{
    public static async Task EnsureAsync(WithinDbContext db, IConfiguration configuration)
    {
        var email = configuration["AdminBootstrap:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["AdminBootstrap:Password"];
        var displayName = configuration["AdminBootstrap:DisplayName"]?.Trim();
        var resetPassword = configuration.GetValue("AdminBootstrap:ResetPassword", false);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email);
        if (user is null)
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Within Admin" : displayName,
                Email = email,
                PasswordHash = Passwords.Hash(password),
                RoleId = RoleCatalog.AdminRoleId,
                CreatedUtc = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
            return;
        }

        var changed = false;
        if (user.RoleId != RoleCatalog.AdminRoleId)
        {
            user.RoleId = RoleCatalog.AdminRoleId;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(displayName) && user.DisplayName != displayName)
        {
            user.DisplayName = displayName;
            changed = true;
        }

        if (resetPassword && !Passwords.Verify(password, user.PasswordHash))
        {
            user.PasswordHash = Passwords.Hash(password);
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
    }
}
