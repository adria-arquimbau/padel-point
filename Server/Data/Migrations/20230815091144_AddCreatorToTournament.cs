using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorToTournament : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Tournament",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tournament_CreatorId",
                table: "Tournament",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tournament_Player_CreatorId",
                table: "Tournament",
                column: "CreatorId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tournament_Player_CreatorId",
                table: "Tournament");

            migrationBuilder.DropIndex(
                name: "IX_Tournament_CreatorId",
                table: "Tournament");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Tournament");
        }
    }
}
