using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public class TransladoGerador : GeradorBase
{
    public TransladoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "3";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("destinoMunicipio", "Município de destino", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("destinoUf", "UF de destino", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("destinoLocal", "Cemitério/endereço de destino", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("empresaExecutoraId", "Empresa executora", TipoCampo.Selecao, FonteOpcoes: "empresas"),
        new CampoDefinicao("dataSaida", "Data de saída", TipoCampo.Data),
        new CampoDefinicao("horaSaida", "Horário de saída", TipoCampo.Hora)
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var atendimento = await DadosComuns.AtendimentoCompletoAsync(db, ctx.EntidadeId);
        var falecido = await DadosComuns.FalecidoAsync(db, atendimento.FalecidoId);
        var responsavel = await DadosComuns.ResponsavelAsync(db, atendimento.ResponsavelId, atendimento.GrauParentesco);

        var destinoMunicipio = ctx.Campos.Get("destinoMunicipio");
        var destinoUf = ctx.Campos.Get("destinoUf");
        var destinoLocal = ctx.Campos.Get("destinoLocal");
        Exigir(
            ("Município de destino", destinoMunicipio),
            ("UF de destino", destinoUf),
            ("Cemitério/endereço de destino", destinoLocal));

        var empresaExecutoraId = ctx.Campos.GetInt("empresaExecutoraId") ?? atendimento.EmpresaExecutoraId;
        if (empresaExecutoraId == null)
            throw new DocumentoBloqueadoException("Translado exige empresa executora");

        var empresa = await db.Funerarias.FindAsync(empresaExecutoraId.Value);
        if (empresa == null)
            throw new DocumentoBloqueadoException("Empresa executora não encontrada");

        var dataSaida = ctx.Campos.GetData("dataSaida");
        var horaSaida = ctx.Campos.GetHora("horaSaida");
        if (dataSaida == null && horaSaida == null)
            throw new DocumentoBloqueadoException("Informe a data e o horário de saída");

        var pt = new System.Globalization.CultureInfo("pt-BR");
        var dataHoraSaida = dataSaida.HasValue
            ? dataSaida.Value.ToString("dd 'DE' MMMM 'DE' yyyy", pt).ToUpper(pt)
            : string.Empty;
        if (horaSaida.HasValue)
            dataHoraSaida += (dataHoraSaida.Length > 0 ? " - " : "") + $"{horaSaida.Value.Hours:00}:{horaSaida.Value.Minutes:00} HORAS";

        var municipioUfDestino = string.Join(" - ", new[]
        {
            string.IsNullOrWhiteSpace(destinoMunicipio) ? null : destinoMunicipio,
            string.IsNullOrWhiteSpace(destinoUf) ? null : destinoUf
        }.Where(p => p != null));

        return Pdf(ctx, "TERMO DE TRANSLADO", col =>
        {
            col.FaixaServicos(false, false, false, true);
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

            col.TituloBloco("DADOS DO TRANSLADO");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Município/UF de destino", string.IsNullOrWhiteSpace(municipioUfDestino) ? null : municipioUfDestino, 12)
                },
                new (string, string?, int)[]
                {
                    ("Cemitério/endereço de destino", string.IsNullOrWhiteSpace(destinoLocal) ? null : destinoLocal, 12)
                },
                new (string, string?, int)[]
                {
                    ("Data e hora de saída", string.IsNullOrWhiteSpace(dataHoraSaida) ? null : dataHoraSaida, 12)
                },
                new (string, string?, int)[]
                {
                    ("Funerária executora", empresa.Nome, 12)
                });

            col.Declaracao(TextosPadrao.TransladoDeclaracao);
            col.FechoModelo(ctx, "Assinatura do Responsável", "Assinatura da Funerária");
        });
    }
}
