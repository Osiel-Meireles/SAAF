using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sakrus.Core.Entities;
using System;
using System.IO;

namespace Sakrus.Services;

public class PdfGeneratorService
{
    public PdfGeneratorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarTermoConcessaoPdf(Jazigo jazigo, Responsavel responsavel, HistoricoTitularidadeJazigo historico)
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // CABEÇALHO
                page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                {
                    var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo1.svg");
                    if (File.Exists(logoPath))
                    {
                        var svgData = File.ReadAllText(logoPath);
                        row.ConstantItem(120).Height(60).Svg(svgData);
                    }
                    else
                    {
                        row.ConstantItem(120).Text("SAAF").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                    }

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("TERMO DE CONCESSÃO").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("SAAF - Sistema de Acolhimento e Assistência Funerária").FontSize(10).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Emissão: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });

                // CONTEÚDO
                page.Content().PaddingVertical(20).Column(x =>
                {
                    x.Spacing(20);
                    
                    x.Item().AlignCenter().Text("TERMO DE CONCESSÃO DE USO DE JAZIGO").FontSize(14).Bold().FontColor(Colors.Black);
                    
                    x.Item().Text(t =>
                    {
                        t.Span("Pelo presente instrumento, a administração cemiterial concede o direito de uso do ");
                        t.Span($"Jazigo Código {jazigo.CodigoIdentificador}").Bold();
                        t.Span(" ao concessionário abaixo qualificado, de acordo com as normativas vigentes.");
                    });

                    // Tabela de Dados do Concessionário
                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                        });

                        table.Cell().Background(Colors.Blue.Lighten5).Padding(5).Text("DADOS DO CONCESSIONÁRIO").SemiBold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        
                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text($"Nome: {responsavel.Nome}");
                            c.Item().Text($"CPF: {responsavel.CPF}");
                            c.Item().Text($"Endereço: {responsavel.Endereco}");
                        });
                    });

                    // Tabela de Dados do Jazigo
                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                        });

                        table.Cell().Background(Colors.Blue.Lighten5).Padding(5).Text("DADOS DO JAZIGO").SemiBold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        
                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text($"Identificador: {jazigo.CodigoIdentificador}").Bold();
                            c.Item().Text($"Modelo: {(jazigo.ModeloJazigo != null ? jazigo.ModeloJazigo.Nome : "Não Especificado")}");
                            c.Item().Text($"Classificação: {(jazigo.IsInfantil ? "Infantil" : "Adulto/Padrão")}");
                            c.Item().Text($"Status: {(jazigo.Ocupado ? "Ocupado" : "Livre")}").FontColor(jazigo.Ocupado ? Colors.Red.Medium : Colors.Green.Medium);
                        });
                    });

                    if (historico != null)
                    {
                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                            });

                            table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("HISTÓRICO DE TITULARIDADE").SemiBold().FontSize(12).FontColor(Colors.Grey.Darken3);
                            
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                            {
                                c.Item().Text($"Transferido em: {historico.DataTransferencia:dd/MM/yyyy}");
                                c.Item().Text($"Motivo: {historico.Motivo}");
                            });
                        });
                    }
                });

                // RODAPÉ
                page.Footer().Column(footerCol =>
                {
                    footerCol.Item().PaddingBottom(20).Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Assinatura do Concessionário").FontSize(10);
                        });
                        row.ConstantItem(50);
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Administração SAAF").FontSize(10);
                        });
                    });
                    
                    footerCol.Item().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }

    public byte[] GerarRequerimentoExumacaoPdf(ExumacaoRegistro exumacao)
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                {
                    var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo1.svg");
                    if (File.Exists(logoPath))
                    {
                        var svgData = File.ReadAllText(logoPath);
                        row.ConstantItem(120).Height(60).Svg(svgData);
                    }
                    else
                    {
                        row.ConstantItem(120).Text("SAAF").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                    }

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("REQUERIMENTO DE EXUMAÇÃO").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("SAAF - Sistema de Acolhimento e Assistência Funerária").FontSize(10).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Emissão: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });

                page.Content().PaddingVertical(20).Column(x =>
                {
                    x.Spacing(15);
                    x.Item().Text(t =>
                    {
                        t.Span("Requerimento formal para exumação dos restos mortais do falecido ID ");
                        t.Span($"{exumacao.FalecidoId}").Bold();
                        t.Span(".");
                    });
                    
                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Data de Autorização").SemiBold();
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Setor Autorizador").SemiBold();
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{exumacao.DataAutorizacao:dd/MM/yyyy HH:mm}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{exumacao.SetorAutorizador}");
                    });
                    
                    if (exumacao.OssuarioDestinoId.HasValue)
                    {
                        x.Item().PaddingTop(10).Background(Colors.Blue.Lighten5).Padding(10).Text(t => 
                        {
                            t.Span("Destino aprovado dos restos mortais: ").SemiBold();
                            t.Span($"Ossuário ID {exumacao.OssuarioDestinoId}").Bold().FontColor(Colors.Blue.Darken2);
                        });
                    }
                    
                    if (!string.IsNullOrEmpty(exumacao.Observacoes))
                    {
                        x.Item().PaddingTop(10).Text("Observações Adicionais:").SemiBold();
                        x.Item().Padding(5).Text(exumacao.Observacoes).Italic();
                    }
                });
                
                page.Footer().Column(footerCol =>
                {
                    footerCol.Item().PaddingBottom(20).Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Assinatura do Requerente").FontSize(10);
                        });
                        row.ConstantItem(50);
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Autorização SAAF").FontSize(10);
                        });
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }
}
