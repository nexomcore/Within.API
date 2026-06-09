using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserWellbeingProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DidMeditateToday",
                schema: "within",
                table: "DailyCheckIns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DidMoveToday",
                schema: "within",
                table: "DailyCheckIns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EnergyLevel",
                schema: "within",
                table: "DailyCheckIns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JournalEntry",
                schema: "within",
                table: "DailyCheckIns",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MoodScore",
                schema: "within",
                table: "DailyCheckIns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StressLevel",
                schema: "within",
                table: "DailyCheckIns",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserWellbeingGoals",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    GoalLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWellbeingGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserWellbeingInterests",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InterestKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    InterestLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWellbeingInterests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserWellbeingProfiles",
                schema: "within",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    UsePseudonym = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Pseudonym = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    AgeRange = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Gender = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    LocationCity = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    LocationSuburb = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ProfilePhotoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    ActivityLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AverageSleepHours = table.Column<decimal>(type: "numeric(3,1)", nullable: true),
                    WaterIntakeLitres = table.Column<decimal>(type: "numeric(3,1)", nullable: true),
                    ExerciseDaysPerWeek = table.Column<int>(type: "integer", nullable: true),
                    MeditationFrequency = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    StressLevelBaseline = table.Column<int>(type: "integer", nullable: true),
                    EnergyLevelBaseline = table.Column<int>(type: "integer", nullable: true),
                    MoodLevelBaseline = table.Column<int>(type: "integer", nullable: true),
                    BodyFatPercentage = table.Column<decimal>(type: "numeric(4,1)", nullable: true),
                    RestingHeartRate = table.Column<int>(type: "integer", nullable: true),
                    Vo2Max = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    BloodPressureSystolic = table.Column<int>(type: "integer", nullable: true),
                    BloodPressureDiastolic = table.Column<int>(type: "integer", nullable: true),
                    WearableProvider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    WearableConnected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastWearableSyncAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OnboardingCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWellbeingProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWellbeingGoals_UserId_GoalKey",
                schema: "within",
                table: "UserWellbeingGoals",
                columns: new[] { "UserId", "GoalKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWellbeingInterests_UserId_InterestKey",
                schema: "within",
                table: "UserWellbeingInterests",
                columns: new[] { "UserId", "InterestKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWellbeingProfiles_UserId",
                schema: "within",
                table: "UserWellbeingProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWellbeingGoals",
                schema: "within");

            migrationBuilder.DropTable(
                name: "UserWellbeingInterests",
                schema: "within");

            migrationBuilder.DropTable(
                name: "UserWellbeingProfiles",
                schema: "within");

            migrationBuilder.DropColumn(
                name: "DidMeditateToday",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "DidMoveToday",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "EnergyLevel",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "JournalEntry",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "MoodScore",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "StressLevel",
                schema: "within",
                table: "DailyCheckIns");
        }
    }
}
