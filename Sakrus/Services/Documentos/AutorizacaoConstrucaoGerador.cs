using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 16 — Autorização para Construção ou Reforma de Jazigo.
/// Finalidade é lista fechada (não campo livre). Mostra a composição atual do jazigo
/// (gaveta e situação/sepultado), o revestimento relacionado à obra e a empresa executora
/// selecionada do cadastro — o nome da empresa nunca fica fixo no modelo.
/// </summary>
public class AutorizacaoConstrucaoGerador : GeradorBase
{
    public AutorizacaoConstrucaoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "16";

    public static readonly IReadOnlyList<string> Finalidades = new[]
    {
        "Construção de gaveta",
        "Construção de carneiro",
        "Ampliação",
        "Reforma",
        "Manutenção",
        "Aplicação/alteração de revestimento"
    };

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("finalidade", "Finalidade da autorização", TipoCampo.Selecao, Obrigatorio: true, Opcoes: Finalidades),
        new CampoDefinicao("empresaExecutoraId", "Empresa responsável pela execução", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "empresas"),
        new CampoDefinicao("revestimento", "Revestimento relacionado à obra", TipoCampo.Texto),
        new CampoDefinicao("projetoAnexo", "Projeto anexo", TipoCampo.SimNao),
        new CampoDefinicao("descricaoObra", "Descrição complementar da intervenção", TipoCampo.TextoLongo)
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

        return null;
    }

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var jazigo = await DadosComuns.JazigoCompletoAsync(db, ctx.EntidadeId);
        var requerente = DadosComuns.Qualificacao(DadosComuns.TitularAtivo(jazigo));
        if (string.IsNullOrWhiteSpace(requerente.Nome))
            throw new DocumentoBloqueadoException("O jazigo não possui titular ativo cadastrado");

        var finalidade = ctx.Campos.Get("finalidade");
        Exigir(("Finalidade da autorização", finalidade));
        if (!Finalidades.Contains(finalidade!, StringComparer.OrdinalIgnoreCase))
            throw new DocumentoBloqueadoException("Finalidade inválida. Opções: " + string.Join(", ", Finalidades));

        var empresaId = ctx.Campos.GetInt("empresaExecutoraId")
            ?? throw new DocumentoBloqueadoException("Campo obrigatório não informado: Empresa responsável pela execução");
        var empresa = await db.Funerarias.FindAsync(empresaId)
            ?? throw new DocumentoBloqueadoException("Empresa responsável pela execução não encontrada");

        var revestimento = Tr(ctx.Campos.Get("revestimento"));
        var descricaoObra = Tr(ctx.Campos.Get("descricaoObra"));
        var temProjeto = ctx.Campos.GetBool("projetoAnexo");

        var linhasGavetas = FolhaRostoGerador.MontarLinhas(jazigo)
            .Select(l => (IReadOnlyList<string>)new[] { l.Numero, l.Falecido })
            .ToList();

        return Pdf(ctx, "AUTORIZAÇÃO PARA CONSTRUÇÃO OU REFORMA DE JAZIGO", col =>
        {
            col.Secao("Dados do requerente");
            col.Campos(
                ("Nome", requerente.Nome),
                ("CPF", requerente.Cpf),
                ("RG", requerente.Rg),
                ("Órgão emissor", requerente.OrgaoEmissor),
                ("Estado civil", requerente.EstadoCivil),
                ("Profissão", requerente.Profissao),
                ("Endereço", requerente.Endereco),
                ("Telefone", requerente.Telefone));

            FinanceiroJazigoComum.RenderIdentificacaoLote(col, jazigo, null);

            col.Secao("Composição atual do jazigo");
            col.Tabela(new[] { "Gaveta", "Situação" }, linhasGavetas);

            col.Secao("Finalidade da autorização");
            col.Item().PaddingTop(6).Column(c =>
            {
                foreach (var opcao in Finalidades)
                {
                    var selecionado = string.Equals(opcao, finalidade, StringComparison.OrdinalIgnoreCase);
                    c.Item().PaddingTop(2).Row(r =>
                    {
                        DocumentoBlocos.Caixa(r, selecionado);
                        r.AutoItem().PaddingLeft(4).Text(opcao).Bold();
                    });
                }
            });

            col.Secao("Empresa responsável pela execução");
            col.Campos(("Empresa", empresa.Nome), ("Revestimento relacionado à obra", revestimento));

            if (!string.IsNullOrWhiteSpace(descricaoObra))
            {
                col.Secao("Descrição da intervenção");
                col.Paragrafo(descricaoObra!);
            }

            col.Paragrafo(TextosPadrao.AutorizacaoConstrucaoTexto);
            if (temProjeto)
                col.Paragrafo("A intervenção foi apresentada com projeto anexo, que passa a integrar a presente autorização.");
            col.Paragrafo(TextosPadrao.AutorizacaoConstrucaoCondicoes);

            col.LocalEData(ctx);
            col.BlocoAssinaturas(
                $"Assinatura do Responsável — {requerente.Nome}",
                $"Empresa executora — {empresa.Nome}");
            col.AssinaturaPresidenteConselho(ctx);
            col.AssinaturaCoordenacao(ctx);
        });
    }

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
