using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/api/notifications").RequireAuthorization();

        notifications.MapPost("", async (CreateNotificationDto request, NotificationService notificationService, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (request.UserId != userId && !principal.IsInRole(nameof(WithinRole.Admin))) return Results.Forbid();
            var notification = await notificationService.CreateAsync(new NotificationCreateRequest(
                request.UserId,
                request.Kind,
                request.Title,
                request.Body,
                request.TargetType,
                request.TargetId,
                request.ActorUserId,
                request.CircleId,
                request.EventId,
                request.RelatedUserId));
            return notification is null
                ? Results.NoContent()
                : Results.Created($"/api/notifications/{notification.Id}", notificationService.ToDto(notification));
        });

        notifications.MapGet("", async (WithinDbContext db, NotificationService notificationService, ClaimsPrincipal principal, bool? unreadOnly, int? pageSize) =>
        {
            var userId = principal.UserId();
            var size = Math.Clamp(pageSize ?? 50, 1, 100);
            var query = db.Notifications.Where(item => item.UserId == userId);
            if (unreadOnly is true) query = query.Where(item => !item.IsRead);
            var items = await query.OrderByDescending(item => item.CreatedUtc).Take(size).ToArrayAsync();
            return Results.Ok(items.Select(notificationService.ToDto).ToArray());
        });

        notifications.MapGet("/unread-count", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var count = await db.Notifications.CountAsync(item => item.UserId == userId && !item.IsRead);
            return Results.Ok(new { count });
        });

        notifications.MapPost("/{notificationId:guid}/read", async (Guid notificationId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var notification = await db.Notifications.FirstOrDefaultAsync(item => item.Id == notificationId && item.UserId == userId);
            if (notification is null) return Results.NotFound();
            notification.IsRead = true;
            notification.ReadUtc ??= DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        notifications.MapPost("/read-all", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var now = DateTimeOffset.UtcNow;
            await db.Notifications
                .Where(item => item.UserId == userId && !item.IsRead)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.IsRead, true)
                    .SetProperty(item => item.ReadUtc, now));
            return Results.NoContent();
        });

        notifications.MapPost("/push-token", async (PushTokenDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var now = DateTimeOffset.UtcNow;
            var existing = await db.PushTokens.FirstOrDefaultAsync(item => item.Token == request.Token);
            if (existing is null)
            {
                db.PushTokens.Add(new PushToken { Id = Guid.NewGuid(), UserId = userId, Token = request.Token, Platform = request.Platform, CreatedUtc = now, UpdatedUtc = now });
            }
            else
            {
                existing.UserId = userId;
                existing.Platform = request.Platform;
                existing.UpdatedUtc = now;
            }
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        notifications.MapPost("/device-token", async (DeviceTokenDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var existing = await db.DeviceTokens.FirstOrDefaultAsync(item => item.Token == request.Token);
            if (existing is null)
            {
                db.DeviceTokens.Add(new DeviceToken { Id = Guid.NewGuid(), UserId = userId, Token = request.Token, Platform = request.Platform, CreatedUtc = DateTimeOffset.UtcNow });
            }
            else
            {
                existing.UserId = userId;
                existing.Platform = request.Platform;
            }
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        notifications.MapGet("/preferences", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var prefs = await db.NotificationPreferences.FirstOrDefaultAsync(item => item.UserId == userId);
            return Results.Ok(prefs is null
                ? new NotificationPreferencesDto(true, true, true, true, true, true, true, true, true, true, WithinLens.Feel)
                : ToPreferencesDto(prefs));
        });

        notifications.MapPut("/preferences", async (NotificationPreferencesDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var prefs = await db.NotificationPreferences.FirstOrDefaultAsync(item => item.UserId == userId);
            if (prefs is null)
            {
                prefs = new NotificationPreference { Id = Guid.NewGuid(), UserId = userId };
                db.NotificationPreferences.Add(prefs);
            }
            prefs.DailyMotivationEnabled = request.DailyMotivationEnabled;
            prefs.EventRemindersEnabled = request.EventRemindersEnabled;
            prefs.CommunitySummariesEnabled = request.CommunitySummariesEnabled;
            prefs.ProviderNewEventsEnabled = request.ProviderNewEventsEnabled;
            prefs.FriendRequestsEnabled = request.FriendRequestsEnabled;
            prefs.EventInvitesEnabled = request.EventInvitesEnabled;
            prefs.FriendActivityEnabled = request.FriendActivityEnabled;
            prefs.CircleRepliesEnabled = request.CircleRepliesEnabled;
            prefs.CommentRepliesEnabled = request.CommentRepliesEnabled;
            prefs.MentionsEnabled = request.MentionsEnabled;
            prefs.PreferredLens = request.PreferredLens;
            await db.SaveChangesAsync();
            return Results.Ok(ToPreferencesDto(prefs));
        });

        notifications.MapGet("/mutes", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var mutes = await db.NotificationMutes
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CreatedUtc)
                .Select(item => new NotificationMuteDto(item.TargetType, item.TargetId))
                .ToArrayAsync();
            return Results.Ok(mutes);
        });

        notifications.MapPut("/mutes", async (NotificationMuteDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await db.NotificationMutes.AnyAsync(item => item.UserId == userId && item.TargetType == request.TargetType && item.TargetId == request.TargetId))
            {
                db.NotificationMutes.Add(new NotificationMute
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TargetType = request.TargetType,
                    TargetId = request.TargetId,
                    CreatedUtc = DateTimeOffset.UtcNow
                });
                await db.SaveChangesAsync();
            }
            return Results.NoContent();
        });

        notifications.MapDelete("/mutes/{targetType}/{targetId:guid}", async (NotificationMuteTargetType targetType, Guid targetId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            await db.NotificationMutes
                .Where(item => item.UserId == userId && item.TargetType == targetType && item.TargetId == targetId)
                .ExecuteDeleteAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static NotificationPreferencesDto ToPreferencesDto(NotificationPreference prefs) => new(
        prefs.DailyMotivationEnabled,
        prefs.EventRemindersEnabled,
        prefs.CommunitySummariesEnabled,
        prefs.ProviderNewEventsEnabled,
        prefs.FriendRequestsEnabled,
        prefs.EventInvitesEnabled,
        prefs.FriendActivityEnabled,
        prefs.CircleRepliesEnabled,
        prefs.CommentRepliesEnabled,
        prefs.MentionsEnabled,
        prefs.PreferredLens);
}
