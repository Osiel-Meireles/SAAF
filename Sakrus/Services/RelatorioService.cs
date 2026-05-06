using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sakrus.Core.Entities;

namespace Sakrus.Services;

public class RelatorioService
{
    public byte[] GerarPdfOrdemServico(Atendimento atendimento)
    {
        // Configuração obrigatória da licença para uso comunitário
        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                // CABEÇALHO
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PREFEITURA MUNICIPAL DE LUÍS EDUARDO MAGALHÃES").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("SISTEMA SAKRUS - GESTÃO CEMITERIAL").FontSize(9);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"OS Nº: {atendimento.NumeroOsAuxilio ?? "PENDENTE"}").FontSize(12).SemiBold();
                        col.Item().Text($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}");
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
                    x.Item().Text($"Causa da Morte: {atendimento.Falecido?.CausaDaMorte}");

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
                        x.Span("Documento gerado pelo sistema Sakrus - ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }
}