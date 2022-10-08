using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuildId = table.Column<int>(type: "int", nullable: true),
                    ActiveEmote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectionMessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    GameRoleId = table.Column<int>(type: "int", nullable: true),
                    HasActiveRole = table.Column<bool>(type: "bit", nullable: false),
                    ActiveCheckRoomId = table.Column<int>(type: "int", nullable: true),
                    ModAcceptRoomId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisordId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Resetable = table.Column<bool>(type: "bit", nullable: false),
                    NeedsModApproval = table.Column<bool>(type: "bit", nullable: false),
                    Emote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GameId = table.Column<int>(type: "int", nullable: true),
                    GameId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Roles_Games_GameId1",
                        column: x => x.GameId1,
                        principalTable: "Games",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    SelectionRoomId = table.Column<int>(type: "int", nullable: true),
                    MemberRoleId = table.Column<int>(type: "int", nullable: true),
                    ArchiveCategoryId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    RuleRoomId = table.Column<int>(type: "int", nullable: true),
                    RuleMessageText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleMessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guilds_Roles_MemberRoleId",
                        column: x => x.MemberRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Guilds_Rooms_RuleRoomId",
                        column: x => x.RuleRoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Guilds_Rooms_SelectionRoomId",
                        column: x => x.SelectionRoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Songs_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_ActiveCheckRoomId",
                table: "Games",
                column: "ActiveCheckRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_GameRoleId",
                table: "Games",
                column: "GameRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_GuildId",
                table: "Games",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_ModAcceptRoomId",
                table: "Games",
                column: "ModAcceptRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_MemberRoleId",
                table: "Guilds",
                column: "MemberRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_RuleRoomId",
                table: "Guilds",
                column: "RuleRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_SelectionRoomId",
                table: "Guilds",
                column: "SelectionRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_GameId",
                table: "Roles",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_GameId1",
                table: "Roles",
                column: "GameId1");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_GameId",
                table: "Rooms",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_GuildId",
                table: "Songs",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Guilds_GuildId",
                table: "Games",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Roles_GameRoleId",
                table: "Games",
                column: "GameRoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Rooms_ActiveCheckRoomId",
                table: "Games",
                column: "ActiveCheckRoomId",
                principalTable: "Rooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Rooms_ModAcceptRoomId",
                table: "Games",
                column: "ModAcceptRoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Guilds_GuildId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Roles_GameRoleId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Rooms_ActiveCheckRoomId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Rooms_ModAcceptRoomId",
                table: "Games");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
