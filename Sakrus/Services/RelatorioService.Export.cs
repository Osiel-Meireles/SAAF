using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sakrus.Core.Entities;

namespace Sakrus.Services;

public partial class RelatorioService
{
    private void ConfigurarPaginaPadrao(PageDescriptor page, string tituloRelatorio, string periodo = "")
    {
        page.Size(PageSizes.A4);
        page.Margin(1, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(10));

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
                col.Item().Text(tituloRelatorio).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                if (!string.IsNullOrEmpty(periodo))
                    col.Item().Text($"Período: {periodo}").FontSize(10).SemiBold();
                col.Item().Text("SAAF - Sistema de Acolhimento e Assistência Funerária").FontSize(10).FontColor(Colors.Grey.Medium);
                col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });

        page.Footer().PaddingTop(10).AlignCenter().Text(x =>
        {
            x.Span("Documento gerado pelo SAAF - ");
            x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            x.Span(" - Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }

    // --- ATENDIMENTOS ---
    public byte[] GerarPdfAtendimentos(List<Atendimento> atendimentos, string periodo)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPaginaPadrao(page, "RELATÓRIO DE ATENDIMENTOS", periodo);
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(50); // ID
                            c.RelativeColumn();   // Falecido
                            c.RelativeColumn();   // Responsavel
                            c.ConstantColumn(80); // Data
                        });
                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).Text("ID").SemiBold();
                            h.Cell().BorderBottom(1).Text("Falecido").SemiBold();
                            h.Cell().BorderBottom(1).Text("Responsável").SemiBold();
                            h.Cell().BorderBottom(1).Text("Sepultamento").SemiBold();
                        });
                        foreach (var a in atendimentos)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(a.Id.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(a.Falecido?.Nome ?? "N/A");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(a.Responsavel?.Nome ?? "N/A");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(a.DataSepultamento?.ToString("dd/MM/yyyy") ?? "-");
                        }
                    });
                    col.Item().PaddingTop(10).AlignRight().Text($"Total de registros: {atendimentos.Count}").SemiBold();
                });
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] GerarTxtAtendimentos(List<Atendimento> atendimentos, string periodo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SAAF - RELATÓRIO DE ATENDIMENTOS");
        sb.AppendLine($"Período: {periodo}");
        sb.AppendLine($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"{"ID",-5} | {"Falecido",-30} | {"Responsável",-25} | {"Data",-10}");
        sb.AppendLine(new string('-', 80));
        foreach (var a in atendimentos)
        {
            var falecido = (a.Falecido?.Nome ?? "N/A").PadRight(30);
            if (falecido.Length > 30) falecido = falecido.Substring(0, 30);
            var resp = (a.Responsavel?.Nome ?? "N/A").PadRight(25);
            if (resp.Length > 25) resp = resp.Substring(0, 25);
            var data = (a.DataSepultamento?.ToString("dd/MM/yyyy") ?? "-").PadRight(10);
            sb.AppendLine($"{a.Id,-5} | {falecido} | {resp} | {data}");
        }
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"Total de registros: {atendimentos.Count}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    // --- JAZIGOS ---
    public byte[] GerarPdfJazigos(List<Jazigo> jazigos)
    {
        var total = jazigos.Count;
        var ocupados = jazigos.Count(j => j.Ocupado);
        var disponiveis = total - ocupados;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPaginaPadrao(page, "RELATÓRIO DE LOTAÇÃO DE JAZIGOS", "Geral");
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(5).Text($"Resumo de Ocupação").FontSize(14).SemiBold();
                    col.Item().Text($"Total de Jazigos: {total}");
                    col.Item().Text($"Jazigos Ocupados: {ocupados}");
                    col.Item().Text($"Jazigos Disponíveis: {disponiveis}").FontColor(Colors.Green.Darken2).SemiBold();
                    col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingVertical(10).Text("Lista de Jazigos").FontSize(14).SemiBold();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();   // Código
                            c.RelativeColumn();   // Modelo
                            c.RelativeColumn();   // Quadra/Ala
                            c.ConstantColumn(80); // Status
                        });
                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).Text("Código").SemiBold();
                            h.Cell().BorderBottom(1).Text("Modelo").SemiBold();
                            h.Cell().BorderBottom(1).Text("Quadra / Ala").SemiBold();
                            h.Cell().BorderBottom(1).Text("Status").SemiBold();
                        });
                        foreach (var j in jazigos.OrderBy(x => x.Quadra).ThenBy(x => x.CodigoIdentificador))
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(j.CodigoIdentificador);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(j.ModeloJazigo?.Nome ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text($"{j.Quadra} / {j.Ala}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2)
                                 .Text(j.Ocupado ? "Ocupado" : "Livre")
                                 .FontColor(j.Ocupado ? Colors.Red.Medium : Colors.Green.Medium);
                        }
                    });
                });
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] GerarTxtJazigos(List<Jazigo> jazigos)
    {
        var total = jazigos.Count;
        var ocupados = jazigos.Count(j => j.Ocupado);
        var disponiveis = total - ocupados;

        var sb = new StringBuilder();
        sb.AppendLine("SAAF - RELATÓRIO DE LOTAÇÃO DE JAZIGOS");
        sb.AppendLine($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"RESUMO:");
        sb.AppendLine($"Total de Jazigos: {total}");
        sb.AppendLine($"Jazigos Ocupados: {ocupados}");
        sb.AppendLine($"Jazigos Disponíveis: {disponiveis}");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"{"Código",-15} | {"Modelo",-20} | {"Quadra / Ala",-20} | {"Status",-10}");
        sb.AppendLine(new string('-', 80));
        foreach (var j in jazigos.OrderBy(x => x.Quadra).ThenBy(x => x.CodigoIdentificador))
        {
            var modelo = (j.ModeloJazigo?.Nome ?? "-").PadRight(20);
            if (modelo.Length > 20) modelo = modelo.Substring(0, 20);
            var qa = $"{j.Quadra} / {j.Ala}".PadRight(20);
            if (qa.Length > 20) qa = qa.Substring(0, 20);
            var status = (j.Ocupado ? "Ocupado" : "Livre").PadRight(10);
            sb.AppendLine($"{j.CodigoIdentificador,-15} | {modelo} | {qa} | {status}");
        }
        sb.AppendLine(new string('-', 80));
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    // --- FINANCEIRO ---
    public byte[] GerarPdfFinanceiro(List<Atendimento> atendimentos, string periodo)
    {
        var total = atendimentos.SelectMany(a => a.ItensFaturados).Sum(i => i.ValorTotalCalculado);
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPaginaPadrao(page, "RELATÓRIO FINANCEIRO (OS)", periodo);
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(5).Text($"Resumo Financeiro").FontSize(14).SemiBold();
                    col.Item().Text($"Total de OS Emitidas: {atendimentos.Count}");
                    col.Item().Text($"Faturamento Total: R$ {total:N2}").FontColor(Colors.Blue.Darken2).SemiBold();
                    col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingVertical(10).Text("Detalhamento por OS").FontSize(14).SemiBold();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(80); // Numero OS
                            c.ConstantColumn(80); // Data
                            c.RelativeColumn();   // Responsavel
                            c.ConstantColumn(80); // Valor
                        });
                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).Text("OS").SemiBold();
                            h.Cell().BorderBottom(1).Text("Data").SemiBold();
                            h.Cell().BorderBottom(1).Text("Responsável").SemiBold();
                            h.Cell().BorderBottom(1).AlignRight().Text("Valor").SemiBold();
                        });
                        foreach (var a in atendimentos)
                        {
                            var valorOs = a.ItensFaturados.Sum(i => i.ValorTotalCalculado);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(a.NumeroOsAuxilio);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(a.DataSepultamento?.ToString("dd/MM/yy") ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(a.Responsavel?.Nome ?? "N/A");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).AlignRight().Text($"R$ {valorOs:N2}");
                        }
                    });
                });
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] GerarTxtFinanceiro(List<Atendimento> atendimentos, string periodo)
    {
        var total = atendimentos.SelectMany(a => a.ItensFaturados).Sum(i => i.ValorTotalCalculado);
        var sb = new StringBuilder();
        sb.AppendLine("SAAF - RELATÓRIO FINANCEIRO (OS)");
        sb.AppendLine($"Período: {periodo}");
        sb.AppendLine($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"Total de OS Emitidas: {atendimentos.Count}");
        sb.AppendLine($"Faturamento Total: R$ {total:N2}");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"{"OS",-15} | {"Data",-10} | {"Responsável",-35} | {"Valor",-12}");
        sb.AppendLine(new string('-', 80));
        foreach (var a in atendimentos)
        {
            var valorOs = a.ItensFaturados.Sum(i => i.ValorTotalCalculado);
            var resp = (a.Responsavel?.Nome ?? "N/A").PadRight(35);
            if (resp.Length > 35) resp = resp.Substring(0, 35);
            var data = (a.DataSepultamento?.ToString("dd/MM/yyyy") ?? "-").PadRight(10);
            sb.AppendLine($"{a.NumeroOsAuxilio,-15} | {data} | {resp} | R$ {valorOs,9:N2}");
        }
        sb.AppendLine(new string('-', 80));
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    // --- EXUMAÇÕES ---
    public byte[] GerarPdfExumacoes(List<ExumacaoRegistro> exumacoes, string periodo)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPaginaPadrao(page, "RELATÓRIO DE EXUMAÇÕES", periodo);
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(80); // Data
                            c.RelativeColumn();   // Falecido
                            c.RelativeColumn();   // Executor
                            c.RelativeColumn();   // Observações
                        });
                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).Text("Data").SemiBold();
                            h.Cell().BorderBottom(1).Text("Falecido").SemiBold();
                            h.Cell().BorderBottom(1).Text("Executor").SemiBold();
                            h.Cell().BorderBottom(1).Text("Observações").SemiBold();
                        });
                        foreach (var e in exumacoes)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(e.DataExecucao?.ToString("dd/MM/yyyy") ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(e.Falecido?.Nome ?? "N/A");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(e.Executor.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(e.Observacoes ?? "-");
                        }
                    });
                    col.Item().PaddingTop(10).AlignRight().Text($"Total de exumações: {exumacoes.Count}").SemiBold();
                });
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] GerarTxtExumacoes(List<ExumacaoRegistro> exumacoes, string periodo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SAAF - RELATÓRIO DE EXUMAÇÕES");
        sb.AppendLine($"Período: {periodo}");
        sb.AppendLine($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine(new string('-', 100));
        sb.AppendLine($"{"Data",-10} | {"Falecido",-30} | {"Executor",-25} | {"Observações",-25}");
        sb.AppendLine(new string('-', 100));
        foreach (var e in exumacoes)
        {
            var falecido = (e.Falecido?.Nome ?? "N/A").PadRight(30);
            if (falecido.Length > 30) falecido = falecido.Substring(0, 30);
            var executor = (e.Executor.ToString()).PadRight(25);
            if (executor.Length > 25) executor = executor.Substring(0, 25);
            var obs = (e.Observacoes ?? "-").PadRight(25);
            if (obs.Length > 25) obs = obs.Substring(0, 25);
            
            var data = (e.DataExecucao?.ToString("dd/MM/yyyy") ?? "-").PadRight(10);
            sb.AppendLine($"{data} | {falecido} | {executor} | {obs}");
        }
        sb.AppendLine(new string('-', 100));
        sb.AppendLine($"Total de exumações: {exumacoes.Count}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    // --- ESTATÍSTICAS ---
    public byte[] GerarPdfEstatisticas(Dictionary<string, int> causas, string periodo)
    {
        var total = causas.Values.Sum();
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPaginaPadrao(page, "ESTATÍSTICAS DE CAUSAS DE ÓBITO", periodo);
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();   // Causa
                            c.ConstantColumn(80); // Quantidade
                            c.ConstantColumn(80); // %
                        });
                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).Text("Causa do Óbito").SemiBold();
                            h.Cell().BorderBottom(1).AlignRight().Text("Quantidade").SemiBold();
                            h.Cell().BorderBottom(1).AlignRight().Text("%").SemiBold();
                        });
                        foreach (var c in causas.OrderByDescending(x => x.Value))
                        {
                            var pct = total > 0 ? (c.Value / (double)total) * 100 : 0;
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).Text(c.Key);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).AlignRight().Text(c.Value.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(2).AlignRight().Text($"{pct:F1}%");
                        }
                    });
                    col.Item().PaddingTop(10).AlignRight().Text($"Total de Óbitos: {total}").SemiBold();
                });
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] GerarTxtEstatisticas(Dictionary<string, int> causas, string periodo)
    {
        var total = causas.Values.Sum();
        var sb = new StringBuilder();
        sb.AppendLine("SAAF - ESTATÍSTICAS DE CAUSAS DE ÓBITO");
        sb.AppendLine($"Período: {periodo}");
        sb.AppendLine($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine(new string('-', 60));
        sb.AppendLine($"{"Causa do Óbito",-30} | {"Quantidade",-10} | {"%",-10}");
        sb.AppendLine(new string('-', 60));
        foreach (var c in causas.OrderByDescending(x => x.Value))
        {
            var pct = total > 0 ? (c.Value / (double)total) * 100 : 0;
            sb.AppendLine($"{c.Key,-30} | {c.Value,10} | {pct,9:F1}%");
        }
        sb.AppendLine(new string('-', 60));
        sb.AppendLine($"Total de Óbitos: {total}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
