using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class StageChannelUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuildId",
                table: "Rooms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SpeakerRoleId",
                table: "Rooms",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_GuildId",
                table: "Rooms",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Guilds_GuildId",
                table: "Rooms",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Guilds_GuildId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_GuildId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "SpeakerRoleId",
                table: "Rooms");
        }
    }
}
