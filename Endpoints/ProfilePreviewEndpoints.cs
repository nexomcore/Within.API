using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

/// <summary>
/// The single, backend-controlled entry point for tapping a displayed identity (spec §4, §9).
/// The frontend references the clicked identity only by (contextType, contextId,
/// targetContextProfileId); the backend resolves the real user internally and never returns
/// real identity fields for pseudonym/hidden users.
/// </summary>
public static class ProfilePreviewEndpoints
{
    public static IEndpointRouteBuilder MapProfilePreviewEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/profile-preview", async (
            ProfileContextType contextType,
            Guid contextId,
            Guid targetContextProfileId,
            PrivacyService privacy,
            ClaimsPrincipal principal) =>
        {
            var card = await privacy.GetDisplayProfileCard(principal.UserId(), contextType, contextId, targetContextProfileId);
            return card is null ? Results.NotFound() : Results.Ok(card);
        }).RequireAuthorization();

        var connections = app.MapGroup("/api/connections").RequireAuthorization();

        connections.MapPost("/request-from-context", async (
            ConnectionFromContextDto request, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var requesterUserId = principal.UserId();
            var resolution = await privacy.ResolveContextProfile(requesterUserId, request.ContextType, request.ContextId, request.TargetContextProfileId);
            if (resolution is null) return Results.NotFound();
            if (!await privacy.CanRequestConnectionFromContext(requesterUserId, request.ContextType, request.ContextId, request.TargetContextProfileId))
            {
                return Results.BadRequest(new { message = "Connection requests are not available from this space." });
            }

            var targetUserId = resolution.TargetUserId;
            var existing = await FindConnection(db, requesterUserId, targetUserId);
            if (existing is not null && existing.Status is ConnectionStatus.Pending or ConnectionStatus.Accepted)
            {
                return Results.Conflict(new { message = "A connection already exists or is pending." });
            }

            var now = DateTimeOffset.UtcNow;
            var connection = existing ?? new Connection
            {
                Id = Guid.NewGuid(),
                RequesterUserId = requesterUserId,
                ReceiverUserId = targetUserId,
                CreatedAt = now
            };
            // Reuse a terminal row but always point it from the current requester.
            connection.RequesterUserId = requesterUserId;
            connection.ReceiverUserId = targetUserId;
            connection.Status = ConnectionStatus.Pending;
            connection.UpdatedAt = now;
            connection.RespondedAt = null;
            connection.BlockedAt = null;
            connection.BlockedByUserId = null;
            if (existing is null) db.Connections.Add(connection);
            await db.SaveChangesAsync();
            // Intentionally no body: returning a ConnectionDto would leak the real display name.
            return Results.NoContent();
        });

        connections.MapPost("/block-from-context", async (
            BlockFromContextDto request, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var blockerUserId = principal.UserId();
            var resolution = await privacy.ResolveContextProfile(blockerUserId, request.ContextType, request.ContextId, request.TargetContextProfileId);
            if (resolution is null) return Results.NotFound();
            var targetUserId = resolution.TargetUserId;
            if (blockerUserId == targetUserId) return Results.BadRequest(new { message = "You cannot block yourself." });

            var now = DateTimeOffset.UtcNow;
            var connection = await FindConnection(db, blockerUserId, targetUserId);
            if (connection is null)
            {
                connection = new Connection
                {
                    Id = Guid.NewGuid(),
                    RequesterUserId = blockerUserId,
                    ReceiverUserId = targetUserId,
                    CreatedAt = now
                };
                db.Connections.Add(connection);
            }

            connection.Status = ConnectionStatus.Blocked;
            connection.BlockedByUserId = blockerUserId;
            connection.BlockedAt = now;
            connection.UpdatedAt = now;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        connections.MapPost("/report-from-context", async (
            ReportFromContextDto request, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var reporterUserId = principal.UserId();
            var resolution = await privacy.ResolveContextProfile(reporterUserId, request.ContextType, request.ContextId, request.TargetContextProfileId);
            if (resolution is null) return Results.NotFound();
            var targetUserId = resolution.TargetUserId;
            if (reporterUserId == targetUserId) return Results.BadRequest(new { message = "You cannot report yourself." });

            var now = DateTimeOffset.UtcNow;
            db.UserReports.Add(new UserReport
            {
                Id = Guid.NewGuid(),
                ReportedByUserId = reporterUserId,
                ReportedUserId = targetUserId,
                SourceType = ToMentionSource(request.ContextType),
                SourceId = request.TargetContextProfileId,
                Reason = request.Reason,
                Details = request.Details?.Trim(),
                Status = UserReportStatus.Open,
                CreatedAt = now,
                UpdatedAt = now
            });
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static MentionSourceType? ToMentionSource(ProfileContextType contextType) => contextType switch
    {
        ProfileContextType.EventComment => MentionSourceType.EventComment,
        ProfileContextType.CirclePost => MentionSourceType.CirclePost,
        ProfileContextType.CircleComment => MentionSourceType.CircleComment,
        _ => null
    };

    private static async Task<Connection?> FindConnection(WithinDbContext db, Guid firstUserId, Guid secondUserId) =>
        await db.Connections.FirstOrDefaultAsync(item =>
            (item.RequesterUserId == firstUserId && item.ReceiverUserId == secondUserId) ||
            (item.RequesterUserId == secondUserId && item.ReceiverUserId == firstUserId));
}
