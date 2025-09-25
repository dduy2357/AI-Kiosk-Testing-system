using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class updateQUestionShareTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionShares",
                columns: table => new
                {
                    QuestionShareId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    QuestionBankId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SharedWithUserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionShares", x => x.QuestionShareId);
                    table.ForeignKey(
                        name: "FK_QuestionShares_QuestionBanks_QuestionBankId",
                        column: x => x.QuestionBankId,
                        principalTable: "QuestionBanks",
                        principalColumn: "QuestionBankId");
                    table.ForeignKey(
                        name: "FK_QuestionShares_Users_SharedWithUserId",
                        column: x => x.SharedWithUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionShares_QuestionBankId",
                table: "QuestionShares",
                column: "QuestionBankId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionShares_SharedWithUserId",
                table: "QuestionShares",
                column: "SharedWithUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionShares");
        }
    }
}
