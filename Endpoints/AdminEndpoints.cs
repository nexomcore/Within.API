using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin");

        admin.MapPost("/login", async (LoginDto request, WithinDbContext db, AuthTokenService tokens) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email);
            if (user is null || user.IsDeleted || !Passwords.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            if (user.RoleEnum != WithinRole.Admin && user.RoleEnum != WithinRole.CircleAdmin)
            {
                return Results.Json(new { message = "Admin or Circle Admin access required." }, statusCode: StatusCodes.Status403Forbidden);
            }

            return Results.Ok(await tokens.CreateResponse(user));
        });

        admin.MapGet("/submissions", async (IMarketFitSubmissionService service, string? audience, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetSubmissionsAsync(audience, cancellationToken))).RequireAuthorization("AdminOnly");

        admin.MapGet("/stats", async (IMarketFitSubmissionService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetStatsAsync(cancellationToken))).RequireAuthorization("AdminOnly");

        admin.MapGet("/users", async (WithinDbContext db) =>
        {
            var rows = await db.Users
                .OrderByDescending(item => item.CreatedUtc)
                .Select(item => new { item.Id, item.DisplayName, item.Email, item.RoleId, item.CreatedUtc })
                .ToArrayAsync();
            var users = rows
                .Select(item => new AdminUserDto(item.Id, item.DisplayName, item.Email, RoleCatalog.RoleOf(item.RoleId), item.CreatedUtc))
                .ToArray();
            return Results.Ok(users);
        }).RequireAuthorization("AdminOnly");

        admin.MapGet("/roles", async (WithinDbContext db) =>
            Results.Ok(await db.Roles
                .OrderBy(item => item.Rank)
                .Select(item => new RoleDto(item.Key, item.Name, item.Rank, item.Description))
                .ToArrayAsync())).RequireAuthorization("AdminOnly");

        admin.MapPut("/users/{id:guid}/role", async (Guid id, UpdateUserRoleDto request, WithinDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(item => item.Id == id);
            if (user is null || user.IsDeleted) return Results.NotFound();

            var newRoleId = RoleCatalog.IdFor(request.Role);
            // Don't strand the platform without an admin.
            if (user.RoleId == RoleCatalog.AdminRoleId && newRoleId != RoleCatalog.AdminRoleId)
            {
                var activeAdmins = await db.Users.CountAsync(item => item.RoleId == RoleCatalog.AdminRoleId && !item.IsDeleted);
                if (activeAdmins <= 1)
                {
                    return Results.Json(new { message = "Assign another admin before changing the last admin's role." }, statusCode: StatusCodes.Status409Conflict);
                }
            }

            user.RoleId = newRoleId;
            await db.SaveChangesAsync();
            return Results.Ok(new AdminUserDto(user.Id, user.DisplayName, user.Email, user.RoleEnum, user.CreatedUtc));
        }).RequireAuthorization("AdminOnly");

        admin.MapDelete("/submissions/{id:guid}", async (Guid id, IMarketFitSubmissionService service, CancellationToken cancellationToken) =>
            await service.DeleteSubmissionAsync(id, cancellationToken) ? Results.NoContent() : Results.NotFound()).RequireAuthorization("AdminOnly");

        admin.MapDelete("/users/{id:guid}", async (Guid id, WithinDbContext db, AccountDeletionService deletion) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(item => item.Id == id);
            if (user is null || user.IsDeleted) return Results.NotFound();
            if (user.RoleId == RoleCatalog.AdminRoleId)
            {
                var activeAdmins = await db.Users.CountAsync(item => item.RoleId == RoleCatalog.AdminRoleId && !item.IsDeleted);
                if (activeAdmins <= 1)
                {
                    return Results.Json(new { message = "Cannot delete the last remaining admin account." }, statusCode: StatusCodes.Status409Conflict);
                }
            }

            var result = await deletion.DeleteAccountAsync(id);
            return result.Status switch
            {
                AccountDeletionStatus.Deleted => Results.NoContent(),
                AccountDeletionStatus.Blocked => Results.Json(new { message = result.Message }, statusCode: StatusCodes.Status409Conflict),
                _ => Results.NotFound()
            };
        }).RequireAuthorization("AdminOnly");

        // Permanently purge an already-deleted (tombstone) account and every remaining reference.
        admin.MapDelete("/users/{id:guid}/purge", async (Guid id, WithinDbContext db, AccountDeletionService deletion) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(item => item.Id == id);
            if (user is null) return Results.NotFound();
            if (user.RoleId == RoleCatalog.AdminRoleId)
            {
                var activeAdmins = await db.Users.CountAsync(item => item.RoleId == RoleCatalog.AdminRoleId && !item.IsDeleted);
                if (activeAdmins <= 1)
                {
                    return Results.Json(new { message = "Cannot purge the last remaining admin account." }, statusCode: StatusCodes.Status409Conflict);
                }
            }

            var result = await deletion.HardDeleteAccountAsync(id);
            return result.Status switch
            {
                AccountDeletionStatus.Deleted => Results.NoContent(),
                AccountDeletionStatus.Blocked => Results.Json(new { message = result.Message }, statusCode: StatusCodes.Status409Conflict),
                _ => Results.NotFound()
            };
        }).RequireAuthorization("AdminOnly");

        return app;
    }
}
