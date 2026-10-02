using Sakrus.Core.Entities;
using System.Globalization;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Sakrus.Services.Documentos;

public static class DocumentoLayout
{
    private static readonly byte[]? Brasao = CarregarAsset("brasao.png");

    public static readonly byte[]? AssinaturaResponsavel = CarregarAsset("assinatura-saaf.png");

    static DocumentoLayout()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static byte[]? CarregarAsset(string nome)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Assets", nome);
        return File.Exists(caminho) ? File.ReadAllBytes(caminho) : null;
    }

    public static byte[] Criar(ContextoEmissao ctx, string titulo, Action<ColumnDescriptor> conteudo)
    {
        var tituloNumero = ctx.Numero != null ? $"{titulo} Nº {ctx.Numero}" : titulo;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(60).Height(70).AlignMiddle().AlignLeft().Element(c =>
                        {
                            if (Brasao != null) c.Image(Brasao).FitArea();
                        });
                        row.RelativeItem().AlignMiddle().Column(c =>
                        {
                            c.Item().AlignCenter().Text(ctx.Instituicao.CabecalhoLinha1).Bold().FontSize(11);
                            c.Item().AlignCenter().Text(ctx.Instituicao.CabecalhoLinha2).Bold().FontSize(9);
                        });
                    });
                    col.Item().PaddingTop(6).AlignCenter().Text(tituloNumero).Bold().FontSize(13);
                });

                page.Content().PaddingTop(8).Column(conteudo);

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(8));
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}

public static class DocumentoBlocos
{
    private const string CaixaVaziaSvg =
        "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 12 12'>" +
        "<rect x='0.5' y='0.5' width='11' height='11' fill='none' stroke='black' stroke-width='1'/></svg>";

    private const string CaixaMarcadaSvg =
        "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 12 12'>" +
        "<rect x='0.5' y='0.5' width='11' height='11' fill='none' stroke='black' stroke-width='1'/>" +
        "<polyline points='2.5,6.5 5,9 9.5,3' fill='none' stroke='black' stroke-width='1.5'/></svg>";

    public static void Caixa(RowDescriptor row, bool marcado)
    {
        row.ConstantItem(12).Height(12).Svg(marcado ? CaixaMarcadaSvg : CaixaVaziaSvg);
    }

    private static void Opcoes(IContainer container, params (string rotulo, bool marcado)[] itens)
    {
        container.Row(row =>
        {
            foreach (var (rotulo, marcado) in itens)
            {
                row.AutoItem().PaddingRight(10).Row(r =>
                {
                    Caixa(r, marcado);
                    r.AutoItem().PaddingLeft(4).Text(rotulo).Bold();
                });
            }
        });
    }

    public static void FaixaServicos(this ColumnDescriptor col, bool liberacao, bool capela, bool sepultamento, bool translado)
    {
        col.Item().PaddingTop(4).Element(c => Opcoes(c,
            ("LIBERAÇÃO", liberacao), ("CAPELA", capela), ("SEPULTAMENTO", sepultamento), ("TRANSLADO", translado)));
    }

    public static void TituloBloco(this ColumnDescriptor col, string titulo)
    {
        col.Item().PaddingTop(8).Border(0.5f).Background(Colors.Grey.Lighten3).Padding(3).Text(titulo).Bold();
    }

    public static void BlocoTipoAtendimento(this ColumnDescriptor col, TipoAtendimentoFunerario? tipo, string? funeraria)
    {
        col.TituloBloco("TIPO DE ATENDIMENTO E FUNERÁRIA PRESTADORA DO SERVIÇO");
        col.Item().Border(0.5f).Padding(3).Column(c =>
        {
            c.Item().Element(e => Opcoes(e,
                ("PARTICULAR", tipo == TipoAtendimentoFunerario.Particular),
                ("ASSISTÊNCIA", tipo == TipoAtendimentoFunerario.Assistencia),
                ("ASSOCIADO", tipo is TipoAtendimentoFunerario.Associado or TipoAtendimentoFunerario.PlanoFuneral),
                ("AUXÍLIO FUNERAL", tipo == TipoAtendimentoFunerario.AuxilioFuneral)));
            c.Item().PaddingTop(3).Text(t =>
            {
                t.Span("FUNERÁRIA: ").Bold();
                t.Span(funeraria ?? string.Empty);
            });
        });
    }

