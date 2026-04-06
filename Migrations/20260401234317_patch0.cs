using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordRPGController.Migrations
{
    /// <inheritdoc />
    public partial class patch0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Battles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<string>(type: "TEXT", nullable: false),
                    TeamCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentTurn = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Money = table.Column<int>(type: "INTEGER", nullable: false),
                    HP = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxHP = table.Column<int>(type: "INTEGER", nullable: false),
                    SP = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxSP = table.Column<int>(type: "INTEGER", nullable: false),
                    AP = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAP = table.Column<int>(type: "INTEGER", nullable: false),
                    FP = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxFP = table.Column<int>(type: "INTEGER", nullable: false),
                    Energy = table.Column<int>(type: "INTEGER", nullable: false),
                    ATK = table.Column<int>(type: "INTEGER", nullable: false),
                    DEF = table.Column<int>(type: "INTEGER", nullable: false),
                    Dice = table.Column<int>(type: "INTEGER", nullable: false),
                    IsInBattle = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeamNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    BattleId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Battles_BattleId",
                        column: x => x.BattleId,
                        principalTable: "Battles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Combatants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerCharacterId = table.Column<int>(type: "INTEGER", nullable: true),
                    TurnOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    HP = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxHP = table.Column<int>(type: "INTEGER", nullable: false),
                    SP = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxSP = table.Column<int>(type: "INTEGER", nullable: false),
                    Energy = table.Column<int>(type: "INTEGER", nullable: false),
                    ATK = table.Column<int>(type: "INTEGER", nullable: false),
                    DEF = table.Column<int>(type: "INTEGER", nullable: false),
                    Dice = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combatants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Combatants_Players_PlayerCharacterId",
                        column: x => x.PlayerCharacterId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Combatants_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Combatants_PlayerCharacterId",
                table: "Combatants",
                column: "PlayerCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Combatants_TeamId",
                table: "Combatants",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_BattleId",
                table: "Teams",
                column: "BattleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Combatants");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Battles");
        }
    }
}
