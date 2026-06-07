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

        providers.MapGet("/me", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());
            return provider is null ? Results.Forbid() : Results.Ok(provider.ToDto());
        }).RequireAuthorization("ProviderOnly");

        providers.MapGet("/me/events", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == userId);
            if (provider is null) return Results.Forbid();

            var events = await ApiMapping
                .ProjectEvents(db.Events.Where(item => item.ProviderId == provider.Id).OrderByDescending(item => item.StartUtc), db, userId)
                .ToArrayAsync();

            return Results.Ok(events);
        }).RequireAuthorization("ProviderOnly");

        providers.MapGet("/me/events/{eventId:guid}/engagement", async (Guid eventId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == userId);
            if (provider is null) return Results.Forbid();

            var evt = await db.Events.FirstOrDefaultAsync(item => item.Id == eventId && item.ProviderId == provider.Id);
            if (evt is null) return Results.NotFound();

            var registrations = await (
                from registration in db.EventRegistrations
                join user in db.Users on registration.UserId equals user.Id
                where registration.EventId == eventId
                orderby registration.UpdatedUtc descending
                select new
                {
                    registration.State,
                    Participant = new ProviderEventParticipantDto(user.Id, user.DisplayName, registration.UpdatedUtc)
                }).ToArrayAsync();

            var saved = await (
                from savedEvent in db.SavedEvents
                join user in db.Users on savedEvent.UserId equals user.Id
                where savedEvent.EventId == eventId
                orderby savedEvent.CreatedUtc descending
                select new ProviderEventParticipantDto(user.Id, user.DisplayName, savedEvent.CreatedUtc)
            ).ToArrayAsync();

            var going = registrations.Where(item => item.State == EventJoinState.Going).Select(item => item.Participant).ToArray();
            var interested = registrations.Where(item => item.State == EventJoinState.Interested).Select(item => item.Participant).ToArray();
            var declined = registrations.Where(item => item.State == EventJoinState.Declined).Select(item => item.Participant).ToArray();

            return Results.Ok(new ProviderEventEngagementDto(
                evt.Id,
                evt.Title,
                going.Length,
                interested.Length,
                declined.Length,
                saved.Length,
                going,
                interested,
                declined,
                saved));
        }).RequireAuthorization("ProviderOnly");

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
