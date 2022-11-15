using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class ChangedQuestGame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quests_Games_GameId",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_Quests_GameId",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Quests");

            migrationBuilder.AddColumn<string>(
                name: "GameName",
                table: "Quests",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameName",
                table: "Quests");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Quests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quests_GameId",
                table: "Quests",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quests_Games_GameId",
                table: "Quests",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");
        }
    }
}
