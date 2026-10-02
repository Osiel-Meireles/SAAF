using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public class DocumentoRegularizacaoGerador : GeradorBase
{
    public DocumentoRegularizacaoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "14-REGULARIZACAO";

    public override async Task<string?> ValidarPreCondicoesAsync(int entidadeId)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var protocolo = await db.Protocolos
            .Include(p => p.AssuntoProtocolo)
            .FirstOrDefaultAsync(p => p.Id == entidadeId);
        if (protocolo == null)
            return "Protocolo não encontrado";

        if (protocolo.AssuntoProtocolo?.GeraRegularizacao != true)
            return "O assunto deste protocolo não gera documento de regularização de lote";

        if (protocolo.JazigoId == null)
            return "O protocolo não está vinculado a um lote para regularização";

        return null;
    }

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var protocolo = await db.Protocolos
            .Include(p => p.AssuntoProtocolo)
            .Include(p => p.Responsavel)
            .Include(p => p.Jazigo).ThenInclude(j => j.Cemiterio)
            .Include(p => p.Jazigo).ThenInclude(j => j.ClassificacaoEspaco)
            .Include(p => p.Jazigo).ThenInclude(j => j.Gavetas).ThenInclude(g => g.Falecido)
            .FirstOrDefaultAsync(p => p.Id == ctx.EntidadeId);
        if (protocolo == null)
            throw new DocumentoBloqueadoException("Protocolo não encontrado");

        var jazigo = protocolo.Jazigo;
        if (jazigo == null)
            throw new DocumentoBloqueadoException("O protocolo não está vinculado a um lote para regularização");

        var linhas = jazigo.Gavetas
            .OrderBy(g => g.Numero, StringComparer.OrdinalIgnoreCase)
            .Select(g => (IReadOnlyList<string>)new[]
            {
                g.Numero,
                g.Falecido?.Nome ?? "Livre",
                g.Falecido?.DataSepultamento?.ToString("dd/MM/yyyy") ?? "—"
            })
            .ToList();

        return Pdf(ctx, "DOCUMENTO DE REGULARIZAÇÃO DE LOTE", col =>
        {
            col.Secao("Identificação do protocolo");
            col.Campos(
                ("Número do protocolo", ctx.Numero ?? protocolo.Numero),
                ("Data de abertura", protocolo.DataAbertura.ToString("dd/MM/yyyy")),
                ("Assunto", protocolo.AssuntoProtocolo?.Nome));

            col.Secao("Dados do requerente");
            col.Campos(
                ("Nome", protocolo.Responsavel?.Nome),
                ("CPF", DadosComuns.FormatarCpf(protocolo.Responsavel?.CPF)),
                ("RG", string.IsNullOrWhiteSpace(protocolo.Responsavel?.RG) ? null : protocolo.Responsavel.RG),
                ("Endereço", string.IsNullOrWhiteSpace(protocolo.Responsavel?.Endereco) ? null : protocolo.Responsavel.Endereco),
                ("Telefone", string.IsNullOrWhiteSpace(protocolo.Responsavel?.Telefone) ? null : protocolo.Responsavel.Telefone));

            col.Secao("Dados do lote");
            col.Campos(
                ("Cemitério", jazigo.Cemiterio?.Nome),
                ("Quadra", string.IsNullOrWhiteSpace(jazigo.Quadra) ? null : jazigo.Quadra),
                ("Ala", string.IsNullOrWhiteSpace(jazigo.Ala) ? null : jazigo.Ala),
                ("Lote", string.IsNullOrWhiteSpace(jazigo.NumeroLote) ? null : jazigo.NumeroLote),
                ("Código do jazigo", jazigo.CodigoIdentificador),
                ("Classificação", jazigo.ClassificacaoEspaco?.Nome));

            col.Secao("Falecidos vinculados às gavetas");
            col.Tabela(
                new[] { "Gaveta", "Falecido", "Data do sepultamento" },
                linhas);

            col.LocalEData(ctx);
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
