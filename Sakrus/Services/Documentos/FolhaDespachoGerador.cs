using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public class FolhaDespachoGerador : GeradorBase
{
    public FolhaDespachoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "14-DESPACHO";

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var protocolo = await db.Protocolos
            .Include(p => p.AssuntoProtocolo)
            .Include(p => p.Responsavel)
            .FirstOrDefaultAsync(p => p.Id == ctx.EntidadeId);
        if (protocolo == null)
            throw new DocumentoBloqueadoException("Protocolo não encontrado");

        var numero = ctx.Numero ?? protocolo.Numero;

        return Pdf(ctx, "FOLHA DE DESPACHO", col =>
        {
            col.Secao("Identificação do protocolo");
            col.Campos(
                ("Número do protocolo", numero),
                ("Data de abertura", protocolo.DataAbertura.ToString("dd/MM/yyyy")),
                ("Assunto", protocolo.AssuntoProtocolo?.Nome),
                ("Requerente", protocolo.Responsavel?.Nome));

            col.Secao("Despachos e tramitações");
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c => c.RelativeColumn());
                for (var i = 0; i < 14; i++)
                    table.Cell().Border(0.5f).Height(30);
            });

            col.LocalEData(ctx);
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
