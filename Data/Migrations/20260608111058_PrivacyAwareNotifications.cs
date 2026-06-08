using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrivacyAwareNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationPreferences",
                schema: "within",
                table: "NotificationPreferences");

            migrationBuilder.RenameTable(
                name: "NotificationPreferences",
                schema: "within",
                newName: "notification_preferences",
                newSchema: "within");

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
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
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

            migrationBuilder.AlterColumn<bool>(
                name: "ProviderNewEventsEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "EventRemindersEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "DailyMotivationEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "CommunitySummariesEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "CircleRepliesEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "CommentRepliesEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EventInvitesEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "FriendActivityEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "FriendRequestsEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MentionsEnabled",
                schema: "within",
                table: "notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_notification_preferences",
                schema: "within",
                table: "notification_preferences",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "notification_mutes",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_mutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: true),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: true),
                    CircleId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "push_tokens",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Platform = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_push_tokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_UserId",
                schema: "within",
                table: "notification_preferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_mutes_UserId_TargetType_TargetId",
                schema: "within",
                table: "notification_mutes",
                columns: new[] { "UserId", "TargetType", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_TargetType_TargetId",
                schema: "within",
                table: "notifications",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId_IsRead_CreatedUtc",
                schema: "within",
                table: "notifications",
                columns: new[] { "UserId", "IsRead", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_push_tokens_Token",
                schema: "within",
                table: "push_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_push_tokens_UserId",
                schema: "within",
                table: "push_tokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_mutes",
                schema: "within");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "within");

            migrationBuilder.DropTable(
                name: "push_tokens",
                schema: "within");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notification_preferences",
                schema: "within",
                table: "notification_preferences");

            migrationBuilder.DropIndex(
                name: "IX_notification_preferences_UserId",
                schema: "within",
                table: "notification_preferences");

            migrationBuilder.DropColumn(
                name: "CircleRepliesEnabled",
                schema: "within",
                table: "notification_preferences");

            migrationBuilder.DropColumn(
                name: "CommentRepliesEnabled",
                schema: "within",
                table: "notification_preferences");

            migrationBuilder.DropColumn(
                name: "EventInvitesEnabled",
                schema: "within",
                table: "notification_preferences");

            migrationBuilder.DropColumn(
                name: "FriendActivityEnabled",
                schema: "within",
                table: "notification_preferences");

            migrationBuilder.DropColumn(
                name: "FriendRequestsEnabled",
                schema: "within",
                table: "notification_preferences");

            migrationBuilder.DropColumn(
                name: "MentionsEnabled",
                schema: "within",
                table: "notification_preferences");

            migrationBuilder.RenameTable(
                name: "notification_preferences",
                schema: "within",
                newName: "NotificationPreferences",
                newSchema: "within");

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
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
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

            migrationBuilder.AlterColumn<bool>(
                name: "ProviderNewEventsEnabled",
                schema: "within",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EventRemindersEnabled",
                schema: "within",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "DailyMotivationEnabled",
                schema: "within",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "CommunitySummariesEnabled",
                schema: "within",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationPreferences",
                schema: "within",
                table: "NotificationPreferences",
                column: "Id");
        }
    }
}
