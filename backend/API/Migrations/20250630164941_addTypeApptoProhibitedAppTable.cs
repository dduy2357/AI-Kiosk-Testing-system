using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class addTypeApptoProhibitedAppTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExamLiveStatus",
                table: "Exams");

            migrationBuilder.AddColumn<int>(
                name: "ExtraTimeMinutes",
                table: "StudentExams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeApp",
                table: "ProhibitedApps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProhibitedApps_TypeApp",
                table: "ProhibitedApps",
                column: "TypeApp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProhibitedApps_TypeApp",
                table: "ProhibitedApps");

            migrationBuilder.DropColumn(
                name: "ExtraTimeMinutes",
                table: "StudentExams");

            migrationBuilder.DropColumn(
                name: "TypeApp",
                table: "ProhibitedApps");

            migrationBuilder.AddColumn<int>(
                name: "ExamLiveStatus",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
