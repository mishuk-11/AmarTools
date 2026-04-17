using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmarTools.InvoiceGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRateToLineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "LineItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "LineItems");
        }
    }
}
