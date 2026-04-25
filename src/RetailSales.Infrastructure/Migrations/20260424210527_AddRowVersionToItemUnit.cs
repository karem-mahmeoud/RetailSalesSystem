using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailSales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToItemUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ItemUnits",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ItemUnits");
        }
    }
}
