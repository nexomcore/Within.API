using Microsoft.EntityFrameworkCore;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Services;

/// <summary>Result of resolving a context-referenced display identity to its real target.</summary>
public sealed record ContextResolution(
    Guid TargetUserId,
    DisplayIdentity Identity,
    bool SharesCircleOrEventWithViewer,
    bool SharesCircleWithViewer);

public sealed class PrivacyService(WithinDbContext db)
{
    public async Task<UserPrivacySettings> GetOrCreateSettings(Guid userId)
    {
        var settings = await db.UserPrivacySettings.FindAsync(userId);
        if (settings is not null) return settings;

        var now = DateTimeOffset.UtcNow;
        settings = new UserPrivacySettings
        {
            UserId = userId,
            ProfileVisibility = ProfileVisibility.FriendsOnly,
            DefaultRsvpVisibility = RsvpVisibility.FriendsOnly,
            TaggingPermission = TaggingPermission.FriendsOnly,
            FriendRequestPermission = FriendRequestPermission.SameCircleOrEvent,
            ShowActivityToFriends = true,
            AllowEventInviteFromFriends = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.UserPrivacySettings.Add(settings);
        await db.SaveChangesAsync();
        return settings;
    }

    public async Task<bool> AreConnected(Guid firstUserId, Guid secondUserId) =>
        await db.Connections.AnyAsync(item =>
            item.Status == ConnectionStatus.Accepted &&
            ((item.RequesterUserId == firstUserId && item.ReceiverUserId == secondUserId) ||
             (item.RequesterUserId == secondUserId && item.ReceiverUserId == firstUserId)));

    public async Task<bool> IsBlocked(Guid firstUserId, Guid secondUserId) =>
        await db.Connections.AnyAsync(item =>
            item.Status == ConnectionStatus.Blocked &&
            ((item.RequesterUserId == firstUserId && item.ReceiverUserId == secondUserId) ||
             (item.RequesterUserId == secondUserId && item.ReceiverUserId == firstUserId)));

    public async Task<bool> ShareCircle(Guid firstUserId, Guid secondUserId) =>
        await (
            from first in db.CircleMembers
            join second in db.CircleMembers on first.CircleId equals second.CircleId
            where first.UserId == firstUserId &&
                  second.UserId == secondUserId &&
                  first.Status == CircleMemberStatus.Active &&
                  second.Status == CircleMemberStatus.Active
            select first.CircleId).AnyAsync();

    public async Task<bool> ShareVisibleEvent(Guid firstUserId, Guid secondUserId) =>
        await (
            from first in db.EventRegistrations
            join second in db.EventRegistrations on first.EventId equals second.EventId
            where first.UserId == firstUserId &&
                  second.UserId == secondUserId &&
                  first.State != EventJoinState.Declined &&
                  second.State != EventJoinState.Declined
            select first.EventId).AnyAsync();

    public async Task<bool> CanSendConnectionRequest(Guid requesterUserId, Guid receiverUserId)
    {
        if (requesterUserId == receiverUserId) return false;
        if (await IsBlocked(requesterUserId, receiverUserId)) return false;
        var receiverSettings = await GetOrCreateSettings(receiverUserId);
        return receiverSettings.FriendRequestPermission switch
        {
            FriendRequestPermission.Everyone => true,
            FriendRequestPermission.FriendsOfFriends => false,
            FriendRequestPermission.SameCircleOrEvent => await ShareCircle(requesterUserId, receiverUserId) || await ShareVisibleEvent(requesterUserId, receiverUserId),
            FriendRequestPermission.NoOne => false,
            _ => false
        };
    }

    public async Task<bool> CanViewEventRsvp(Guid viewerUserId, Event evt, EventRegistration rsvp)
    {
        if (viewerUserId == rsvp.UserId) return true;
        var provider = await db.Providers.FindAsync(evt.ProviderId);
        var viewer = await db.Users.FindAsync(viewerUserId);
        var isPrivileged = viewer?.RoleEnum == WithinRole.Admin || provider?.OwnerUserId == viewerUserId;

        return ProfileAccessRules.CanViewRsvp(
            isPrivileged,
            rsvp.Visibility,
            await AreConnected(viewerUserId, rsvp.UserId),
            await ShareEventCircle(viewerUserId, rsvp.UserId, evt.Id));
    }

    public async Task<bool> ShareEventCircle(Guid viewerUserId, Guid attendeeUserId, Guid eventId) =>
        await (
            from share in db.CircleEvents
            join viewer in db.CircleMembers on share.CircleId equals viewer.CircleId
            join attendee in db.CircleMembers on share.CircleId equals attendee.CircleId
            where share.EventId == eventId &&
                  share.Status == CircleEventStatus.Active &&
                  viewer.UserId == viewerUserId &&
                  attendee.UserId == attendeeUserId &&
                  viewer.Status == CircleMemberStatus.Active &&
                  attendee.Status == CircleMemberStatus.Active
            select share.Id).AnyAsync();

    public async Task<bool> CanMentionUser(Guid actorUserId, Guid targetUserId, Guid? circleId = null)
    {
        if (actorUserId == targetUserId) return true;
        if (await IsBlocked(actorUserId, targetUserId)) return false;
        var settings = await GetOrCreateSettings(targetUserId);
        return settings.TaggingPermission switch
        {
            TaggingPermission.Everyone => true,
            TaggingPermission.FriendsOnly => await AreConnected(actorUserId, targetUserId),
            TaggingPermission.CircleMembersOnly => circleId is null
                ? await ShareCircle(actorUserId, targetUserId)
                : await IsCircleMember(circleId.Value, actorUserId) && await IsCircleMember(circleId.Value, targetUserId),
            TaggingPermission.NoOne => false,
            _ => false
        };
    }

    public async Task<bool> IsCircleMember(Guid circleId, Guid userId) =>
        await db.CircleMembers.AnyAsync(item => item.CircleId == circleId && item.UserId == userId && item.Status == CircleMemberStatus.Active);

    public async Task<bool> CanViewCircleMember(Guid viewerUserId, Guid circleId, Guid targetUserId)
    {
        if (viewerUserId == targetUserId) return true;
        var circle = await db.Circles.FindAsync(circleId);
        if (circle is null) return false;
        var isMember = await IsCircleMember(circleId, viewerUserId);
        var isModerator = await db.CircleRoles.AnyAsync(item => item.CircleId == circleId && item.UserId == viewerUserId);
        var viewer = await db.Users.FindAsync(viewerUserId);
        var isPrivileged = viewer?.RoleEnum == WithinRole.Admin || isModerator;

        return ProfileAccessRules.CanViewCircleMember(isPrivileged, circle.MemberListVisibility, isMember);
    }

    public async Task<DisplayIdentity> GetDisplayIdentityForCircle(Guid? viewerUserId, Guid circleId, Guid targetUserId)
    {
        var user = await db.Users.FindAsync(targetUserId);
        if (user is null) return new DisplayIdentity("Circle Member", false, CircleIdentityMode.HiddenProfile);
        var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == targetUserId);
        return ProfileAccessRules.ResolveCircleIdentity(
            user.DisplayName,
            member?.IdentityMode,
            member?.DisplayNameOverride,
            viewerUserId == targetUserId);
    }

