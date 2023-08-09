using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMinimumLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimumLevel",
                table: "Match",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumLevel",
                table: "Match");
        }
    }
}
