using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class EloHistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EloHistory_Match_MatchId",
                table: "EloHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_EloHistory_Player_PlayerId",
                table: "EloHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EloHistory",
                table: "EloHistory");

            migrationBuilder.RenameTable(
                name: "EloHistory",
                newName: "EloHistories");

            migrationBuilder.RenameIndex(
                name: "IX_EloHistory_PlayerId",
                table: "EloHistories",
                newName: "IX_EloHistories_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_EloHistory_MatchId",
                table: "EloHistories",
                newName: "IX_EloHistories_MatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EloHistories",
                table: "EloHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EloHistories_Match_MatchId",
                table: "EloHistories",
                column: "MatchId",
                principalTable: "Match",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EloHistories_Player_PlayerId",
                table: "EloHistories",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EloHistories_Match_MatchId",
                table: "EloHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_EloHistories_Player_PlayerId",
                table: "EloHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EloHistories",
                table: "EloHistories");

            migrationBuilder.RenameTable(
                name: "EloHistories",
                newName: "EloHistory");

            migrationBuilder.RenameIndex(
                name: "IX_EloHistories_PlayerId",
                table: "EloHistory",
                newName: "IX_EloHistory_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_EloHistories_MatchId",
                table: "EloHistory",
                newName: "IX_EloHistory_MatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EloHistory",
                table: "EloHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EloHistory_Match_MatchId",
                table: "EloHistory",
                column: "MatchId",
                principalTable: "Match",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EloHistory_Player_PlayerId",
                table: "EloHistory",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
