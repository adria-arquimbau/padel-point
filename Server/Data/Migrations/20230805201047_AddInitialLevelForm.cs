﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialLevelForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InitialLevelForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OtherRacketSportsYearsPlaying = table.Column<int>(type: "int", nullable: false),
                    PlayedOtherRacketSportsBefore = table.Column<bool>(type: "bit", nullable: false),
                    OtherRacketSportsLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfAssessedPadelSkillLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YearsPlayingPadel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialLevelForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InitialLevelForms_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InitialLevelForms_PlayerId",
                table: "InitialLevelForms",
                column: "PlayerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InitialLevelForms");
        }
    }
}
