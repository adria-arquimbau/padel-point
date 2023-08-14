using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTournaments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tournament",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournament", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Couple",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Player1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Couple", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Couple_Player_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Player",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Couple_Player_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Player",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Couple_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Couple_Player1Id",
                table: "Couple",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Couple_Player2Id",
                table: "Couple",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Couple_TournamentId",
                table: "Couple",
                column: "TournamentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Couple");

            migrationBuilder.DropTable(
                name: "Tournament");
        }
    }
}
