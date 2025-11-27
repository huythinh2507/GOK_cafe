using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GOKCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlavourProfileAndEquipmentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlavourProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlavourProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductEquipments",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductEquipments", x => new { x.ProductId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_ProductEquipments_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductEquipments_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductFlavourProfiles",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlavourProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFlavourProfiles", x => new { x.ProductId, x.FlavourProfileId });
                    table.ForeignKey(
                        name: "FK_ProductFlavourProfiles_FlavourProfiles_FlavourProfileId",
                        column: x => x.FlavourProfileId,
                        principalTable: "FlavourProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductFlavourProfiles_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsActive_DisplayOrder",
                table: "Products",
                columns: new[] { "CategoryId", "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_DisplayOrder",
                table: "Products",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeatured",
                table: "Products",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_DisplayOrder",
                table: "Equipments",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_IsActive",
                table: "Equipments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_Name",
                table: "Equipments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlavourProfiles_DisplayOrder",
                table: "FlavourProfiles",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FlavourProfiles_IsActive",
                table: "FlavourProfiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FlavourProfiles_Name",
                table: "FlavourProfiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductEquipments_EquipmentId",
                table: "ProductEquipments",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEquipments_ProductId",
                table: "ProductEquipments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFlavourProfiles_FlavourProfileId",
                table: "ProductFlavourProfiles",
                column: "FlavourProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFlavourProfiles_ProductId",
                table: "ProductFlavourProfiles",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductEquipments");

            migrationBuilder.DropTable(
                name: "ProductFlavourProfiles");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "FlavourProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_IsActive_DisplayOrder",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_DisplayOrder",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsFeatured",
                table: "Products");
        }
    }
}
