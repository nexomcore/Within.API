using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventDeclinedJoinState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:event_join_state", "interested,going,attended")
                .Annotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .Annotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .Annotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .Annotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .Annotation("Npgsql:Enum:signup_type", "internal,external")
                .Annotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .Annotation("Npgsql:Enum:within_role", "user,provider,admin")
                .OldAnnotation("Npgsql:Enum:event_join_state", "interested,going,attended,declined")
                .OldAnnotation("Npgsql:Enum:event_status", "draft,published,cancelled")
                .OldAnnotation("Npgsql:Enum:notification_kind", "daily_motivation,event_reminder24h,event_reminder2h,event_updated,community_summary,provider_new_event")
                .OldAnnotation("Npgsql:Enum:provider_application_status", "submitted,in_review,more_info_requested,approved,rejected")
                .OldAnnotation("Npgsql:Enum:provider_category", "business_studio,individual_practitioner,collective_community_group,retreat_program_organiser,venue_space_partner,corporate_workplace_wellness")
                .OldAnnotation("Npgsql:Enum:signup_type", "internal,external")
                .OldAnnotation("Npgsql:Enum:within_lens", "move,feel,seek")
                .OldAnnotation("Npgsql:Enum:within_role", "user,provider,admin");
        }
    }
}
