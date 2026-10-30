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
        new CampoDefinicao("nomeFalecido", "Nome do falecido", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("dataObito", "Data do óbito", TipoCampo.Data),
        new CampoDefinicao("cemiterio", "Cemitério", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("municipio", "Município", TipoCampo.Texto),
        new CampoDefinicao("quadra", "Quadra", TipoCampo.Texto),
        new CampoDefinicao("ala", "Ala", TipoCampo.Texto),
        new CampoDefinicao("lote", "Lote", TipoCampo.Texto),
        new CampoDefinicao("gaveta", "Gaveta", TipoCampo.Texto),
        new CampoDefinicao("classificacao", "Tipo/classificação de sepultura/jazigo", TipoCampo.Texto),
        new CampoDefinicao("finalidade", "Finalidade da informação", TipoCampo.Texto)
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var requerente = await DadosComuns.ResponsavelAsync(db, ctx.EntidadeId);
        if (string.IsNullOrWhiteSpace(requerente.Nome))
            throw new DocumentoBloqueadoException("Responsável não encontrado");

        var nomeFalecido = ctx.Campos.Get("nomeFalecido");
        Exigir(("Nome do falecido", nomeFalecido));

        var cemiterio = Tr(ctx.Campos.Get("cemiterio"));
        Exigir(("Cemitério", cemiterio));

        var dataObito = ctx.Campos.GetData("dataObito");
        var municipio = Tr(ctx.Campos.Get("municipio"));
        var quadra = Tr(ctx.Campos.Get("quadra"));
        var ala = Tr(ctx.Campos.Get("ala"));
        var lote = Tr(ctx.Campos.Get("lote"));
        var gaveta = Tr(ctx.Campos.Get("gaveta"));
        var classificacao = Tr(ctx.Campos.Get("classificacao"));
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
                ("Nome", nomeFalecido),
                ("Data do óbito", dataObito?.ToString("dd/MM/yyyy")));

            col.Secao("Disponibilidade e localização do espaço");
            col.Campos(
                ("Cemitério", cemiterio),
                ("Município", municipio),
                ("Quadra", quadra),
                ("Ala", ala),
                ("Lote", lote),
                ("Gaveta", gaveta),
                ("Tipo/classificação de sepultura/jazigo", classificacao),
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
