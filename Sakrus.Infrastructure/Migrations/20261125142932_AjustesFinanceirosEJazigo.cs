using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustesFinanceirosEJazigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorManutencaoAnual",
                table: "ModelosJazigos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeGavetas",
                table: "Jazigos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeGavetasOssuario",
                table: "Jazigos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorConcessaoM2Demais",
                table: "ConfiguracoesFinanceiras",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorConcessaoM2Quadras0102",
                table: "ConfiguracoesFinanceiras",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Configurações já existentes passam a usar os valores de concessão por quadra.
            migrationBuilder.Sql("""
                UPDATE "ConfiguracoesFinanceiras"
                SET "ValorConcessaoM2Quadras0102" = 276.66,
                    "ValorConcessaoM2Demais" = 366.55
                WHERE "ValorConcessaoM2Quadras0102" = 0
                  AND "ValorConcessaoM2Demais" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorManutencaoAnual",
                table: "ModelosJazigos");

            migrationBuilder.DropColumn(
                name: "QuantidadeGavetas",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "QuantidadeGavetasOssuario",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "ValorConcessaoM2Demais",
                table: "ConfiguracoesFinanceiras");

            migrationBuilder.DropColumn(
                name: "ValorConcessaoM2Quadras0102",
                table: "ConfiguracoesFinanceiras");
        }
    }
}
