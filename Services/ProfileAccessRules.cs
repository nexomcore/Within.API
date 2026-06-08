using WithinAPI.Domain;

namespace WithinAPI.Services;

/// <summary>How a user's identity should be displayed in a given context.</summary>
public sealed record DisplayIdentity(string DisplayName, bool ProfileLinkAllowed, CircleIdentityMode IdentityMode);

/// <summary>
/// Pure, side-effect-free privacy/identity decisions for the Profile Preview &amp;
/// connection flow. PrivacyService fetches the data; these functions decide. Keeping
/// the rules pure makes the spec §14 matrix unit-testable without a database, and gives
/// the system a single source of truth for "what may a displayed identity lead to".
/// </summary>
public static class ProfileAccessRules
{
    /// <summary>Maps a stored connection row to the viewer-relative state shown on a preview.</summary>
    public static (ProfileConnectionState State, Guid? ConnectionId) ConnectionState(Connection? connection, Guid viewerUserId)
    {
        if (connection is null) return (ProfileConnectionState.None, null);
        return connection.Status switch
        {
            ConnectionStatus.Blocked => (ProfileConnectionState.Blocked, connection.Id),
            ConnectionStatus.Accepted => (ProfileConnectionState.Connected, connection.Id),
            ConnectionStatus.Pending => (
                connection.RequesterUserId == viewerUserId
                    ? ProfileConnectionState.PendingSent
                    : ProfileConnectionState.PendingReceived,
                connection.Id),
            // Rejected / Cancelled / Removed are terminal: treat as connectable again,
            // but surface the row id so a new request can reuse it.
            _ => (ProfileConnectionState.None, connection.Id)
        };
    }

    /// <summary>
    /// Whether a Connect action may be offered from the current context (spec §6).
    /// Assumes the caller has already confirmed the target is visible in this context.
    /// </summary>
    public static bool CanRequestConnection(
        bool isSelf,
        bool blocked,
        ProfileConnectionState connectionState,
        FriendRequestPermission targetPermission,
        bool sharesCircleOrEvent,
        CircleIdentityMode identityMode)
    {
        if (isSelf) return false;
        if (blocked) return false;
        if (connectionState is ProfileConnectionState.Connected
            or ProfileConnectionState.PendingSent
            or ProfileConnectionState.PendingReceived
            or ProfileConnectionState.Blocked)
        {
            return false;
        }

        // A hidden circle identity must not surface a connect action (spec §5, §7.5).
        if (identityMode == CircleIdentityMode.HiddenProfile) return false;

        return targetPermission switch
        {
            FriendRequestPermission.Everyone => true,
            FriendRequestPermission.FriendsOfFriends => false, // not modelled in this MVP
            FriendRequestPermission.SameCircleOrEvent => sharesCircleOrEvent,
            FriendRequestPermission.NoOne => false,
            _ => false
        };
    }

    /// <summary>
    /// Whether full profile navigation is permitted (spec §5). Pseudonym/Hidden identities
    /// never resolve to a full real profile, regardless of profile visibility.
    /// </summary>
    public static bool CanViewFullProfile(
        bool isSelf,
        ProfileVisibility visibility,
        bool connected,
        bool sharesCircle,
        CircleIdentityMode identityMode)
    {
        if (isSelf) return true;
        if (identityMode != CircleIdentityMode.RealProfile) return false;

        return visibility switch
        {
            ProfileVisibility.Public => true,
            ProfileVisibility.FriendsOnly => connected,
            ProfileVisibility.CircleMembersOnly => sharesCircle,
            ProfileVisibility.Private => false,
            _ => false
        };
    }

    /// <summary>
    /// Resolves how a user should appear inside a circle (spec §5). Pure mirror of the
    /// circle identity resolver: never returns the real name for Pseudonym/HiddenProfile
    /// to anyone other than the member themselves.
    /// </summary>
    public static DisplayIdentity ResolveCircleIdentity(
        string realDisplayName,
        CircleIdentityMode? memberIdentityMode,
        string? displayNameOverride,
        bool isViewerSelf)
    {
        if (memberIdentityMode is null or CircleIdentityMode.RealProfile)
        {
            return new DisplayIdentity(realDisplayName, true, CircleIdentityMode.RealProfile);
        }

        if (isViewerSelf)
        {
            return new DisplayIdentity(displayNameOverride ?? realDisplayName, true, memberIdentityMode.Value);
        }

        return memberIdentityMode.Value switch
        {
            CircleIdentityMode.Pseudonym => new DisplayIdentity(displayNameOverride ?? "Circle Member", false, CircleIdentityMode.Pseudonym),
            CircleIdentityMode.HiddenProfile => new DisplayIdentity("Circle Member", false, CircleIdentityMode.HiddenProfile),
            _ => new DisplayIdentity(realDisplayName, true, CircleIdentityMode.RealProfile)
        };
    }

    /// <summary>Whether the viewer may see a given RSVP (spec §7.1).</summary>
    public static bool CanViewRsvp(bool isPrivileged, RsvpVisibility visibility, bool connected, bool sharesEventCircle)
    {
        if (isPrivileged) return true;
        return visibility switch
        {
            RsvpVisibility.Public => true,
            RsvpVisibility.FriendsOnly => connected,
            RsvpVisibility.CircleMembersOnly => sharesEventCircle,
            RsvpVisibility.Private => false,
            _ => false
        };
    }

    /// <summary>Whether the viewer may see a circle member at all (spec §7.4).</summary>
    public static bool CanViewCircleMember(bool isPrivileged, MemberListVisibility visibility, bool isMember)
    {
        if (isPrivileged) return true;
        return visibility switch
        {
            MemberListVisibility.Public => true,
            MemberListVisibility.MembersOnly => isMember,
            MemberListVisibility.AdminsOnly => false,
            MemberListVisibility.Hidden => false,
            _ => false
        };
    }

    /// <summary>
    /// Whether a displayed identity should be tappable at all. Hidden identities are still
    /// tappable (they open a safety/hidden card with report/block), but a resolved-away
    /// identity (null) is not.
    /// </summary>
    public static bool IsClickable(bool targetResolved, bool visibleInContext) => targetResolved && visibleInContext;
}
