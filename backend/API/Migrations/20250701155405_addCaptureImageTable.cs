using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class addCaptureImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaceCaptures",
                columns: table => new
                {
                    CaptureId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    StudentExamId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogType = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expression = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Confidence = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceCaptures", x => x.CaptureId);
                    table.ForeignKey(
                        name: "FK_FaceCaptures_StudentExams_StudentExamId",
                        column: x => x.StudentExamId,
                        principalTable: "StudentExams",
                        principalColumn: "StudentExamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceCaptures_Confidence",
                table: "FaceCaptures",
                column: "Confidence");

            migrationBuilder.CreateIndex(
                name: "IX_FaceCaptures_Expression",
                table: "FaceCaptures",
                column: "Expression");

            migrationBuilder.CreateIndex(
                name: "IX_FaceCaptures_ImageUrl",
                table: "FaceCaptures",
                column: "ImageUrl");

            migrationBuilder.CreateIndex(
                name: "IX_FaceCaptures_LogType",
                table: "FaceCaptures",
                column: "LogType");

            migrationBuilder.CreateIndex(
                name: "IX_FaceCaptures_StudentExamId",
                table: "FaceCaptures",
                column: "StudentExamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceCaptures");
        }
    }
}
