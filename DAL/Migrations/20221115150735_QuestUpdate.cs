using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.Migrations
{
    public partial class QuestUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CategoryId",
                table: "Games",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ModQuestRoomId",
                table: "Games",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    AuthorId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TakerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    QuestMessage = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    QuestMessageChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ModMessage = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quests_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompletedQuests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameName = table.Column<string>(type: "text", nullable: true),
                    QuestCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedQuestsUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedQuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletedQuests_Users_CompletedQuestsUserId",
                        column: x => x.CompletedQuestsUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_ModQuestRoomId",
                table: "Games",
                column: "ModQuestRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedQuests_CompletedQuestsUserId",
                table: "CompletedQuests",
                column: "CompletedQuestsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_GameId",
                table: "Quests",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Rooms_ModQuestRoomId",
                table: "Games",
                column: "ModQuestRoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Rooms_ModQuestRoomId",
                table: "Games");

            migrationBuilder.DropTable(
                name: "CompletedQuests");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Games_ModQuestRoomId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ModQuestRoomId",
                table: "Games");
        }
    }
}
