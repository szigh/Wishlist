using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WishlistModels.Migrations
{
    /// <inheritdoc />
    public partial class Protected_GiftDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Protected",
                table: "GiftDays",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Protected",
                table: "GiftDays");
        }
    }
}
