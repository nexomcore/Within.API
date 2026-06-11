using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class RetreatEventType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventType",
                schema: "within",
                table: "Events",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "class");

            migrationBuilder.AddColumn<bool>(
                name: "AccommodationIncluded",
                schema: "within",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MealsIncluded",
                schema: "within",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TransportIncluded",
                schema: "within",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RetreatDuration",
                schema: "within",
                table: "Events",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetreatFocus",
                schema: "within",
                table: "Events",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyLevel",
                schema: "within",
                table: "Events",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsIncluded",
                schema: "within",
                table: "Events",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatToBring",
                schema: "within",
                table: "Events",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "FacilitiesAvailable",
                schema: "within",
                table: "Events",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AccommodationIncluded",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MealsIncluded",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TransportIncluded",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RetreatDuration",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RetreatFocus",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "WhatsIncluded",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "WhatToBring",
                schema: "within",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "FacilitiesAvailable",
                schema: "within",
                table: "Events");
        }
    }
}