    // ---- Profile Preview context resolution (spec §8) ----

    /// <summary>
    /// Resolves a clicked display identity (referenced safely by context) to its real user
    /// and circle-safe display identity, after enforcing the context's visibility gate.
    /// Returns null when the viewer may not see the identity, so callers expose nothing.
    /// </summary>
    public async Task<ContextResolution?> ResolveContextProfile(
        Guid? viewerUserId, ProfileContextType contextType, Guid contextId, Guid targetContextProfileId)
    {
        // Every Profile Preview surface requires an authenticated viewer.
        if (viewerUserId is null) return null;
        var viewer = viewerUserId.Value;

        switch (contextType)
        {
            case ProfileContextType.EventAttendee:
            case ProfileContextType.EventFriendGoing:
            {
                var evt = await db.Events.FindAsync(contextId);
                if (evt is null) return null;
                var targetUserId = targetContextProfileId; // attendees are referenced by real userId
                var registration = await db.EventRegistrations.FirstOrDefaultAsync(item =>
                    item.EventId == contextId && item.UserId == targetUserId && item.State != EventJoinState.Declined);
                if (registration is null) return null;
                if (!await CanViewEventRsvp(viewer, evt, registration)) return null;
                if (contextType == ProfileContextType.EventFriendGoing
                    && viewer != targetUserId
                    && !await AreConnected(viewer, targetUserId))
                {
                    return null;
                }
                var user = await db.Users.FindAsync(targetUserId);
                if (user is null) return null;
                return await BuildResolution(viewer, targetUserId, new DisplayIdentity(user.DisplayName, true, CircleIdentityMode.RealProfile));
            }

            case ProfileContextType.EventComment:
            {
                var comment = await db.Comments.FirstOrDefaultAsync(item =>
                    item.Id == targetContextProfileId && item.EventId == contextId && !item.IsHidden);
                if (comment is null) return null;
                var user = await db.Users.FindAsync(comment.AuthorUserId);
                if (user is null) return null;
                return await BuildResolution(viewer, comment.AuthorUserId, new DisplayIdentity(user.DisplayName, true, CircleIdentityMode.RealProfile));
            }

            case ProfileContextType.CircleMember:
            {
                var member = await db.CircleMembers.FirstOrDefaultAsync(item =>
                    item.Id == targetContextProfileId && item.CircleId == contextId && item.Status == CircleMemberStatus.Active);
                if (member is null) return null;
                if (!await CanViewCircleMember(viewer, contextId, member.UserId)) return null;
                var identity = await GetDisplayIdentityForCircle(viewer, contextId, member.UserId);
                return await BuildResolution(viewer, member.UserId, identity);
            }

            case ProfileContextType.CirclePost:
            {
                var thread = await db.CircleThreads.FirstOrDefaultAsync(item =>
                    item.Id == targetContextProfileId && item.CircleId == contextId && item.Status != CommunityContentStatus.Hidden);
                if (thread is null) return null;
                if (!await CanViewCircleMember(viewer, contextId, thread.UserId) && !await IsCircleMember(contextId, viewer)) return null;
                var identity = await GetDisplayIdentityForCircle(viewer, contextId, thread.UserId);
                return await BuildResolution(viewer, thread.UserId, identity);
            }

            case ProfileContextType.CircleComment:
            {
                var comment = await db.CircleThreadComments.FirstOrDefaultAsync(item =>
                    item.Id == targetContextProfileId && item.Status != CommunityContentStatus.Hidden);
                if (comment is null) return null;
                var thread = await db.CircleThreads.FindAsync(comment.ThreadId);
                if (thread is null || thread.CircleId != contextId) return null;
                if (!await CanViewCircleMember(viewer, contextId, comment.UserId) && !await IsCircleMember(contextId, viewer)) return null;
                var identity = await GetDisplayIdentityForCircle(viewer, contextId, comment.UserId);
                return await BuildResolution(viewer, comment.UserId, identity);
            }

            default:
                return null;
        }
    }

