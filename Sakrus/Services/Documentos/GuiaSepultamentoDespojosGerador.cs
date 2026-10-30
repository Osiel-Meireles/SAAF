using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 10 — Guia de Sepultamento de Despojos.
/// </summary>
public class GuiaSepultamentoDespojosGerador : GeradorBase
{
    public GuiaSepultamentoDespojosGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "10";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("empresaExecutoraId", "Empresa executora", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "empresas"),
        new CampoDefinicao("cemiterioOrigem", "Cemitério de origem", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("municipioOrigem", "Município de origem", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("numeroAutorizacaoTransladacao", "Nº da Autorização de Transladação (Doc 10-A)", TipoCampo.Texto),
        new CampoDefinicao("dataAutorizacaoTransladacao", "Data da Autorização de Transladação", TipoCampo.Data),
        new CampoDefinicao("cemiterioDestino", "Cemitério de destino (LEM)", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("quadra", "Quadra", TipoCampo.Texto),
        new CampoDefinicao("ala", "Ala", TipoCampo.Texto),
        new CampoDefinicao("lote", "Lote", TipoCampo.Texto),
        new CampoDefinicao("gaveta", "Gaveta", TipoCampo.Texto),
        new CampoDefinicao("tipoSepultura", "Tipo/Classificação da sepultura", TipoCampo.Texto),
        new CampoDefinicao("dataSepultamento", "Data do novo sepultamento", TipoCampo.Data),
        new CampoDefinicao("horarioSepultamento", "Horário do novo sepultamento", TipoCampo.Hora),
        new CampoDefinicao("responsavelId", "Responsável / Requerente", TipoCampo.Selecao, FonteOpcoes: "responsaveis")
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var (falecido, atendimento, responsavelPadrao, _) = await DadosComuns.ResolverFalecidoEOrigemAsync(db, ctx.EntidadeId);

        var responsavelId = ctx.Campos.GetInt("responsavelId") ?? responsavelPadrao?.Id;
        PessoaQualificacao? responsavel = null;
        if (responsavelId.HasValue)
        {
            responsavel = await DadosComuns.ResponsavelAsync(db, responsavelId.Value);
        }

        var empresaId = ctx.Campos.GetInt("empresaExecutoraId");
        var empresa = empresaId.HasValue ? await db.Funerarias.FindAsync(empresaId.Value) : null;

        var cemiterioOrigem = ctx.Campos.Get("cemiterioOrigem");
        var municipioOrigem = ctx.Campos.Get("municipioOrigem");
        var cemiterioDestino = ctx.Campos.Get("cemiterioDestino");

        Exigir(
            ("Empresa executora", empresa?.Nome),
            ("Cemitério de origem", cemiterioOrigem),
            ("Município de origem", municipioOrigem),
            ("Cemitério de destino", cemiterioDestino));

        var numAuto = ctx.Campos.Get("numeroAutorizacaoTransladacao") ?? ctx.Numero ?? "—";
        var dataAuto = ctx.Campos.GetData("dataAutorizacaoTransladacao");

        var quadra = ctx.Campos.Get("quadra");
        var ala = ctx.Campos.Get("ala");
        var lote = ctx.Campos.Get("lote");
        var gaveta = ctx.Campos.Get("gaveta");
        var tipoSepultura = ctx.Campos.Get("tipoSepultura");

        var dataSepultamento = ctx.Campos.GetData("dataSepultamento") ?? ctx.DataEmissao;
        var horaSepultamento = ctx.Campos.GetHora("horarioSepultamento");
        var dataHoraSepultamento = horaSepultamento.HasValue
            ? $"{dataSepultamento:dd/MM/yyyy} às {horaSepultamento.Value:hh\\:mm}"
            : dataSepultamento.ToString("dd/MM/yyyy");

        var falecidoDados = await DadosComuns.FalecidoAsync(db, falecido.Id);

        var textoEmpresa = TextosPadrao.GuiaSepultamentoDespojosAutorizacaoEmpresa
            .Replace("{{empresa.nome}}", empresa?.Nome ?? "[EMPRESA EXECUTORA]");

        return Pdf(ctx, "GUIA DE SEPULTAMENTO DE DESPOJOS", col =>
        {
            col.TituloBloco("DADOS DO FALECIDO E REGISTRO CIVIL");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome", falecidoDados.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("Sexo", falecidoDados.Sexo, 2), ("Idade", falecidoDados.Idade?.ToString(), 2),
                    ("Data do óbito", falecidoDados.DataObito?.ToString("dd/MM/yyyy"), 4),
                    ("Nº da D.O.", falecidoDados.NumeroDeclaracaoObito, 4)
                },
                new (string, string?, int)[]
                {
                    ("Cartório de Registro Civil", falecidoDados.Cartorio, 5),
                    ("Município do cartório", falecidoDados.MunicipioCartorio, 3),
                    ("Matrícula de óbito", falecidoDados.MatriculaObito, 4)
                });

            col.TituloBloco("DADOS DO RESPONSÁVEL / REQUERENTE");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome", responsavel?.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("CPF", responsavel?.Cpf, 4), ("RG", responsavel?.Rg, 4), ("Parentesco", responsavel?.Parentesco, 4)
                },
                new (string, string?, int)[]
                {
                    ("Endereço", responsavel?.Endereco, 6), ("Telefone", responsavel?.Telefone, 6)
                });

            col.TituloBloco("ORIGEM DOS DESPOJOS E AUTORIZAÇÃO DE TRANSLADAÇÃO");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Cemitério de origem", cemiterioOrigem, 6),
                    ("Município de origem", municipioOrigem, 6)
                },
                new (string, string?, int)[]
                {
                    ("Nº Autorização de Transladação", numAuto, 6),
                    ("Data da Autorização", dataAuto?.ToString("dd/MM/yyyy") ?? "—", 6)
                });

            col.TituloBloco("LOCAL DO NOVO SEPULTAMENTO (LUÍS EDUARDO MAGALHÃES)");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Cemitério", cemiterioDestino, 6),
                    ("Tipo de sepultura", tipoSepultura ?? "Despojos", 6)
                },
                new (string, string?, int)[]
                {
                    ("Quadra", quadra, 3), ("Ala", ala, 3), ("Lote", lote, 3), ("Gaveta", gaveta, 3)
                },
                new (string, string?, int)[]
                {
                    ("Data e horário do sepultamento", dataHoraSepultamento, 6),
                    ("Empresa responsável pelo sepultamento", empresa?.Nome, 6)
                });

            col.Declaracao(TextosPadrao.GuiaSepultamentoDespojosDeclaracao);
            col.Paragrafo(textoEmpresa);

            col.LocalEData(ctx);
            col.BlocoAssinaturas("Assinatura do Responsável / Requerente", $"Empresa Responsável ({empresa?.Nome})");
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
