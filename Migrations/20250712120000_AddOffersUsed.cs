using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfferManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOffersUsed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OffersUsed",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffersUsed",
                table: "Companies");
        }
    }
}
