using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class fixDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSupervisors_Exams_ExamId1",
                table: "ExamSupervisors");

            migrationBuilder.DropIndex(
                name: "IX_ExamSupervisors_ExamId1",
                table: "ExamSupervisors");

            migrationBuilder.DropColumn(
                name: "ExamId1",
                table: "ExamSupervisors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExamId1",
                table: "ExamSupervisors",
                type: "nvarchar(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSupervisors_ExamId1",
                table: "ExamSupervisors",
                column: "ExamId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSupervisors_Exams_ExamId1",
                table: "ExamSupervisors",
                column: "ExamId1",
                principalTable: "Exams",
                principalColumn: "ExamId");
        }
    }
}
