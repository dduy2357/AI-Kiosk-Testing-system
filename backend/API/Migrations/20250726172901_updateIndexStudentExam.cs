using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class updateIndexStudentExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StartTime",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_Status",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentExamId",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_SubmitTime",
                table: "StudentExams");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_ExamId_StudentId",
                table: "StudentExams",
                columns: new[] { "ExamId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentExamId_ExamId",
                table: "StudentExams",
                columns: new[] { "StudentExamId", "ExamId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentExamId_ExamId_Status",
                table: "StudentExams",
                columns: new[] { "StudentExamId", "ExamId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentExamId_Status",
                table: "StudentExams",
                columns: new[] { "StudentExamId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentExamId_StudentId",
                table: "StudentExams",
                columns: new[] { "StudentExamId", "StudentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentExams_ExamId_StudentId",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentExamId_ExamId",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentExamId_ExamId_Status",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentExamId_Status",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentExamId_StudentId",
                table: "StudentExams");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StartTime",
                table: "StudentExams",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_Status",
                table: "StudentExams",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentExamId",
                table: "StudentExams",
                column: "StudentExamId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_SubmitTime",
                table: "StudentExams",
                column: "SubmitTime");
        }
    }
}
