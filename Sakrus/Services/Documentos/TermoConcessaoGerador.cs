using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 12 — Termo de Concessão de Espaço Cemiterial.
/// </summary>
public class TermoConcessaoGerador : GeradorBase
{
    public TermoConcessaoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "12";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("revestimento", "Revestimento", TipoCampo.Texto, Ajuda: "Ex: Granito, Cerâmica, Alvenaria"),
        new CampoDefinicao("dataAssinatura", "Data da assinatura", TipoCampo.Data),
        new CampoDefinicao("testemunha1", "Testemunha 1 — nome e CPF", TipoCampo.Texto),
        new CampoDefinicao("testemunha2", "Testemunha 2 — nome e CPF", TipoCampo.Texto)
    };

    public override async Task<string?> ValidarPreCondicoesAsync(int entidadeId)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var jazigo = await db.Jazigos
            .Include(j => j.Proprietarios)
            .FirstOrDefaultAsync(j => j.Id == entidadeId);

        if (jazigo == null)
            return "Jazigo não encontrado";

        if (DadosComuns.TitularAtivo(jazigo) == null)
            return "O jazigo não possui titular ativo cadastrado";

        var temConcessaoPendente = await db.LancamentosFinanceiros
            .AnyAsync(l => l.JazigoId == entidadeId && l.Tipo == TipoLancamento.Concessao && !l.Pago);

        if (temConcessaoPendente)
            return "O Termo de Concessão só pode ser emitido após a quitação integral da Taxa de Concessão.";

        return null;
    }

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var jazigo = await DadosComuns.JazigoCompletoAsync(db, ctx.EntidadeId);
        var titular = DadosComuns.TitularAtivo(jazigo);
        if (titular == null)
            throw new DocumentoBloqueadoException("O jazigo não possui titular ativo cadastrado");

        var concessionario = DadosComuns.Qualificacao(titular);
        var revestimento = ctx.Campos.Get("revestimento") ?? "Padrão Cemiterial";
        var dataAssinatura = ctx.Campos.GetData("dataAssinatura") ?? ctx.DataEmissao;

        var dimensoes = (jazigo.Largura.HasValue && jazigo.Comprimento.HasValue)
            ? $"{jazigo.Largura:0.##}m x {jazigo.Comprimento:0.##}m"
            : "—";

        return Pdf(ctx, "TERMO DE CONCESSÃO DE ESPAÇO CEMITERIAL", col =>
        {
            col.TituloBloco("QUALIFICAÇÃO DO(A) CONCESSIONÁRIO(A)");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome do Concessionário", concessionario.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("CPF", concessionario.Cpf, 4), ("RG", concessionario.Rg, 4), ("Órgão emissor", concessionario.OrgaoEmissor, 4)
                },
                new (string, string?, int)[]
                {
                    ("Endereço", concessionario.Endereco, 8), ("Telefone", concessionario.Telefone, 4)
                });

            col.TituloBloco("IDENTIFICAÇÃO DO ESPAÇO CEMITERIAL");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Cemitério", jazigo.Cemiterio?.Nome, 6),
                    ("Classificação", jazigo.ClassificacaoEspaco?.Nome ?? "Jazigo Particular", 6)
                },
                new (string, string?, int)[]
                {
                    ("Quadra", jazigo.Quadra, 4), ("Ala", jazigo.Ala ?? "—", 4), ("Lote nº", jazigo.NumeroLote, 4)
                },
                new (string, string?, int)[]
                {
                    ("Dimensões", dimensoes, 4),
                    ("Área total", DadosComuns.FormatarArea(jazigo.AreaM2), 4),
                    ("Revestimento", revestimento, 4)
                });

            col.TituloBloco("CLÁUSULAS E CONDIÇÕES DA CONCESSÃO");
            col.Paragrafo(
                "O Município de Luís Eduardo Magalhães - BA, por intermédio da Coordenação de Administração e Apoio Funerário e Cemiterial (CAAFE), " +
                "outorga ao(à) Concessionário(a) acima qualificado(a) a concessão de uso do espaço cemiterial identificado neste termo, " +
                "em conformidade com as disposições da Lei Municipal nº 877/2019 e suas alterações posteriores.");

            col.Paragrafo(
                "O(A) Concessionário(a) declara estar ciente de suas obrigações relativas à conservação, segurança e limpeza do jazigo, " +
                "comprometendo-se a manter em dia as taxas anuais de manutenção cemiterial previstas na legislação, " +
                "sob pena de aplicação das sanções legais cabíveis.");

            col.LocalEData(new ContextoEmissao(ctx.EntidadeId, ctx.Campos, ctx.Instituicao, ctx.Numero, dataAssinatura));

            col.BlocoAssinaturas($"Concessionário(a) — {concessionario.Nome}");

            col.AssinaturasDiretoria(ctx);

            col.BlocoAssinaturas("Testemunha 1", "Testemunha 2");
            col.Campos(
                ("Testemunha 1", ctx.Campos.Get("testemunha1")),
                ("Testemunha 2", ctx.Campos.Get("testemunha2")));
        });
    }
}
