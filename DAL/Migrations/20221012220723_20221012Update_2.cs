using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class _20221012Update_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Games_GameId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Games_GameId1",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "GameId1",
                table: "Roles",
                newName: "ModAcceptRoleGameId");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "Roles",
                newName: "ActiveRoleGameId");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_GameId1",
                table: "Roles",
                newName: "IX_Roles_ModAcceptRoleGameId");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_GameId",
                table: "Roles",
                newName: "IX_Roles_ActiveRoleGameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Games_ActiveRoleGameId",
                table: "Roles",
                column: "ActiveRoleGameId",
                principalTable: "Games",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Games_ModAcceptRoleGameId",
                table: "Roles",
                column: "ModAcceptRoleGameId",
                principalTable: "Games",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Games_ActiveRoleGameId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Games_ModAcceptRoleGameId",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "ModAcceptRoleGameId",
                table: "Roles",
                newName: "GameId1");

            migrationBuilder.RenameColumn(
                name: "ActiveRoleGameId",
                table: "Roles",
                newName: "GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_ModAcceptRoleGameId",
                table: "Roles",
                newName: "IX_Roles_GameId1");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_ActiveRoleGameId",
                table: "Roles",
                newName: "IX_Roles_GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Games_GameId",
                table: "Roles",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Games_GameId1",
                table: "Roles",
                column: "GameId1",
                principalTable: "Games",
                principalColumn: "Id");
        }
    }
}
