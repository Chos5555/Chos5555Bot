using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class _20221012Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Emote",
                table: "Roles",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "ChoiceEmote",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChoiceEmote",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Roles",
                newName: "Emote");
        }
    }
}
