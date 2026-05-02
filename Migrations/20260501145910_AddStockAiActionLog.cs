using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockifyPlus.Migrations
{
    public partial class AddStockAiActionLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockAiActionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    EntityKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    UserPrompt = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: false),
                    AgentResponse = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2026, 5, 1, 17, 59, 9, 934, DateTimeKind.Local).AddTicks(8362))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAiActionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockAiActionLog_ActionType",
                table: "StockAiActionLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_StockAiActionLog_CreatedAt",
                table: "StockAiActionLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StockAiActionLog_User_CreatedAt",
                table: "StockAiActionLogs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockAiActionLogs");
        }
    }
}
