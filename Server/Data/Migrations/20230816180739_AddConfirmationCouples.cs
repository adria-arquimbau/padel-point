using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfirmationCouples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Player1Confirmed",
                table: "Couple",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Player2Confirmed",
                table: "Couple",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Player1Confirmed",
                table: "Couple");

            migrationBuilder.DropColumn(
                name: "Player2Confirmed",
                table: "Couple");
        }
    }
}
