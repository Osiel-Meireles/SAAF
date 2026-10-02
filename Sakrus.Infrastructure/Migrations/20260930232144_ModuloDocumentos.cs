using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sakrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModuloDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "Responsaveis",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CemiterioId",
                table: "Jazigos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassificacaoEspacoId",
                table: "Jazigos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Comprimento",
                table: "Jazigos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Largura",
                table: "Jazigos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Revestimento",
                table: "Jazigos",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamento",
                table: "JazigoProprietarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PagamentoConfirmado",
                table: "JazigoProprietarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CemiterioId",
                table: "GavetasPublicas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassificacaoEspacoId",
                table: "GavetasPublicas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Funerarias",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EhExecutora",
                table: "Funerarias",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CartorioRegistro",
                table: "Falecidos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataSepultamento",
                table: "Falecidos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Falecidos",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "Falecidos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalCorpo",
                table: "Falecidos",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatriculaObito",
                table: "Falecidos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipioCartorio",
                table: "Falecidos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Naturalidade",
                table: "Falecidos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeMae",
                table: "Falecidos",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomePai",
                table: "Falecidos",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profissao",
                table: "Falecidos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sexo",
                table: "Falecidos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaExecutoraId",
                table: "Atendimentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrauParentesco",
                table: "Atendimentos",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LiberacaoMunicipal",
                table: "Atendimentos",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoAtendimento",
                table: "Atendimentos",
                type: "text",
                nullable: false,
                defaultValue: "Particular");

            migrationBuilder.CreateTable(
                name: "AssuntosProtocolo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    GeraRegularizacao = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssuntosProtocolo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cemiterios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Municipio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cemiterios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassificacoesEspaco",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Natureza = table.Column<string>(type: "text", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificacoesEspaco", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesInstitucionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeCoordenador = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CargoCoordenador = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NomePresidenteConselho = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MunicipioEmissao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UfEmissao = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CabecalhoLinha1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CabecalhoLinha2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesInstitucionais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LancamentosFinanceiros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JazigoId = table.Column<int>(type: "integer", nullable: false),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    Exercicio = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Pago = table.Column<bool>(type: "boolean", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentosFinanceiros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentosFinanceiros_Jazigos_JazigoId",
                        column: x => x.JazigoId,
                        principalTable: "Jazigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NumerosRegistro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Chave = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EntidadeTipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EntidadeId = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Sequencia = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumerosRegistro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicosAuxilio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicosAuxilio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Motor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TemplateRef = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ExigeNumeroUnico = table.Column<bool>(type: "boolean", nullable: false),
                    GeraNumeroNaPrimeiraEmissao = table.Column<bool>(type: "boolean", nullable: false),
                    ChaveNumeracao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    EntidadeTipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValoresMetroQuadrado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Exercicio = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValoresMetroQuadrado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssuntosProtocoloDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssuntoProtocoloId = table.Column<int>(type: "integer", nullable: false),
                    TipoDocumentoCodigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssuntosProtocoloDocumentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssuntosProtocoloDocumentos_AssuntosProtocolo_AssuntoProtoc~",
                        column: x => x.AssuntoProtocoloId,
                        principalTable: "AssuntosProtocolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Protocolos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Sequencia = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssuntoProtocoloId = table.Column<int>(type: "integer", nullable: false),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: false),
                    JazigoId = table.Column<int>(type: "integer", nullable: true),
                    Observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protocolos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Protocolos_AssuntosProtocolo_AssuntoProtocoloId",
                        column: x => x.AssuntoProtocoloId,
                        principalTable: "AssuntosProtocolo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Protocolos_Jazigos_JazigoId",
                        column: x => x.JazigoId,
                        principalTable: "Jazigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Protocolos_Responsaveis_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Gavetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JazigoId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClassificacaoEspacoId = table.Column<int>(type: "integer", nullable: true),
                    FalecidoId = table.Column<int>(type: "integer", nullable: true),
                    DataSepultamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gavetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gavetas_ClassificacoesEspaco_ClassificacaoEspacoId",
                        column: x => x.ClassificacaoEspacoId,
                        principalTable: "ClassificacoesEspaco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gavetas_Falecidos_FalecidoId",
                        column: x => x.FalecidoId,
                        principalTable: "Falecidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Gavetas_Jazigos_JazigoId",
                        column: x => x.JazigoId,
                        principalTable: "Jazigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AtendimentosServicosAuxilio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AtendimentoId = table.Column<int>(type: "integer", nullable: false),
                    ServicoAuxilioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtendimentosServicosAuxilio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtendimentosServicosAuxilio_Atendimentos_AtendimentoId",
                        column: x => x.AtendimentoId,
                        principalTable: "Atendimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtendimentosServicosAuxilio_ServicosAuxilio_ServicoAuxilioId",
                        column: x => x.ServicoAuxilioId,
                        principalTable: "ServicosAuxilio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosEmitidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoDocumentoId = table.Column<int>(type: "integer", nullable: false),
                    EntidadeTipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EntidadeId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DadosJson = table.Column<string>(type: "text", nullable: false),
                    Versao = table.Column<int>(type: "integer", nullable: false),
                    EmitidoPorId = table.Column<int>(type: "integer", nullable: true),
                    EmitidoPorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EmitidoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PdfCaminho = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosEmitidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosEmitidos_TiposDocumento_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jazigos_CemiterioId",
                table: "Jazigos",
                column: "CemiterioId");

            migrationBuilder.CreateIndex(
                name: "IX_Jazigos_ClassificacaoEspacoId",
                table: "Jazigos",
                column: "ClassificacaoEspacoId");

            migrationBuilder.CreateIndex(
                name: "IX_GavetasPublicas_CemiterioId",
                table: "GavetasPublicas",
                column: "CemiterioId");

            migrationBuilder.CreateIndex(
                name: "IX_GavetasPublicas_ClassificacaoEspacoId",
                table: "GavetasPublicas",
                column: "ClassificacaoEspacoId");

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_EmpresaExecutoraId",
                table: "Atendimentos",
                column: "EmpresaExecutoraId");

            migrationBuilder.CreateIndex(
                name: "IX_AssuntosProtocoloDocumentos_AssuntoProtocoloId_TipoDocument~",
                table: "AssuntosProtocoloDocumentos",
                columns: new[] { "AssuntoProtocoloId", "TipoDocumentoCodigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtendimentosServicosAuxilio_AtendimentoId_ServicoAuxilioId",
                table: "AtendimentosServicosAuxilio",
                columns: new[] { "AtendimentoId", "ServicoAuxilioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtendimentosServicosAuxilio_ServicoAuxilioId",
                table: "AtendimentosServicosAuxilio",
                column: "ServicoAuxilioId");

            migrationBuilder.CreateIndex(
                name: "IX_Cemiterios_Nome",
                table: "Cemiterios",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosEmitidos_TipoDocumentoId_EntidadeTipo_EntidadeId",
                table: "DocumentosEmitidos",
                columns: new[] { "TipoDocumentoId", "EntidadeTipo", "EntidadeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Gavetas_ClassificacaoEspacoId",
                table: "Gavetas",
                column: "ClassificacaoEspacoId");

            migrationBuilder.CreateIndex(
                name: "IX_Gavetas_FalecidoId",
                table: "Gavetas",
                column: "FalecidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Gavetas_JazigoId_Numero",
                table: "Gavetas",
                columns: new[] { "JazigoId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_JazigoId",
                table: "LancamentosFinanceiros",
                column: "JazigoId");

            migrationBuilder.CreateIndex(
                name: "IX_NumerosRegistro_Chave_Ano_Sequencia",
                table: "NumerosRegistro",
                columns: new[] { "Chave", "Ano", "Sequencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumerosRegistro_Chave_EntidadeTipo_EntidadeId",
                table: "NumerosRegistro",
                columns: new[] { "Chave", "EntidadeTipo", "EntidadeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Protocolos_Ano_Sequencia",
                table: "Protocolos",
                columns: new[] { "Ano", "Sequencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Protocolos_AssuntoProtocoloId",
                table: "Protocolos",
                column: "AssuntoProtocoloId");

            migrationBuilder.CreateIndex(
                name: "IX_Protocolos_JazigoId",
                table: "Protocolos",
                column: "JazigoId");

            migrationBuilder.CreateIndex(
                name: "IX_Protocolos_Numero",
                table: "Protocolos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Protocolos_ResponsavelId",
                table: "Protocolos",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicosAuxilio_Codigo",
                table: "ServicosAuxilio",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumento_Codigo",
                table: "TiposDocumento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValoresMetroQuadrado_Exercicio",
                table: "ValoresMetroQuadrado",
                column: "Exercicio",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Atendimentos_Funerarias_EmpresaExecutoraId",
                table: "Atendimentos",
                column: "EmpresaExecutoraId",
                principalTable: "Funerarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GavetasPublicas_Cemiterios_CemiterioId",
                table: "GavetasPublicas",
                column: "CemiterioId",
                principalTable: "Cemiterios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GavetasPublicas_ClassificacoesEspaco_ClassificacaoEspacoId",
                table: "GavetasPublicas",
                column: "ClassificacaoEspacoId",
                principalTable: "ClassificacoesEspaco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jazigos_Cemiterios_CemiterioId",
                table: "Jazigos",
                column: "CemiterioId",
                principalTable: "Cemiterios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jazigos_ClassificacoesEspaco_ClassificacaoEspacoId",
                table: "Jazigos",
                column: "ClassificacaoEspacoId",
                principalTable: "ClassificacoesEspaco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atendimentos_Funerarias_EmpresaExecutoraId",
                table: "Atendimentos");

            migrationBuilder.DropForeignKey(
                name: "FK_GavetasPublicas_Cemiterios_CemiterioId",
                table: "GavetasPublicas");

            migrationBuilder.DropForeignKey(
                name: "FK_GavetasPublicas_ClassificacoesEspaco_ClassificacaoEspacoId",
                table: "GavetasPublicas");

            migrationBuilder.DropForeignKey(
                name: "FK_Jazigos_Cemiterios_CemiterioId",
                table: "Jazigos");

            migrationBuilder.DropForeignKey(
                name: "FK_Jazigos_ClassificacoesEspaco_ClassificacaoEspacoId",
                table: "Jazigos");

            migrationBuilder.DropTable(
                name: "AssuntosProtocoloDocumentos");

            migrationBuilder.DropTable(
                name: "AtendimentosServicosAuxilio");

            migrationBuilder.DropTable(
                name: "Cemiterios");

            migrationBuilder.DropTable(
                name: "ConfiguracoesInstitucionais");

            migrationBuilder.DropTable(
                name: "DocumentosEmitidos");

            migrationBuilder.DropTable(
                name: "Gavetas");

            migrationBuilder.DropTable(
                name: "LancamentosFinanceiros");

            migrationBuilder.DropTable(
                name: "NumerosRegistro");

            migrationBuilder.DropTable(
                name: "Protocolos");

            migrationBuilder.DropTable(
                name: "ValoresMetroQuadrado");

            migrationBuilder.DropTable(
                name: "ServicosAuxilio");

            migrationBuilder.DropTable(
                name: "TiposDocumento");

            migrationBuilder.DropTable(
                name: "ClassificacoesEspaco");

            migrationBuilder.DropTable(
                name: "AssuntosProtocolo");

            migrationBuilder.DropIndex(
                name: "IX_Jazigos_CemiterioId",
                table: "Jazigos");

            migrationBuilder.DropIndex(
                name: "IX_Jazigos_ClassificacaoEspacoId",
                table: "Jazigos");

            migrationBuilder.DropIndex(
                name: "IX_GavetasPublicas_CemiterioId",
                table: "GavetasPublicas");

            migrationBuilder.DropIndex(
                name: "IX_GavetasPublicas_ClassificacaoEspacoId",
                table: "GavetasPublicas");

            migrationBuilder.DropIndex(
                name: "IX_Atendimentos_EmpresaExecutoraId",
                table: "Atendimentos");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Responsaveis");

            migrationBuilder.DropColumn(
                name: "CemiterioId",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "ClassificacaoEspacoId",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "Comprimento",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "Largura",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "Revestimento",
                table: "Jazigos");

            migrationBuilder.DropColumn(
                name: "DataPagamento",
                table: "JazigoProprietarios");

            migrationBuilder.DropColumn(
                name: "PagamentoConfirmado",
                table: "JazigoProprietarios");

            migrationBuilder.DropColumn(
                name: "CemiterioId",
                table: "GavetasPublicas");

            migrationBuilder.DropColumn(
                name: "ClassificacaoEspacoId",
                table: "GavetasPublicas");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Funerarias");

            migrationBuilder.DropColumn(
                name: "EhExecutora",
                table: "Funerarias");

            migrationBuilder.DropColumn(
                name: "CartorioRegistro",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "DataSepultamento",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "LocalCorpo",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "MatriculaObito",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "MunicipioCartorio",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "Naturalidade",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "NomeMae",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "NomePai",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "Profissao",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "Sexo",
                table: "Falecidos");

            migrationBuilder.DropColumn(
                name: "EmpresaExecutoraId",
                table: "Atendimentos");

            migrationBuilder.DropColumn(
                name: "GrauParentesco",
                table: "Atendimentos");

            migrationBuilder.DropColumn(
                name: "LiberacaoMunicipal",
                table: "Atendimentos");

            migrationBuilder.DropColumn(
                name: "TipoAtendimento",
                table: "Atendimentos");
        }
    }
}
