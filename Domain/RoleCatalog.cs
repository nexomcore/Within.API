namespace WithinAPI.Domain;

// Single source of truth mapping the WithinRole enum to the normalized Roles lookup rows.
// The fixed ids are seeded by the role-normalization migration and reused everywhere a
// User.RoleId is read or written, so code keeps using the strongly-typed enum.
public static class RoleCatalog
{
    public static readonly Guid UserRoleId = Guid.Parse("a0a0a0a0-0000-0000-0000-000000000001");
    public static readonly Guid ProviderRoleId = Guid.Parse("a0a0a0a0-0000-0000-0000-000000000002");
    public static readonly Guid AdminRoleId = Guid.Parse("a0a0a0a0-0000-0000-0000-000000000003");
    public static readonly Guid CircleAdminRoleId = Guid.Parse("a0a0a0a0-0000-0000-0000-000000000004");

    public static Guid IdFor(WithinRole role) => role switch
    {
        WithinRole.Provider => ProviderRoleId,
        WithinRole.Admin => AdminRoleId,
        WithinRole.CircleAdmin => CircleAdminRoleId,
        _ => UserRoleId
    };

    public static WithinRole RoleOf(Guid roleId) =>
        roleId == AdminRoleId ? WithinRole.Admin :
        roleId == CircleAdminRoleId ? WithinRole.CircleAdmin :
        roleId == ProviderRoleId ? WithinRole.Provider :
        WithinRole.User;

    public static string KeyFor(WithinRole role) => role switch
    {
        WithinRole.Provider => "provider",
        WithinRole.Admin => "admin",
        WithinRole.CircleAdmin => "circle_admin",
        _ => "user"
    };

    public static bool TryFromKey(string? key, out WithinRole role)
    {
        switch (key?.Trim().ToLowerInvariant())
        {
            case "user": role = WithinRole.User; return true;
            case "provider": role = WithinRole.Provider; return true;
            case "admin": role = WithinRole.Admin; return true;
            case "circle_admin": role = WithinRole.CircleAdmin; return true;
            default: role = WithinRole.User; return false;
        }
    }

    public static IReadOnlyList<Role> SeedRows() =>
    [
        new Role { Id = UserRoleId, Key = "user", Name = "Member", Rank = 0, Description = "Standard member." },
        new Role { Id = ProviderRoleId, Key = "provider", Name = "Provider", Rank = 10, Description = "Verified provider with a public profile and events." },
        new Role { Id = CircleAdminRoleId, Key = "circle_admin", Name = "Circle Admin", Rank = 20, Description = "Creates and runs their own circles from the circle portal." },
        new Role { Id = AdminRoleId, Key = "admin", Name = "Admin", Rank = 100, Description = "Full platform administrator." }
    ];
}
