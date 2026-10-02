using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public class DispensaCapelaGerador : GeradorBase
{
    public DispensaCapelaGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "5";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("localVelorio", "Local do velório", TipoCampo.Texto, Obrigatorio: true)
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var atendimento = await DadosComuns.AtendimentoCompletoAsync(db, ctx.EntidadeId);
        var falecido = await DadosComuns.FalecidoAsync(db, atendimento.FalecidoId);
        var responsavel = await DadosComuns.ResponsavelAsync(db, atendimento.ResponsavelId, atendimento.GrauParentesco);

        var localVelorio = ctx.Campos.Get("localVelorio");
        Exigir(("Local do velório", localVelorio));

        return Pdf(ctx, "TERMO DE DISPENSA DE USO DA CAPELA", col =>
        {
            col.BlocoTipoAtendimento(atendimento.TipoAtendimento, atendimento.Funeraria?.Nome);

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
                    ("Nº da D.O", string.IsNullOrWhiteSpace(atendimento.NumeroDeclaracaoObito) ? null : atendimento.NumeroDeclaracaoObito, 12)
                },
                new (string, string?, int)[]
                {
                    ("Endereço", falecido.Endereco, 5), ("Nº", null, 1), ("Bairro", null, 2),
                    ("Município/UF", null, 2), ("Data óbito", falecido.DataObito?.ToString("dd/MM/yyyy"), 2)
                },
                new (string, string?, int)[]
                {
                    ("Local do óbito", string.IsNullOrWhiteSpace(atendimento.LocalFalecimento) ? null : atendimento.LocalFalecimento, 12)
                },
                new (string, string?, int)[] { ("Causa do óbito", falecido.CausaObito, 12) });

            col.TituloBloco("DADOS DO RESPONSÁVEL");
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

            col.TituloBloco("DADOS DO VELÓRIO");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Local do velório", string.IsNullOrWhiteSpace(localVelorio) ? null : localVelorio, 12) });

            col.Declaracao(TextosPadrao.DispensaCapelaDeclaracao);
            col.FechoModelo(ctx, "Assinatura do Responsável", "Assinatura da CAAFE");
        });
    }
}
