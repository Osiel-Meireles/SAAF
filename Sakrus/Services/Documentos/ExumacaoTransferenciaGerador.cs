using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 07 — Termo de Exumação e Transferência de Despojos.
/// </summary>
public class ExumacaoTransferenciaGerador : GeradorBase
{
    public ExumacaoTransferenciaGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "7";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("empresaExecutoraId", "Empresa executora", TipoCampo.Selecao, Obrigatorio: true, FonteOpcoes: "empresas"),
        new CampoDefinicao("destinoTipo", "Tipo de destino", TipoCampo.Texto, Obrigatorio: true, Ajuda: "Ex: Ossuário, Gaveta, Jazigo"),
        new CampoDefinicao("destinoDescricao", "Identificação/local de destino", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("dataExumacao", "Data da exumação", TipoCampo.Data),
        new CampoDefinicao("horarioExumacao", "Horário da exumação", TipoCampo.Hora),
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

        var destinoTipo = ctx.Campos.Get("destinoTipo");
        var destinoDescricao = ctx.Campos.Get("destinoDescricao");
        Exigir(
            ("Empresa executora", empresa?.Nome),
            ("Tipo de destino", destinoTipo),
            ("Identificação/local de destino", destinoDescricao));

        var dataExumacao = ctx.Campos.GetData("dataExumacao") ?? ctx.DataEmissao;
        var horaExumacao = ctx.Campos.GetHora("horarioExumacao");
        var dataHoraExumacao = horaExumacao.HasValue
            ? $"{dataExumacao:dd/MM/yyyy} às {horaExumacao.Value:hh\\:mm}"
            : dataExumacao.ToString("dd/MM/yyyy");

        var falecidoDados = await DadosComuns.FalecidoAsync(db, falecido.Id);

        return Pdf(ctx, "TERMO DE EXUMAÇÃO E TRANSFERÊNCIA DE DESPOJOS", col =>
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

            col.TituloBloco("DESTINO E EXECUÇÃO DA EXUMAÇÃO");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Tipo de destino", destinoTipo, 4),
                    ("Identificação / local de destino", destinoDescricao, 8)
                },
                new (string, string?, int)[]
                {
                    ("Data e horário da exumação", dataHoraExumacao, 6),
                    ("Empresa executora", empresa?.Nome, 6)
                });

            col.Declaracao(TextosPadrao.ExumacaoTransferenciaAutorizacao);
            col.Paragrafo(TextosPadrao.ExumacaoTransferenciaDeclaracao);

            col.LocalEData(ctx);
            col.BlocoAssinaturas("Assinatura do Responsável / Requerente", $"Empresa Executora ({empresa?.Nome})");
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
