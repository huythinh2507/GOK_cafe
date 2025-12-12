using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GOKCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDetailsAndOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableGrinds",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailableSizes",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Process",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TastingNote",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableGrinds",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AvailableSizes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Process",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TastingNote",
                table: "Products");
        }
    }
}
