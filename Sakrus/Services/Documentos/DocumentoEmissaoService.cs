using Microsoft.EntityFrameworkCore;
using Sakrus.Core;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;
using Sakrus.Services;

namespace Sakrus.Services.Documentos;

public record ResultadoEmissao(int DocumentoEmitidoId, string? Numero, int Versao, byte[] Pdf);

public interface IDocumentoEmissaoService
{
    IReadOnlyList<CampoDefinicao> ObterCampos(string codigo);
    Task<IReadOnlyList<TipoDocumento>> ListarDisponiveisAsync(string entidadeTipo);
    Task<ResultadoEmissao> EmitirAsync(string codigo, int entidadeId, CamposValores campos);
    Task<byte[]?> ReimprimirAsync(int documentoEmitidoId);
    Task<List<DocumentoEmitido>> HistoricoAsync(string entidadeTipo, int entidadeId);
}

public class DocumentoEmissaoService : IDocumentoEmissaoService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IEnumerable<IDocumentoGerador> _geradores;
    private readonly INumeracaoService _numeracaoService;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly FileStorageService _fileStorage;

    public DocumentoEmissaoService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IEnumerable<IDocumentoGerador> geradores,
        INumeracaoService numeracaoService,
        ICurrentUserService currentUser,
        TimeProvider timeProvider,
        FileStorageService fileStorage)
    {
        _dbFactory = dbFactory;
        _geradores = geradores;
        _numeracaoService = numeracaoService;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _fileStorage = fileStorage;
    }

    public IReadOnlyList<CampoDefinicao> ObterCampos(string codigo)
    {
        var gerador = _geradores.FirstOrDefault(g => string.Equals(g.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        return gerador?.Campos ?? Array.Empty<CampoDefinicao>();
    }

    public async Task<IReadOnlyList<TipoDocumento>> ListarDisponiveisAsync(string entidadeTipo)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        return await db.TiposDocumento
            .Where(t => t.Ativo && t.EntidadeTipo == entidadeTipo)
            .OrderBy(t => t.Ordem)
            .ToListAsync();
    }

    public async Task<ResultadoEmissao> EmitirAsync(string codigo, int entidadeId, CamposValores campos)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var tipo = await db.TiposDocumento.FirstOrDefaultAsync(t => t.Codigo == codigo && t.Ativo);
        if (tipo == null)
            throw new DocumentoBloqueadoException($"Tipo de documento não encontrado ou inativo: {codigo}");

        var gerador = _geradores.FirstOrDefault(g => string.Equals(g.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        if (gerador == null)
            throw new DocumentoBloqueadoException($"Gerador não implementado para o documento {codigo}");

        var faltantes = gerador.Campos
            .Where(c => c.Obrigatorio && string.IsNullOrWhiteSpace(campos.Get(c.Chave)))
            .Select(c => $"Campo obrigatório não informado: {c.Rotulo}")
            .ToList();
        if (faltantes.Count > 0)
            throw new DocumentoBloqueadoException(faltantes);

        var preCondicao = await gerador.ValidarPreCondicoesAsync(entidadeId);
        if (preCondicao != null)
            throw new DocumentoBloqueadoException(preCondicao);

        string? numero = null;
        if (tipo.ExigeNumeroUnico)
        {
            if (string.IsNullOrWhiteSpace(tipo.ChaveNumeracao))
                throw new DocumentoBloqueadoException($"Documento {codigo} exige número único mas não possui chave de numeração configurada");

            if (tipo.GeraNumeroNaPrimeiraEmissao)
            {
                var registro = await _numeracaoService.ObterOuGerarAsync(tipo.ChaveNumeracao, tipo.EntidadeTipo, entidadeId);
                numero = registro.Numero;
            }
            else
            {
                var registro = await _numeracaoService.ObterAsync(tipo.ChaveNumeracao, tipo.EntidadeTipo, entidadeId);
                if (registro == null)
                    throw new DocumentoBloqueadoException($"Emita primeiro o documento que gera o número {tipo.ChaveNumeracao}");
                numero = registro.Numero;
            }
        }

        var instituicao = await db.ConfiguracoesInstitucionais.FirstOrDefaultAsync();
        if (instituicao == null)
            throw new DocumentoBloqueadoException("Configuração institucional não encontrada");

        var ctx = new ContextoEmissao(
            entidadeId,
            campos,
            instituicao,
            numero,
            _timeProvider.GetUtcNow().UtcDateTime);

        var pdf = await gerador.GerarAsync(ctx);

        var pdfCaminho = await _fileStorage.SalvarBytesAsync("documentos-emitidos", entidadeId, pdf);

        var versao = (await db.DocumentosEmitidos
            .Where(d => d.TipoDocumentoId == tipo.Id && d.EntidadeTipo == tipo.EntidadeTipo && d.EntidadeId == entidadeId)
            .MaxAsync(d => (int?)d.Versao) ?? 0) + 1;

        var emitido = new DocumentoEmitido
        {
            TipoDocumentoId = tipo.Id,
            EntidadeTipo = tipo.EntidadeTipo,
            EntidadeId = entidadeId,
            Numero = numero,
            DadosJson = campos.ToJson(),
            Versao = versao,
            EmitidoPorId = _currentUser.UserId,
            EmitidoPorNome = _currentUser.UserName,
            EmitidoEm = _timeProvider.GetUtcNow().UtcDateTime,
            PdfCaminho = pdfCaminho
        };

        db.DocumentosEmitidos.Add(emitido);
        await db.SaveChangesAsync();

        return new ResultadoEmissao(emitido.Id, numero, versao, pdf);
    }

    public async Task<byte[]?> ReimprimirAsync(int documentoEmitidoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var emitido = await db.DocumentosEmitidos.FindAsync(documentoEmitidoId);
        if (emitido == null || string.IsNullOrWhiteSpace(emitido.PdfCaminho))
            return null;

        return await _fileStorage.LerArquivoAsync(emitido.PdfCaminho);
    }

    public async Task<List<DocumentoEmitido>> HistoricoAsync(string entidadeTipo, int entidadeId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        return await db.DocumentosEmitidos
            .Include(d => d.TipoDocumento)
            .Where(d => d.EntidadeTipo == entidadeTipo && d.EntidadeId == entidadeId)
            .OrderByDescending(d => d.EmitidoEm)
            .ToListAsync();
    }
}
