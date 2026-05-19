using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sakrus.Core.Entities;
using System;

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
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PREFEITURA MUNICIPAL DE LUÍS EDUARDO MAGALHÃES").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("SISTEMA SAKRUS - GESTÃO CEMITERIAL").FontSize(9);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("TERMO DE CONCESSÃO").FontSize(14).SemiBold();
                        col.Item().Text($"Emissão: {DateTime.Now:dd/MM/yyyy}");
                    });
                });

                // CONTEÚDO
                page.Content().PaddingVertical(20).Column(x =>
                {
                    x.Spacing(10);
                    
                    x.Item().AlignCenter().Text("TERMO DE CONCESSÃO DE USO DE JAZIGO").FontSize(16).Bold().Underline();
                    
                    x.Item().PaddingTop(15).Text(t =>
                    {
                        t.Span("Pelo presente instrumento, o Município de Luís Eduardo Magalhães concede o direito de uso perpétuo do ");
                        t.Span($"Jazigo Código {jazigo.CodigoIdentificador}").Bold();
                        t.Span(" ao concessionário abaixo qualificado, de acordo com as leis municipais vigentes.");
                    });

                    x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    
                    x.Item().Text("DADOS DO CONCESSIONÁRIO").SemiBold().FontSize(12);
                    x.Item().Text($"Nome: {responsavel.Nome}");
                    x.Item().Text($"CPF: {responsavel.CPF}");
                    x.Item().Text($"Endereço: {responsavel.Endereco}");

                    x.Item().PaddingTop(10).Text("DADOS DO JAZIGO").SemiBold().FontSize(12);
                    x.Item().Text($"Identificador: {jazigo.CodigoIdentificador}");
                    x.Item().Text($"Modelo: {(jazigo.ModeloJazigo != null ? jazigo.ModeloJazigo.Nome : "Não Especificado")}");
                    x.Item().Text($"Infantil: {(jazigo.IsInfantil ? "Sim" : "Não")}");

                    if (historico != null)
                    {
                        x.Item().PaddingTop(10).Text("HISTÓRICO DE TITULARIDADE").SemiBold().FontSize(12).FontColor(Colors.Grey.Darken2);
                        x.Item().Text($"Transferido em: {historico.DataTransferencia:dd/MM/yyyy}");
                        x.Item().Text($"Motivo: {historico.Motivo}");
                    }
                });

                // RODAPÉ
                page.Footer().PaddingTop(40).Column(footerCol =>
                {
                    footerCol.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Concessionário");
                        });
                        row.ConstantItem(50);
                        row.RelativeItem().Column(c => {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Administração do Cemitério");
                        });
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

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PREFEITURA MUNICIPAL DE LUÍS EDUARDO MAGALHÃES").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("REQUERIMENTO DE EXUMAÇÃO").FontSize(12);
                    });
                });

                page.Content().PaddingVertical(20).Column(x =>
                {
                    x.Spacing(10);
                    x.Item().Text($"Requerimento formal para exumação dos restos mortais do falecido ID {exumacao.FalecidoId}.");
                    x.Item().Text($"Data de Autorização: {exumacao.DataAutorizacao:dd/MM/yyyy}");
                    x.Item().Text($"Setor Autorizador: {exumacao.SetorAutorizador}");
                    
                    if (exumacao.OssuarioDestinoId.HasValue)
                    {
                        x.Item().Text("Destino dos restos mortais: Ossuário ID " + exumacao.OssuarioDestinoId).Bold();
                    }
                    
                    x.Item().PaddingTop(20).Text("Assinatura do Requerente:");
                    x.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Black);
                });
            });
        });

        return documento.GeneratePdf();
    }
}
