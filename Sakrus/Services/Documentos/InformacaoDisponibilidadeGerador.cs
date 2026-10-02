using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 17 — Informação de Disponibilidade e Localização para Sepultamento de Despojos.
/// Vinculado ao responsável (requerente): declara que ele dispõe de local para sepultar os
/// despojos do falecido indicado, servindo de instrução ao procedimento de transladação.
/// </summary>
public class InformacaoDisponibilidadeGerador : GeradorBase
{
    public InformacaoDisponibilidadeGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "17";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("falecidoId", "Falecido", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "falecidos"),
        new CampoDefinicao("gavetaEscolhida", "Local do sepultamento", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "gavetas"),
        new CampoDefinicao("dataSepultamento", "Data prevista do sepultamento", TipoCampo.Data),
        new CampoDefinicao("finalidade", "Finalidade da informação", TipoCampo.Texto)
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var requerente = await DadosComuns.ResponsavelAsync(db, ctx.EntidadeId);
        if (string.IsNullOrWhiteSpace(requerente.Nome))
            throw new DocumentoBloqueadoException("Responsável não encontrado");

        var falecidoId = ctx.Campos.GetInt("falecidoId")
            ?? throw new DocumentoBloqueadoException("Campo obrigatório não informado: Falecido");
        var falecido = await DadosComuns.FalecidoAsync(db, falecidoId);

        var chaveLocal = ctx.Campos.Get("gavetaEscolhida");
        Exigir(("Local do sepultamento", chaveLocal));
        var local = await GavetaSelecaoService.ResolverLocalAsync(db, chaveLocal!)
            ?? throw new DocumentoBloqueadoException("Local do sepultamento não encontrado");

        var dataSepultamento = ctx.Campos.GetData("dataSepultamento");
        var finalidade = Tr(ctx.Campos.Get("finalidade"));

        return Pdf(ctx, "INFORMAÇÃO DE DISPONIBILIDADE E LOCALIZAÇÃO PARA SEPULTAMENTO DE DESPOJOS", col =>
        {
            col.Secao("Dados do requerente");
            col.Campos(
                ("Nome", requerente.Nome),
                ("CPF", requerente.Cpf),
                ("RG", requerente.Rg),
                ("Órgão emissor", requerente.OrgaoEmissor),
                ("Endereço", requerente.Endereco),
                ("Telefone", requerente.Telefone));

            col.Secao("Dados do falecido");
            col.Campos(
                ("Nome", falecido.Nome),
                ("Data do óbito", falecido.DataObito?.ToString("dd/MM/yyyy")),
                ("Local de sepultamento/gaveta", GavetaSelecaoService.DescricaoLocal(local)),
                ("Naturalidade", falecido.Naturalidade),
                ("Nome do pai", falecido.NomePai),
                ("Nome da mãe", falecido.NomeMae));

            col.Secao("Disponibilidade e localização do espaço");
            col.Campos(
                ("Cemitério", local.Cemiterio),
                ("Município", local.Municipio),
                ("Quadra", local.Quadra),
                ("Ala", local.Ala),
                ("Lote", local.Lote),
                ("Gaveta", local.Gaveta),
                ("Tipo/classificação de sepultura/jazigo", local.Classificacao),
                ("Natureza do espaço", local.Tipo == "Particular" ? "Particular" : "Pública"),
                ("Data prevista do sepultamento", dataSepultamento?.ToString("dd/MM/yyyy")),
                ("Finalidade da informação", finalidade));

            col.Paragrafo(TextosPadrao.InformacaoDisponibilidadeRequerente);
            col.Paragrafo(TextosPadrao.InformacaoDisponibilidadeCaafe);

            col.LocalEData(ctx);
            col.BlocoAssinaturas("Assinatura do Responsável (Requerente)", "Assinatura da CAAFE");
            col.AssinaturaCoordenacao(ctx);
        });
    }

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
