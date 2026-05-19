using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustandoCausaMorteEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CausaDaMorte",
                table: "Falecidos",
                newName: "Nome");

            migrationBuilder.AddColumn<string>(
                name: "CausaMorte",
                table: "Falecidos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Falecidos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFalecimento",
                table: "Falecidos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "JazigoId",
                table: "Falecidos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Falecidos_JazigoId",
                table: "Falecidos",
                column: "JazigoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Falecidos_Jazigos_JazigoId",
                table: "Falecidos",
                column: "JazigoId",
                principalTable: "Jazigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Falecidos_Jazigos_JazigoId",
                table: "Falecidos");

            migrationBuilder.DropIndex(
                name: "IX_Falecidos_JazigoId",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "CausaMorte",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "DataFalecimento",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "JazigoId",
                table: "Falecidos");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Falecidos",
                newName: "CausaDaMorte");
        }
    }
}
