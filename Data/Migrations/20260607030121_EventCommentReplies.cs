using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventCommentReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentCommentId",
                schema: "within",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "within",
                table: "Comments",
                column: "ParentCommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "within",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                schema: "within",
                table: "Comments");
        }
    }
}