    private async Task<ContextResolution> BuildResolution(Guid viewerUserId, Guid targetUserId, DisplayIdentity identity)
    {
        var sharesCircle = await ShareCircle(viewerUserId, targetUserId);
        var sharesEvent = await ShareVisibleEvent(viewerUserId, targetUserId);
        return new ContextResolution(targetUserId, identity, sharesCircle || sharesEvent, sharesCircle);
    }

    private async Task<Connection?> FindConnection(Guid firstUserId, Guid secondUserId) =>
        await db.Connections.FirstOrDefaultAsync(item =>
            (item.RequesterUserId == firstUserId && item.ReceiverUserId == secondUserId) ||
            (item.RequesterUserId == secondUserId && item.ReceiverUserId == firstUserId));

    public async Task<(ProfileConnectionState State, Guid? ConnectionId)> GetConnectionState(Guid viewerUserId, Guid targetUserId) =>
        ProfileAccessRules.ConnectionState(await FindConnection(viewerUserId, targetUserId), viewerUserId);

    private async Task<bool> CanViewFullProfileFor(Guid viewerUserId, ContextResolution resolution)
    {
        var settings = await GetOrCreateSettings(resolution.TargetUserId);
        return ProfileAccessRules.CanViewFullProfile(
            viewerUserId == resolution.TargetUserId,
            settings.ProfileVisibility,
            await AreConnected(viewerUserId, resolution.TargetUserId),
            resolution.SharesCircleWithViewer,
            resolution.Identity.IdentityMode);
    }

    private async Task<bool> CanRequestConnectionFor(Guid viewerUserId, ContextResolution resolution)
    {
        var (state, _) = await GetConnectionState(viewerUserId, resolution.TargetUserId);
        var settings = await GetOrCreateSettings(resolution.TargetUserId);
        return ProfileAccessRules.CanRequestConnection(
            viewerUserId == resolution.TargetUserId,
            await IsBlocked(viewerUserId, resolution.TargetUserId),
            state,
            settings.FriendRequestPermission,
            resolution.SharesCircleOrEventWithViewer,
            resolution.Identity.IdentityMode);
    }

