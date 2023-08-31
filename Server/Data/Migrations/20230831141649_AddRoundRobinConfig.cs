using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundRobinConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoundRobinPhaseGroups",
                table: "Tournament",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "RobinPhaseGroup",
                table: "Match",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RobinPhaseRound",
                table: "Match",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoundRobinPhaseGroups",
                table: "Tournament");

            migrationBuilder.DropColumn(
                name: "RobinPhaseGroup",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "RobinPhaseRound",
                table: "Match");
        }
    }
}
