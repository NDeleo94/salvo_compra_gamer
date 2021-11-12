using Microsoft.EntityFrameworkCore.Migrations;

namespace Salvo.Migrations
{
    public partial class modifyScoreEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Finish",
                table: "Scores",
                newName: "FinishDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FinishDate",
                table: "Scores",
                newName: "Finish");
        }
    }
}
