using Microsoft.EntityFrameworkCore;
using WithinAPI.Data;
using WithinAPI.Domain;

namespace WithinAPI.Services;

public enum AccountDeletionStatus
{
    Deleted,
    NotFound,
    Blocked
}

public sealed record AccountDeletionResult(AccountDeletionStatus Status, string? Message = null);

/// <summary>
/// Deletes a user account in line with APP 11.2: destroys PII and sensitive personal data,
/// and de-identifies retained audit/history by keeping a scrubbed "tombstone" user row.
/// </summary>
public sealed class AccountDeletionService(WithinDbContext db)
{
    public async Task<AccountDeletionResult> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            return new AccountDeletionResult(AccountDeletionStatus.NotFound);
        }

        // ---- Guards: block while the user still owns business/community structures ----
        if (await db.Providers.AnyAsync(item => item.OwnerUserId == userId && item.IsActive, cancellationToken))
        {
            return new AccountDeletionResult(AccountDeletionStatus.Blocked,
                "Transfer or deactivate your provider profile before deleting your account.");
        }

        var soleAdminCircle = await db.CircleMembers
            .Where(member => member.UserId == userId
                && member.Status == CircleMemberStatus.Active
                && member.Role == CircleMemberRole.Admin)
            .Select(member => member.CircleId)
            .Where(circleId => db.CircleMembers.Count(other =>
                other.CircleId == circleId
                && other.Status == CircleMemberStatus.Active
                && other.Role == CircleMemberRole.Admin) == 1)
            .AnyAsync(cancellationToken);
        if (soleAdminCircle)
        {
            return new AccountDeletionResult(AccountDeletionStatus.Blocked,
                "Assign another admin to your circle(s) before deleting your account.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // ---- Hard-delete: PII, sensitive personal data, and personal operational data ----
        // Auth / session
        await db.RefreshTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.PushTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.DeviceTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // Health / wellbeing (sensitive)
        await db.UserWellbeingProfiles.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserWellbeingInterests.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserWellbeingGoals.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.DailyCheckIns.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.MonthlyProfiles.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserHabits.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.HabitCompletions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // Preferences / notifications
        await db.NotificationPreferences.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.NotificationMutes.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.NotificationSchedules.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.Notifications.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserPrivacySettings.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // Lightweight personal operational data (no audit value)
        await db.SavedEvents.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.Reactions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleReactions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleHelpfulReactions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CirclePollVotes.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleWeeklyCheckInResponses.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CommunityMembers.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // Relationships and inbound invites (no longer meaningful once the account is gone)
        await db.Connections
            .Where(item => item.RequesterUserId == userId || item.ReceiverUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await db.EventInvites.Where(item => item.InvitedUserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleInvites.Where(item => item.InvitedUserId == userId).ExecuteDeleteAsync(cancellationToken);

        // ---- Retain, de-identified: leave the user's circle membership as history, inactive ----
        await db.CircleMembers
            .Where(item => item.UserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(member => member.Status, CircleMemberStatus.Left), cancellationToken);

        // Retained content/audit (circle threads & comments, reviews, event registrations,
        // reports, role/join history, mentions, shared events, announcements) keep their
        // UserId references; they render via the scrubbed tombstone user below.

        // ---- Scrub the user row (tombstone) ----
        user.Email = $"deleted-{user.Id}@deleted.invalid";
        user.DisplayName = "Deleted user";
        user.PasswordHash = "";
        user.IsDeleted = true;
        user.DeletedUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return new AccountDeletionResult(AccountDeletionStatus.Deleted);
    }

    /// <summary>
    /// Permanently purges an already-deleted (tombstone) account: removes the user row and
    /// every remaining row that references it, including the content they authored and its
    /// dependents (so nothing is left dangling — there are no DB-level FK cascades).
    /// Only operates on tombstones; soft-delete the account first via <see cref="DeleteAccountAsync"/>.
    /// </summary>
    public async Task<AccountDeletionResult> HardDeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return new AccountDeletionResult(AccountDeletionStatus.NotFound);
        }

        // Purge is only for already-deleted tombstone rows. Active accounts must be
        // soft-deleted first so PII destruction + the ownership guards below are applied.
        if (!user.IsDeleted)
        {
            return new AccountDeletionResult(AccountDeletionStatus.Blocked,
                "Delete this account first, then purge the remaining tombstone.");
        }

        // ---- Guards: non-nullable ownership columns can't be cleared, so block instead ----
        if (await db.Providers.AnyAsync(item => item.OwnerUserId == userId, cancellationToken))
        {
            return new AccountDeletionResult(AccountDeletionStatus.Blocked,
                "Delete the provider linked to this account before purging it.");
        }

        if (await db.Circles.AnyAsync(item => item.CreatedByUserId == userId, cancellationToken))
        {
            return new AccountDeletionResult(AccountDeletionStatus.Blocked,
                "This account created one or more circles. Reassign or delete them before purging.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // ---- Collect IDs of content authored by the user (and dependents) up front ----
        // (ExecuteDelete runs immediately, so gather parent ids before deleting them.)
        var threadIds = await db.CircleThreads
            .Where(item => item.UserId == userId).Select(item => item.Id).ToListAsync(cancellationToken);
        var pollIds = await db.CirclePolls
            .Where(item => threadIds.Contains(item.ThreadId)).Select(item => item.Id).ToListAsync(cancellationToken);
        var commentIds = await db.CircleThreadComments
            .Where(item => item.UserId == userId || threadIds.Contains(item.ThreadId))
            .Select(item => item.Id).ToListAsync(cancellationToken);

        var postIds = await db.Posts
            .Where(item => item.AuthorUserId == userId).Select(item => item.Id).ToListAsync(cancellationToken);
        var legacyCommentIds = await db.Comments
            .Where(item => item.AuthorUserId == userId || (item.PostId != null && postIds.Contains(item.PostId.Value)))
            .Select(item => item.Id).ToListAsync(cancellationToken);

        // Mentions reference content by a generic SourceId; match the affected content ids.
        var contentSourceIds = threadIds
            .Concat(commentIds).Concat(postIds).Concat(legacyCommentIds).ToList();

        // ---- Circle content + dependents ----
        await db.CirclePollVotes
            .Where(item => item.UserId == userId || pollIds.Contains(item.PollId))
            .ExecuteDeleteAsync(cancellationToken);
        await db.CirclePollOptions
            .Where(item => pollIds.Contains(item.PollId)).ExecuteDeleteAsync(cancellationToken);
        await db.CirclePolls
            .Where(item => pollIds.Contains(item.Id)).ExecuteDeleteAsync(cancellationToken);
        await db.CircleWeeklyCheckInResponses
            .Where(item => item.UserId == userId || threadIds.Contains(item.ThreadId))
            .ExecuteDeleteAsync(cancellationToken);
        await db.CircleHelpfulReactions
            .Where(item => item.UserId == userId
                || (item.ThreadId != null && threadIds.Contains(item.ThreadId.Value))
                || (item.CommentId != null && commentIds.Contains(item.CommentId.Value)))
            .ExecuteDeleteAsync(cancellationToken);
        await db.CircleReactions
            .Where(item => item.UserId == userId
                || (item.ThreadId != null && threadIds.Contains(item.ThreadId.Value))
                || (item.CommentId != null && commentIds.Contains(item.CommentId.Value)))
            .ExecuteDeleteAsync(cancellationToken);
        await db.CircleReports
            .Where(item => item.ReporterUserId == userId
                || (item.ThreadId != null && threadIds.Contains(item.ThreadId.Value))
                || (item.CommentId != null && commentIds.Contains(item.CommentId.Value)))
            .ExecuteDeleteAsync(cancellationToken);
        await db.CircleThreadComments
            .Where(item => commentIds.Contains(item.Id)).ExecuteDeleteAsync(cancellationToken);
        await db.CircleThreads
            .Where(item => threadIds.Contains(item.Id)).ExecuteDeleteAsync(cancellationToken);

        // ---- Legacy community content + dependents ----
        await db.Reactions
            .Where(item => item.UserId == userId
                || (item.PostId != null && postIds.Contains(item.PostId.Value))
                || (item.CommentId != null && legacyCommentIds.Contains(item.CommentId.Value)))
            .ExecuteDeleteAsync(cancellationToken);
        await db.Comments
            .Where(item => legacyCommentIds.Contains(item.Id)).ExecuteDeleteAsync(cancellationToken);
        await db.Posts
            .Where(item => postIds.Contains(item.Id)).ExecuteDeleteAsync(cancellationToken);

        // ---- Mentions of / by the user, and mentions on the deleted content ----
        await db.Mentions
            .Where(item => item.MentionedUserId == userId
                || item.MentionedByUserId == userId
                || contentSourceIds.Contains(item.SourceId))
            .ExecuteDeleteAsync(cancellationToken);

        // ---- The user's participation / membership / history everywhere ----
        await db.EventRegistrations.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.SavedEvents.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.Reviews.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CommunityMembers.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleMembers.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleRoles.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleJoinRequests.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleInvites
            .Where(item => item.InvitedUserId == userId || item.InvitedByUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await db.CircleEvents.Where(item => item.SharedByUserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.CircleAnnouncements.Where(item => item.AuthorUserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.Connections
            .Where(item => item.RequesterUserId == userId || item.ReceiverUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await db.EventInvites
            .Where(item => item.InvitedUserId == userId || item.InvitedByUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await db.UserReports
            .Where(item => item.ReportedByUserId == userId || item.ReportedUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        // ---- Personal / preference / auth data (idempotent with the soft-delete pass) ----
        await db.SavedEvents.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.Notifications.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.NotificationPreferences.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.NotificationMutes.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.NotificationSchedules.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserPrivacySettings.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.RefreshTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.PushTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.DeviceTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserWellbeingProfiles.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserWellbeingInterests.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserWellbeingGoals.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.DailyCheckIns.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.MonthlyProfiles.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.HabitCompletions.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserHabits.Where(item => item.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // ---- Clear nullable "reviewer / actor" audit columns on rows we keep (no dangling) ----
        await db.CircleJoinRequests
            .Where(item => item.ReviewedByUserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.ReviewedByUserId, (Guid?)null), cancellationToken);
        await db.CircleRoles
            .Where(item => item.AssignedByUserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.AssignedByUserId, (Guid?)null), cancellationToken);
        await db.CircleReports
            .Where(item => item.ReviewedByUserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.ReviewedByUserId, (Guid?)null), cancellationToken);
        await db.Notifications
            .Where(item => item.ActorUserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.ActorUserId, (Guid?)null), cancellationToken);
        await db.Notifications
            .Where(item => item.RelatedUserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.RelatedUserId, (Guid?)null), cancellationToken);

        // ---- Finally, the user row itself ----
        await db.Users.Where(item => item.Id == userId).ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return new AccountDeletionResult(AccountDeletionStatus.Deleted);
    }
}
