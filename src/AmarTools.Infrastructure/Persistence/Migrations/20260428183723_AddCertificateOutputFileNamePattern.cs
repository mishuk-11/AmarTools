using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmarTools.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateOutputFileNamePattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsPublished",
                table: "photo_frame_configs",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "event_tools",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputFileNamePattern",
                table: "certificate_template_configs",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputFilePath",
                table: "certificate_generation_batches",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputFileNamePattern",
                table: "certificate_template_configs");

            migrationBuilder.DropColumn(
                name: "OutputFilePath",
                table: "certificate_generation_batches");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublished",
                table: "photo_frame_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "event_tools",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
