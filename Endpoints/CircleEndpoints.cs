using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class CircleEndpoints
{
    public static IEndpointRouteBuilder MapCircleEndpoints(this IEndpointRouteBuilder app)
    {
        var circles = app.MapGroup("/api/circles");

        circles.MapGet("", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var items = await db.Circles
                .Where(circle => circle.Visibility != CircleVisibility.Hidden && circle.Status == CircleStatus.Active)
                .OrderBy(circle => circle.Name)
                .ToArrayAsync();
            return Results.Ok(await ToCircleDtos(db, items, userId));
        });

        circles.MapGet("/my", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var items = await (
                    from circle in db.Circles
                    join member in db.CircleMembers on circle.Id equals member.CircleId
                    where member.UserId == userId && member.Status == CircleMemberStatus.Active
                    orderby circle.Name
                    select circle)
                .ToArrayAsync();
            return Results.Ok(await ToCircleDtos(db, items, userId));
        }).RequireAuthorization();

        circles.MapGet("/{circleId:guid}", async (Guid circleId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var circle = await db.Circles.FindAsync(circleId);
            if (circle is null || circle.Status != CircleStatus.Active) return Results.NotFound();
            if (circle.Visibility == CircleVisibility.Hidden && (userId is null || !await IsCircleParticipant(db, circleId, userId.Value)))
            {
                return Results.NotFound();
            }

            var guidelines = await db.CircleGuidelines
                .Where(item => item.CircleId == circleId && item.IsActive)
                .OrderBy(item => item.SortOrder)
                .Select(item => new CircleGuidelineDto(item.Id, item.Title, item.Body, item.SortOrder))
                .ToArrayAsync();

            var latestThreads = await db.CircleThreads
                .Where(item => item.CircleId == circleId && item.Status != CommunityContentStatus.Hidden)
                .OrderByDescending(item => item.CreatedAt)
                .Take(5)
                .ToArrayAsync();

            var announcements = await db.CircleAnnouncements
                .Where(item => item.CircleId == circleId)
                .OrderByDescending(item => item.IsPinned)
                .ThenByDescending(item => item.CreatedAt)
                .Take(5)
                .ToArrayAsync();

            var events = await SharedEventsQuery(db, circleId, userId).Take(5).ToArrayAsync();
            return Results.Ok(new CircleDetailDto(
                await ToCircleDto(db, circle, userId),
                guidelines,
                await ToAnnouncementDtos(db, announcements),
                await ToThreadDtos(db, latestThreads, userId),
                events));
        });

        circles.MapPost("", async (CircleCreateDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var validation = ValidateCircle(request.Name, request.Description);
            if (validation is not null) return Results.BadRequest(new { message = validation });
            if (request.Visibility == CircleVisibility.Hidden) return Results.BadRequest(new { message = "Hidden invite-only circles are not enabled yet." });

            var now = DateTimeOffset.UtcNow;
            var circle = new Circle
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Slug = await UniqueSlug(db, request.Name),
                Description = request.Description.Trim(),
                CreatedByUserId = userId,
                Type = CircleType.PrivateSupport,
                Visibility = request.Visibility,
                PrivacyType = request.Visibility == CircleVisibility.Private ? CirclePrivacyType.ApprovalRequired : CirclePrivacyType.Open,
                Status = CircleStatus.Active,
                Lens = request.Lens,
                CreatedAt = now
            };
            db.Circles.Add(circle);
            db.CircleMembers.Add(new CircleMember
            {
                Id = Guid.NewGuid(),
                CircleId = circle.Id,
                UserId = userId,
                Role = CircleMemberRole.Admin,
                Status = CircleMemberStatus.Active,
                JoinedAt = now,
                UpdatedAt = now
            });
            db.CircleRoles.Add(new CircleRole
            {
                Id = Guid.NewGuid(),
                CircleId = circle.Id,
                UserId = userId,
                Role = CircleRoleKind.Admin,
                AssignedByUserId = userId,
                AssignedAt = now
            });
            await db.SaveChangesAsync();
            return Results.Created($"/api/circles/{circle.Id}", await ToCircleDto(db, circle, userId));
        }).RequireAuthorization();

        circles.MapPut("/{circleId:guid}", async (Guid circleId, CircleUpdateDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await CanAdminCircle(db, principal, circleId)) return Results.Forbid();
            var circle = await db.Circles.FindAsync(circleId);
            if (circle is null) return Results.NotFound();
            var validation = ValidateCircle(request.Name, request.Description);
            if (validation is not null) return Results.BadRequest(new { message = validation });
            if (request.Visibility == CircleVisibility.Hidden) return Results.BadRequest(new { message = "Hidden invite-only circles are not enabled yet." });

            circle.Name = request.Name.Trim();
            circle.Description = request.Description.Trim();
            circle.Lens = request.Lens;
            circle.Visibility = request.Visibility;
            circle.PrivacyType = request.Visibility == CircleVisibility.Private ? CirclePrivacyType.ApprovalRequired : CirclePrivacyType.Open;
            await db.SaveChangesAsync();
            return Results.Ok(await ToCircleDto(db, circle, principal.UserId()));
        }).RequireAuthorization();

        circles.MapPost("/{circleId:guid}/join", async (Guid circleId, WithinDbContext db, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var circle = await db.Circles.FirstOrDefaultAsync(item => item.Id == circleId && item.Status == CircleStatus.Active);
            if (circle is null || circle.Visibility == CircleVisibility.Hidden)
            {
                return Results.NotFound();
            }

            var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == userId);
            if (member is null)
            {
                member = new CircleMember
                {
                    Id = Guid.NewGuid(),
                    CircleId = circleId,
                    UserId = userId,
                    Role = CircleMemberRole.Member,
                    Status = circle.Visibility == CircleVisibility.Private ? CircleMemberStatus.Pending : CircleMemberStatus.Active,
                    JoinedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                db.CircleMembers.Add(member);
            }
            else
            {
                member.Status = circle.Visibility == CircleVisibility.Private ? CircleMemberStatus.Pending : CircleMemberStatus.Active;
                member.LeftAt = null;
                if (member.JoinedAt == default) member.JoinedAt = DateTimeOffset.UtcNow;
                member.UpdatedAt = DateTimeOffset.UtcNow;
            }

            if (circle.Visibility == CircleVisibility.Private)
            {
                var request = await db.CircleJoinRequests.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == userId);
                if (request is null)
                {
                    request = new CircleJoinRequest
                    {
                        Id = Guid.NewGuid(),
                        CircleId = circleId,
                        UserId = userId,
                        RequestedAt = DateTimeOffset.UtcNow
                    };
                    db.CircleJoinRequests.Add(request);
                }
                request.Status = CircleJoinRequestStatus.Pending;
                request.ReviewedAt = null;
                request.ReviewedByUserId = null;
            }
            await db.SaveChangesAsync();
            if (circle.Visibility == CircleVisibility.Private)
            {
                await notifications.NotifyCircleJoinRequest(circleId, userId);
            }
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapDelete("/{circleId:guid}/leave", async (Guid circleId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == userId);
            if (member is null) return Results.NoContent();
            if (member.Status == CircleMemberStatus.Active && member.Role == CircleMemberRole.Admin && await ActiveAdminCount(db, circleId) <= 1)
            {
                return Results.BadRequest(new { message = "Transfer admin role before leaving. A circle must have at least one admin." });
            }

            member.Status = CircleMemberStatus.Left;
            member.LeftAt = DateTimeOffset.UtcNow;
            member.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapGet("/{circleId:guid}/me/identity", async (Guid circleId, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == userId && item.Status == CircleMemberStatus.Active);
            if (member is null) return Results.NotFound();
            var identity = await privacy.GetDisplayIdentityForCircle(userId, circleId, userId);
            return Results.Ok(new CircleIdentityDto(circleId, member.IdentityMode, member.DisplayNameOverride, identity.DisplayName, identity.ProfileLinkAllowed));
        }).RequireAuthorization();

        circles.MapPut("/{circleId:guid}/me/identity", async (Guid circleId, UpdateCircleIdentityDto request, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var circle = await db.Circles.FindAsync(circleId);
            if (circle is null) return Results.NotFound();
            if (request.IdentityMode == CircleIdentityMode.Pseudonym && !circle.AllowPseudonyms) return Results.BadRequest(new { message = "This Circle does not allow nicknames." });
            if (request.IdentityMode == CircleIdentityMode.HiddenProfile && !circle.AllowHiddenProfiles) return Results.BadRequest(new { message = "This Circle does not allow hidden profiles." });
            if (request.IdentityMode == CircleIdentityMode.Pseudonym)
            {
                var name = request.DisplayNameOverride?.Trim();
                if (string.IsNullOrWhiteSpace(name) || name.Length > 40) return Results.BadRequest(new { message = "Nickname is required and must be 40 characters or less." });
                if (name.Contains("admin", StringComparison.OrdinalIgnoreCase) || name.Contains("within", StringComparison.OrdinalIgnoreCase)) return Results.BadRequest(new { message = "Choose a nickname that does not impersonate Within or admins." });
            }

            var now = DateTimeOffset.UtcNow;
            var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == userId);
            if (member is null)
            {
                member = new CircleMember { Id = Guid.NewGuid(), CircleId = circleId, UserId = userId, JoinedAt = now };
                db.CircleMembers.Add(member);
            }

            member.Status = CircleMemberStatus.Active;
            member.IdentityMode = request.IdentityMode;
            member.DisplayNameOverride = request.IdentityMode == CircleIdentityMode.Pseudonym ? request.DisplayNameOverride?.Trim() : null;
            member.UpdatedAt = now;
            member.LeftAt = null;
            await db.SaveChangesAsync();

            var identity = await privacy.GetDisplayIdentityForCircle(userId, circleId, userId);
            return Results.Ok(new CircleIdentityDto(circleId, member.IdentityMode, member.DisplayNameOverride, identity.DisplayName, identity.ProfileLinkAllowed));
        }).RequireAuthorization();

        circles.MapGet("/{circleId:guid}/members", async (Guid circleId, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var viewerUserId = principal.UserId();
            if (!await db.Circles.AnyAsync(item => item.Id == circleId)) return Results.NotFound();
            if (!await privacy.CanViewCircleMember(viewerUserId, circleId, viewerUserId)) return Results.Forbid();
            var members = await db.CircleMembers
                .Where(item => item.CircleId == circleId && item.Status == CircleMemberStatus.Active)
                .OrderBy(item => item.JoinedAt)
                .ToArrayAsync();
            var response = new List<CircleMemberDto>(members.Length);
            foreach (var member in members)
            {
                if (!await privacy.CanViewCircleMember(viewerUserId, circleId, member.UserId)) continue;
                var identity = await privacy.GetDisplayIdentityForCircle(viewerUserId, circleId, member.UserId);
                // Real userId is exposed only for real profiles (or the viewer themselves);
                // pseudonym/hidden members are referenced safely by their membership id.
                var exposeUserId = identity.IdentityMode == CircleIdentityMode.RealProfile || member.UserId == viewerUserId;
                response.Add(new CircleMemberDto(
                    exposeUserId ? member.UserId : null,
                    identity.DisplayName,
                    identity.IdentityMode,
                    identity.ProfileLinkAllowed,
                    member.Role,
                    member.Status,
                    member.JoinedAt,
                    member.Id,
                    IsClickable: true));
            }
            return Results.Ok(response.ToArray());
        }).RequireAuthorization();

        circles.MapGet("/{circleId:guid}/join-requests", async (Guid circleId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await CanAdminCircle(db, principal, circleId)) return Results.Forbid();
            var requests = await db.CircleJoinRequests
                .Where(item => item.CircleId == circleId)
                .OrderByDescending(item => item.RequestedAt)
                .ToArrayAsync();
            return Results.Ok(await ToJoinRequestDtos(db, requests));
        }).RequireAuthorization();

        circles.MapPost("/{circleId:guid}/join-requests/{requestId:guid}/approve", async (Guid circleId, Guid requestId, WithinDbContext db, ClaimsPrincipal principal) =>
            await ReviewJoinRequest(db, principal, circleId, requestId, CircleJoinRequestStatus.Approved)).RequireAuthorization();

        circles.MapPost("/{circleId:guid}/join-requests/{requestId:guid}/reject", async (Guid circleId, Guid requestId, WithinDbContext db, ClaimsPrincipal principal) =>
            await ReviewJoinRequest(db, principal, circleId, requestId, CircleJoinRequestStatus.Rejected)).RequireAuthorization();

        circles.MapDelete("/{circleId:guid}/members/{memberUserId:guid}", async (Guid circleId, Guid memberUserId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await CanAdminCircle(db, principal, circleId)) return Results.Forbid();
            var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == memberUserId);
            if (member is null) return Results.NotFound();
            if (member.Status == CircleMemberStatus.Active && member.Role == CircleMemberRole.Admin && await ActiveAdminCount(db, circleId) <= 1)
            {
                return Results.BadRequest(new { message = "Assign another admin before removing the only admin." });
            }
            member.Status = CircleMemberStatus.Removed;
            member.LeftAt = DateTimeOffset.UtcNow;
            member.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapPut("/{circleId:guid}/members/{memberUserId:guid}/role", async (Guid circleId, Guid memberUserId, CircleRoleUpdateDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await CanAdminCircle(db, principal, circleId)) return Results.Forbid();
            var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == memberUserId && item.Status == CircleMemberStatus.Active);
            if (member is null) return Results.NotFound();
            if (member.Role == CircleMemberRole.Admin && request.Role != CircleMemberRole.Admin && await ActiveAdminCount(db, circleId) <= 1)
            {
                return Results.BadRequest(new { message = "Assign another admin before changing the only admin role." });
            }

            member.Role = request.Role;
            member.UpdatedAt = DateTimeOffset.UtcNow;
            await SyncCircleRole(db, circleId, memberUserId, request.Role, principal.UserId());
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapPost("/{circleId:guid}/announcements", async (Guid circleId, CircleAnnouncementCreateDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await CanModerateCircle(db, principal, circleId)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1000)
            {
                return Results.BadRequest(new { message = "Announcement body is required and must be 1000 characters or less." });
            }
            if (request.IsPinned)
            {
                await db.CircleAnnouncements
                    .Where(item => item.CircleId == circleId && item.IsPinned)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsPinned, false));
            }
            var now = DateTimeOffset.UtcNow;
            var announcement = new CircleAnnouncement
            {
                Id = Guid.NewGuid(),
                CircleId = circleId,
                AuthorUserId = principal.UserId(),
                Body = request.Body.Trim(),
                IsPinned = request.IsPinned,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.CircleAnnouncements.Add(announcement);
            await db.SaveChangesAsync();
            return Results.Created($"/api/circles/{circleId}/announcements/{announcement.Id}", (await ToAnnouncementDtos(db, [announcement]))[0]);
        }).RequireAuthorization();

        circles.MapGet("/{circleId:guid}/threads", async (Guid circleId, int? page, int? pageSize, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await db.Circles.AnyAsync(item => item.Id == circleId && item.Status == CircleStatus.Active)) return Results.NotFound();
            var currentPage = Math.Max(page ?? 1, 1);
            var size = Math.Clamp(pageSize ?? 20, 1, 50);
            var userId = principal.TryUserId();
            var threads = await db.CircleThreads
                .Where(item => item.CircleId == circleId && item.Status != CommunityContentStatus.Hidden)
                .OrderByDescending(item => item.CreatedAt)
                .Skip((currentPage - 1) * size)
                .Take(size)
                .ToArrayAsync();
            return Results.Ok(await ToThreadDtos(db, threads, userId));
        });

        circles.MapGet("/threads", async (Guid? eventId, int? pageSize, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var size = Math.Clamp(pageSize ?? 20, 1, 50);
            var query = db.CircleThreads.Where(item => item.Status != CommunityContentStatus.Hidden);
            if (eventId is not null) query = query.Where(item => item.LinkedEventId == eventId);
            var threads = await query
                .OrderByDescending(item => item.CreatedAt)
                .Take(size)
                .ToArrayAsync();
            return Results.Ok(await ToThreadDtos(db, threads, userId));
        });

        circles.MapPost("/{circleId:guid}/threads", async (Guid circleId, CircleCreateThreadDto request, WithinDbContext db, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await IsCircleParticipant(db, circleId, userId) && !principal.IsInRole(nameof(WithinRole.Admin)))
            {
                return Results.Forbid();
            }

            var validation = ValidateThread(request.Title, request.Body);
            if (validation is not null) return Results.BadRequest(new { message = validation });

            if (request.LinkedEventId is not null && !await db.Events.AnyAsync(item => item.Id == request.LinkedEventId && item.Status == EventStatus.Published))
            {
                return Results.BadRequest(new { message = "Linked event was not found." });
            }

            var now = DateTimeOffset.UtcNow;
            var thread = new CircleThread
            {
                Id = Guid.NewGuid(),
                CircleId = circleId,
                UserId = userId,
                ThreadType = request.ThreadType,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                LinkedEventId = request.LinkedEventId,
                Status = CommunityContentStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.CircleThreads.Add(thread);
            await db.SaveChangesAsync();
            await notifications.NotifyMentions(userId, thread.Body, MentionSourceType.CirclePost, thread.Id, circleId, thread.LinkedEventId);
            return Results.Created($"/api/circles/threads/{thread.Id}", await ToThreadDto(db, thread, userId));
        }).RequireAuthorization();

        circles.MapGet("/threads/{threadId:guid}", async (Guid threadId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null || thread.Status == CommunityContentStatus.Hidden) return Results.NotFound();
            var userId = principal.TryUserId();
            var comments = await db.CircleThreadComments
                .Where(item => item.ThreadId == threadId && item.Status != CommunityContentStatus.Hidden)
                .OrderBy(item => item.CreatedAt)
                .ToArrayAsync();
            return Results.Ok(new CircleThreadDetailDto(await ToThreadDto(db, thread, userId), await ToCommentDtos(db, comments, userId)));
        });

        circles.MapPut("/threads/{threadId:guid}", async (Guid threadId, CircleUpdateThreadDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null) return Results.NotFound();
            if (!await CanModerateOrOwn(db, principal, thread.CircleId, thread.UserId)) return Results.Forbid();

            var validation = ValidateThread(request.Title, request.Body);
            if (validation is not null) return Results.BadRequest(new { message = validation });

            thread.ThreadType = request.ThreadType;
            thread.Title = request.Title.Trim();
            thread.Body = request.Body.Trim();
            thread.LinkedEventId = request.LinkedEventId;
            thread.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToThreadDto(db, thread, principal.UserId()));
        }).RequireAuthorization();

        circles.MapDelete("/threads/{threadId:guid}", async (Guid threadId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null) return Results.NotFound();
            if (!await CanModerateOrOwn(db, principal, thread.CircleId, thread.UserId)) return Results.Forbid();
            thread.Status = CommunityContentStatus.Removed;
            thread.DeletedAt = DateTimeOffset.UtcNow;
            thread.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapPost("/threads/{threadId:guid}/comments", async (Guid threadId, CircleCreateCommentDto request, WithinDbContext db, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null || thread.Status == CommunityContentStatus.Hidden) return Results.NotFound();
            if (!await IsCircleParticipant(db, thread.CircleId, userId) && !principal.IsInRole(nameof(WithinRole.Admin))) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1200)
            {
                return Results.BadRequest(new { message = "Comment body is required and must be 1200 characters or less." });
            }

            var now = DateTimeOffset.UtcNow;
            var comment = new CircleThreadComment
            {
                Id = Guid.NewGuid(),
                ThreadId = threadId,
                UserId = userId,
                Body = request.Body.Trim(),
                Status = CommunityContentStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.CircleThreadComments.Add(comment);
            await db.SaveChangesAsync();
            await notifications.NotifyCircleThreadReply(threadId, comment.Id, userId);
            await notifications.NotifyMentions(userId, comment.Body, MentionSourceType.CircleComment, comment.Id, thread.CircleId, thread.LinkedEventId);
            return Results.Created($"/api/circles/comments/{comment.Id}", await ToCommentDto(db, comment, userId));
        }).RequireAuthorization();

        circles.MapPut("/comments/{commentId:guid}", async (Guid commentId, CircleCreateCommentDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var comment = await db.CircleThreadComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            var thread = await db.CircleThreads.FindAsync(comment.ThreadId);
            if (thread is null) return Results.NotFound();
            if (!await CanModerateOrOwn(db, principal, thread.CircleId, comment.UserId)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1200) return Results.BadRequest(new { message = "Comment body is required and must be 1200 characters or less." });
            comment.Body = request.Body.Trim();
            comment.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToCommentDto(db, comment, principal.UserId()));
        }).RequireAuthorization();

        circles.MapDelete("/comments/{commentId:guid}", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var comment = await db.CircleThreadComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            var thread = await db.CircleThreads.FindAsync(comment.ThreadId);
            if (thread is null) return Results.NotFound();
            if (!await CanModerateOrOwn(db, principal, thread.CircleId, comment.UserId)) return Results.Forbid();
            comment.Status = CommunityContentStatus.Removed;
            comment.DeletedAt = DateTimeOffset.UtcNow;
            comment.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapPost("/threads/{threadId:guid}/helpful", async (Guid threadId, WithinDbContext db, ClaimsPrincipal principal) =>
            await SetHelpful(db, principal.UserId(), threadId, null)).RequireAuthorization();

        circles.MapDelete("/threads/{threadId:guid}/helpful", async (Guid threadId, WithinDbContext db, ClaimsPrincipal principal) =>
            await RemoveHelpful(db, principal.UserId(), threadId, null)).RequireAuthorization();

        circles.MapPost("/comments/{commentId:guid}/helpful", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
            await SetHelpful(db, principal.UserId(), null, commentId)).RequireAuthorization();

        circles.MapDelete("/comments/{commentId:guid}/helpful", async (Guid commentId, WithinDbContext db, ClaimsPrincipal principal) =>
            await RemoveHelpful(db, principal.UserId(), null, commentId)).RequireAuthorization();

        circles.MapGet("/{circleId:guid}/events", async (Guid circleId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            if (!await db.Circles.AnyAsync(item => item.Id == circleId && item.Status == CircleStatus.Active)) return Results.NotFound();
            return Results.Ok(await SharedEventsQuery(db, circleId, userId).ToArrayAsync());
        });

        circles.MapPost("/{circleId:guid}/events", async (Guid circleId, CircleShareEventDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!await IsCircleParticipant(db, circleId, userId) && !principal.IsInRole(nameof(WithinRole.Admin))) return Results.Forbid();
            var evt = await db.Events.FindAsync(request.EventId);
            if (evt is null || evt.Status != EventStatus.Published) return Results.BadRequest(new { message = "Event was not found." });

            var existing = await db.CircleEvents.FirstOrDefaultAsync(item => item.CircleId == circleId && item.EventId == request.EventId);
            if (existing is null)
            {
                db.CircleEvents.Add(new CircleEvent
                {
                    Id = Guid.NewGuid(),
                    CircleId = circleId,
                    EventId = request.EventId,
                    SharedByUserId = userId,
                    OptionalNote = request.OptionalNote?.Trim(),
                    Status = CircleEventStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                existing.Status = CircleEventStatus.Active;
                existing.OptionalNote = request.OptionalNote?.Trim();
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapDelete("/{circleId:guid}/events/{eventId:guid}", async (Guid circleId, Guid eventId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (!await CanModerateCircle(db, principal, circleId)) return Results.Forbid();
            var share = await db.CircleEvents.FirstOrDefaultAsync(item => item.CircleId == circleId && item.EventId == eventId);
            if (share is null) return Results.NoContent();
            share.Status = CircleEventStatus.Removed;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        circles.MapPost("/reports", async (CircleReportRequestDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var targets = new[] { request.ThreadId, request.CommentId, request.CircleEventId }.Count(item => item is not null);
            if (targets != 1) return Results.BadRequest(new { message = "Report must target exactly one thread, comment, or event share." });

            Guid circleId;
            if (request.ThreadId is not null)
            {
                var thread = await db.CircleThreads.FindAsync(request.ThreadId.Value);
                if (thread is null) return Results.NotFound();
                circleId = thread.CircleId;
            }
            else if (request.CommentId is not null)
            {
                var comment = await db.CircleThreadComments.FindAsync(request.CommentId.Value);
                if (comment is null) return Results.NotFound();
                var thread = await db.CircleThreads.FindAsync(comment.ThreadId);
                if (thread is null) return Results.NotFound();
                circleId = thread.CircleId;
            }
            else
            {
                var share = await db.CircleEvents.FindAsync(request.CircleEventId!.Value);
                if (share is null) return Results.NotFound();
                circleId = share.CircleId;
            }

            var report = new CircleReport
            {
                Id = Guid.NewGuid(),
                ReporterUserId = principal.UserId(),
                CircleId = circleId,
                ThreadId = request.ThreadId,
                CommentId = request.CommentId,
                CircleEventId = request.CircleEventId,
                Reason = request.Reason,
                Description = request.Description?.Trim(),
                Status = CommunityReportStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.CircleReports.Add(report);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/circles/reports/{report.Id}", await ToReportDto(db, report, principal.UserId()));
        }).RequireAuthorization();

        var admin = app.MapGroup("/api/admin/circles").RequireAuthorization("AdminOnly");

        admin.MapGet("/reports", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var reports = await db.CircleReports.OrderByDescending(item => item.CreatedAt).Take(100).ToArrayAsync();
            var userId = principal.UserId();
            var response = new List<CircleReportDto>(reports.Length);
            foreach (var report in reports)
            {
                response.Add(await ToReportDto(db, report, userId));
            }
            return Results.Ok(response.ToArray());
        });

        admin.MapPost("/reports/{reportId:guid}/review", async (Guid reportId, CircleReviewReportDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            if (request.Status == CommunityReportStatus.Pending) return Results.BadRequest(new { message = "Choose a completed review status." });
            var report = await db.CircleReports.FindAsync(reportId);
            if (report is null) return Results.NotFound();
            report.Status = request.Status;
            report.ReviewedByUserId = principal.UserId();
            report.ReviewedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToReportDto(db, report, principal.UserId()));
        });

        admin.MapPost("/threads/{threadId:guid}/remove", async (Guid threadId, WithinDbContext db) =>
        {
            var thread = await db.CircleThreads.FindAsync(threadId);
            if (thread is null) return Results.NotFound();
            thread.Status = CommunityContentStatus.Removed;
            thread.DeletedAt = DateTimeOffset.UtcNow;
            thread.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        admin.MapPost("/comments/{commentId:guid}/remove", async (Guid commentId, WithinDbContext db) =>
        {
            var comment = await db.CircleThreadComments.FindAsync(commentId);
            if (comment is null) return Results.NotFound();
            comment.Status = CommunityContentStatus.Removed;
            comment.DeletedAt = DateTimeOffset.UtcNow;
            comment.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        admin.MapPost("/events/{circleEventId:guid}/remove", async (Guid circleEventId, WithinDbContext db) =>
        {
            var share = await db.CircleEvents.FindAsync(circleEventId);
            if (share is null) return Results.NotFound();
            share.Status = CircleEventStatus.Removed;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static string? ValidateThread(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 140) return "Title is required and must be 140 characters or less.";
        if (string.IsNullOrWhiteSpace(body) || body.Trim().Length > 4000) return "Body is required and must be 4000 characters or less.";
        return null;
    }

    private static string? ValidateCircle(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120) return "Circle name is required and must be 120 characters or less.";
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 600) return "Description is required and must be 600 characters or less.";
        return null;
    }

    private static async Task<string> UniqueSlug(WithinDbContext db, string name)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        var suffix = 2;
        while (await db.Circles.AnyAsync(item => item.Slug == slug))
        {
            slug = $"{baseSlug}-{suffix++}";
        }
        return slug;
    }

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(slug) ? $"circle-{Guid.NewGuid():N}"[..20] : slug;
    }

    private static async Task<bool> IsCircleParticipant(WithinDbContext db, Guid circleId, Guid userId) =>
        await db.CircleMembers.AnyAsync(item => item.CircleId == circleId && item.UserId == userId && item.Status == CircleMemberStatus.Active);

    private static async Task<bool> CanModerateCircle(WithinDbContext db, ClaimsPrincipal principal, Guid circleId)
    {
        if (principal.IsInRole(nameof(WithinRole.Admin))) return true;
        var userId = principal.UserId();
        return await db.CircleMembers.AnyAsync(item =>
                item.CircleId == circleId &&
                item.UserId == userId &&
                item.Status == CircleMemberStatus.Active &&
                (item.Role == CircleMemberRole.Admin || item.Role == CircleMemberRole.Moderator))
            || await db.CircleRoles.AnyAsync(item => item.CircleId == circleId && item.UserId == userId);
    }

    private static async Task<bool> CanAdminCircle(WithinDbContext db, ClaimsPrincipal principal, Guid circleId)
    {
        if (principal.IsInRole(nameof(WithinRole.Admin))) return true;
        var userId = principal.UserId();
        return await db.CircleMembers.AnyAsync(item =>
                item.CircleId == circleId &&
                item.UserId == userId &&
                item.Status == CircleMemberStatus.Active &&
                item.Role == CircleMemberRole.Admin)
            || await db.CircleRoles.AnyAsync(item => item.CircleId == circleId && item.UserId == userId && item.Role == CircleRoleKind.Admin);
    }

    private static async Task<bool> CanModerateOrOwn(WithinDbContext db, ClaimsPrincipal principal, Guid circleId, Guid ownerUserId) =>
        principal.UserId() == ownerUserId || await CanModerateCircle(db, principal, circleId);

    private static async Task<int> ActiveAdminCount(WithinDbContext db, Guid circleId) =>
        await db.CircleMembers.CountAsync(item => item.CircleId == circleId && item.Status == CircleMemberStatus.Active && item.Role == CircleMemberRole.Admin);

    private static async Task SyncCircleRole(WithinDbContext db, Guid circleId, Guid userId, CircleMemberRole role, Guid assignedByUserId)
    {
        await db.CircleRoles.Where(item => item.CircleId == circleId && item.UserId == userId).ExecuteDeleteAsync();
        if (role == CircleMemberRole.Member) return;
        db.CircleRoles.Add(new CircleRole
        {
            Id = Guid.NewGuid(),
            CircleId = circleId,
            UserId = userId,
            Role = role == CircleMemberRole.Admin ? CircleRoleKind.Admin : CircleRoleKind.Moderator,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> ReviewJoinRequest(WithinDbContext db, ClaimsPrincipal principal, Guid circleId, Guid requestId, CircleJoinRequestStatus status)
    {
        if (!await CanAdminCircle(db, principal, circleId)) return Results.Forbid();
        var request = await db.CircleJoinRequests.FirstOrDefaultAsync(item => item.Id == requestId && item.CircleId == circleId);
        if (request is null) return Results.NotFound();
        var now = DateTimeOffset.UtcNow;
        request.Status = status;
        request.ReviewedByUserId = principal.UserId();
        request.ReviewedAt = now;

        var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == request.UserId);
        if (member is null)
        {
            member = new CircleMember
            {
                Id = Guid.NewGuid(),
                CircleId = circleId,
                UserId = request.UserId,
                Role = CircleMemberRole.Member,
                JoinedAt = now
            };
            db.CircleMembers.Add(member);
        }
        member.Status = status == CircleJoinRequestStatus.Approved ? CircleMemberStatus.Active : CircleMemberStatus.Rejected;
        member.UpdatedAt = now;
        member.LeftAt = null;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> SetHelpful(WithinDbContext db, Guid userId, Guid? threadId, Guid? commentId)
    {
        if (threadId is not null && !await db.CircleThreads.AnyAsync(item => item.Id == threadId)) return Results.NotFound();
        if (commentId is not null && !await db.CircleThreadComments.AnyAsync(item => item.Id == commentId)) return Results.NotFound();
        var exists = await db.CircleHelpfulReactions.AnyAsync(item => item.UserId == userId && item.ThreadId == threadId && item.CommentId == commentId);
        if (!exists)
        {
            db.CircleHelpfulReactions.Add(new CircleHelpfulReaction { Id = Guid.NewGuid(), UserId = userId, ThreadId = threadId, CommentId = commentId, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveHelpful(WithinDbContext db, Guid userId, Guid? threadId, Guid? commentId)
    {
        await db.CircleHelpfulReactions
            .Where(item => item.UserId == userId && item.ThreadId == threadId && item.CommentId == commentId)
            .ExecuteDeleteAsync();
        return Results.NoContent();
    }

    private static IQueryable<EventDto> SharedEventsQuery(WithinDbContext db, Guid circleId, Guid? userId) =>
        ApiMapping.ProjectEvents(
            from evt in db.Events
            join share in db.CircleEvents on evt.Id equals share.EventId
            where share.CircleId == circleId && share.Status == CircleEventStatus.Active && evt.Status == EventStatus.Published
            orderby evt.StartUtc
            select evt,
            db,
            userId);

    private static async Task<CircleDto[]> ToCircleDtos(WithinDbContext db, Circle[] circles, Guid? currentUserId)
    {
        var response = new List<CircleDto>(circles.Length);
        foreach (var circle in circles)
        {
            response.Add(await ToCircleDto(db, circle, currentUserId));
        }
        return response.ToArray();
    }

    private static async Task<CircleDto> ToCircleDto(WithinDbContext db, Circle circle, Guid? currentUserId) => new(
        circle.Id,
        circle.Name,
        circle.Slug,
        circle.Description,
        circle.CreatedByUserId,
        circle.Type,
        circle.Visibility,
        circle.Status,
        circle.Lens,
        await db.CircleMembers.CountAsync(item => item.CircleId == circle.Id && item.Status == CircleMemberStatus.Active),
        await db.CircleThreads.CountAsync(item => item.CircleId == circle.Id && item.Status == CommunityContentStatus.Active),
        await db.CircleEvents.CountAsync(item => item.CircleId == circle.Id && item.Status == CircleEventStatus.Active),
        currentUserId is not null && await db.CircleMembers.AnyAsync(item => item.CircleId == circle.Id && item.UserId == currentUserId && item.Status == CircleMemberStatus.Active),
        currentUserId is not null && await db.CircleMembers.AnyAsync(item => item.CircleId == circle.Id && item.UserId == currentUserId && item.Status == CircleMemberStatus.Pending),
        currentUserId is null ? null : await db.CircleMembers.Where(item => item.CircleId == circle.Id && item.UserId == currentUserId && item.Status == CircleMemberStatus.Active).Select(item => (CircleMemberRole?)item.Role).FirstOrDefaultAsync(),
        currentUserId is not null && await db.CircleMembers.AnyAsync(item => item.CircleId == circle.Id && item.UserId == currentUserId && item.Status == CircleMemberStatus.Active && item.Role == CircleMemberRole.Admin));

    private static async Task<CircleThreadDto[]> ToThreadDtos(WithinDbContext db, CircleThread[] threads, Guid? currentUserId)
    {
        var response = new List<CircleThreadDto>(threads.Length);
        foreach (var thread in threads)
        {
            response.Add(await ToThreadDto(db, thread, currentUserId));
        }
        return response.ToArray();
    }

    private static async Task<CircleJoinRequestDto[]> ToJoinRequestDtos(WithinDbContext db, CircleJoinRequest[] requests)
    {
        var response = new List<CircleJoinRequestDto>(requests.Length);
        foreach (var request in requests)
        {
            var circleName = await db.Circles.Where(item => item.Id == request.CircleId).Select(item => item.Name).FirstOrDefaultAsync() ?? "Circle";
            response.Add(new CircleJoinRequestDto(
                request.Id,
                request.CircleId,
                circleName,
                await ToAuthorDto(db, request.UserId),
                request.Status,
                request.RequestedAt,
                request.ReviewedByUserId is null ? null : await ToAuthorDto(db, request.ReviewedByUserId.Value),
                request.ReviewedAt));
        }
        return response.ToArray();
    }

    private static async Task<CircleAnnouncementDto[]> ToAnnouncementDtos(WithinDbContext db, CircleAnnouncement[] announcements)
    {
        var response = new List<CircleAnnouncementDto>(announcements.Length);
        foreach (var announcement in announcements)
        {
            response.Add(new CircleAnnouncementDto(
                announcement.Id,
                announcement.Body,
                announcement.IsPinned,
                await ToAuthorDto(db, announcement.AuthorUserId),
                announcement.CreatedAt,
                announcement.UpdatedAt));
        }
        return response.ToArray();
    }

    private static async Task<CircleThreadDto> ToThreadDto(WithinDbContext db, CircleThread thread, Guid? currentUserId)
    {
        var circleName = await db.Circles.Where(item => item.Id == thread.CircleId).Select(item => item.Name).FirstOrDefaultAsync() ?? "Circle";
        var (author, identityMode) = await ToCircleAuthor(db, thread.CircleId, thread.UserId, currentUserId);
        var body = thread.Status == CommunityContentStatus.Removed ? "This thread has been removed." : thread.Body;
        var title = thread.Status == CommunityContentStatus.Removed ? "Removed thread" : thread.Title;
        return new CircleThreadDto(
            thread.Id,
            thread.CircleId,
            circleName,
            thread.ThreadType,
            title,
            body,
            thread.Status,
            author,
            await ToEventSummary(db, thread.LinkedEventId),
            await db.CircleHelpfulReactions.CountAsync(item => item.ThreadId == thread.Id),
            await db.CircleThreadComments.CountAsync(item => item.ThreadId == thread.Id && item.Status == CommunityContentStatus.Active),
            currentUserId is not null && await db.CircleHelpfulReactions.AnyAsync(item => item.ThreadId == thread.Id && item.UserId == currentUserId),
            thread.CreatedAt,
            thread.UpdatedAt,
            identityMode,
            AuthorIsClickable: true);
    }

    private static async Task<CircleThreadCommentDto[]> ToCommentDtos(WithinDbContext db, CircleThreadComment[] comments, Guid? currentUserId)
    {
        var response = new List<CircleThreadCommentDto>(comments.Length);
        foreach (var comment in comments)
        {
            response.Add(await ToCommentDto(db, comment, currentUserId));
        }
        return response.ToArray();
    }

    private static async Task<CircleThreadCommentDto> ToCommentDto(WithinDbContext db, CircleThreadComment comment, Guid? currentUserId)
    {
        var body = comment.Status == CommunityContentStatus.Removed ? "This comment has been removed." : comment.Body;
        var circleId = await db.CircleThreads.Where(item => item.Id == comment.ThreadId).Select(item => item.CircleId).FirstOrDefaultAsync();
        var (author, identityMode) = await ToCircleAuthor(db, circleId, comment.UserId, currentUserId);
        return new CircleThreadCommentDto(
            comment.Id,
            comment.ThreadId,
            body,
            comment.Status,
            author,
            await db.CircleHelpfulReactions.CountAsync(item => item.CommentId == comment.Id),
            currentUserId is not null && await db.CircleHelpfulReactions.AnyAsync(item => item.CommentId == comment.Id && item.UserId == currentUserId),
            comment.CreatedAt,
            comment.UpdatedAt,
            circleId,
            identityMode,
            AuthorIsClickable: true);
    }

    private static async Task<CircleReportDto> ToReportDto(WithinDbContext db, CircleReport report, Guid? currentUserId)
    {
        var circle = await db.Circles.FindAsync(report.CircleId);
        var thread = report.ThreadId is null ? null : await db.CircleThreads.FindAsync(report.ThreadId.Value);
        CircleThreadComment? comment = null;
        if (report.CommentId is not null) comment = await db.CircleThreadComments.FindAsync(report.CommentId.Value);

        EventDto? sharedEvent = null;
        if (report.CircleEventId is not null)
        {
            var eventId = await db.CircleEvents.Where(item => item.Id == report.CircleEventId.Value).Select(item => (Guid?)item.EventId).FirstOrDefaultAsync();
            if (eventId is not null)
            {
                sharedEvent = await ApiMapping.ProjectEvents(db.Events.Where(item => item.Id == eventId.Value), db, currentUserId).FirstOrDefaultAsync();
            }
        }

        return new CircleReportDto(
            report.Id,
            report.CircleId,
            report.CircleEventId,
            circle?.Name ?? "Circle",
            report.Reason,
            report.Description,
            report.Status,
            thread is null ? null : await ToThreadDto(db, thread, currentUserId),
            comment is null ? null : await ToCommentDto(db, comment, currentUserId),
            sharedEvent,
            await ToAuthorDto(db, report.ReporterUserId),
            report.ReviewedByUserId is null ? null : await ToAuthorDto(db, report.ReviewedByUserId.Value),
            report.CreatedAt,
            report.ReviewedAt);
    }

    private static async Task<CommunityAuthorDto> ToAuthorDto(WithinDbContext db, Guid userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return new CommunityAuthorDto(userId, "Unknown user", WithinRole.User, false);
        var verified = user.Role == WithinRole.Provider && await db.Providers.AnyAsync(item => item.OwnerUserId == userId && item.IsVerified);
        return new CommunityAuthorDto(user.Id, user.DisplayName, user.Role, verified);
    }

    /// <summary>
    /// Builds a circle-identity-safe author for posts/comments. For Pseudonym/HiddenProfile
    /// authors the real user id and name never leave the server (spec §7.5, §12); the viewer
    /// always sees their own identity in full.
    /// </summary>
    private static async Task<(CommunityAuthorDto Author, CircleIdentityMode Mode)> ToCircleAuthor(
        WithinDbContext db, Guid circleId, Guid authorUserId, Guid? viewerUserId)
    {
        var user = await db.Users.FindAsync(authorUserId);
        var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == authorUserId);
        var identity = ProfileAccessRules.ResolveCircleIdentity(
            user?.DisplayName ?? "Circle member",
            member?.IdentityMode,
            member?.DisplayNameOverride,
            viewerUserId == authorUserId);

        if (identity.IdentityMode == CircleIdentityMode.RealProfile && user is not null)
        {
            var verified = user.Role == WithinRole.Provider && await db.Providers.AnyAsync(item => item.OwnerUserId == user.Id && item.IsVerified);
            return (new CommunityAuthorDto(user.Id, user.DisplayName, user.Role, verified), identity.IdentityMode);
        }

        return (new CommunityAuthorDto(Guid.Empty, identity.DisplayName, WithinRole.User, false), identity.IdentityMode);
    }

    private static async Task<CommunityEventSummaryDto?> ToEventSummary(WithinDbContext db, Guid? eventId)
    {
        if (eventId is null) return null;
        return await (
                from evt in db.Events
                join provider in db.Providers on evt.ProviderId equals provider.Id
                where evt.Id == eventId
                select new CommunityEventSummaryDto(evt.Id, evt.Title, provider.Name, evt.StartUtc, evt.LocationName))
            .FirstOrDefaultAsync();
    }
}
