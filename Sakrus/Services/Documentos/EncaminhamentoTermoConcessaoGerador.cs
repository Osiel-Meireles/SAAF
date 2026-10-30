using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 13 — Encaminhamento para Assinatura do Termo de Concessão.
/// </summary>
public class EncaminhamentoTermoConcessaoGerador : GeradorBase
{
    public EncaminhamentoTermoConcessaoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "13";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("dataEncaminhamento", "Data do encaminhamento", TipoCampo.Data),
        new CampoDefinicao("horarioEncaminhamento", "Horário do encaminhamento", TipoCampo.Hora)
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
        var titular = DadosComuns.TitularAtivo(jazigo);
        if (titular == null)
            throw new DocumentoBloqueadoException("O jazigo não possui titular ativo cadastrado");

        var concessionario = DadosComuns.Qualificacao(titular);
        var dataEnc = ctx.Campos.GetData("dataEncaminhamento") ?? ctx.DataEmissao;
        var horaEnc = ctx.Campos.GetHora("horarioEncaminhamento");
        var dataHoraTexto = horaEnc.HasValue
            ? $"{dataEnc:dd/MM/yyyy} às {horaEnc.Value:hh\\:mm}"
            : dataEnc.ToString("dd/MM/yyyy");

        var numeroContrato = ctx.Numero ?? "—";

        return Pdf(ctx, "ENCAMINHAMENTO PARA ASSINATURA DO TERMO DE CONCESSÃO", col =>
        {
            col.Item().PaddingTop(6).Border(0.5f).Padding(4).Column(c =>
            {
                c.Item().Text(t =>
                {
                    t.Span("Ao Ilustríssimo(a) Senhor(a)\n").Bold();
                    t.Span("Presidente do Conselho Municipal Deliberativo dos Serviços Funerários\n").SemiBold();
                    t.Span("Luís Eduardo Magalhães - BA");
                });
            });

            col.TituloBloco("REFERÊNCIA DO PROCESSO / CONTRATO");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Contrato de Concessão nº", numeroContrato, 6),
                    ("Data e Horário de Encaminhamento", dataHoraTexto, 6)
                });

            col.TituloBloco("DADOS DO(A) CONCESSIONÁRIO(A) / REQUERENTE");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome", concessionario.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("CPF", concessionario.Cpf, 6), ("RG", concessionario.Rg, 6)
                },
                new (string, string?, int)[]
                {
                    ("Endereço", concessionario.Endereco, 8), ("Telefone", concessionario.Telefone, 4)
                });

            col.TituloBloco("DADOS DO ESPAÇO CEMITERIAL OBJETO DA CONCESSÃO");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Cemitério", jazigo.Cemiterio?.Nome, 6),
                    ("Classificação", jazigo.ClassificacaoEspaco?.Nome ?? "Jazigo Particular", 6)
                },
                new (string, string?, int)[]
                {
                    ("Quadra", jazigo.Quadra, 4), ("Ala", jazigo.Ala ?? "—", 4), ("Lote nº", jazigo.NumeroLote, 4)
                });

            col.Declaracao(
                $"Encaminhamos a Vossa Senhoria o Termo de Concessão de Espaço Cemiterial correspondente ao Contrato nº {numeroContrato}, " +
                "devidamente instruído e com a quitação integral das taxas e encargos cemiteriais comprovada nos autos, " +
                "para fins de homologação e respectiva assinatura.");

            col.LocalEData(new ContextoEmissao(ctx.EntidadeId, ctx.Campos, ctx.Instituicao, ctx.Numero, dataEnc));

            col.AssinaturaCoordenacao(ctx);

            col.TituloBloco("DESPACHO / RECEBIMENTO DO CONSELHO MUNICIPAL");
            col.Paragrafo(
                "Recebido e analisado o processo de concessão de espaço cemiterial. " +
                "Homologo os termos apresentados e firmo a respectiva assinatura no instrumento contratual.");

            col.AssinaturaPresidenteConselho(ctx);
        });
    }
}
