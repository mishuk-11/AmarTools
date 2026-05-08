using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmarTools.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCertificateEmailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailBody",
                table: "certificate_template_configs");

            migrationBuilder.DropColumn(
                name: "EmailSubject",
                table: "certificate_template_configs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailBody",
                table: "certificate_template_configs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailSubject",
                table: "certificate_template_configs",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
