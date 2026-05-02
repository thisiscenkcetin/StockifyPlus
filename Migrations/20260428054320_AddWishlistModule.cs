using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockifyPlus.Migrations
{
    public partial class AddWishlistModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "MovementDate",
                table: "StockMovements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 136, DateTimeKind.Local).AddTicks(4057),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 627, DateTimeKind.Local).AddTicks(5618));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProductCustomFields",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 137, DateTimeKind.Local).AddTicks(6450),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(9347));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "NotificationSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 137, DateTimeKind.Local).AddTicks(4318),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(6954));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 137, DateTimeKind.Local).AddTicks(968),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(3333));

            migrationBuilder.CreateTable(
                name: "Wishlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TargetPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CurrentPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProductUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsNotified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsPurchased = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 138, DateTimeKind.Local).AddTicks(2675)),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wishlist_User",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_IsPurchased",
                table: "Wishlists",
                column: "IsPurchased");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_Priority",
                table: "Wishlists",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_User_Purchased",
                table: "Wishlists",
                columns: new[] { "UserId", "IsPurchased" });

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_UserId",
                table: "Wishlists",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wishlists");

            migrationBuilder.AlterColumn<DateTime>(
                name: "MovementDate",
                table: "StockMovements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 627, DateTimeKind.Local).AddTicks(5618),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 136, DateTimeKind.Local).AddTicks(4057));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProductCustomFields",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(9347),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 137, DateTimeKind.Local).AddTicks(6450));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "NotificationSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(6954),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 137, DateTimeKind.Local).AddTicks(4318));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(3333),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 8, 43, 20, 137, DateTimeKind.Local).AddTicks(968));
        }
    }
}
