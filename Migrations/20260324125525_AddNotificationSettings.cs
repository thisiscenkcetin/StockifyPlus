using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockifyPlus.Migrations
{
    public partial class AddNotificationSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "MovementDate",
                table: "StockMovements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 3, 24, 15, 55, 25, 302, DateTimeKind.Local).AddTicks(2487),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 3, 22, 14, 19, 11, 949, DateTimeKind.Local).AddTicks(5181));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 3, 24, 15, 55, 25, 307, DateTimeKind.Local).AddTicks(6936),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 3, 22, 14, 19, 11, 950, DateTimeKind.Local).AddTicks(5177));

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AlertEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2026, 3, 24, 15, 55, 25, 308, DateTimeKind.Local).AddTicks(624))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "MovementDate",
                table: "StockMovements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 3, 22, 14, 19, 11, 949, DateTimeKind.Local).AddTicks(5181),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 3, 24, 15, 55, 25, 302, DateTimeKind.Local).AddTicks(2487));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 3, 22, 14, 19, 11, 950, DateTimeKind.Local).AddTicks(5177),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 3, 24, 15, 55, 25, 307, DateTimeKind.Local).AddTicks(6936));
        }
    }
}
