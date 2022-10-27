using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    GuildId = table.Column<int>(type: "integer", nullable: true),
                    ActiveEmote = table.Column<string>(type: "text", nullable: true),
                    SelectionMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GameRoleGameId = table.Column<int>(type: "integer", nullable: true),
                    MainActiveRoleGameId = table.Column<int>(type: "integer", nullable: true),
                    HasActiveRole = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveCheckRoomId = table.Column<int>(type: "integer", nullable: true),
                    ModAcceptRoomId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Resettable = table.Column<bool>(type: "boolean", nullable: false),
                    NeedsModApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ChoiceEmote = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ActiveRoleGameId = table.Column<int>(type: "integer", nullable: true),
                    ModAcceptRoleGameId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Games_ActiveRoleGameId",
                        column: x => x.ActiveRoleGameId,
                        principalTable: "Games",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Roles_Games_ModAcceptRoleGameId",
                        column: x => x.ModAcceptRoleGameId,
                        principalTable: "Games",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SelectionRoomId = table.Column<int>(type: "integer", nullable: true),
                    MemberRoleId = table.Column<int>(type: "integer", nullable: true),
                    ArchiveCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RuleRoomId = table.Column<int>(type: "integer", nullable: true),
                    RuleMessageText = table.Column<string>(type: "text", nullable: true),
                    RuleMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserLeaveMessageRoomId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<int>(type: "integer", nullable: true)
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
                name: "IX_Games_GameRoleGameId",
                table: "Games",
                column: "GameRoleGameId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_GuildId",
                table: "Games",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_MainActiveRoleGameId",
                table: "Games",
                column: "MainActiveRoleGameId");

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
                name: "IX_Roles_ActiveRoleGameId",
                table: "Roles",
                column: "ActiveRoleGameId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ModAcceptRoleGameId",
                table: "Roles",
                column: "ModAcceptRoleGameId");

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
                name: "FK_Games_Roles_GameRoleGameId",
                table: "Games",
                column: "GameRoleGameId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Roles_MainActiveRoleGameId",
                table: "Games",
                column: "MainActiveRoleGameId",
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
                name: "FK_Games_Roles_GameRoleGameId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Roles_MainActiveRoleGameId",
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
