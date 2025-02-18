using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Liana.Database.Migrations
{
    /// <inheritdoc />
    public partial class Changeattachmentstononnullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "Attachments",
                keyValue: null,
                column: "Attachments",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Attachments",
                table: "Messages",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Attachments",
                table: "Messages",
                type: "json",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "json")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
