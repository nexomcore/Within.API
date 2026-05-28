using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class ProviderEndpoints
{
    public static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder app)
    {
        var providers = app.MapGroup("/api/providers");

        providers.MapGet("", async (WithinDbContext db, WithinLens? lens) =>
        {
            var query = db.Providers.AsQueryable();
            if (lens is not null) query = query.Where(item => item.Lens == lens);
            return Results.Ok(await query.OrderBy(item => item.Name).Select(item => item.ToDto()).ToArrayAsync());
        });

        providers.MapGet("/{id:guid}", async (Guid id, WithinDbContext db) =>
        {
            var provider = await db.Providers.FindAsync(id);
            return provider is null ? Results.NotFound() : Results.Ok(provider.ToDto());
        });

        providers.MapPost("", async (UpsertProviderDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var provider = new Provider
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                Name = request.Name.Trim(),
                Slug = Slugs.From(request.Name),
                Bio = request.Bio.Trim(),
                Lens = request.Lens,
                Location = request.Location.Trim(),
                WebsiteUrl = request.WebsiteUrl,
                InstagramUrl = request.InstagramUrl,
                IsVerified = false,
                CreatedUtc = DateTimeOffset.UtcNow
            };
            db.Providers.Add(provider);
            await db.SaveChangesAsync();
            return Results.Created($"/api/providers/{provider.Id}", provider.ToDto());
        }).RequireAuthorization();

        return app;
    }
}
