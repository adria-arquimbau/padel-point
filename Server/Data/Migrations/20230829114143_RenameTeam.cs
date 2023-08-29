using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Set_Match_MatchId",
                table: "Set");

            migrationBuilder.DropTable(
                name: "Couple");

            migrationBuilder.AlterColumn<Guid>(
                name: "MatchId",
                table: "Set",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player1Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    Player2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player2Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Team_Player_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Player",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Team_Player_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Player",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Team_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Team_Player1Id",
                table: "Team",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Team_Player2Id",
                table: "Team",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Team_TournamentId",
                table: "Team",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Set_Match_MatchId",
                table: "Set",
                column: "MatchId",
                principalTable: "Match",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Set_Match_MatchId",
                table: "Set");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.AlterColumn<Guid>(
                name: "MatchId",
                table: "Set",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Couple",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Player1Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    Player2Confirmed = table.Column<bool>(type: "bit", nullable: false)
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.AddForeignKey(
                name: "FK_Set_Match_MatchId",
                table: "Set",
                column: "MatchId",
                principalTable: "Match",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
