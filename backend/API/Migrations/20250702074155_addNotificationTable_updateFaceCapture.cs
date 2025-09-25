using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class addNotificationTable_updateFaceCapture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FaceCaptures_Confidence",
                table: "FaceCaptures");

            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "FaceCaptures");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "FaceCaptures",
                newName: "Emotions");

            migrationBuilder.RenameColumn(
                name: "Expression",
                table: "FaceCaptures",
                newName: "DominantEmotion");

            migrationBuilder.RenameIndex(
                name: "IX_FaceCaptures_Expression",
                table: "FaceCaptures",
                newName: "IX_FaceCaptures_DominantEmotion");

            migrationBuilder.AddColumn<bool>(
                name: "IsDetected",
                table: "FaceCaptures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ExamSupervisors",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", maxLength: 36, nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SendToId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_SendToId",
                        column: x => x.SendToId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedBy",
                table: "Notifications",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SendToId",
                table: "Notifications",
                column: "SendToId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsDetected",
                table: "FaceCaptures");

            migrationBuilder.RenameColumn(
                name: "Emotions",
                table: "FaceCaptures",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "DominantEmotion",
                table: "FaceCaptures",
                newName: "Expression");

            migrationBuilder.RenameIndex(
                name: "IX_FaceCaptures_DominantEmotion",
                table: "FaceCaptures",
                newName: "IX_FaceCaptures_Expression");

            migrationBuilder.AddColumn<int>(
                name: "Confidence",
                table: "FaceCaptures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ExamSupervisors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaceCaptures_Confidence",
                table: "FaceCaptures",
                column: "Confidence");
        }
    }
}
