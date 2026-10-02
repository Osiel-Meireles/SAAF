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

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var atendimento = await DadosComuns.AtendimentoCompletoAsync(db, ctx.EntidadeId);
        var falecido = await DadosComuns.FalecidoAsync(db, atendimento.FalecidoId);
        var responsavel = await DadosComuns.ResponsavelAsync(db, atendimento.ResponsavelId, atendimento.GrauParentesco);

        if (atendimento.TipoAtendimento != TipoAtendimentoFunerario.AuxilioFuneral && atendimento.ServicosAuxilio.Count == 0)
            throw new DocumentoBloqueadoException(
                "O documento de Auxílio Funeral exige atendimento do tipo Auxílio Funeral ou ao menos um serviço de auxílio cadastrado");

        var servicos = atendimento.ServicosAuxilio
            .Select(s => s.ServicoAuxilio?.Descricao)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d!)
            .ToList();

        return Pdf(ctx, "AUXÍLIO FUNERAL — REQUERIMENTO E ORDEM DE SERVIÇO", col =>
        {
            col.BlocoTipoAtendimento(atendimento.TipoAtendimento, atendimento.Funeraria?.Nome);

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
                    ("Endereço", responsavel.Endereco, 5), ("Nº", null, 1), ("Bairro", null, 3), ("Município/UF", null, 3)
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
                    ("Endereço", falecido.Endereco, 5), ("Nº", null, 1), ("Bairro", null, 2),
                    ("Município/UF", null, 2), ("Data óbito", falecido.DataObito?.ToString("dd/MM/yyyy"), 2)
                });

            col.TituloBloco("SERVIÇOS DE AUXÍLIO FUNERAL");
            col.ListaComCaixas(servicos);

            col.Paragrafo(TextosPadrao.AuxilioFuneralTexto);

            col.TituloBloco("FUNERÁRIA INDICADA");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Funerária", string.IsNullOrWhiteSpace(atendimento.Funeraria?.Nome) ? null : atendimento.Funeraria.Nome, 12)
                });

            col.BlocoAssinaturas("Requerente", "Funerária");
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
