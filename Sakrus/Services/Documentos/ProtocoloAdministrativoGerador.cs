using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public class ProtocoloAdministrativoGerador : GeradorBase
{
    public ProtocoloAdministrativoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "14";

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var protocolo = await db.Protocolos
            .Include(p => p.AssuntoProtocolo)
            .Include(p => p.Responsavel)
            .Include(p => p.Jazigo).ThenInclude(j => j.Cemiterio)
            .Include(p => p.Jazigo).ThenInclude(j => j.ClassificacaoEspaco)
            .FirstOrDefaultAsync(p => p.Id == ctx.EntidadeId);
        if (protocolo == null)
            throw new DocumentoBloqueadoException("Protocolo não encontrado");

        var numero = ctx.Numero ?? protocolo.Numero;
        var responsavel = protocolo.Responsavel;
        var jazigo = protocolo.Jazigo;

        return Pdf(ctx, "PROTOCOLO ADMINISTRATIVO", col =>
        {
            col.Secao("Identificação do protocolo");
            col.Campos(
                ("Número do protocolo", numero),
                ("Data de abertura", protocolo.DataAbertura.ToString("dd/MM/yyyy")),
                ("Assunto", protocolo.AssuntoProtocolo?.Nome),
                ("Requerente", responsavel?.Nome));

            col.Secao("Dados do requerente");
            col.Campos(
                ("Nome", responsavel?.Nome),
                ("CPF", DadosComuns.FormatarCpf(responsavel?.CPF)),
                ("RG", string.IsNullOrWhiteSpace(responsavel?.RG) ? null : responsavel.RG),
                ("Endereço", string.IsNullOrWhiteSpace(responsavel?.Endereco) ? null : responsavel.Endereco),
                ("Telefone", string.IsNullOrWhiteSpace(responsavel?.Telefone) ? null : responsavel.Telefone));

            if (jazigo != null)
            {
                col.Secao("Dados do lote");
                col.Campos(
                    ("Cemitério", jazigo.Cemiterio?.Nome),
                    ("Quadra", string.IsNullOrWhiteSpace(jazigo.Quadra) ? null : jazigo.Quadra),
                    ("Ala", string.IsNullOrWhiteSpace(jazigo.Ala) ? null : jazigo.Ala),
                    ("Lote", string.IsNullOrWhiteSpace(jazigo.NumeroLote) ? null : jazigo.NumeroLote),
                    ("Código do jazigo", jazigo.CodigoIdentificador),
                    ("Classificação", jazigo.ClassificacaoEspaco?.Nome));
            }

            if (!string.IsNullOrWhiteSpace(protocolo.Observacao))
            {
                col.Secao("Observação");
                col.Paragrafo(protocolo.Observacao);
            }

            col.LocalEData(ctx);
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
