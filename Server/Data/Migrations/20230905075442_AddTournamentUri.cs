using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentUri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUri",
                table: "Tournament",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUri",
                table: "Tournament");
        }
    }
}
