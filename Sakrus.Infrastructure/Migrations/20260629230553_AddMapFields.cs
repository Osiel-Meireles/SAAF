using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMapFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ala",
                table: "Ossuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroLote",
                table: "Ossuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Quadra",
                table: "Ossuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ala",
                table: "Jazigos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroLote",
                table: "Jazigos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Quadra",
                table: "Jazigos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ala",
                table: "Ossuarios");

            migrationBuilder.DropColumn(
                name: "NumeroLote",
                table: "Ossuarios");

            migrationBuilder.DropColumn(
                name: "Quadra",
                table: "Ossuarios");

            migrationBuilder.DropColumn(
                name: "Ala",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "NumeroLote",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "Quadra",
                table: "Jazigos");
        }
    }
}
