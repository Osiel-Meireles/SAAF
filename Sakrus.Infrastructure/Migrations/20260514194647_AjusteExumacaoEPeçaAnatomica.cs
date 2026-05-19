using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteExumacaoEPeçaAnatomica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Falecidos_Jazigos_JazigoId",
                table: "Falecidos");

            migrationBuilder.AlterColumn<int>(
                name: "JazigoId",
                table: "Falecidos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsPecaAnatomica",
                table: "Falecidos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Falecidos_Jazigos_JazigoId",
                table: "Falecidos",
                column: "JazigoId",
                principalTable: "Jazigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Falecidos_Jazigos_JazigoId",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "IsPecaAnatomica",
                table: "Falecidos");

            migrationBuilder.AlterColumn<int>(
                name: "JazigoId",
                table: "Falecidos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Falecidos_Jazigos_JazigoId",
                table: "Falecidos",
                column: "JazigoId",
                principalTable: "Jazigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
