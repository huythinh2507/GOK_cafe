using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GOKCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomValueToProductAttributeSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductAttributeSelections_ProductId_ProductAttributeId_ProductAttributeValueId",
                table: "ProductAttributeSelections");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductAttributeValueId",
                table: "ProductAttributeSelections",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "CustomValue",
                table: "ProductAttributeSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeSelections_ProductId_ProductAttributeId_ProductAttributeValueId",
                table: "ProductAttributeSelections",
                columns: new[] { "ProductId", "ProductAttributeId", "ProductAttributeValueId" },
                unique: true,
                filter: "[ProductAttributeValueId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductAttributeSelections_ProductId_ProductAttributeId_ProductAttributeValueId",
                table: "ProductAttributeSelections");

            migrationBuilder.DropColumn(
                name: "CustomValue",
                table: "ProductAttributeSelections");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductAttributeValueId",
                table: "ProductAttributeSelections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeSelections_ProductId_ProductAttributeId_ProductAttributeValueId",
                table: "ProductAttributeSelections",
                columns: new[] { "ProductId", "ProductAttributeId", "ProductAttributeValueId" },
                unique: true);
        }
    }
}
