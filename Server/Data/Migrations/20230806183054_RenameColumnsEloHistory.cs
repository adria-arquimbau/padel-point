using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsEloHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousElo",
                table: "EloHistories",
                newName: "OldElo");

            migrationBuilder.RenameColumn(
                name: "CurrentElo",
                table: "EloHistories",
                newName: "NewElo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OldElo",
                table: "EloHistories",
                newName: "PreviousElo");

            migrationBuilder.RenameColumn(
                name: "NewElo",
                table: "EloHistories",
                newName: "CurrentElo");
        }
    }
}
