using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 08 — Termo de Exumação e Transferência de Despojos para Outro Cemitério.
/// </summary>
public class ExumacaoOutroCemiterioGerador : GeradorBase
{
    public ExumacaoOutroCemiterioGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "8";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("empresaExecutoraId", "Empresa executora", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "empresas"),
        new CampoDefinicao("cemiterioDestino", "Nome do cemitério de destino", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("municipioDestino", "Município de destino", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("ufDestino", "UF de destino", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("dataExumacao", "Data da exumação/transferência", TipoCampo.Data),
        new CampoDefinicao("responsavelId", "Responsável / Requerente", TipoCampo.Selecao, FonteOpcoes: "responsaveis")
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

        var cemiterioDestino = ctx.Campos.Get("cemiterioDestino");
        var municipioDestino = ctx.Campos.Get("municipioDestino");
        var ufDestino = ctx.Campos.Get("ufDestino");
        Exigir(
            ("Empresa executora", empresa?.Nome),
            ("Nome do cemitério de destino", cemiterioDestino),
            ("Município de destino", municipioDestino),
            ("UF de destino", ufDestino));

        var dataExumacao = (ctx.Campos.GetData("dataExumacao") ?? ctx.DataEmissao).ToString("dd/MM/yyyy");

        var falecidoDados = await DadosComuns.FalecidoAsync(db, falecido.Id);

        return Pdf(ctx, "TERMO DE EXUMAÇÃO E TRANSFERÊNCIA DE DESPOJOS PARA OUTRO CEMITÉRIO", col =>
        {
            col.TituloBloco("DADOS DO FALECIDO");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome", falecidoDados.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("Sexo", falecidoDados.Sexo, 2), ("Idade", falecidoDados.Idade?.ToString(), 2),
                    ("Estado civil", falecidoDados.EstadoCivil, 4), ("Causa do óbito", falecidoDados.CausaObito, 4)
                },
                new (string, string?, int)[]
                {
                    ("Data do óbito", falecidoDados.DataObito?.ToString("dd/MM/yyyy"), 4),
                    ("Data do sepultamento", falecidoDados.DataSepultamento?.ToString("dd/MM/yyyy"), 4),
                    ("Nº D.O.", falecidoDados.NumeroDeclaracaoObito, 4)
                });

            col.TituloBloco("LOCAL ATUAL DOS RESTOS MORTAIS");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Localização atual", localAtual, 12) });

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

            col.TituloBloco("DESTINO E EXECUÇÃO DA TRANSFERÊNCIA");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Cemitério de destino", cemiterioDestino, 6),
                    ("Município / UF de destino", $"{municipioDestino} / {ufDestino}", 6)
                },
                new (string, string?, int)[]
                {
                    ("Data da exumação/transferência", dataExumacao, 6),
                    ("Empresa executora", empresa?.Nome, 6)
                });

            col.Declaracao(TextosPadrao.ExumacaoOutroCemiterioAutorizacao);
            col.Paragrafo(TextosPadrao.ExumacaoTransferenciaDeclaracao);

            col.LocalEData(ctx);
            col.BlocoAssinaturas("Assinatura do Responsável / Requerente", $"Empresa Executora ({empresa?.Nome})");
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
