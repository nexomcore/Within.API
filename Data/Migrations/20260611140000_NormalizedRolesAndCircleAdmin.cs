using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizedRolesAndCircleAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .Annotation("Npgsql:Enum:circle_invite_status", "pending,accepted,declined,cancelled")
                .Annotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .Annotation("Npgsql:Enum:circle_post_type", "standard,system,announcement,event_share,weekly_check_in,poll")
                .Annotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .Annotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .Annotation("Npgsql:Enum:circle_reaction_type", "support,grateful,inspired,motivated,growing")
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
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request,circle_invite,circle_join_approved")
                .Annotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .Annotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .Annotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:provider_price_type", "free,fixed,from_price,contact_provider")
                .Annotation("Npgsql:Enum:provider_service_delivery_mode", "in_person,online,hybrid")
                .Annotation("Npgsql:Enum:provider_type", "individual,business")
                .Annotation("Npgsql:Enum:provider_verification_status", "unverified,pending,verified,rejected")
                .Annotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .Annotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .Annotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .Annotation("Npgsql:Enum:weekly_check_in_mood", "great,good,okay,struggling")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin,circle_admin")
                .OldAnnotation("Npgsql:Enum:circle_event_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .OldAnnotation("Npgsql:Enum:circle_invite_status", "pending,accepted,declined,cancelled")
                .OldAnnotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .OldAnnotation("Npgsql:Enum:circle_post_type", "standard,system,announcement,event_share,weekly_check_in,poll")
                .OldAnnotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .OldAnnotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .OldAnnotation("Npgsql:Enum:circle_reaction_type", "support,grateful,inspired,motivated,growing")
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
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request,circle_invite,circle_join_approved")
                .OldAnnotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .OldAnnotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .OldAnnotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:provider_price_type", "free,fixed,from_price,contact_provider")
                .OldAnnotation("Npgsql:Enum:provider_service_delivery_mode", "in_person,online,hybrid")
                .OldAnnotation("Npgsql:Enum:provider_type", "individual,business")
                .OldAnnotation("Npgsql:Enum:provider_verification_status", "unverified,pending,verified,rejected")
                .OldAnnotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .OldAnnotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .OldAnnotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .OldAnnotation("Npgsql:Enum:weekly_check_in_mood", "great,good,okay,struggling")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Key",
                schema: "within",
                table: "Roles",
                column: "Key",
                unique: true);

            migrationBuilder.InsertData(
                schema: "within",
                table: "Roles",
                columns: new[] { "Id", "Key", "Name", "Rank", "Description" },
                values: new object[,]
                {
                    { new Guid("a0a0a0a0-0000-0000-0000-000000000001"), "user", "Member", 0, "Standard member." },
                    { new Guid("a0a0a0a0-0000-0000-0000-000000000002"), "provider", "Provider", 10, "Verified provider with a public profile and events." },
                    { new Guid("a0a0a0a0-0000-0000-0000-000000000004"), "circle_admin", "Circle Admin", 20, "Creates and runs their own circles from the circle portal." },
                    { new Guid("a0a0a0a0-0000-0000-0000-000000000003"), "admin", "Admin", 100, "Full platform administrator." }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                schema: "within",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("UPDATE within.\"Users\" SET \"RoleId\" = 'a0a0a0a0-0000-0000-0000-000000000002' WHERE \"Role\" = 1;");
            migrationBuilder.Sql("UPDATE within.\"Users\" SET \"RoleId\" = 'a0a0a0a0-0000-0000-0000-000000000003' WHERE \"Role\" = 2;");
            migrationBuilder.Sql("UPDATE within.\"Users\" SET \"RoleId\" = 'a0a0a0a0-0000-0000-0000-000000000001' WHERE \"RoleId\" IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "RoleId",
                schema: "within",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a0a0a0a0-0000-0000-0000-000000000001"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "within",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                schema: "within",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                schema: "within",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "within",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE within.\"Users\" SET \"Role\" = 1 WHERE \"RoleId\" = 'a0a0a0a0-0000-0000-0000-000000000002';");
            migrationBuilder.Sql("UPDATE within.\"Users\" SET \"Role\" = 2 WHERE \"RoleId\" = 'a0a0a0a0-0000-0000-0000-000000000003';");

            migrationBuilder.DropColumn(
                name: "RoleId",
                schema: "within",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "within");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:circle_event_status", "active,removed")
                .Annotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .Annotation("Npgsql:Enum:circle_invite_status", "pending,accepted,declined,cancelled")
                .Annotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .Annotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .Annotation("Npgsql:Enum:circle_post_type", "standard,system,announcement,event_share,weekly_check_in,poll")
                .Annotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .Annotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .Annotation("Npgsql:Enum:circle_reaction_type", "support,grateful,inspired,motivated,growing")
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
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request,circle_invite,circle_join_approved")
                .Annotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .Annotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .Annotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:provider_price_type", "free,fixed,from_price,contact_provider")
                .Annotation("Npgsql:Enum:provider_service_delivery_mode", "in_person,online,hybrid")
                .Annotation("Npgsql:Enum:provider_type", "individual,business")
                .Annotation("Npgsql:Enum:provider_verification_status", "unverified,pending,verified,rejected")
                .Annotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .Annotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .Annotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .Annotation("Npgsql:Enum:weekly_check_in_mood", "great,good,okay,struggling")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:circle_event_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:circle_identity_mode", "real_profile,pseudonym,hidden_profile")
                .OldAnnotation("Npgsql:Enum:circle_invite_status", "pending,accepted,declined,cancelled")
                .OldAnnotation("Npgsql:Enum:circle_join_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:circle_member_role", "admin,moderator,member")
                .OldAnnotation("Npgsql:Enum:circle_member_status", "active,left,removed,pending,rejected,blocked")
                .OldAnnotation("Npgsql:Enum:circle_post_type", "standard,system,announcement,event_share,weekly_check_in,poll")
                .OldAnnotation("Npgsql:Enum:circle_post_visibility", "public,members_only,private")
                .OldAnnotation("Npgsql:Enum:circle_privacy_type", "open,approval_required,private_invite_only,sensitive")
                .OldAnnotation("Npgsql:Enum:circle_reaction_type", "support,grateful,inspired,motivated,growing")
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
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event,friend_request_received,friend_request_accepted,event_invite,public_friend_rsvp,circle_thread_reply,comment_reply,user_mention,event_reminder,circle_join_request,circle_invite,circle_join_approved")
                .OldAnnotation("Npgsql:Enum:notification_mute_target_type", "circle,event,user")
                .OldAnnotation("Npgsql:Enum:notification_target_type", "event,circle,circle_thread,community_post,profile,connection,comment")
                .OldAnnotation("Npgsql:Enum:profile_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:provider_price_type", "free,fixed,from_price,contact_provider")
                .OldAnnotation("Npgsql:Enum:provider_service_delivery_mode", "in_person,online,hybrid")
                .OldAnnotation("Npgsql:Enum:provider_type", "individual,business")
                .OldAnnotation("Npgsql:Enum:provider_verification_status", "unverified,pending,verified,rejected")
                .OldAnnotation("Npgsql:Enum:rsvp_visibility", "public,friends_only,circle_members_only,private")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:tagging_permission", "everyone,friends_only,circle_members_only,no_one")
                .OldAnnotation("Npgsql:Enum:user_report_reason", "harassment,spam,hate_or_abuse,impersonation,privacy_concern,other")
                .OldAnnotation("Npgsql:Enum:user_report_status", "open,under_review,resolved,dismissed")
                .OldAnnotation("Npgsql:Enum:weekly_check_in_mood", "great,good,okay,struggling")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin,circle_admin");
        }
    }
}

