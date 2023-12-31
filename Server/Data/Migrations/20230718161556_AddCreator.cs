﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Match",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Match_CreatorId",
                table: "Match",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Player_CreatorId",
                table: "Match",
                column: "CreatorId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Player_CreatorId",
                table: "Match");

            migrationBuilder.DropIndex(
                name: "IX_Match_CreatorId",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Match");
        }
    }
}
