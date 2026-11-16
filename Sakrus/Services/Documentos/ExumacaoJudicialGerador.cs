using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 09 — Termo de Exumação por Mandado Judicial.
/// </summary>
public class ExumacaoJudicialGerador : GeradorBase
{
    public ExumacaoJudicialGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "9";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("numeroProcesso", "Número do Processo Judicial", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("autoridadeJuizo", "Autoridade judicial / Juízo competente", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("tipoDocumentoJudicial", "Tipo de documento (Decisão, Despacho, Mandado)", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("dataDecisao", "Data da decisão/mandado", TipoCampo.Data),
        new CampoDefinicao("destinoDeterminado", "Destino determinado dos restos mortais", TipoCampo.Texto, Obrigatorio: true, Ajuda: "Ex: Órgão pericial, outro cemitério, etc."),
        new CampoDefinicao("empresaExecutoraId", "Empresa ou entidade executora", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "empresas"),
        new CampoDefinicao("dataExumacao", "Data da exumação", TipoCampo.Data),
        new CampoDefinicao("responsavelId", "Responsável / Acompanhante", TipoCampo.Selecao, FonteOpcoes: "responsaveis")
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var (falecido, atendimento, responsavelPadrao, localAtual) = await DadosComuns.ResolverFalecidoEOrigemAsync(db, ctx.EntidadeId);

        var responsavelId = ctx.Campos.GetInt("responsavelId") ?? responsavelPadrao?.Id;
        PessoaQualificacao? responsavel = null;
        if (responsavelId.HasValue)
        {
            responsavel = await DadosComuns.ResponsavelAsync(db, responsavelId.Value);
        }

        var empresaId = ctx.Campos.GetInt("empresaExecutoraId");
        var empresa = empresaId.HasValue ? await db.Funerarias.FindAsync(empresaId.Value) : null;

        var numeroProcesso = ctx.Campos.Get("numeroProcesso");
        var autoridadeJuizo = ctx.Campos.Get("autoridadeJuizo");
        var tipoDocJudicial = ctx.Campos.Get("tipoDocumentoJudicial");
        var destinoDeterminado = ctx.Campos.Get("destinoDeterminado");
        Exigir(
            ("Número do Processo Judicial", numeroProcesso),
            ("Autoridade judicial / Juízo competente", autoridadeJuizo),
            ("Tipo de documento judicial", tipoDocJudicial),
            ("Destino determinado", destinoDeterminado),
            ("Empresa ou entidade executora", empresa?.Nome));

        var dataDecisao = ctx.Campos.GetData("dataDecisao");
        var dataExumacao = (ctx.Campos.GetData("dataExumacao") ?? ctx.DataEmissao).ToString("dd/MM/yyyy");

        var falecidoDados = await DadosComuns.FalecidoAsync(db, falecido.Id);

        var textoAutorizacao = TextosPadrao.ExumacaoJudicialAutorizacao
            .Replace("{{processo.numero}}", numeroProcesso);

        return Pdf(ctx, "TERMO DE EXUMAÇÃO POR MANDADO JUDICIAL", col =>
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

            col.TituloBloco("DADOS DA DETERMINAÇÃO JUDICIAL");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Processo nº", numeroProcesso, 6),
                    ("Juízo / Autoridade", autoridadeJuizo, 6)
                },
                new (string, string?, int)[]
                {
                    ("Tipo de ato", tipoDocJudicial, 6),
                    ("Data da decisão/mandado", dataDecisao?.ToString("dd/MM/yyyy") ?? "—", 6)
                });

            col.TituloBloco("LOCAL ATUAL DOS RESTOS MORTAIS");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Localização atual", localAtual, 12) });

            col.TituloBloco("DADOS DA EXECUÇÃO E DESTINO DETERMINADO");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Destino determinado", destinoDeterminado, 12)
                },
                new (string, string?, int)[]
                {
                    ("Data da exumação", dataExumacao, 6),
                    ("Empresa / Entidade executora", empresa?.Nome, 6)
                });

            if (responsavel != null && !string.IsNullOrWhiteSpace(responsavel.Nome))
            {
                col.TituloBloco("RESPONSÁVEL / ACOMPANHANTE QUALIFICADO");
                col.TabelaModelo(
                    new (string, string?, int)[] { ("Nome", responsavel.Nome, 8), ("CPF", responsavel.Cpf, 4) },
                    new (string, string?, int)[] { ("Telefone", responsavel.Telefone, 6), ("Parentesco", responsavel.Parentesco, 6) });
            }

            col.Declaracao(textoAutorizacao);

            col.LocalEData(ctx);
            if (responsavel != null && !string.IsNullOrWhiteSpace(responsavel.Nome))
            {
                col.BlocoAssinaturas("Assinatura do Responsável / Acompanhante", $"Empresa / Entidade Executora ({empresa?.Nome})");
            }
            else
            {
                col.BlocoAssinaturas($"Empresa / Entidade Executora ({empresa?.Nome})");
            }
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
