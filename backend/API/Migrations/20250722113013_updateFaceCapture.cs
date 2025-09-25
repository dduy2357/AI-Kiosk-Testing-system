using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class updateFaceCapture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvgArousal",
                table: "FaceCaptures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AvgValence",
                table: "FaceCaptures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InferredState",
                table: "FaceCaptures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "FaceCaptures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "FaceCaptures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FaceCaptures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_CreatedAt",
                table: "UserLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_Description",
                table: "UserLogs",
                column: "Description");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_Status",
                table: "UserLogs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLogs_CreatedAt",
                table: "UserLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserLogs_Description",
                table: "UserLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserLogs_Status",
                table: "UserLogs");

            migrationBuilder.DropColumn(
                name: "AvgArousal",
                table: "FaceCaptures");

            migrationBuilder.DropColumn(
                name: "AvgValence",
                table: "FaceCaptures");

            migrationBuilder.DropColumn(
                name: "InferredState",
                table: "FaceCaptures");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "FaceCaptures");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "FaceCaptures");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FaceCaptures");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
