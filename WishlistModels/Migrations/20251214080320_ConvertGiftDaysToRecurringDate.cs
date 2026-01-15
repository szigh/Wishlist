using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WishlistModels.Migrations
{
    /// <inheritdoc />
    public partial class ConvertGiftDaysToRecurringDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "GiftDays");

            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "GiftDays",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "GiftDays",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "GiftDays");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "GiftDays");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "GiftDays",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
