using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusFalecidoEDocumentosAnexos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ocupado",
                table: "Ossuarios");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Falecidos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DocumentosAnexos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AtendimentoId = table.Column<int>(type: "integer", nullable: false),
                    FalecidoId = table.Column<int>(type: "integer", nullable: false),
                    NomeArquivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    CaminhoArquivo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    DataAnexo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosAnexos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosAnexos_Atendimentos_AtendimentoId",
                        column: x => x.AtendimentoId,
                        principalTable: "Atendimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosAnexos_Falecidos_FalecidoId",
                        column: x => x.FalecidoId,
                        principalTable: "Falecidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_DataSepultamento",
                table: "Atendimentos",
                column: "DataSepultamento");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAnexos_AtendimentoId",
                table: "DocumentosAnexos",
                column: "AtendimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAnexos_FalecidoId",
                table: "DocumentosAnexos",
                column: "FalecidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosAnexos");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Atendimentos_DataSepultamento",
                table: "Atendimentos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Falecidos");

            migrationBuilder.AddColumn<bool>(
                name: "Ocupado",
                table: "Ossuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
