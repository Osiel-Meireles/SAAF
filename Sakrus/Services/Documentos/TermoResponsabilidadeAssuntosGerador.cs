using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 11 — Termo de Responsabilidade para Tratar de Assuntos Funerários.
/// Vinculado ao jazigo: qualifica o titular/autorizante pelo cadastro e a pessoa autorizada
/// selecionada, descreve o objeto da autorização e reproduz nomes/CPFs nos campos de assinatura.
/// </summary>
public class TermoResponsabilidadeAssuntosGerador : GeradorBase
{
    public TermoResponsabilidadeAssuntosGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "11";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("pessoaAutorizadaId", "Pessoa autorizada", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "responsaveis"),
        new CampoDefinicao("gavetaObjeto", "Gaveta (objeto da autorização)", TipoCampo.Texto),
        new CampoDefinicao("dataAssinatura", "Data da assinatura", TipoCampo.Data, Ajuda: "Opcional; em branco usa a data de emissão."),
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

        return null;
    }

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var jazigo = await DadosComuns.JazigoCompletoAsync(db, ctx.EntidadeId);
        var titular = DadosComuns.Qualificacao(DadosComuns.TitularAtivo(jazigo));
        if (string.IsNullOrWhiteSpace(titular.Nome))
            throw new DocumentoBloqueadoException("O jazigo não possui titular ativo cadastrado");

        var autorizadaId = ctx.Campos.GetInt("pessoaAutorizadaId")
            ?? throw new DocumentoBloqueadoException("Campo obrigatório não informado: Pessoa autorizada");
        var autorizada = await DadosComuns.ResponsavelAsync(db, autorizadaId);

        var gaveta = Tr(ctx.Campos.Get("gavetaObjeto"));
        var dataAssinatura = ctx.Campos.GetData("dataAssinatura") ?? ctx.DataEmissao;

        return Pdf(ctx, "TERMO DE RESPONSABILIDADE PARA TRATAR DE ASSUNTOS FUNERÁRIOS", col =>
        {
            col.Secao("Titular / autorizante");
            col.Campos(
                ("Nome", titular.Nome),
                ("CPF", titular.Cpf),
                ("RG", titular.Rg),
                ("Órgão emissor", titular.OrgaoEmissor),
                ("Endereço", titular.Endereco),
                ("Telefone", titular.Telefone));

            col.Secao("Pessoa autorizada");
            col.Campos(
                ("Nome", autorizada.Nome),
                ("CPF", autorizada.Cpf),
                ("RG", autorizada.Rg),
                ("Órgão emissor", autorizada.OrgaoEmissor),
                ("Endereço", autorizada.Endereco),
                ("Telefone", autorizada.Telefone));

            col.Secao("Objeto da autorização");
            col.Campos(
                ("Cemitério", jazigo.Cemiterio?.Nome),
                ("Quadra", Tr(jazigo.Quadra)),
                ("Ala", Tr(jazigo.Ala)),
                ("Lote", Tr(jazigo.NumeroLote)),
                ("Gaveta", gaveta),
                ("Tipo/classificação de sepultura/jazigo", jazigo.ClassificacaoEspaco?.Nome));

            col.Paragrafo(TextosPadrao.TermoResponsabilidadeAssuntosTexto);
            col.Paragrafo(TextosPadrao.TermoResponsabilidadeRevogacao);

            col.LocalEData(ComData(ctx, dataAssinatura));

            col.BlocoAssinaturas(
                $"Assinatura do Responsável — {titular.Nome}",
                $"Pessoa autorizada — {autorizada.Nome}");

            col.Campos(
                ("Titular/autorizante CPF", titular.Cpf),
                ("Pessoa autorizada CPF", autorizada.Cpf));

            col.AssinaturaCoordenacao(ctx);

            col.BlocoAssinaturas("Testemunha 1", "Testemunha 2");
            col.Campos(
                ("Testemunha 1", Tr(ctx.Campos.Get("testemunha1"))),
                ("Testemunha 2", Tr(ctx.Campos.Get("testemunha2"))));
        });
    }

    private static ContextoEmissao ComData(ContextoEmissao ctx, DateTime data)
        => new(ctx.EntidadeId, ctx.Campos, ctx.Instituicao, ctx.Numero, data);

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
