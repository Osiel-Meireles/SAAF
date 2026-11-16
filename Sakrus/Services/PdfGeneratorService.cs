using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sakrus.Core.Entities;
using System;
using System.IO;
using System.Collections.Generic;

namespace Sakrus.Services;

public class PdfGeneratorService
{
    private static readonly byte[]? Brasao = CarregarBrasao();

    public PdfGeneratorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static byte[]? CarregarBrasao()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Assets", "brasao.png");
        return File.Exists(caminho) ? File.ReadAllBytes(caminho) : null;
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
                    row.ConstantItem(120).Height(60).Element(c =>
                    {
                        if (Brasao != null) c.Image(Brasao).FitArea();
                    });

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

    private static string DescreverOrigemExumacao(ExumacaoRegistro e)
    {
        if (e.GavetaPublica != null)
        {
            var g = e.GavetaPublica;
            return $"Gaveta pública {g.NumeroGaveta} - Quadra {g.Quadra}, Setor {g.Setor}, Lote {g.Lote}";
        }
        if (e.Jazigo != null)
        {
            var j = e.Jazigo;
            return $"Jazigo {j.CodigoIdentificador} - Quadra {j.Quadra}, Ala {j.Ala}, Lote {j.NumeroLote}";
        }
        return "Não informado";
    }

    private static string DescreverDestinoExumacao(ExumacaoRegistro e)
    {
        if (e.OssuarioDestino != null)
        {
            var o = e.OssuarioDestino;
            return $"Ossuário {o.Identificador} - Quadra {o.Quadra}, Ala {o.Ala}, Lote {o.NumeroLote}";
        }
        if (e.OssuarioDestinoId.HasValue)
            return $"Ossuário ID {e.OssuarioDestinoId}";
        return "Não informado";
    }

    private static string QualificarPessoa(Responsavel r)
    {
        var partes = new List<string> { r.Nome.ToUpperInvariant() };
        if (!string.IsNullOrWhiteSpace(r.EstadoCivil)) partes.Add(r.EstadoCivil.ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(r.Profissao)) partes.Add(r.Profissao.ToLowerInvariant());
        partes.Add($"inscrito(a) no CPF sob Nº {(string.IsNullOrWhiteSpace(r.CPF) ? "____________" : Documentos.DadosComuns.FormatarCpf(r.CPF))}");
        var rg = string.IsNullOrWhiteSpace(r.RG) ? "____________" : r.RG;
        var org = string.IsNullOrWhiteSpace(r.OrgaoEmissor) ? "" : $" {r.OrgaoEmissor}";
        partes.Add($"portador(a) do RG Nº {rg}{org}");
        var end = new List<string>();
        if (!string.IsNullOrWhiteSpace(r.Endereco)) end.Add(r.Endereco);
        if (!string.IsNullOrWhiteSpace(r.Numero)) end.Add(r.Numero);
        if (!string.IsNullOrWhiteSpace(r.Bairro)) end.Add(r.Bairro);
        partes.Add($"domiciliado(a) em {(end.Count > 0 ? string.Join(", ", end) : "____________")}, {(string.IsNullOrWhiteSpace(r.MunicipioUf) ? "Luís Eduardo Magalhães/BA" : r.MunicipioUf)}");
        return string.Join(", ", partes);
    }

