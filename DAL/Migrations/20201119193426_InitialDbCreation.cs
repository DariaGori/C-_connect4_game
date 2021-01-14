using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class InitialDbCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameStates",
                columns: table => new
                {
                    GameStateId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaveTime = table.Column<DateTime>(nullable: false),
                    GameName = table.Column<string>(maxLength: 255, nullable: false),
                    GameSaveName = table.Column<string>(maxLength: 50, nullable: false),
                    BoardWidth = table.Column<int>(nullable: false),
                    BoardHeight = table.Column<int>(nullable: false),
                    PlayerZeroMove = table.Column<bool>(nullable: false),
                    ComputerMove = table.Column<bool>(nullable: false),
                    HumanPlayerCount = table.Column<int>(nullable: false),
                    WinningConditionSequenceLength = table.Column<int>(nullable: false),
                    Player1Name = table.Column<string>(maxLength: 50, nullable: false),
                    Player2Name = table.Column<string>(maxLength: 50, nullable: true),
                    GameOver = table.Column<bool>(nullable: false),
                    BoardStateJson = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStates", x => x.GameStateId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameStates");
        }
    }
}
