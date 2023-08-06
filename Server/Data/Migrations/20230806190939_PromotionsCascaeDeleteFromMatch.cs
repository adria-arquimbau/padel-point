using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class PromotionsCascaeDeleteFromMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotion_Match_MatchId",
                table: "Promotion");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotion_Match_MatchId",
                table: "Promotion",
                column: "MatchId",
                principalTable: "Match",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotion_Match_MatchId",
                table: "Promotion");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotion_Match_MatchId",
                table: "Promotion",
                column: "MatchId",
                principalTable: "Match",
                principalColumn: "Id");
        }
    }
}
