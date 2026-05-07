using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NovasRegras_Jazigos_Financas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exumacoes");

            migrationBuilder.DropTable(
                name: "Obitos");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Capelas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Capelas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "ConfiguracoesFinanceiras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ValorMetroQuadrado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TaxaManutencaoBase = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TaxaConcessaoBase = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesFinanceiras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Falecidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CausaDaMorte = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Falecidos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GavetasPublicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Setor = table.Column<string>(type: "text", nullable: false),
                    Quadra = table.Column<string>(type: "text", nullable: false),
                    Lote = table.Column<string>(type: "text", nullable: false),
                    NumeroGaveta = table.Column<string>(type: "text", nullable: false),
                    Ocupada = table.Column<bool>(type: "boolean", nullable: false),
                    FalecidoId = table.Column<int>(type: "integer", nullable: true),
                    DataOcupacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataPrevisaoExumacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GavetasPublicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelosJazigos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    PercentualConcessao = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PercentualManutencao = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxaConstrucao = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelosJazigos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosCapela",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapelaId = table.Column<int>(type: "integer", nullable: false),
                    AtendimentoId = table.Column<int>(type: "integer", nullable: false),
                    HoraEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HoraSaida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosCapela", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Responsaveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    CPF = table.Column<string>(type: "text", nullable: false),
                    RG = table.Column<string>(type: "text", nullable: false),
                    OrgaoEmissor = table.Column<string>(type: "text", nullable: false),
                    Profissao = table.Column<string>(type: "text", nullable: false),
                    Endereco = table.Column<string>(type: "text", nullable: false),
                    Telefone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsaveis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jazigos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodigoIdentificador = table.Column<string>(type: "text", nullable: false),
                    ModeloJazigoId = table.Column<int>(type: "integer", nullable: false),
                    IsInfantil = table.Column<bool>(type: "boolean", nullable: false),
                    Ocupado = table.Column<bool>(type: "boolean", nullable: false),
                    JazigoPaiId = table.Column<int>(type: "integer", nullable: true),
                    CoordenadasMapa = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jazigos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jazigos_Jazigos_JazigoPaiId",
                        column: x => x.JazigoPaiId,
                        principalTable: "Jazigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jazigos_ModelosJazigos_ModeloJazigoId",
                        column: x => x.ModeloJazigoId,
                        principalTable: "ModelosJazigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Atendimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: false),
                    FalecidoId = table.Column<int>(type: "integer", nullable: false),
                    Perfil = table.Column<string>(type: "text", nullable: false),
                    Origem = table.Column<string>(type: "text", nullable: false),
                    Procedimento = table.Column<string>(type: "text", nullable: false),
                    NumeroOsAuxilio = table.Column<string>(type: "text", nullable: false),
                    NumeroDeclaracaoObito = table.Column<string>(type: "text", nullable: false),
                    LocalFalecimento = table.Column<string>(type: "text", nullable: false),
                    LocalSepultamento = table.Column<string>(type: "text", nullable: false),
                    DataSepultamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HorarioSepultamento = table.Column<TimeSpan>(type: "interval", nullable: true),
                    PecaAnatomicaIdentificacao = table.Column<string>(type: "text", nullable: true),
                    PecaAnatomicaDataCirurgia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PecaAnatomicaHospital = table.Column<string>(type: "text", nullable: true),
                    PecaAnatomicaCidadeOrigem = table.Column<string>(type: "text", nullable: true),
                    PecaAnatomicaEstadoOrigem = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atendimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Atendimentos_Falecidos_FalecidoId",
                        column: x => x.FalecidoId,
                        principalTable: "Falecidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Atendimentos_Responsaveis_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExumacoesRegistros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FalecidoId = table.Column<int>(type: "integer", nullable: false),
                    GavetaPublicaId = table.Column<int>(type: "integer", nullable: true),
                    JazigoId = table.Column<int>(type: "integer", nullable: true),
                    DataAutorizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SetorAutorizador = table.Column<string>(type: "text", nullable: false),
                    DataExecucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Executor = table.Column<string>(type: "text", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExumacoesRegistros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExumacoesRegistros_Falecidos_FalecidoId",
                        column: x => x.FalecidoId,
                        principalTable: "Falecidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExumacoesRegistros_GavetasPublicas_GavetaPublicaId",
                        column: x => x.GavetaPublicaId,
                        principalTable: "GavetasPublicas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExumacoesRegistros_Jazigos_JazigoId",
                        column: x => x.JazigoId,
                        principalTable: "Jazigos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItensFaturados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AtendimentoId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaItem = table.Column<string>(type: "text", nullable: false),
                    QuantidadeOuKm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ValorTotalCalculado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    AbatidoDoEstoque = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensFaturados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensFaturados_Atendimentos_AtendimentoId",
                        column: x => x.AtendimentoId,
                        principalTable: "Atendimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_FalecidoId",
                table: "Atendimentos",
                column: "FalecidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_ResponsavelId",
                table: "Atendimentos",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_ExumacoesRegistros_FalecidoId",
                table: "ExumacoesRegistros",
                column: "FalecidoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExumacoesRegistros_GavetaPublicaId",
                table: "ExumacoesRegistros",
                column: "GavetaPublicaId");

            migrationBuilder.CreateIndex(
                name: "IX_ExumacoesRegistros_JazigoId",
                table: "ExumacoesRegistros",
                column: "JazigoId");

            migrationBuilder.CreateIndex(
                name: "IX_GavetasPublicas_Ocupada",
                table: "GavetasPublicas",
                column: "Ocupada");

            migrationBuilder.CreateIndex(
                name: "IX_GavetasPublicas_Setor_Quadra_Lote_NumeroGaveta",
                table: "GavetasPublicas",
                columns: new[] { "Setor", "Quadra", "Lote", "NumeroGaveta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensFaturados_AtendimentoId",
                table: "ItensFaturados",
                column: "AtendimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jazigos_JazigoPaiId",
                table: "Jazigos",
                column: "JazigoPaiId");

            migrationBuilder.CreateIndex(
                name: "IX_Jazigos_ModeloJazigoId",
                table: "Jazigos",
                column: "ModeloJazigoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesFinanceiras");

            migrationBuilder.DropTable(
                name: "ExumacoesRegistros");

            migrationBuilder.DropTable(
                name: "ItensFaturados");

            migrationBuilder.DropTable(
                name: "RegistrosCapela");

            migrationBuilder.DropTable(
                name: "GavetasPublicas");

            migrationBuilder.DropTable(
                name: "Jazigos");

            migrationBuilder.DropTable(
                name: "Atendimentos");

            migrationBuilder.DropTable(
                name: "ModelosJazigos");

            migrationBuilder.DropTable(
                name: "Falecidos");

            migrationBuilder.DropTable(
                name: "Responsaveis");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Usuarios",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Capelas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Capelas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "Obitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapelaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioCadastroId = table.Column<int>(type: "integer", nullable: false),
                    CausaMortis = table.Column<string>(type: "text", nullable: false),
                    DataObito = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NomeFalecido = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Obitos_Capelas_CapelaId",
                        column: x => x.CapelaId,
                        principalTable: "Capelas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Obitos_Usuarios_UsuarioCadastroId",
                        column: x => x.UsuarioCadastroId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exumacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObitoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioResponsavelId = table.Column<int>(type: "integer", nullable: false),
                    DataPrevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataRealizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocalDestino = table.Column<string>(type: "text", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exumacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exumacoes_Obitos_ObitoId",
                        column: x => x.ObitoId,
                        principalTable: "Obitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exumacoes_Usuarios_UsuarioResponsavelId",
                        column: x => x.UsuarioResponsavelId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exumacoes_ObitoId",
                table: "Exumacoes",
                column: "ObitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Exumacoes_UsuarioResponsavelId",
                table: "Exumacoes",
                column: "UsuarioResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Obitos_CapelaId",
                table: "Obitos",
                column: "CapelaId");

            migrationBuilder.CreateIndex(
                name: "IX_Obitos_UsuarioCadastroId",
                table: "Obitos",
                column: "UsuarioCadastroId");
        }
    }
}
