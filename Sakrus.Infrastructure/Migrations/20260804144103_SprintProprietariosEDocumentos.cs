using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SprintProprietariosEDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Responsaveis",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EhProprietario",
                table: "Responsaveis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Responsaveis",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FalecidoId",
                table: "DocumentosAnexos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "AtendimentoId",
                table: "DocumentosAnexos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "FunerariaId",
                table: "DocumentosAnexos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponsavelId",
                table: "DocumentosAnexos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "DocumentosAnexos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "JazigoProprietarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JazigoId = table.Column<int>(type: "integer", nullable: false),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: false),
                    TipoVinculo = table.Column<string>(type: "text", nullable: false),
                    TipoTitulo = table.Column<string>(type: "text", nullable: false),
                    DataAquisicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    HerdeiroPrevistId = table.Column<int>(type: "integer", nullable: true),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JazigoProprietarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JazigoProprietarios_Jazigos_JazigoId",
                        column: x => x.JazigoId,
                        principalTable: "Jazigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JazigoProprietarios_Responsaveis_HerdeiroPrevistId",
                        column: x => x.HerdeiroPrevistId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JazigoProprietarios_Responsaveis_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JazigoProprietarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAnexos_FunerariaId",
                table: "DocumentosAnexos",
                column: "FunerariaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAnexos_ResponsavelId",
                table: "DocumentosAnexos",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_JazigoProprietarios_HerdeiroPrevistId",
                table: "JazigoProprietarios",
                column: "HerdeiroPrevistId");

            migrationBuilder.CreateIndex(
                name: "IX_JazigoProprietarios_JazigoId_Ativo",
                table: "JazigoProprietarios",
                columns: new[] { "JazigoId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_JazigoProprietarios_ResponsavelId_Ativo",
                table: "JazigoProprietarios",
                columns: new[] { "ResponsavelId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_JazigoProprietarios_UsuarioId",
                table: "JazigoProprietarios",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAnexos_Funerarias_FunerariaId",
                table: "DocumentosAnexos",
                column: "FunerariaId",
                principalTable: "Funerarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAnexos_Responsaveis_ResponsavelId",
                table: "DocumentosAnexos",
                column: "ResponsavelId",
                principalTable: "Responsaveis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAnexos_Funerarias_FunerariaId",
                table: "DocumentosAnexos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAnexos_Responsaveis_ResponsavelId",
                table: "DocumentosAnexos");

            migrationBuilder.DropTable(
                name: "JazigoProprietarios");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosAnexos_FunerariaId",
                table: "DocumentosAnexos");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosAnexos_ResponsavelId",
                table: "DocumentosAnexos");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "EhProprietario",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "FunerariaId",
                table: "DocumentosAnexos");

            migrationBuilder.DropColumn(
                name: "ResponsavelId",
                table: "DocumentosAnexos");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "DocumentosAnexos");

            migrationBuilder.AlterColumn<int>(
                name: "FalecidoId",
                table: "DocumentosAnexos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AtendimentoId",
                table: "DocumentosAnexos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
