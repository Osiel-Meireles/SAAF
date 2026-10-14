using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposEnderecoParentescoFalecidoResponsavel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Responsaveis",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipio",
                table: "Responsaveis",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Responsaveis",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parentesco",
                table: "Responsaveis",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                table: "Responsaveis",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Falecidos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipio",
                table: "Falecidos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Falecidos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                table: "Falecidos",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "Municipio",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "Parentesco",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "Uf",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "Municipio",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "Uf",
                table: "Falecidos");
        }
    }
}
