using QuestPDF.Fluent;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Junta vários PDFs em um único arquivo, para que a emissão de múltiplos documentos
/// abra apenas uma aba no navegador (abas extras são bloqueadas como pop-up).
/// </summary>
public static class PdfMerger
{
    public static byte[] Mesclar(IReadOnlyList<byte[]> pdfs)
    {
        if (pdfs.Count == 0)
            throw new ArgumentException("Nenhum PDF para mesclar.", nameof(pdfs));

        if (pdfs.Count == 1)
            return pdfs[0];

        var pasta = Path.Combine(Path.GetTempPath(), $"sakrus-pdf-{Guid.NewGuid():N}");
        Directory.CreateDirectory(pasta);
        try
        {
            var arquivos = new List<string>();
            for (var i = 0; i < pdfs.Count; i++)
            {
                var caminho = Path.Combine(pasta, $"{i:D3}.pdf");
                File.WriteAllBytes(caminho, pdfs[i]);
                arquivos.Add(caminho);
            }

            var saida = Path.Combine(pasta, "mesclado.pdf");
            var operacao = DocumentOperation.LoadFile(arquivos[0]);
            foreach (var arquivo in arquivos.Skip(1))
                operacao.MergeFile(arquivo);
            operacao.Save(saida);
            return File.ReadAllBytes(saida);
        }
        finally
        {
            try { Directory.Delete(pasta, true); } catch { }
        }
    }
}