    public static void TabelaModelo(this ColumnDescriptor col, params (string rotulo, string? valor, int span)[][] linhas)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                for (var i = 0; i < 12; i++)
                    c.RelativeColumn();
            });

            foreach (var linha in linhas)
            {
                foreach (var (rotulo, valor, span) in linha)
                {
                    table.Cell().ColumnSpan((uint)Math.Clamp(span, 1, 12)).Border(0.5f).Padding(3).Text(t =>
                    {
                        t.Span(rotulo.ToUpperInvariant() + ": ").Bold().FontSize(9);
                        t.Span(valor ?? string.Empty);
                    });
                }
            }
        });
    }

    public static void Declaracao(this ColumnDescriptor col, string texto)
    {
        col.TituloBloco("DECLARAÇÃO");
        col.Item().Border(0.5f).Padding(4).Text(t =>
        {
            t.Span(texto);
            t.Justify();
        });
    }

    public static void FechoModelo(this ColumnDescriptor col, ContextoEmissao ctx, string rotuloEsq, string rotuloDir)
    {
        var data = ctx.DataEmissao.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("pt-BR"));
        col.Item().PaddingTop(14).Text($"Luís Eduardo Magalhães, {data}");
        col.BlocoAssinaturas(rotuloEsq, rotuloDir);
    }

    public static void Secao(this ColumnDescriptor col, string titulo)
    {
        col.Item().PaddingTop(8).Background(Colors.Grey.Lighten3).Padding(4).Text(titulo).Bold();
    }

    public static void Paragrafo(this ColumnDescriptor col, string texto, bool justificado = true)
    {
        col.Item().PaddingTop(6).Text(t =>
        {
            t.Span(texto);
            if (justificado)
                t.Justify();
        });
    }

    public static void Campos(this ColumnDescriptor col, params (string rotulo, string? valor)[] itens)
    {
        if (itens.Length == 0)
            return;

        col.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
            });

            for (var i = 0; i < itens.Length; i += 2)
            {
                table.Cell().Padding(2).Text(t =>
                {
                    t.Span($"{itens[i].rotulo}: ").SemiBold();
                    t.Span(FormatarValor(itens[i].valor));
                });

                if (i + 1 < itens.Length)
                {
                    table.Cell().Padding(2).Text(t =>
                    {
                        t.Span($"{itens[i + 1].rotulo}: ").SemiBold();
                        t.Span(FormatarValor(itens[i + 1].valor));
                    });
                }
            }
        });
    }

    public static void Tabela(this ColumnDescriptor col, IReadOnlyList<string> cabecalho, IEnumerable<IReadOnlyList<string>> linhas)
    {
        col.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                for (var i = 0; i < cabecalho.Count; i++)
                    c.RelativeColumn();
            });

            table.Header(h =>
            {
                foreach (var celula in cabecalho)
                    h.Cell().Border(0.5f).Background(Colors.Grey.Lighten3).Padding(4).Text(celula).SemiBold();
            });

            foreach (var linha in linhas)
            {
                foreach (var celula in linha)
                    table.Cell().Border(0.5f).Padding(4).Text(celula);
            }
        });
    }

    public static void BlocoAssinaturas(this ColumnDescriptor col, params string[] rotulos)
    {
        if (rotulos.Length == 0)
            return;

        var altura = AlturaSlotAssinatura(rotulos);

        col.Item().PaddingTop(16).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
            });

            for (var i = 0; i < rotulos.Length; i += 2)
            {
                table.Cell().Padding(8).SlotAssinatura(rotulos[i], altura);

                if (i + 1 < rotulos.Length)
                    table.Cell().Padding(8).SlotAssinatura(rotulos[i + 1], altura);
            }
        });
    }

    private static void SlotAssinatura(this IContainer container, string rotulo, float altura)
    {
        container.Column(c =>
        {
            if (TemAssinaturaResponsavel(rotulo))
                c.Item().ImageAssinaturaResponsavel(AlturaSlotComAssinatura);
            else
                c.Item().Height(altura);

            c.Item().LineHorizontal(0.5f);
            c.Item().AlignCenter().Text(rotulo).FontSize(10);
        });
    }

    private static bool TemAssinaturaResponsavel(string rotulo)
        => rotulo.Contains("respons", StringComparison.OrdinalIgnoreCase);

    private static float AlturaSlotAssinatura(params string[] rotulos)
        => DocumentoLayout.AssinaturaResponsavel != null && rotulos.Any(TemAssinaturaResponsavel)
            ? AlturaSlotComAssinatura
            : AlturaSlotSemAssinatura;

    private const float AlturaSlotSemAssinatura = 56;
    public const float AlturaSlotComAssinatura = 90;

    public static void ImageAssinaturaResponsavel(this IContainer container, float altura = AlturaSlotComAssinatura)
    {
        if (DocumentoLayout.AssinaturaResponsavel != null)
            container.Height(altura).AlignBottom().AlignCenter()
                .Image(DocumentoLayout.AssinaturaResponsavel).FitArea();
    }

    public static void AssinaturaCoordenacao(this ColumnDescriptor col, ContextoEmissao ctx)
    {
        var nome = string.IsNullOrWhiteSpace(ctx.Instituicao.NomeCoordenador)
            ? "[NOME DO(A) COORDENADOR(A)]"
            : ctx.Instituicao.NomeCoordenador;

        col.Item().ShowEntire().PaddingTop(16).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Height(56);
                c.Item().LineHorizontal(0.5f);
                c.Item().AlignCenter().Text(nome).SemiBold();
                c.Item().AlignCenter().Text(ctx.Instituicao.CargoCoordenador).FontSize(10);
            });
            row.ConstantItem(30);
            row.ConstantItem(140).Height(70).Border(0.5f).Column(c =>
            {
                c.Item().AlignCenter().Text("Carimbo").FontSize(9);
            });
        });
    }

    public static void AssinaturaPresidenteConselho(this ColumnDescriptor col, ContextoEmissao ctx)
    {
        const string cargo = "Presidente do Conselho Municipal Deliberativo dos Serviços Funerários";
        var nome = string.IsNullOrWhiteSpace(ctx.Instituicao.NomePresidenteConselho)
            ? "[NOME DO PRESIDENTE]"
            : ctx.Instituicao.NomePresidenteConselho;

        col.Item().ShowEntire().PaddingTop(16).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Height(56);
                c.Item().LineHorizontal(0.5f);
                c.Item().AlignCenter().Text(nome).SemiBold();
                c.Item().AlignCenter().Text(cargo).FontSize(10);
            });
            row.ConstantItem(30);
            row.ConstantItem(140).Height(70).Border(0.5f).Column(c =>
            {
                c.Item().AlignCenter().Text("Carimbo").FontSize(9);
            });
        });
    }

    public static void LocalEData(this ColumnDescriptor col, ContextoEmissao ctx)
    {
        var dataExtenso = ctx.DataEmissao.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("pt-BR"));
        col.Item().PaddingTop(12).Text($"{ctx.Instituicao.MunicipioEmissao} - {ctx.Instituicao.UfEmissao}, {dataExtenso}");
    }

    private static string FormatarValor(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? "—" : valor;

    public static void ListaComCaixas(this ColumnDescriptor col, IEnumerable<string> itens, bool marcado = true)
    {
        col.Item().Border(0.5f).Padding(4).Column(c =>
        {
            foreach (var item in itens)
            {
                c.Item().PaddingTop(2).Row(r =>
                {
                    Caixa(r, marcado);
                    r.AutoItem().PaddingLeft(4).Text(item);
                });
            }
        });
    }
}
