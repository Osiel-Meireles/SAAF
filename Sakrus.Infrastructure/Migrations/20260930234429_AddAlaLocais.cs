using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlaLocais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ala",
                table: "GavetasPublicas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ala",
                table: "GavetasPublicas");
        }
    }
}
