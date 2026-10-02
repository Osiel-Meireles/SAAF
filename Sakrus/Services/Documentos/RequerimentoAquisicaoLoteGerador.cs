using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 15 — Requerimento para Aquisição de Lote Cemiterial.
/// Exceção à regra de aquisição por falecimento: o servidor seleciona o enquadramento
/// (70+, paciente terminal, familiar de pessoa já sepultada) e o documento mostra
/// claramente o enquadramento escolhido, sem edição manual de texto.
/// </summary>
public class RequerimentoAquisicaoLoteGerador : GeradorBase
{
    public RequerimentoAquisicaoLoteGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "15";

    public static readonly IReadOnlyList<string> Enquadramentos = new[]
    {
        "Pessoa com mais de 70 anos",
        "Paciente em estado terminal",
        "Familiar de pessoa já sepultada"
    };

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("enquadramento", "Enquadramento", TipoCampo.Selecao, Obrigatorio: true, Opcoes: Enquadramentos),
        new CampoDefinicao("gavetaEscolhida", "Localização do lote pretendido", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "gavetas"),
        new CampoDefinicao("falecidoOrigemId", "Falecido relacionado (origem dos restos mortais)", TipoCampo.Selecao, FonteOpcoes: "falecidos"),
        new CampoDefinicao("localOrigemRestos", "Localização de origem dos restos mortais", TipoCampo.Texto),
        new CampoDefinicao("documentacao", "Documentação apresentada", TipoCampo.TextoLongo)
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var requerente = await DadosComuns.ResponsavelAsync(db, ctx.EntidadeId);
        if (string.IsNullOrWhiteSpace(requerente.Nome))
            throw new DocumentoBloqueadoException("Responsável não encontrado");

        var enquadramento = ctx.Campos.Get("enquadramento");
        Exigir(("Enquadramento", enquadramento));
        if (!Enquadramentos.Contains(enquadramento!, StringComparer.OrdinalIgnoreCase))
            throw new DocumentoBloqueadoException("Enquadramento inválido. Opções: " + string.Join(", ", Enquadramentos));

        var chaveGaveta = ctx.Campos.Get("gavetaEscolhida");
        Exigir(("Localização do lote pretendido", chaveGaveta));
        var gaveta = await GavetaSelecaoService.ResolverLocalAsync(db, chaveGaveta!)
            ?? throw new DocumentoBloqueadoException("Localização do lote pretendido não encontrada");

        var falecidoOrigemId = ctx.Campos.GetInt("falecidoOrigemId");
        FalecidoQualificacao? falecidoOrigem = falecidoOrigemId.HasValue
            ? await DadosComuns.FalecidoAsync(db, falecidoOrigemId.Value)
            : null;
        var localOrigem = Tr(ctx.Campos.Get("localOrigemRestos"));

        return Pdf(ctx, "REQUERIMENTO PARA AQUISIÇÃO DE LOTE CEMITERIAL", col =>
        {
            col.Secao("Enquadramento do pedido");
            col.Item().PaddingTop(6).Column(c =>
            {
                foreach (var opcao in Enquadramentos)
                {
                    var selecionado = string.Equals(opcao, enquadramento, StringComparison.OrdinalIgnoreCase);
                    c.Item().PaddingTop(2).Row(r =>
                    {
                        DocumentoBlocos.Caixa(r, selecionado);
                        r.AutoItem().PaddingLeft(4).Text(opcao).Bold();
                    });
                }
            });

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

            col.Secao("Localização do lote pretendido");
            col.Campos(
                ("Cemitério", gaveta.Cemiterio),
                ("Município", gaveta.Municipio),
                ("Quadra", gaveta.Quadra),
                ("Ala", gaveta.Ala),
                ("Lote", gaveta.Lote),
                ("Gaveta", gaveta.Gaveta),
                ("Tipo/classificação de sepultura/jazigo", gaveta.Classificacao),
                ("Natureza do espaço", gaveta.Tipo == "Particular" ? "Particular" : "Pública"));

            if (falecidoOrigem != null || !string.IsNullOrWhiteSpace(localOrigem))
            {
                col.Secao("Origem dos restos mortais");
                col.Campos(
                    ("Falecido relacionado", falecidoOrigem?.Nome),
                    ("Data do óbito", falecidoOrigem?.DataObito?.ToString("dd/MM/yyyy")),
                    ("Localização de origem", localOrigem));
            }

            var documentacao = Tr(ctx.Campos.Get("documentacao"));
            if (!string.IsNullOrWhiteSpace(documentacao))
            {
                col.Secao("Documentação apresentada");
                col.Paragrafo(documentacao!);
            }

            col.Paragrafo(TextosPadrao.RequerimentoLoteTexto);
            col.Paragrafo(TextosPadrao.RequerimentoAquisicaoCondicoes);

            col.Secao("Uso exclusivo da CAAFE — decisão da Coordenação");
            col.Paragrafo(TextosPadrao.RequerimentoLoteDecisao);

            col.LocalEData(ctx);
            col.BlocoAssinaturas("Assinatura do Responsável (Requerente)", "Coordenador(a) da CAAFE");
            col.AssinaturaCoordenacao(ctx);
        });
    }

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
