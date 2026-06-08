using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class CircleApprovalAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .Annotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .Annotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .Annotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .Annotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .Annotation("Npgsql:Enum:circle_status", "active,archived")
                .Annotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .Annotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
                .Annotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .Annotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .Annotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .Annotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .Annotation("Npgsql:Enum:connection_status", "pending,accepted,rejected,cancelled,removed,blocked")
                .Annotation("Npgsql:Enum:event_invite_status", "pending,accepted,declined,cancelled")
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:friend_request_permission", "everyone,friends_of_friends,same_circle_or_event,no_one")
                .Annotation("Npgsql:Enum:member_list_visibility", "public,members_only,admins_only,hidden")
                .Annotation("Npgsql:Enum:mention_source_type", "event_comment,circle_post,circle_comment")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request")
                .Annotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .Annotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .Annotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .Annotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .Annotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:circle_event_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed")
                .OldAnnotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .OldAnnotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .OldAnnotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .OldAnnotation("Npgsql:Enum:circle_status", "active,archived")
                .OldAnnotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .OldAnnotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
                .OldAnnotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .OldAnnotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .OldAnnotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .OldAnnotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .OldAnnotation("Npgsql:Enum:connection_status", "pending,accepted,rejected,cancelled,removed,blocked")
                .OldAnnotation("Npgsql:Enum:event_invite_status", "pending,accepted,declined,cancelled")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:friend_request_permission", "everyone,friends_of_friends,same_circle_or_event,no_one")
                .OldAnnotation("Npgsql:Enum:member_list_visibility", "public,members_only,admins_only,hidden")
                .OldAnnotation("Npgsql:Enum:mention_source_type", "event_comment,circle_post,circle_comment")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder")
                .OldAnnotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .OldAnnotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .OldAnnotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .OldAnnotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .OldAnnotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "within",
                table: "Circles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "within",
                table: "CircleMembers",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.Sql("""
                INSERT INTO within."CircleMembers" ("Id", "CircleId", "UserId", "Role", "Status", "IdentityMode", "DisplayNameOverride", "JoinedAt", "UpdatedAt", "LeftAt")
                SELECT role."Id", role."CircleId", role."UserId",
                       CASE WHEN role."Role" = 1 THEN 0 ELSE 1 END,
                       0, 0, NULL, role."AssignedAt", role."AssignedAt", NULL
                FROM within."CircleRoles" role
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM within."CircleMembers" member
                    WHERE member."CircleId" = role."CircleId" AND member."UserId" = role."UserId"
                );

                UPDATE within."CircleMembers" member
                SET "Role" = CASE WHEN role."Role" = 1 THEN 0 ELSE 1 END,
                    "Status" = 0,
                    "UpdatedAt" = role."AssignedAt"
                FROM within."CircleRoles" role
                WHERE member."CircleId" = role."CircleId"
                  AND member."UserId" = role."UserId";

                UPDATE within."Circles" circle
                SET "CreatedByUserId" = admin_role."UserId"
                FROM (
                    SELECT DISTINCT ON ("CircleId") "CircleId", "UserId"
                    FROM within."CircleRoles"
                    WHERE "Role" = 1
                    ORDER BY "CircleId", "AssignedAt"
                ) admin_role
                WHERE circle."Id" = admin_role."CircleId"
                  AND circle."CreatedByUserId" = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.CreateTable(
                name: "CircleAnnouncements",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleAnnouncements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CircleJoinRequests",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleJoinRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Circles_CreatedByUserId",
                schema: "within",
                table: "Circles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleMembers_CircleId_Role_Status",
                schema: "within",
                table: "CircleMembers",
                columns: new[] { "CircleId", "Role", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleAnnouncements_CircleId_IsPinned_CreatedAt",
                schema: "within",
                table: "CircleAnnouncements",
                columns: new[] { "CircleId", "IsPinned", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleJoinRequests_CircleId_Status_RequestedAt",
                schema: "within",
                table: "CircleJoinRequests",
                columns: new[] { "CircleId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CircleJoinRequests_CircleId_UserId",
                schema: "within",
                table: "CircleJoinRequests",
                columns: new[] { "CircleId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CircleAnnouncements",
                schema: "within");

            migrationBuilder.DropTable(
                name: "CircleJoinRequests",
                schema: "within");

            migrationBuilder.DropIndex(
                name: "IX_Circles_CreatedByUserId",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropIndex(
                name: "IX_CircleMembers_CircleId_Role_Status",
                schema: "within",
                table: "CircleMembers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "within",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "within",
                table: "CircleMembers");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed")
                .Annotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .Annotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .Annotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .Annotation("Npgsql:Enum:circle_status", "active,archived")
                .Annotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .Annotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
                .Annotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .Annotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .Annotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .Annotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .Annotation("Npgsql:Enum:connection_status", "pending,accepted,rejected,cancelled,removed,blocked")
                .Annotation("Npgsql:Enum:event_invite_status", "pending,accepted,declined,cancelled")
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:friend_request_permission", "everyone,friends_of_friends,same_circle_or_event,no_one")
                .Annotation("Npgsql:Enum:member_list_visibility", "public,members_only,admins_only,hidden")
                .Annotation("Npgsql:Enum:mention_source_type", "event_comment,circle_post,circle_comment")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder")
                .Annotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .Annotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .Annotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .Annotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .Annotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:circle_event_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .OldAnnotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .OldAnnotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .OldAnnotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .OldAnnotation("Npgsql:Enum:circle_role_kind", "moderator,admin")
                .OldAnnotation("Npgsql:Enum:circle_status", "active,archived")
                .OldAnnotation("Npgsql:Enum:circle_type", "platform,provider,event_cohort,private_support")
                .OldAnnotation("Npgsql:Enum:circle_visibility", "public,private,hidden")
                .OldAnnotation("Npgsql:Enum:community_content_status", "active,hidden,removed,under_review")
                .OldAnnotation("Npgsql:Enum:community_post_type", "ask_community,share_experience,find_buddy,local_recommendation,reflection")
                .OldAnnotation("Npgsql:Enum:community_report_reason", "spam_or_promotion,harassment_or_abuse,medical_misinformation,inappropriate_content,safety_concern,other")
                .OldAnnotation("Npgsql:Enum:community_report_status", "pending,reviewed,action_taken,dismissed")
                .OldAnnotation("Npgsql:Enum:connection_status", "pending,accepted,rejected,cancelled,removed,blocked")
                .OldAnnotation("Npgsql:Enum:event_invite_status", "pending,accepted,declined,cancelled")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:friend_request_permission", "everyone,friends_of_friends,same_circle_or_event,no_one")
                .OldAnnotation("Npgsql:Enum:member_list_visibility", "public,members_only,admins_only,hidden")
                .OldAnnotation("Npgsql:Enum:mention_source_type", "event_comment,circle_post,circle_comment")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request")
                .OldAnnotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .OldAnnotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .OldAnnotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .OldAnnotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .OldAnnotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");
        }
    }
}
