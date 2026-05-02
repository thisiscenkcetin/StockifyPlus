using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockifyPlus.Migrations
{
    public partial class AddProductCustomFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "MovementDate",
                table: "StockMovements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 627, DateTimeKind.Local).AddTicks(5618),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 27, 18, 2, 49, 885, DateTimeKind.Local).AddTicks(2098));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "NotificationSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(6954),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 27, 18, 2, 49, 886, DateTimeKind.Local).AddTicks(2895));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(3333),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 27, 18, 2, 49, 885, DateTimeKind.Local).AddTicks(9081));

            migrationBuilder.CreateTable(
                name: "ProductCustomFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FieldType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Text"),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(9347))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCustomFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCustomField_Product",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustomField_FieldName",
                table: "ProductCustomFields",
                column: "FieldName");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustomField_ProductId",
                table: "ProductCustomFields",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCustomFields");

            migrationBuilder.AlterColumn<DateTime>(
                name: "MovementDate",
                table: "StockMovements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 27, 18, 2, 49, 885, DateTimeKind.Local).AddTicks(2098),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 627, DateTimeKind.Local).AddTicks(5618));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "NotificationSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 27, 18, 2, 49, 886, DateTimeKind.Local).AddTicks(2895),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(6954));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 27, 18, 2, 49, 885, DateTimeKind.Local).AddTicks(9081),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 4, 28, 0, 40, 16, 628, DateTimeKind.Local).AddTicks(3333));
        }
    }
}
