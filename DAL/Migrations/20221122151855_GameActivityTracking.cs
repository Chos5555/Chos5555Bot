using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.Migrations
{
    public partial class GameActivityTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityCheck",
                table: "Games",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "RemoveAfter",
                table: "Games",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "TrackActivity",
                table: "Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GameActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameName = table.Column<string>(type: "text", nullable: true),
                    LastAppearance = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GameActivityUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameActivities_Users_GameActivityUserId",
                        column: x => x.GameActivityUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameActivities_GameActivityUserId",
                table: "GameActivities",
                column: "GameActivityUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameActivities");

            migrationBuilder.DropColumn(
                name: "LastActivityCheck",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RemoveAfter",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "TrackActivity",
                table: "Games");
        }
    }
}
