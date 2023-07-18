using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class SkillToElo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillLevel",
                table: "Player");

            migrationBuilder.AddColumn<decimal>(
                name: "Elo",
                table: "Player",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Elo",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "SkillLevel",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
