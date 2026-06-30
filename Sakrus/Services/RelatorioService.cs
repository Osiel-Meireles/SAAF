using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Sakrus.Services;

public class RelatorioService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public RelatorioService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }
    public byte[] GerarPdfOrdemServico(Atendimento atendimento)
    {
        // Licença configurada globalmente no Program.cs

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                // CABEÇALHO
                page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                {
                    var logoPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "logo1.svg");
                    if (System.IO.File.Exists(logoPath))
                    {
                        var svgData = System.IO.File.ReadAllText(logoPath);
                        row.ConstantItem(120).Height(60).Svg(svgData);
                    }
                    else
                    {
                        row.ConstantItem(120).Text("SAAF").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                    }

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("ORDEM DE SERVIÇO").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"OS Nº: {atendimento.NumeroOsAuxilio ?? "PENDENTE"}").FontSize(12).SemiBold();
                        col.Item().Text("SAAF - Sistema de Acolhimento e Assistência Funerária").FontSize(10).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });

                // CONTEÚDO
                page.Content().PaddingVertical(10).Column(x =>
                {
                    x.Spacing(5);
                    
                    x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    
                    x.Item().Text("DADOS DO ATENDIMENTO").SemiBold();
                    
                    // Ajustado: Como a entidade Falecido não possui 'Nome', 
                    // usamos o NumeroOsAuxilio ou uma propriedade de identificação do Atendimento
                    x.Item().Text($"Protocolo: {atendimento.NumeroOsAuxilio ?? "Não Gerado"}");
                    x.Item().Text($"Responsável: {atendimento.Responsavel?.Nome ?? "Não Informado"}");
                    x.Item().Text($"Causa da Morte: {atendimento.Falecido?.CausaMorte}");

                    x.Item().PaddingTop(10).Text("ITENS DA ORDEM DE SERVIÇO").SemiBold();
                    
                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).PaddingBottom(2).Text("Descrição").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(2).AlignRight().Text("Qtd/KM").SemiBold();
                            header.Cell().BorderBottom(1).PaddingBottom(2).AlignRight().Text("Valor Total").SemiBold();
                        });

                        foreach (var item in atendimento.ItensFaturados)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(item.CategoriaItem);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).AlignRight().Text(item.QuantidadeOuKm.ToString("N2"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).AlignRight().Text($"R$ {item.ValorTotalCalculado:N2}");
                        }
                    });

                    x.Item().AlignRight().PaddingTop(10).Text($"TOTAL GERAL: R$ {atendimento.ItensFaturados.Sum(i => i.ValorTotalCalculado):N2}").FontSize(12).SemiBold();
                });

                // RODAPÉ PARA ASSINATURAS
                page.Footer().PaddingTop(50).Column(footerCol =>
                {
                    footerCol.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Assinatura do Responsável");
                        });
                        row.ConstantItem(50);
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Carimbo / Servidor");
                        });
                    });
                    
                    footerCol.Item().PaddingTop(20).AlignCenter().Text(x =>
                    {
                        x.Span("Documento gerado pelo SAAF - ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }

    public byte[] GerarPdfGuiaSepultamento(Falecido falecido, Atendimento atendimento)
    {
        // Licença configurada globalmente no Program.cs

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // CABEÇALHO
                page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                {
                    var logoPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "logo1.svg");
                    if (System.IO.File.Exists(logoPath))
                    {
                        var svgData = System.IO.File.ReadAllText(logoPath);
                        row.ConstantItem(120).Height(60).Svg(svgData);
                    }
                    else
                    {
                        row.ConstantItem(120).Text("SAAF").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                    }

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("GUIA DE SEPULTAMENTO").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("SAAF - Sistema de Acolhimento e Assistência Funerária").FontSize(10).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });

                // CONTEÚDO
                page.Content().PaddingVertical(15).Column(x =>
                {
                    x.Spacing(10);
                    
                    x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    
                    x.Item().Text("DADOS DO FALECIDO").SemiBold().FontSize(12);
                    x.Item().Text($"Nome: {falecido.Nome}");
                    x.Item().Text($"CPF: {falecido.Cpf}");
                    x.Item().Text($"Data de Falecimento: {falecido.DataFalecimento:dd/MM/yyyy}");
                    x.Item().Text($"Causa da Morte: {falecido.CausaMorte}");
                    if (falecido.TipoRestosMortais == TipoRestosMortais.PecaAnatomica)
                    {
                        x.Item().Text("Trata-se de sepultamento de Peça Anatômica / Membro Amputado.").SemiBold().FontColor(Colors.Red.Medium);
                    }

                    x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten4);
                    
                    x.Item().Text("DADOS DO SEPULTAMENTO").SemiBold().FontSize(12);
                    
                    if (falecido.Jazigo != null)
                    {
                        x.Item().Text($"Local: Jazigo {falecido.Jazigo.CodigoIdentificador}");
                    }
                    else
                    {
                        x.Item().Text("Local: Gaveta Pública / Não Identificado");
                    }

                    if (atendimento != null && atendimento.Responsavel != null)
                    {
                        x.Item().PaddingTop(10).Text("RESPONSÁVEL PELO SEPULTAMENTO").SemiBold().FontSize(12);
                        x.Item().Text($"Nome: {atendimento.Responsavel.Nome}");
                        x.Item().Text($"CPF: {atendimento.Responsavel.CPF}");
                        x.Item().Text($"Telefone: {atendimento.Responsavel.Telefone}");
                    }
                });

                // RODAPÉ PARA ASSINATURAS
                page.Footer().PaddingTop(40).Column(footerCol =>
                {
                    footerCol.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Assinatura do Responsável");
                        });
                        row.ConstantItem(50);
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Administração do Cemitério");
                        });
                    });
                    
                    footerCol.Item().PaddingTop(20).AlignCenter().Text(x =>
                    {
                        x.Span("Documento gerado pelo SAAF - ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }

    public async Task<Dictionary<string, int>> GetOcupacaoGeralAsync()
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var totalJazigos = await context.Jazigos.CountAsync();
        var ocupados = await context.Jazigos.CountAsync(j => j.Ocupado);
        var livres = totalJazigos - ocupados;

        var totalGavetas = await context.GavetasPublicas.CountAsync();
        var gavetasOcupadas = await context.GavetasPublicas.CountAsync(g => g.Ocupada);
        var gavetasLivres = totalGavetas - gavetasOcupadas;

        return new Dictionary<string, int>
        {
            { "Jazigos Ocupados", ocupados },
            { "Jazigos Livres", livres },
            { "Gavetas Ocupadas", gavetasOcupadas },
            { "Gavetas Livres", gavetasLivres }
        };
    }

    public async Task<Dictionary<string, int>> GetCausasObitoPorPeriodoAsync(DateTime start, DateTime end)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Falecidos
            .Where(f => f.DataFalecimento >= start && f.DataFalecimento <= end)
            .GroupBy(f => f.CausaMorte)
            .Select(g => new { Causa = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Causa, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetSepultamentosPorFunerariaAsync(DateTime start, DateTime end)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Atendimentos
            .Include(a => a.Funeraria)
            .Where(a => a.DataSepultamento >= start && a.DataSepultamento <= end && a.FunerariaId != null)
            .GroupBy(a => a.Funeraria!.Nome)
            .Select(g => new { Funeraria = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Funeraria, x => x.Count);
    }
}