using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmrTools.Migrations
{
    /// <inheritdoc />
    public partial class AutoHashAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("7470b363-c36e-4111-b429-07972cf79cb8"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "GoogleId", "IsAdmin", "IsEmailVerified", "PasswordHash", "Role", "SubscriptionPlan" },
                values: new object[] { new Guid("7470b363-c36e-4111-b429-07972cf7a1b9"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@amrtools.com", "Super Admin", null, false, false, "$2a$11$5g9ndPel6U5OaCfl50Su2.DiWzOPBTXHjxMiqR3MnGwAHYkFvyd.K", "Admin", "Pro" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("7470b363-c36e-4111-b429-07972cf7a1b9"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "GoogleId", "IsAdmin", "IsEmailVerified", "PasswordHash", "Role", "SubscriptionPlan" },
                values: new object[] { new Guid("7470b363-c36e-4111-b429-07972cf79cb8"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@amrtools.com", "Super Admin", null, false, false, "$2a$11$R9h/lIPzHZ75NC6KnIZ.quS32L7KxpSG/+s15UGe.6QO9yYx3C0L.", "Admin", "Pro" });
        }
    }
}
