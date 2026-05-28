using Microsoft.EntityFrameworkCore;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/register", async (RegisterDto request, WithinDbContext db, AuthTokenService tokens) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(user => user.Email == email))
            {
                return Results.Conflict(new { message = "Email is already registered." });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = request.DisplayName.Trim(),
                Email = email,
                PasswordHash = Passwords.Hash(request.Password),
                Role = request.Role,
                CreatedUtc = DateTimeOffset.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Ok(await tokens.CreateResponse(user));
        });

        auth.MapPost("/login", async (LoginDto request, WithinDbContext db, AuthTokenService tokens) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email);
            if (user is null || !Passwords.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(await tokens.CreateResponse(user));
        });

        return app;
    }
}
