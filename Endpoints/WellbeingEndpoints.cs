using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class WellbeingEndpoints
{
    public static IEndpointRouteBuilder MapWellbeingEndpoints(this IEndpointRouteBuilder app)
    {
        var wellbeing = app.MapGroup("/api/wellbeing").RequireAuthorization();

        wellbeing.MapPost("/daily-checkin", async (DailyCheckInDto request, WithinDbContext db, WellbeingScoringService scoring, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var date = DateOnly.Parse(request.CheckInDate);

            if (request.MoodScore is < 1 or > 5 ||
                request.EnergyScore is < 1 or > 5 ||
                request.StressScore is < 1 or > 5 ||
                request.ConnectionScore is < 1 or > 5 ||
                request.MeaningScore is < 1 or > 5)
            {
                return Results.BadRequest(new { message = "All daily pulse scores must be between 1 and 5." });
            }

            if (request.Note?.Length > 500)
            {
                return Results.BadRequest(new { message = "Daily note must be 500 characters or fewer." });
            }

            var saved = await db.DailyCheckIns.FirstOrDefaultAsync(item => item.UserId == userId && item.CheckInDate == date);
            if (saved is null)
            {
                saved = new DailyCheckIn { Id = Guid.NewGuid(), UserId = userId, CheckInDate = date };
                db.DailyCheckIns.Add(saved);
            }
            saved.MoodScore = request.MoodScore;
            saved.EnergyScore = request.EnergyScore;
            saved.StressScore = request.StressScore;
            saved.ConnectionScore = request.ConnectionScore;
            saved.MeaningScore = request.MeaningScore;
            saved.Tags = request.Tags.Select(tag => tag.Trim().ToLowerInvariant()).Where(tag => tag.Length > 0).Distinct().ToArray();
            saved.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            saved.DailyBalanceScore = scoring.CalculateDailyBalance(request);
            await db.SaveChangesAsync();
            return Results.Ok(saved.ToDto());
        });

        wellbeing.MapGet("/daily-checkin/today", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var userId = principal.UserId();
            var checkIn = await db.DailyCheckIns.FirstOrDefaultAsync(item => item.UserId == userId && item.CheckInDate == today);
            return checkIn is null ? Results.NotFound() : Results.Ok(checkIn.ToDto());
        });

        wellbeing.MapGet("/daily-checkin/trend", async (WithinDbContext db, ClaimsPrincipal principal, int? days) =>
        {
            var range = Math.Clamp(days ?? 7, 1, 30);
            var end = DateOnly.FromDateTime(DateTime.UtcNow);
            var start = end.AddDays(-(range - 1));
            var userId = principal.UserId();
            var trend = await db.DailyCheckIns
                .Where(item => item.UserId == userId && item.CheckInDate >= start && item.CheckInDate <= end)
                .OrderBy(item => item.CheckInDate)
                .Select(item => new TrendItemDto(item.CheckInDate.ToString("yyyy-MM-dd"), item.DailyBalanceScore))
                .ToArrayAsync();
            return Results.Ok(trend);
        });

        wellbeing.MapGet("/dashboard", async (WithinDbContext db, ClaimsPrincipal principal, WellbeingScoringService scoring) =>
        {
            var userId = principal.UserId();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekStart = today.AddDays(-6);
            var recent = await db.DailyCheckIns
                .Where(item => item.UserId == userId && item.CheckInDate >= weekStart && item.CheckInDate <= today)
                .OrderBy(item => item.CheckInDate)
                .ToArrayAsync();
            var todayCheckIn = recent.FirstOrDefault(item => item.CheckInDate == today);
            var weeklyAverages = recent.Length == 0
                ? new WeeklyAveragesDto(0, 0, 0, 0, 0)
                : new WeeklyAveragesDto(
                    ApiMapping.Average(recent, item => item.MoodScore),
                    ApiMapping.Average(recent, item => item.EnergyScore),
                    ApiMapping.Average(recent, item => item.StressScore),
                    ApiMapping.Average(recent, item => item.ConnectionScore),
                    ApiMapping.Average(recent, item => item.MeaningScore));
            var areas = recent.Length == 0 ? ((string?)null, (string?)null) : scoring.GetStrongestAndSupport(weeklyAverages);

            return Results.Ok(new WellbeingDashboardDto
            {
                TodayCheckInCompleted = todayCheckIn is not null,
                Today = todayCheckIn?.ToDto(),
                DailyBalanceScore = todayCheckIn?.DailyBalanceScore,
                WeeklyAverages = weeklyAverages,
                StrongestArea = areas.Item1,
                SupportArea = areas.Item2,
                TrendItems = recent.Select(item => new TrendItemDto(item.CheckInDate.ToString("yyyy-MM-dd"), item.DailyBalanceScore)).ToArray(),
                MonthlyProfileCompleted = false,
                Recommendations = [],
                RecentReflections = recent
                    .Where(item => !string.IsNullOrWhiteSpace(item.Note))
                    .OrderByDescending(item => item.CheckInDate)
                    .Take(3)
                    .Select(item => new ReflectionDto(item.Id.ToString(), item.CheckInDate.ToString("yyyy-MM-dd"), "Daily Pulse", item.Note!))
                    .ToArray()
            });
        });

        return app;
    }
}
