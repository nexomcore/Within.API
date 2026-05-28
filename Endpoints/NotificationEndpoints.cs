using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/api/notifications").RequireAuthorization();

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
                ? new NotificationPreferencesDto(true, true, true, true, WithinLens.Feel)
                : new NotificationPreferencesDto(prefs.DailyMotivationEnabled, prefs.EventRemindersEnabled, prefs.CommunitySummariesEnabled, prefs.ProviderNewEventsEnabled, prefs.PreferredLens));
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
            prefs.PreferredLens = request.PreferredLens;
            await db.SaveChangesAsync();
            return Results.Ok(request);
        });

        return app;
    }
}
