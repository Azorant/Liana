using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Liana.Database.Migrations
{
    /// <inheritdoc />
    public partial class Changemessageedits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditedContent",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentsEdits",
                table: "Messages",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContentEdits",
                table: "Messages",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentsEdits",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ContentEdits",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "EditedContent",
                table: "Messages",
                type: "varchar(4000)",
                maxLength: 4000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
