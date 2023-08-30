using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundRobinPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TournamentId",
                table: "Match",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Match_TournamentId",
                table: "Match",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Tournament_TournamentId",
                table: "Match",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Tournament_TournamentId",
                table: "Match");

            migrationBuilder.DropIndex(
                name: "IX_Match_TournamentId",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Match");
        }
    }
}
