using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class GameModelUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Roles_GameRoleId",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "GameRoleId",
                table: "Games",
                newName: "MainActiveRoleGameId");

            migrationBuilder.RenameIndex(
                name: "IX_Games_GameRoleId",
                table: "Games",
                newName: "IX_Games_MainActiveRoleGameId");

            migrationBuilder.AddColumn<int>(
                name: "GameRoleGameId",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_GameRoleGameId",
                table: "Games",
                column: "GameRoleGameId");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Roles_GameRoleGameId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Roles_MainActiveRoleGameId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_GameRoleGameId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "GameRoleGameId",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "MainActiveRoleGameId",
                table: "Games",
                newName: "GameRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Games_MainActiveRoleGameId",
                table: "Games",
                newName: "IX_Games_GameRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Roles_GameRoleId",
                table: "Games",
                column: "GameRoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
