using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public class UsoCapelaGerador : GeradorBase
{
    public UsoCapelaGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "4";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("capelaNome", "Nome da capela", TipoCampo.Texto, Ajuda: "Opcional; se em branco, usa a capela agendada ou padrão."),
        new CampoDefinicao("horaEntrada", "Data/hora de entrada", TipoCampo.Data, Ajuda: "Opcional; se em branco, usa a data do atendimento."),
        new CampoDefinicao("horaSaida", "Data/hora de saída", TipoCampo.Data, Ajuda: "Opcional.")
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var atendimento = await DadosComuns.AtendimentoCompletoAsync(db, ctx.EntidadeId);
        var falecido = await DadosComuns.FalecidoAsync(db, atendimento.FalecidoId);
        var responsavel = await DadosComuns.ResponsavelAsync(db, atendimento.ResponsavelId, atendimento.GrauParentesco);

        var registro = await db.RegistrosCapela
            .Include(r => r.Capela)
            .FirstOrDefaultAsync(r => r.AtendimentoId == atendimento.Id);

        var nomeCapela = registro?.Capela?.Nome
            ?? Tr(ctx.Campos.Get("capelaNome"))
            ?? (await db.Capelas.Where(c => c.Status != "Manutenção").OrderBy(c => c.Nome).Select(c => c.Nome).FirstOrDefaultAsync())
            ?? "Capela Municipal";

        var horaEntrada = registro?.HoraEntrada
            ?? ctx.Campos.GetData("horaEntrada")
            ?? atendimento.DataSepultamento
            ?? ctx.DataEmissao;

        var horaSaida = registro?.HoraSaida
            ?? ctx.Campos.GetData("horaSaida");

        return Pdf(ctx, "TERMO DE USO DE CAPELA", col =>
        {
            col.FaixaServicos(false, true, false, false);
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
                    ("Endereço", falecido.Endereco, 5), ("Nº", falecido.Numero, 1), ("Bairro", falecido.Bairro, 2),
                    ("Município/UF", falecido.MunicipioUf, 2), ("Data óbito", falecido.DataObito?.ToString("dd/MM/yyyy"), 2)
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
                    ("Endereço", responsavel.Endereco, 5), ("Nº", responsavel.Numero, 1), ("Bairro", responsavel.Bairro, 3), ("Município/UF", responsavel.MunicipioUf, 3)
                },
                new (string, string?, int)[]
                {
                    ("Telefone", responsavel.Telefone, 6), ("Profissão", responsavel.Profissao, 6)
                });

            col.TituloBloco("DADOS DA CAPELA");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Capela", nomeCapela, 12) },
                new (string, string?, int)[]
                {
                    ("Hora de entrada", horaEntrada.ToString("dd/MM/yyyy HH:mm"), 6),
                    ("Hora de saída", horaSaida.HasValue ? horaSaida.Value.ToString("dd/MM/yyyy HH:mm") : "—", 6)
                });

            col.Declaracao(TextosPadrao.UsoCapelaDeclaracao);

            col.TituloBloco("RESPONSABILIDADES DO USUÁRIO");
            col.ListaComCaixas(TextosPadrao.UsoCapelaResponsabilidades);

            col.FechoModelo(ctx, "Assinatura do Responsável", "Assinatura da CAAFE");
        });
    }

    private static string? Tr(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
