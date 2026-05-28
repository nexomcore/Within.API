using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Endpoints;

public static class CommunityEndpoints
{
    public static IEndpointRouteBuilder MapCommunityEndpoints(this IEndpointRouteBuilder app)
    {
        var communities = app.MapGroup("/api/communities");

        communities.MapGet("", async (WithinDbContext db, ClaimsPrincipal principal, WithinLens? lens) =>
        {
            var userId = principal.TryUserId();
            var query = db.Communities.AsQueryable();
            if (lens is not null) query = query.Where(item => item.Lens == lens);
            return Results.Ok(await ApiMapping.ProjectCommunities(query, db, userId).ToArrayAsync());
        });

        communities.MapPost("/{id:guid}/join", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await db.Communities.AnyAsync(item => item.Id == id)) return Results.NotFound();
            if (!await db.CommunityMembers.AnyAsync(item => item.CommunityId == id && item.UserId == userId))
            {
                db.CommunityMembers.Add(new CommunityMember { Id = Guid.NewGuid(), CommunityId = id, UserId = userId, JoinedUtc = DateTimeOffset.UtcNow });
                await db.SaveChangesAsync();
            }
            return Results.NoContent();
        }).RequireAuthorization();

        communities.MapGet("/{id:guid}/posts", async (Guid id, WithinDbContext db) =>
            Results.Ok(await ApiMapping.ProjectPosts(db.Posts.Where(item => item.CommunityId == id && !item.IsHidden), db).ToArrayAsync()));

        communities.MapPost("/{id:guid}/posts", async (Guid id, UpsertCommunityPostDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await db.Communities.AnyAsync(item => item.Id == id)) return Results.NotFound();
            var post = new Post { Id = Guid.NewGuid(), CommunityId = id, EventId = request.EventId, AuthorUserId = principal.UserId(), Body = request.Body.Trim(), CreatedUtc = DateTimeOffset.UtcNow };
            db.Posts.Add(post);
            await db.SaveChangesAsync();
            var dto = await ApiMapping.ProjectPosts(db.Posts.Where(item => item.Id == post.Id), db).FirstAsync();
            return Results.Created($"/api/communities/{id}/posts/{post.Id}", dto);
        }).RequireAuthorization();

        return app;
    }
}
