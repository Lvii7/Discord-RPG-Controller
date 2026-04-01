using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordRPGController.Migrations
{
    /// <inheritdoc />
    public partial class Patch1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Combatant_Players_PlayerCharacterId",
                table: "Combatant");

            migrationBuilder.DropForeignKey(
                name: "FK_Combatant_Team_TeamId",
                table: "Combatant");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Battles_BattleId",
                table: "Team");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Team",
                table: "Team");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Combatant",
                table: "Combatant");

            migrationBuilder.DropColumn(
                name: "statuses",
                table: "Combatant");

            migrationBuilder.RenameTable(
                name: "Team",
                newName: "Teams");

            migrationBuilder.RenameTable(
                name: "Combatant",
                newName: "Combatants");

            migrationBuilder.RenameIndex(
                name: "IX_Team_BattleId",
                table: "Teams",
                newName: "IX_Teams_BattleId");

            migrationBuilder.RenameIndex(
                name: "IX_Combatant_TeamId",
                table: "Combatants",
                newName: "IX_Combatants_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Combatant_PlayerCharacterId",
                table: "Combatants",
                newName: "IX_Combatants_PlayerCharacterId");

            migrationBuilder.AddColumn<int>(
                name: "CurrentTurn",
                table: "Battles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teams",
                table: "Teams",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Combatants",
                table: "Combatants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Combatants_Players_PlayerCharacterId",
                table: "Combatants",
                column: "PlayerCharacterId",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Combatants_Teams_TeamId",
                table: "Combatants",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Battles_BattleId",
                table: "Teams",
                column: "BattleId",
                principalTable: "Battles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Combatants_Players_PlayerCharacterId",
                table: "Combatants");

            migrationBuilder.DropForeignKey(
                name: "FK_Combatants_Teams_TeamId",
                table: "Combatants");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Battles_BattleId",
                table: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teams",
                table: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Combatants",
                table: "Combatants");

            migrationBuilder.DropColumn(
                name: "CurrentTurn",
                table: "Battles");

            migrationBuilder.RenameTable(
                name: "Teams",
                newName: "Team");

            migrationBuilder.RenameTable(
                name: "Combatants",
                newName: "Combatant");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_BattleId",
                table: "Team",
                newName: "IX_Team_BattleId");

            migrationBuilder.RenameIndex(
                name: "IX_Combatants_TeamId",
                table: "Combatant",
                newName: "IX_Combatant_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Combatants_PlayerCharacterId",
                table: "Combatant",
                newName: "IX_Combatant_PlayerCharacterId");

            migrationBuilder.AddColumn<string>(
                name: "statuses",
                table: "Combatant",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Team",
                table: "Team",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Combatant",
                table: "Combatant",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Combatant_Players_PlayerCharacterId",
                table: "Combatant",
                column: "PlayerCharacterId",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Combatant_Team_TeamId",
                table: "Combatant",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Battles_BattleId",
                table: "Team",
                column: "BattleId",
                principalTable: "Battles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
