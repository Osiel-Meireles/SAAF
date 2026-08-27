using System.Security.Cryptography;

namespace Sakrus.Services;

/// <summary>
/// SEC — Serviço centralizado de armazenamento de arquivos.
/// CRIT-01: Valida conteúdo real via magic bytes (não apenas extensão/ContentType).
/// CRIT-02: Salva arquivos FORA do wwwroot — inacessíveis via URL direta.
/// CRIT-03: Nome do arquivo em disco é apenas UUID — sem nome original para prevenir path traversal.
/// </summary>
public class FileStorageService
{
    private readonly string _baseStoragePath;
    private readonly ILogger<FileStorageService> _logger;

    // Magic bytes do PDF: %PDF (0x25 0x50 0x44 0x46)
    private static readonly byte[] PdfMagicBytes = { 0x25, 0x50, 0x44, 0x46 };

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
    {
        _logger = logger;

        // Em produção (Docker): /app/secure-uploads/
        // Em desenvolvimento: {ContentRootPath}/secure-uploads/
        _baseStoragePath = env.IsProduction()
            ? "/app/secure-uploads"
            : Path.Combine(env.ContentRootPath, "secure-uploads");

        Directory.CreateDirectory(_baseStoragePath);
    }

    /// <summary>
    /// Salva um arquivo de forma segura.
    /// </summary>
    /// <param name="categoria">Categoria lógica (ex: "atendimentos", "responsaveis", "funerarias").</param>
    /// <param name="entityId">ID da entidade dona do arquivo.</param>
    /// <param name="nomeOriginal">Nome original do arquivo (armazenado apenas no banco, não no disco).</param>
    /// <param name="bytes">Bytes do arquivo já lidos em memória.</param>
    /// <returns>Caminho relativo a ser armazenado no banco de dados.</returns>
    /// <exception cref="InvalidOperationException">Quando o arquivo não é um PDF válido.</exception>
    public async Task<string> SalvarArquivoAsync(string categoria, int entityId, string nomeOriginal, byte[] bytes)
    {
        // CRIT-01: Validação de conteúdo via magic bytes
        if (!IsPdfContent(bytes))
        {
            _logger.LogWarning("Upload rejeitado — arquivo não é PDF válido: {Nome}", nomeOriginal);
            throw new InvalidOperationException($"O arquivo '{nomeOriginal}' não é um PDF válido. Apenas arquivos PDF são aceitos.");
        }

        // CRIT-03: Nome no disco é apenas UUID — sem extensão ou nome original
        var nomeEmDisco = $"{Guid.NewGuid():N}";

        var subPasta = Path.Combine(_baseStoragePath, categoria, entityId.ToString());
        Directory.CreateDirectory(subPasta);

        var caminhoFisico = Path.Combine(subPasta, nomeEmDisco);
        await File.WriteAllBytesAsync(caminhoFisico, bytes);

        // Caminho relativo armazenado no banco (sem path separators do SO)
        var caminhoRelativo = $"secure-uploads/{categoria}/{entityId}/{nomeEmDisco}";
        _logger.LogInformation("Arquivo salvo com segurança: {Categoria}/{EntityId}/{Nome}", categoria, entityId, nomeOriginal);

        return caminhoRelativo;
    }

    /// <summary>
    /// Resolve o caminho físico absoluto a partir do caminho relativo armazenado no banco.
    /// CRIT-02: Garante que o caminho resolvido está dentro do diretório de uploads seguros (previne path traversal).
    /// </summary>
    public string ResolverCaminhoFisico(string caminhoRelativo)
    {
        // Normaliza separadores
        var normalizado = caminhoRelativo
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        var caminhoFisico = Path.GetFullPath(Path.Combine(_baseStoragePath, "..", normalizado));

        // Previne path traversal: o caminho resolvido deve estar dentro do base path
        var basePathNorm = Path.GetFullPath(_baseStoragePath + Path.DirectorySeparatorChar);
        if (!caminhoFisico.StartsWith(basePathNorm, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogCritical("Tentativa de path traversal detectada! CaminhoRelativo: {Caminho}", caminhoRelativo);
            throw new UnauthorizedAccessException("Caminho de arquivo inválido.");
        }

        return caminhoFisico;
    }

    /// <summary>
    /// Exclui fisicamente um arquivo do armazenamento seguro.
    /// </summary>
    public void ExcluirArquivo(string caminhoRelativo)
    {
        try
        {
            var caminhoFisico = ResolverCaminhoFisico(caminhoRelativo);
            if (File.Exists(caminhoFisico))
            {
                File.Delete(caminhoFisico);
                _logger.LogInformation("Arquivo excluído: {Caminho}", caminhoRelativo);
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir arquivo: {Caminho}", caminhoRelativo);
        }
    }

    /// <summary>
    /// Lê os bytes de um arquivo do armazenamento seguro.
    /// </summary>
    public async Task<byte[]> LerArquivoAsync(string caminhoRelativo)
    {
        var caminhoFisico = ResolverCaminhoFisico(caminhoRelativo);
        if (!File.Exists(caminhoFisico))
            throw new FileNotFoundException("Arquivo não encontrado no armazenamento seguro.", caminhoRelativo);

        return await File.ReadAllBytesAsync(caminhoFisico);
    }

    /// <summary>
    /// CRIT-01: Verifica magic bytes do PDF (%PDF nos primeiros 4 bytes).
    /// </summary>
    private static bool IsPdfContent(byte[] bytes)
    {
        if (bytes.Length < 4) return false;
        return bytes[0] == PdfMagicBytes[0]
            && bytes[1] == PdfMagicBytes[1]
            && bytes[2] == PdfMagicBytes[2]
            && bytes[3] == PdfMagicBytes[3];
    }
}
