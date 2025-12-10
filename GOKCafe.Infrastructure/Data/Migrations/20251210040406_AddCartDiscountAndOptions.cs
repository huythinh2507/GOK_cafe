using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GOKCafe.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCartDiscountAndOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedCouponCode",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedCouponId",
                table: "Carts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SelectedGrind",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedSize",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_AppliedCouponId",
                table: "Carts",
                column: "AppliedCouponId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Coupons_AppliedCouponId",
                table: "Carts",
                column: "AppliedCouponId",
                principalTable: "Coupons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Coupons_AppliedCouponId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_AppliedCouponId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "AppliedCouponCode",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "AppliedCouponId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "SelectedGrind",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SelectedSize",
                table: "CartItems");
        }
    }
}
