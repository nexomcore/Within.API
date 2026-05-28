using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Endpoints;

public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var events = app.MapGroup("/api/events");

        events.MapGet("", async (
            WithinDbContext db,
            ClaimsPrincipal principal,
            WithinLens? lens,
            bool? free,
            bool? online,
            bool? weekend,
            string? search,
            string? tag,
            Guid? providerId) =>
        {
            var userId = principal.TryUserId();
            var query = db.Events.Where(item => item.Status == EventStatus.Published);
            if (lens is not null) query = query.Where(item => item.Lens == lens);
            if (free is true) query = query.Where(item => item.PriceAmount == 0);
            if (online is not null) query = query.Where(item => item.IsOnline == online);
            if (weekend is true) query = query.Where(item => item.StartUtc.DayOfWeek == DayOfWeek.Saturday || item.StartUtc.DayOfWeek == DayOfWeek.Sunday);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(item => item.Title.ToLower().Contains(search.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(tag)) query = query.Where(item => item.Tags.Contains(tag.Trim().ToLower()));
            if (providerId is not null) query = query.Where(item => item.ProviderId == providerId);
            return Results.Ok(await ApiMapping.ProjectEvents(query.OrderBy(item => item.StartUtc), db, userId).ToArrayAsync());
        });

        events.MapGet("/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var item = await ApiMapping.ProjectEvents(db.Events.Where(evt => evt.Id == id), db, userId).FirstOrDefaultAsync();
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        events.MapPost("", async (UpsertEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());
            if (provider is null) return Results.Forbid();

            var evt = request.ToEntity(provider.Id);
            db.Events.Add(evt);
            await db.SaveChangesAsync();
            return Results.Created($"/api/events/{evt.Id}", await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == evt.Id), db, principal.UserId()).FirstAsync());
        }).RequireAuthorization();

        events.MapPut("/{id:guid}", async (Guid id, UpsertEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var evt = await db.Events.FindAsync(id);
            var provider = evt is null ? null : await db.Providers.FindAsync(evt.ProviderId);
            if (evt is null || provider is null) return Results.NotFound();
            if (provider.OwnerUserId != principal.UserId()) return Results.Forbid();

            request.ApplyTo(evt);
            await db.SaveChangesAsync();
            return Results.Ok(await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == evt.Id), db, principal.UserId()).FirstAsync());
        }).RequireAuthorization();

        events.MapPost("/{id:guid}/join", async (Guid id, JoinEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var evt = await db.Events.FindAsync(id);
            if (evt is null) return Results.NotFound();

            var registration = await db.EventRegistrations.FirstOrDefaultAsync(item => item.EventId == id && item.UserId == userId);
            if (registration is null)
            {
                registration = new EventRegistration { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow };
                db.EventRegistrations.Add(registration);
            }

            registration.State = request.State;
            registration.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == id), db, userId).FirstAsync());
        }).RequireAuthorization();

        events.MapPost("/{id:guid}/save", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await db.Events.AnyAsync(item => item.Id == id)) return Results.NotFound();
            if (!await db.SavedEvents.AnyAsync(item => item.EventId == id && item.UserId == userId))
            {
                db.SavedEvents.Add(new SavedEvent { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow });
                await db.SaveChangesAsync();
            }
            return Results.NoContent();
        }).RequireAuthorization();

        events.MapDelete("/{id:guid}/save", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            await db.SavedEvents.Where(item => item.EventId == id && item.UserId == userId).ExecuteDeleteAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        events.MapGet("/{id:guid}/comments", async (Guid id, WithinDbContext db) =>
            Results.Ok(await ApiMapping.ProjectComments(db.Comments.Where(item => item.EventId == id && !item.IsHidden), db).ToArrayAsync()));

        events.MapPost("/{id:guid}/comments", async (Guid id, UpsertCommentDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await db.Events.AnyAsync(item => item.Id == id)) return Results.NotFound();
            var comment = new Comment { Id = Guid.NewGuid(), EventId = id, AuthorUserId = principal.UserId(), Body = request.Body.Trim(), CreatedUtc = DateTimeOffset.UtcNow };
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
            var dto = await ApiMapping.ProjectComments(db.Comments.Where(item => item.Id == comment.Id), db).FirstAsync();
            return Results.Created($"/api/events/{id}/comments/{comment.Id}", dto);
        }).RequireAuthorization();

        events.MapPost("/{id:guid}/reviews", async (Guid id, UpsertReviewDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (request.Rating is < 1 or > 5) return Results.BadRequest(new { message = "Rating must be 1-5." });
            var userId = principal.UserId();
            var review = await db.Reviews.FirstOrDefaultAsync(item => item.EventId == id && item.UserId == userId);
            if (review is null)
            {
                review = new Review { Id = Guid.NewGuid(), EventId = id, UserId = userId, CreatedUtc = DateTimeOffset.UtcNow };
                db.Reviews.Add(review);
            }
            review.Rating = request.Rating;
            review.Body = request.Body.Trim();
            await db.SaveChangesAsync();
            return Results.Ok(review);
        }).RequireAuthorization();

        return app;
    }
}
