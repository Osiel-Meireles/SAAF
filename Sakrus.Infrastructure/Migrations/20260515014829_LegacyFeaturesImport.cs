using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LegacyFeaturesImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPecaAnatomica",
                table: "Falecidos");

            migrationBuilder.AddColumn<int>(
                name: "OssuarioId",
                table: "Falecidos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoRestosMortais",
                table: "Falecidos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OssuarioDestinoId",
                table: "ExumacoesRegistros",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FunerariaId",
                table: "Atendimentos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Funerarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CNPJ = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Endereco = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funerarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoTitularidadeJazigos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JazigoId = table.Column<int>(type: "integer", nullable: false),
                    ResponsavelAntigoId = table.Column<int>(type: "integer", nullable: false),
                    ResponsavelNovoId = table.Column<int>(type: "integer", nullable: false),
                    DataTransferencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoTitularidadeJazigos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoTitularidadeJazigos_Jazigos_JazigoId",
                        column: x => x.JazigoId,
                        principalTable: "Jazigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoTitularidadeJazigos_Responsaveis_ResponsavelAntigo~",
                        column: x => x.ResponsavelAntigoId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricoTitularidadeJazigos_Responsaveis_ResponsavelNovoId",
                        column: x => x.ResponsavelNovoId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricoTitularidadeJazigos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ossuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identificador = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    JazigoVinculadoId = table.Column<int>(type: "integer", nullable: true),
                    Capacidade = table.Column<int>(type: "integer", nullable: false),
                    Ocupado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ossuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ossuarios_Jazigos_JazigoVinculadoId",
                        column: x => x.JazigoVinculadoId,
                        principalTable: "Jazigos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProdutosEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    QuantidadeDisponivel = table.Column<int>(type: "integer", nullable: false),
                    EstoqueMinimo = table.Column<int>(type: "integer", nullable: false),
                    Custo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ValorVenda = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutosEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoEstoqueId = table.Column<int>(type: "integer", nullable: false),
                    TipoMovimentacao = table.Column<string>(type: "text", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    DataMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_ProdutosEstoque_ProdutoEstoqueId",
                        column: x => x.ProdutoEstoqueId,
                        principalTable: "ProdutosEstoque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Falecidos_OssuarioId",
                table: "Falecidos",
                column: "OssuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ExumacoesRegistros_OssuarioDestinoId",
                table: "ExumacoesRegistros",
                column: "OssuarioDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_FunerariaId",
                table: "Atendimentos",
                column: "FunerariaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoTitularidadeJazigos_JazigoId",
                table: "HistoricoTitularidadeJazigos",
                column: "JazigoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoTitularidadeJazigos_ResponsavelAntigoId",
                table: "HistoricoTitularidadeJazigos",
                column: "ResponsavelAntigoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoTitularidadeJazigos_ResponsavelNovoId",
                table: "HistoricoTitularidadeJazigos",
                column: "ResponsavelNovoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoTitularidadeJazigos_UsuarioId",
                table: "HistoricoTitularidadeJazigos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoEstoqueId",
                table: "MovimentacoesEstoque",
                column: "ProdutoEstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Ossuarios_JazigoVinculadoId",
                table: "Ossuarios",
                column: "JazigoVinculadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Atendimentos_Funerarias_FunerariaId",
                table: "Atendimentos",
                column: "FunerariaId",
                principalTable: "Funerarias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExumacoesRegistros_Ossuarios_OssuarioDestinoId",
                table: "ExumacoesRegistros",
                column: "OssuarioDestinoId",
                principalTable: "Ossuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Falecidos_Ossuarios_OssuarioId",
                table: "Falecidos",
                column: "OssuarioId",
                principalTable: "Ossuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atendimentos_Funerarias_FunerariaId",
                table: "Atendimentos");

            migrationBuilder.DropForeignKey(
                name: "FK_ExumacoesRegistros_Ossuarios_OssuarioDestinoId",
                table: "ExumacoesRegistros");

            migrationBuilder.DropForeignKey(
                name: "FK_Falecidos_Ossuarios_OssuarioId",
                table: "Falecidos");

            migrationBuilder.DropTable(
                name: "Funerarias");

            migrationBuilder.DropTable(
                name: "HistoricoTitularidadeJazigos");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "Ossuarios");

            migrationBuilder.DropTable(
                name: "ProdutosEstoque");

            migrationBuilder.DropIndex(
                name: "IX_Falecidos_OssuarioId",
                table: "Falecidos");

            migrationBuilder.DropIndex(
                name: "IX_ExumacoesRegistros_OssuarioDestinoId",
                table: "ExumacoesRegistros");

            migrationBuilder.DropIndex(
                name: "IX_Atendimentos_FunerariaId",
                table: "Atendimentos");

            migrationBuilder.DropColumn(
                name: "OssuarioId",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "TipoRestosMortais",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "OssuarioDestinoId",
                table: "ExumacoesRegistros");

            migrationBuilder.DropColumn(
                name: "FunerariaId",
                table: "Atendimentos");

            migrationBuilder.AddColumn<bool>(
                name: "IsPecaAnatomica",
                table: "Falecidos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
