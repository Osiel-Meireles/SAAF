using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public class AuxilioFuneralGerador : GeradorBase
{
    public AuxilioFuneralGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "6";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("funerariaNome", "Funerária indicada", TipoCampo.Texto, Ajuda: "Opcional; se em branco, usa a funerária do atendimento.")
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var atendimento = await DadosComuns.AtendimentoCompletoAsync(db, ctx.EntidadeId);
        var falecido = await DadosComuns.FalecidoAsync(db, atendimento.FalecidoId);
        var responsavel = await DadosComuns.ResponsavelAsync(db, atendimento.ResponsavelId, atendimento.GrauParentesco);

        var servicos = atendimento.ServicosAuxilio
            .Select(s => s.ServicoAuxilio?.Descricao)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d!)
            .ToList();

        if (servicos.Count == 0)
        {
            var servicosDb = await db.ServicosAuxilio
                .OrderBy(s => s.Codigo)
                .Select(s => s.Descricao)
                .ToListAsync();

            servicos = servicosDb.Count > 0
                ? servicosDb
                : new List<string> { "Liberação", "Urna funerária", "Preparo", "Translado" };
        }

        var nomeFuneraria = atendimento.Funeraria?.Nome
            ?? Tr(ctx.Campos.Get("funerariaNome"))
            ?? "A designar";

        return Pdf(ctx, "AUXÍLIO FUNERAL — REQUERIMENTO E ORDEM DE SERVIÇO", col =>
        {
            col.BlocoTipoAtendimento(
                atendimento.TipoAtendimento == TipoAtendimentoFunerario.AuxilioFuneral
                    ? atendimento.TipoAtendimento
                    : TipoAtendimentoFunerario.AuxilioFuneral,
                nomeFuneraria);

            col.TituloBloco("DADOS DO REQUERENTE");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome", responsavel.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("RG", responsavel.Rg, 3), ("CPF", responsavel.Cpf, 3),
                    ("Estado civil", responsavel.EstadoCivil, 3), ("Parentesco", responsavel.Parentesco, 3)
                },
                new (string, string?, int)[]
                {
                    ("Endereço", responsavel.Endereco, 5), ("Nº", responsavel.Numero, 1), ("Bairro", responsavel.Bairro, 3), ("Município/UF", responsavel.MunicipioUf, 3)
                },
                new (string, string?, int)[]
                {
                    ("Telefone", responsavel.Telefone, 6), ("Profissão", responsavel.Profissao, 6)
                });

            col.TituloBloco("DADOS DO FALECIDO");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome", falecido.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("Sexo", falecido.Sexo, 2), ("Idade", falecido.Idade?.ToString(), 2),
                    ("Estado civil", falecido.EstadoCivil, 4), ("Profissão", falecido.Profissao, 4)
                },
                new (string, string?, int)[]
                {
                    ("Endereço", falecido.Endereco, 5), ("Nº", falecido.Numero, 1), ("Bairro", falecido.Bairro, 2),
                    ("Município/UF", falecido.MunicipioUf, 2), ("Data óbito", falecido.DataObito?.ToString("dd/MM/yyyy"), 2)
                });

            col.TituloBloco("SERVIÇOS DE AUXÍLIO FUNERAL");
            col.ListaComCaixas(servicos);

            col.Paragrafo(TextosPadrao.AuxilioFuneralTexto);

            col.TituloBloco("FUNERÁRIA INDICADA");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Funerária", nomeFuneraria, 12)
                });

            col.BlocoAssinaturas("Requerente", $"Funerária ({nomeFuneraria})");
            col.AssinaturaCoordenacao(ctx);
        });
    }

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