    public async Task<bool> CanOpenProfileFromContext(Guid viewerUserId, ProfileContextType contextType, Guid contextId, Guid targetContextProfileId)
    {
        var resolution = await ResolveContextProfile(viewerUserId, contextType, contextId, targetContextProfileId);
        return resolution is not null && await CanViewFullProfileFor(viewerUserId, resolution);
    }

    public async Task<bool> CanRequestConnectionFromContext(Guid viewerUserId, ProfileContextType contextType, Guid contextId, Guid targetContextProfileId)
    {
        var resolution = await ResolveContextProfile(viewerUserId, contextType, contextId, targetContextProfileId);
        return resolution is not null && await CanRequestConnectionFor(viewerUserId, resolution);
    }

    /// <summary>The privacy-safe identity for a context, or null if the viewer may not see it.</summary>
    public async Task<DisplayIdentityDto?> GetDisplayIdentityForContext(
        Guid? viewerUserId, ProfileContextType contextType, Guid contextId, Guid targetContextProfileId)
    {
        var resolution = await ResolveContextProfile(viewerUserId, contextType, contextId, targetContextProfileId);
        return resolution is null
            ? null
            : new DisplayIdentityDto(resolution.Identity.DisplayName, resolution.Identity.IdentityMode, true, resolution.Identity.ProfileLinkAllowed);
    }

    /// <summary>Builds the full Profile Preview card for a clicked identity (spec §8.2, §8.3).</summary>
    public async Task<ProfilePreviewDto?> GetDisplayProfileCard(
        Guid viewerUserId, ProfileContextType contextType, Guid contextId, Guid targetContextProfileId)
    {
        var resolution = await ResolveContextProfile(viewerUserId, contextType, contextId, targetContextProfileId);
        if (resolution is null) return null;

        var isSelf = viewerUserId == resolution.TargetUserId;
        var (state, connectionId) = await GetConnectionState(viewerUserId, resolution.TargetUserId);
        var isReal = resolution.Identity.IdentityMode == CircleIdentityMode.RealProfile;

        var sharedSummary = isReal && !isSelf
            ? await BuildSharedContextSummary(viewerUserId, resolution.TargetUserId)
            : null;

        return new ProfilePreviewDto(
            DisplayName: resolution.Identity.DisplayName,
            AvatarUrl: null,
            BioPreview: null,
            LocationPreview: null,
            SharedContextSummary: sharedSummary,
            IdentityMode: resolution.Identity.IdentityMode,
            CanViewFullProfile: await CanViewFullProfileFor(viewerUserId, resolution),
            CanRequestConnection: await CanRequestConnectionFor(viewerUserId, resolution),
            ConnectionStatus: state,
            CanReport: !isSelf,
            CanBlock: !isSelf,
            SafeProfileRoute: null,
            ConnectionId: connectionId);
    }

    private async Task<string?> BuildSharedContextSummary(Guid viewerUserId, Guid targetUserId)
    {
        var sharedCircles = await (
            from viewerMembership in db.CircleMembers
            join targetMembership in db.CircleMembers on viewerMembership.CircleId equals targetMembership.CircleId
            where viewerMembership.UserId == viewerUserId &&
                  targetMembership.UserId == targetUserId &&
                  viewerMembership.Status == CircleMemberStatus.Active &&
                  targetMembership.Status == CircleMemberStatus.Active
            select viewerMembership.CircleId).Distinct().CountAsync();

        var sharedEvents = await (
            from viewerReg in db.EventRegistrations
            join targetReg in db.EventRegistrations on viewerReg.EventId equals targetReg.EventId
            where viewerReg.UserId == viewerUserId &&
                  targetReg.UserId == targetUserId &&
                  viewerReg.State != EventJoinState.Declined &&
                  targetReg.State != EventJoinState.Declined
            select viewerReg.EventId).Distinct().CountAsync();

        if (sharedCircles == 0 && sharedEvents == 0) return null;

        var parts = new List<string>();
        if (sharedEvents > 0) parts.Add($"{sharedEvents} event{(sharedEvents == 1 ? "" : "s")}");
        if (sharedCircles > 0) parts.Add($"{sharedCircles} circle{(sharedCircles == 1 ? "" : "s")}");
        return "Shared with you: " + string.Join(", ", parts);
    }
}