    public byte[] GerarContratoAutorizacaoJazigoPdf(Jazigo jazigo, Responsavel autorizante, Responsavel autorizado, DateTime? data = null)
    {
        var dataContrato = data ?? DateTime.Now;
        var cemiterio = jazigo.Cemiterio?.Nome ?? "CEMITÉRIO MUNICIPAL JARDIM DA SAUDADE";
        var classificacao = jazigo.ClassificacaoEspaco?.Nome ?? "—";
        var cultura = new System.Globalization.CultureInfo("pt-BR");

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().PaddingBottom(8).Row(row =>
                {
                    row.ConstantItem(80).Height(60).Element(c => { if (Brasao != null) c.Image(Brasao).FitArea(); });
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().AlignCenter().Text("ESTADO DA BAHIA").Bold().FontSize(10);
                        col.Item().AlignCenter().Text("PREFEITURA MUNICIPAL DE LUÍS EDUARDO MAGALHÃES").Bold().FontSize(10);
                        col.Item().AlignCenter().Text("SECRETARIA MUNICIPAL DA FAZENDA").Bold().FontSize(10);
                        col.Item().AlignCenter().Text("COORDENAÇÃO DE ADMINISTRAÇÃO FUNERÁRIA E ATENDIMENTO À FAMÍLIA ENLUTADA").Bold().FontSize(9);
                    });
                });

                page.Content().Column(x =>
                {
                    x.Spacing(10);
                    x.Item().PaddingVertical(6).AlignCenter().Text("CONTRATO DE AUTORIZAÇÃO DE USO E GERENCIAMENTO DE JAZIGO CEMITERIAL").Bold().FontSize(13);

                    x.Item().Text(t =>
                    {
                        t.Justify();
                        t.Span("O(a) Concessionário(a) ");
                        t.Span(QualificarPessoa(autorizante)).Bold();
                        t.Span(", atual titular sobre a concessão pública, de direito de aproveitamento de uso com afetação especial e nominativa, em conformidade com o disposto na Lei Municipal Nº 877/2019, ora denominado(a) ");
                        t.Span("AUTORIZANTE").Bold();
                        t.Span(" e de outro lado, ");
                        t.Span(QualificarPessoa(autorizado)).Bold();
                        t.Span(", ora denominado(a) ");
                        t.Span("AUTORIZADO").Bold();
                        t.Span(", têm entre si, justo e contratado o presente instrumento de Autorização de uso e Gerenciamento de Jazigo Cemiterial, nas condições estipuladas neste ato e nas prerrogativas legais da Lei Municipal Nº 877/2019.");
                    });

                    x.Item().Text("CLÁUSULA PRIMEIRA – DO OBJETO").Bold();
                    x.Item().Text("Neste ato fica pactuada a autorização de uso e gerenciamento, referente ao jazigo localizado:").AlignLeft();
                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(2); c.RelativeColumn(4); });
                        void Cab(string s) => table.Cell().Border(0.5f).Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(s).Bold().FontSize(9);
                        void Val(string s) => table.Cell().Border(0.5f).Padding(4).AlignCenter().Text(s);
                        Cab("QUADRA"); Cab("ALA"); Cab("LOTE / GAVETA"); Cab("CLASSIFICAÇÃO DO JAZIGO");
                        Val(jazigo.Quadra); Val(jazigo.Ala ?? "—"); Val(jazigo.NumeroLote); Val(classificacao);
                    });
                    x.Item().Text(t => { t.Span("Cemitério: ").Bold(); t.Span(cemiterio.ToUpperInvariant()); });

                    x.Item().Text("CLÁUSULA SEGUNDA – DAS CONDICIONANTES").Bold();
                    x.Item().Text("O(a) AUTORIZADO declara que cumprirá todos os requisitos legais pertinentes para consolidar a autorização de uso e gerenciamento, assim como todas as normas dispostas na Lei Municipal Nº 877/2019, tais como, mas não restringindo a:").AlignLeft();
                    x.Item().PaddingLeft(14).Column(l =>
                    {
                        l.Spacing(3);
                        l.Item().Text("a) Utilização e conservação correta do jazigo, na forma definida em legislação vigente;");
                        l.Item().Text("b) Pagamento de eventuais taxas de serviços por ele solicitado e/ou autorizado e entrega de todas as documentações exigidas pela CAAFE.");
                    });
                    x.Item().Text("Por meio deste contrato o autorizado permite a emissão e lançamento de taxas em seu nome.");
                    x.Item().Text("O(a) Autorizado declara ainda que está ciente de que, se comprovadamente falsa a declaração e/ou documentação, sujeitar-se-á às sanções civis, administrativas e criminais previstas nas legislações aplicáveis.");

                    x.Item().Text("CLÁUSULA TERCEIRA – DAS OBRIGAÇÕES DO AUTORIZADO").Bold();
                    x.Item().Text("Constituem-se obrigações do Autorizado:");
                    x.Item().PaddingLeft(14).Column(l =>
                    {
                        l.Spacing(3);
                        l.Item().Text("a) Apresentar todos os documentos legais necessários à efetivação deste contrato;");
                        l.Item().Text("b) Manter atualizados os seus dados cadastrais, bem como comunicar qualquer alteração perante a Coordenação de Administração Funerária e Atendimento à Família Enlutada – CAAFE;");
                        l.Item().Text("c) Manter em boas condições de segurança, salubridade e decência o jazigo cujo direito de uso lhe foi permitido.");
                    });

                    x.Item().Text("CLÁUSULA QUARTA – DO FORO").Bold();
                    x.Item().Text("As partes estabelecem o foro da cidade de Luís Eduardo Magalhães, estado da Bahia, para dirimir possíveis dúvidas ou litígios decorrentes da execução deste contrato.");

                    x.Item().PaddingTop(4).Text("E por estarem justos e contratados, AUTORIZANTE e AUTORIZADO assinam o presente em (03) três vias de igual teor, para um único efeito, na presença das testemunhas abaixo.");
                    x.Item().ShowEntire().Column(fim =>
                    {
                        fim.Item().PaddingTop(8).Text($"Luís Eduardo Magalhães, {dataContrato.ToString("d 'de' MMMM 'de' yyyy", cultura)}.");
                        fim.Item().PaddingTop(60).Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().LineHorizontal(0.5f);
                                c.Item().AlignCenter().Text(autorizante.Nome.ToUpperInvariant()).FontSize(10);
                                c.Item().AlignCenter().Text($"CPF {Documentos.DadosComuns.FormatarCpf(autorizante.CPF)} — AUTORIZANTE").FontSize(9);
                            });
                            r.ConstantItem(30);
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().LineHorizontal(0.5f);
                                c.Item().AlignCenter().Text(autorizado.Nome.ToUpperInvariant()).FontSize(10);
                                c.Item().AlignCenter().Text($"CPF {Documentos.DadosComuns.FormatarCpf(autorizado.CPF)} — AUTORIZADO").FontSize(9);
                            });
                        });
                        fim.Item().PaddingTop(50).Row(r =>
                        {
                            r.RelativeItem().Column(c => { c.Item().LineHorizontal(0.5f); c.Item().AlignCenter().Text("Testemunha 1 — Nome e CPF").FontSize(9); });
                            r.ConstantItem(30);
                            r.RelativeItem().Column(c => { c.Item().LineHorizontal(0.5f); c.Item().AlignCenter().Text("Testemunha 2 — Nome e CPF").FontSize(9); });
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
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                {
                    row.ConstantItem(120).Height(60).Element(c =>
                    {
                        if (Brasao != null) c.Image(Brasao).FitArea();
                    });

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
                        t.Span("Requerimento formal para exumação dos restos mortais do falecido ");
                        t.Span(exumacao.Falecido != null ? $"{exumacao.Falecido.Nome} (ID {exumacao.FalecidoId})" : $"{exumacao.FalecidoId}").Bold();
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
                    
                    x.Item().PaddingTop(10).Column(loc =>
                    {
                        loc.Spacing(6);
                        loc.Item().Background(Colors.Grey.Lighten4).Padding(10).Text(t =>
                        {
                            t.Span("Local de origem (onde o corpo estava): ").SemiBold();
                            t.Span(DescreverOrigemExumacao(exumacao)).Bold();
                        });
                        loc.Item().Background(Colors.Blue.Lighten5).Padding(10).Text(t =>
                        {
                            t.Span("Local de destino (para onde o corpo foi): ").SemiBold();
                            t.Span(DescreverDestinoExumacao(exumacao)).Bold().FontColor(Colors.Blue.Darken2);
                        });
                    });

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
