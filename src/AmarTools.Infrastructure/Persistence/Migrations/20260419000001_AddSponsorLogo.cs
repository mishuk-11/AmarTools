using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmarTools.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SponsorLogoPath",
                table: "photo_frame_configs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SponsorLogoPath",
                table: "photo_frame_configs");
        }
    }
}
