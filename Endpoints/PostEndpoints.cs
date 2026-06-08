using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var posts = app.MapGroup("/api/posts").RequireAuthorization();

        posts.MapGet("/{id:guid}/comments", async (Guid id, WithinDbContext db) =>
        {
            if (!await db.Posts.AnyAsync(item => item.Id == id)) return Results.NotFound();
            return Results.Ok(await ApiMapping.ProjectComments(db.Comments.Where(item => item.PostId == id && !item.IsHidden), db).ToArrayAsync());
        });

        posts.MapPost("/{id:guid}/comments", async (Guid id, UpsertCommentDto request, WithinDbContext db, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            if (!await db.Posts.AnyAsync(item => item.Id == id)) return Results.NotFound();
            if (request.ParentCommentId is not null && !await db.Comments.AnyAsync(item => item.Id == request.ParentCommentId && item.PostId == id && !item.IsHidden))
            {
                return Results.BadRequest(new { message = "Parent comment was not found for this post." });
            }
            var comment = new Comment { Id = Guid.NewGuid(), PostId = id, ParentCommentId = request.ParentCommentId, AuthorUserId = principal.UserId(), Body = request.Body.Trim(), CreatedUtc = DateTimeOffset.UtcNow };
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
            if (request.ParentCommentId is not null)
            {
                var parent = await db.Comments.FindAsync(request.ParentCommentId.Value);
                if (parent is not null && parent.AuthorUserId != comment.AuthorUserId)
                {
                    await notifications.CreateAsync(new NotificationCreateRequest(
                        parent.AuthorUserId,
                        NotificationKind.CommentReply,
                        "New comment reply",
                        $"{await db.Users.Where(item => item.Id == comment.AuthorUserId).Select(item => item.DisplayName).FirstOrDefaultAsync() ?? "Within user"} replied to your comment.",
                        NotificationTargetType.CommunityPost,
                        id,
                        comment.AuthorUserId,
                        RelatedUserId: comment.AuthorUserId));
                }
            }
            var dto = await ApiMapping.ProjectComments(db.Comments.Where(item => item.Id == comment.Id), db).FirstAsync();
            return Results.Created($"/api/posts/{id}/comments/{comment.Id}", dto);
        });

        posts.MapPost("/{id:guid}/reactions/{kind}", async (Guid id, string kind, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await db.Posts.AnyAsync(item => item.Id == id)) return Results.NotFound();
            var userId = principal.UserId();
            if (!await db.Reactions.AnyAsync(item => item.PostId == id && item.UserId == userId && item.Kind == kind))
            {
                db.Reactions.Add(new Reaction { Id = Guid.NewGuid(), PostId = id, UserId = userId, Kind = kind, CreatedUtc = DateTimeOffset.UtcNow });
                await db.SaveChangesAsync();
            }
            return Results.NoContent();
        });

        posts.MapPost("/{id:guid}/report", async (Guid id, WithinDbContext db) =>
        {
            var post = await db.Posts.FindAsync(id);
            if (post is null) return Results.NotFound();
            post.IsHidden = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}
