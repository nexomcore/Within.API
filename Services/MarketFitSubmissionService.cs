using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Services;

public interface IMarketFitSubmissionService
{
    Task<MarketFitSubmissionResponseDto?> CreateAsync(MarketFitSubmissionDto request, CancellationToken cancellationToken = default);
    Task<AdminSubmissionDto[]> GetSubmissionsAsync(string? audience, CancellationToken cancellationToken = default);
    Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteSubmissionAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class MarketFitSubmissionService(WithinDbContext db) : IMarketFitSubmissionService
{
    public async Task<MarketFitSubmissionResponseDto?> CreateAsync(MarketFitSubmissionDto request, CancellationToken cancellationToken = default)
    {
        var audience = request.Audience.Trim().ToLowerInvariant();
        if (audience is not ("user" or "provider" or "interest") ||
            string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Contact))
        {
            return null;
        }

        var submission = new MarketFitSubmission
        {
            Id = Guid.NewGuid(),
            Audience = audience,
            Name = request.Name.Trim(),
            Contact = request.Contact.Trim(),
            Source = string.IsNullOrWhiteSpace(request.Source) ? "landing-page" : request.Source.Trim(),
            AnswersJson = JsonSerializer.Serialize(request.Answers),
            CreatedUtc = DateTimeOffset.UtcNow
        };

        db.MarketFitSubmissions.Add(submission);
        await db.SaveChangesAsync(cancellationToken);

        return new MarketFitSubmissionResponseDto(
            submission.Id,
            submission.Audience,
            submission.Name,
            submission.Contact,
            submission.Source,
            submission.CreatedUtc);
    }

    public async Task<AdminSubmissionDto[]> GetSubmissionsAsync(string? audience, CancellationToken cancellationToken = default)
    {
        var query = db.MarketFitSubmissions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(audience))
        {
            var filter = audience.Trim().ToLowerInvariant();
            query = query.Where(item => item.Audience == filter);
        }

        var rows = await query.OrderByDescending(item => item.CreatedUtc).ToArrayAsync(cancellationToken);
        return rows.Select(item => new AdminSubmissionDto(
            item.Id,
            item.Audience,
            item.Name,
            item.Contact,
            item.Source,
            JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(item.AnswersJson) ? "{}" : item.AnswersJson),
            item.CreatedUtc)).ToArray();
    }

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var submissions = await db.MarketFitSubmissions
            .GroupBy(item => item.Audience)
            .Select(group => new { Audience = group.Key, Count = group.Count() })
            .ToArrayAsync(cancellationToken);
        var userCount = submissions.FirstOrDefault(item => item.Audience == "user")?.Count ?? 0;
        var providerCount = submissions.FirstOrDefault(item => item.Audience == "provider")?.Count ?? 0;
        var total = submissions.Sum(item => item.Count);
        var latest = await db.MarketFitSubmissions
            .OrderByDescending(item => item.CreatedUtc)
            .Select(item => (DateTimeOffset?)item.CreatedUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var userTotals = await db.Users
            .GroupBy(item => item.Role)
            .Select(group => new { Role = group.Key, Count = group.Count() })
            .ToArrayAsync(cancellationToken);
        var totalUsers = userTotals.Sum(item => item.Count);
        var providerUsers = userTotals.FirstOrDefault(item => item.Role == WithinRole.Provider)?.Count ?? 0;
        var adminUsers = userTotals.FirstOrDefault(item => item.Role == WithinRole.Admin)?.Count ?? 0;

        return new AdminStatsDto(total, userCount, providerCount, totalUsers, providerUsers, adminUsers, latest);
    }

    public async Task<bool> DeleteSubmissionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await db.MarketFitSubmissions.Where(item => item.Id == id).ExecuteDeleteAsync(cancellationToken);
        return deleted > 0;
    }
}
