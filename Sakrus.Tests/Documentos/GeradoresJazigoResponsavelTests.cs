using System.Text;
using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Core.Helpers;
using Sakrus.Infrastructure.Data;
using Sakrus.Services.Documentos;

namespace Sakrus.Tests.Documentos;

/// <summary>
/// Cobre os documentos vinculados a jazigo/responsável que não dependem de atendimento:
/// 11 (termo de responsabilidade), 15 (requerimento de lote), 16 (autorização de obra),
/// 17 (informação de disponibilidade), 19 (folha financeira) e 20 (folha de regularização).
/// </summary>
public class GeradoresJazigoResponsavelTests
{
    private static readonly DateTime DataEmissao = new(2026, 10, 2, 10, 0, 0);

    private sealed record Base(
        TestDbContextFactory Factory,
        int JazigoId,
        int TitularId,
        int AutorizadaId,
        int EmpresaId,
        int FalecidoId,
        int GavetaPublicaId);

    private static async Task<Base> CriarBaseAsync(
        decimal largura = 2m,
        decimal comprimento = 3m,
        int exercicioValor = 2026,
        decimal valorM2 = 100m)
    {
        var factory = new TestDbContextFactory("jazigo-resp-" + Guid.NewGuid());
        using var db = factory.CreateDbContext();

        var cemiterio = new Cemiterio { Nome = "Bela Vista" };
        var classificacao = new ClassificacaoEspaco { Nome = "Jazigo Tipo A", Natureza = NaturezaEspaco.Jazigo };
        var classificacaoPublica = new ClassificacaoEspaco { Nome = "Sepultura Simples", Natureza = NaturezaEspaco.Sepultura };
        var modelo = new ModeloJazigo { Nome = "Tradicional", ValorManutencaoAnual = 100m };
        var titular = new Responsavel { Nome = "João Titular", CPF = "11122233344", RG = "MG-1", Endereco = "Rua A, 100", Telefone = "77 99999-0001" };
        var autorizada = new Responsavel { Nome = "Maria Autorizada", CPF = "55566677788" };
        var empresa = new Funeraria { Nome = "Funerária São Lucas", Ativo = true, EhExecutora = true };

        var falecido = new Falecido
        {
            Nome = "Antônio Falecido",
            DataFalecimento = new DateTime(2026, 4, 5),
            DataSepultamento = new DateTime(2026, 4, 8),
            CausaMorte = CausaMorte.Natural
        };

        var jazigo = new Jazigo
        {
            CodigoIdentificador = "J-01",
            Quadra = "3",
            Ala = "A",
            NumeroLote = "12",
            Largura = largura,
            Comprimento = comprimento,
            Revestimento = "Granito",
            Cemiterio = cemiterio,
            ClassificacaoEspaco = classificacao,
            ModeloJazigo = modelo
        };
        jazigo.Gavetas.Add(new Gaveta { Numero = "01", Falecido = falecido, DataSepultamento = new DateTime(2026, 4, 8) });
        jazigo.Gavetas.Add(new Gaveta { Numero = "02" });

        db.Cemiterios.Add(cemiterio);
        db.Jazigos.Add(jazigo);
        db.Responsaveis.AddRange(titular, autorizada);
        db.Funerarias.Add(empresa);
        db.ClassificacoesEspaco.AddRange(classificacao, classificacaoPublica);
        db.ValoresMetroQuadrado.Add(new ValorMetroQuadrado { Exercicio = exercicioValor, Valor = valorM2 });
        db.ConfiguracoesFinanceiras.Add(new ConfiguracaoFinanceira
        {
            ValorMetroQuadrado = valorM2,
            TaxaManutencaoBase = 50m,
            ValorConcessaoM2Quadras0102 = 276.66m,
            ValorConcessaoM2Demais = 366.55m
        });
        db.GavetasPublicas.Add(new GavetaPublica
        {
            Setor = "Planta Geral",
            Quadra = "5",
            Ala = "B",
            Lote = "07",
            NumeroGaveta = "01",
            Ocupada = false,
            Cemiterio = cemiterio,
            ClassificacaoEspaco = classificacaoPublica
        });
        db.JazigoProprietarios.Add(new JazigoProprietario
        {
            Jazigo = jazigo,
            Responsavel = titular,
            Ativo = true,
            TipoVinculo = TipoVinculoJazigo.Titular,
            DataAquisicao = new DateTime(exercicioValor, 1, 15)
        });
        await db.SaveChangesAsync();

        return new Base(factory, jazigo.Id, titular.Id, autorizada.Id, empresa.Id, falecido.Id,
            (await db.GavetasPublicas.FirstAsync()).Id);
    }

    private static ContextoEmissao Ctx(int entidadeId, params (string, string)[] campos)
    {
        var valores = new CamposValores();
        foreach (var (chave, valor) in campos)
            valores[chave] = valor;

        return new ContextoEmissao(entidadeId, valores, new ConfiguracaoInstitucional(), null, DataEmissao);
    }

    private static void AssertPdf(byte[] pdf)
        => Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));

    private static int ContarPaginas(byte[] pdf)
    {
        var ascii = Encoding.ASCII.GetString(pdf);
        int count = 0;
        int idx = 0;
        while ((idx = ascii.IndexOf("/Type /Page", idx, StringComparison.Ordinal)) >= 0)
        {
            int after = idx + "/Type /Page".Length;
            if (after >= ascii.Length || ascii[after] != 's')
                count++;
            idx = after;
        }
        return count == 0 ? 1 : count;
    }

    private static async Task<Jazigo> RecarregarJazigoAsync(Base base_)
    {
        using var db = base_.Factory.CreateDbContext();
        return await db.Jazigos
            .Include(j => j.Cemiterio)
            .Include(j => j.ClassificacaoEspaco)
            .Include(j => j.ModeloJazigo)
            .Include(j => j.Gavetas).ThenInclude(g => g.Falecido)
            .Include(j => j.Proprietarios).ThenInclude(p => p.Responsavel)
            .FirstAsync(j => j.Id == base_.JazigoId);
    }

    [Fact]
    public async Task Documento11_GeraPdfComTitularEPessoaAutorizada()
    {
        var b = await CriarBaseAsync();
        var gerador = new TermoResponsabilidadeAssuntosGerador(b.Factory);

        Assert.Null(await gerador.ValidarPreCondicoesAsync(b.JazigoId));

        var pdf = await gerador.GerarAsync(Ctx(b.JazigoId,
            ("pessoaAutorizadaId", b.AutorizadaId.ToString()),
            ("gavetaObjeto", "01"),
            ("testemunha1", "Teste — CPF 123")));

        AssertPdf(pdf);
    }

    [Fact]
    public async Task Documento11_SemTitularAtivo_Bloqueia()
    {
        var b = await CriarBaseAsync();
        using (var db = b.Factory.CreateDbContext())
        {
            var vinculo = await db.JazigoProprietarios.FirstAsync(p => p.JazigoId == b.JazigoId);
            vinculo.Ativo = false;
            await db.SaveChangesAsync();
        }

        var gerador = new TermoResponsabilidadeAssuntosGerador(b.Factory);

        Assert.Contains("titular", await gerador.ValidarPreCondicoesAsync(b.JazigoId));
        await Assert.ThrowsAsync<DocumentoBloqueadoException>(
            () => gerador.GerarAsync(Ctx(b.JazigoId, ("pessoaAutorizadaId", b.AutorizadaId.ToString()))));
    }

    [Fact]
    public async Task Documento11_SemPessoaAutorizada_Bloqueia()
    {
        var b = await CriarBaseAsync();
        var gerador = new TermoResponsabilidadeAssuntosGerador(b.Factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(() => gerador.GerarAsync(Ctx(b.JazigoId)));
        Assert.Contains("Pessoa autorizada", ex.Message);
    }

    [Fact]
    public async Task Documento15_GeraPdfComEnquadramentoSelecionado()
    {
        var b = await CriarBaseAsync();
        var gerador = new RequerimentoAquisicaoLoteGerador(b.Factory);

        var pdf = await gerador.GerarAsync(Ctx(b.TitularId,
            ("enquadramento", RequerimentoAquisicaoLoteGerador.Enquadramentos[0]),
            ("gavetaEscolhida", "U:" + b.GavetaPublicaId),
            ("documentacao", "RG, CPF e comprovante de residência")));

        AssertPdf(pdf);
        Assert.Equal(1, ContarPaginas(pdf));
    }

    [Fact]
    public async Task Documento15_EnquadramentoInvalido_Bloqueia()
    {
        var b = await CriarBaseAsync();
        var gerador = new RequerimentoAquisicaoLoteGerador(b.Factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(() => gerador.GerarAsync(Ctx(b.TitularId,
            ("enquadramento", "Qualquer outra coisa"),
            ("gavetaEscolhida", "U:" + b.GavetaPublicaId))));

        Assert.Contains("Enquadramento inválido", ex.Message);
    }

    [Fact]
    public async Task Documento15_SemLocalPretendido_Bloqueia()
    {
        var b = await CriarBaseAsync();
        var gerador = new RequerimentoAquisicaoLoteGerador(b.Factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(() => gerador.GerarAsync(Ctx(b.TitularId,
            ("enquadramento", RequerimentoAquisicaoLoteGerador.Enquadramentos[1]))));

        Assert.Contains("Localização do lote pretendido", ex.Message);
    }

    [Fact]
    public async Task Documento16_GeraPdfComEmpresaDoCadastro()
    {
        var b = await CriarBaseAsync();
        var gerador = new AutorizacaoConstrucaoGerador(b.Factory);

        Assert.Null(await gerador.ValidarPreCondicoesAsync(b.JazigoId));

        var pdf = await gerador.GerarAsync(Ctx(b.JazigoId,
            ("finalidade", AutorizacaoConstrucaoGerador.Finalidades[0]),
            ("empresaExecutoraId", b.EmpresaId.ToString()),
            ("revestimento", "Mármore"),
            ("projetoAnexo", "true")));

        AssertPdf(pdf);
    }

    [Fact]
    public async Task Documento16_FinalidadeForaDaLista_Bloqueia()
    {
        var b = await CriarBaseAsync();
        var gerador = new AutorizacaoConstrucaoGerador(b.Factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(() => gerador.GerarAsync(Ctx(b.JazigoId,
            ("finalidade", "Construir um quiosque"),
            ("empresaExecutoraId", b.EmpresaId.ToString()))));

        Assert.Contains("Finalidade inválida", ex.Message);
    }

    [Fact]
    public async Task Documento16_EmpresaInexistente_Bloqueia()
    {
        var b = await CriarBaseAsync();
        var gerador = new AutorizacaoConstrucaoGerador(b.Factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(() => gerador.GerarAsync(Ctx(b.JazigoId,
            ("finalidade", AutorizacaoConstrucaoGerador.Finalidades[3]),
            ("empresaExecutoraId", "999999"))));

        Assert.Contains("Empresa", ex.Message);
    }

    [Fact]
    public async Task Documento17_GeraPdfComLocalDisponivel()
    {
        var b = await CriarBaseAsync();
        var gerador = new InformacaoDisponibilidadeGerador(b.Factory);

        var pdf = await gerador.GerarAsync(Ctx(b.TitularId,
            ("nomeFalecido", "Falecido de Teste"),
            ("cemiterio", "Cemitério Municipal Teste"),
            ("municipio", "Luís Eduardo Magalhães"),
            ("quadra", "A"),
            ("lote", "10"),
            ("gaveta", "05"),
            ("finalidade", "Sepultamento de despojos")));

        AssertPdf(pdf);
    }

    [Fact]
    public async Task Documento17_SemFalecido_Bloqueia()
    {
        var b = await CriarBaseAsync();
        var gerador = new InformacaoDisponibilidadeGerador(b.Factory);

        var ex = await Assert.ThrowsAsync<DocumentoBloqueadoException>(() => gerador.GerarAsync(Ctx(b.TitularId,
            ("cemiterio", "Cemitério Municipal Teste"))));

        Assert.Contains("Nome do falecido", ex.Message);
    }

    [Fact]
    public async Task Documento19_GeraPdfComLinhaPorExercicio()
    {
        var b = await CriarBaseAsync(exercicioValor: 2024, valorM2: 100m);
        var gerador = new FolhaFinanceiraGerador(b.Factory);

        Assert.Null(await gerador.ValidarPreCondicoesAsync(b.JazigoId));
        AssertPdf(await gerador.GerarAsync(Ctx(b.JazigoId)));

        var jazigo = await RecarregarJazigoAsync(b);
        using var db = b.Factory.CreateDbContext();
        var linhas = await FinanceiroJazigoComum.MontarLancamentosAsync(db, jazigo, new DateTime(2024, 1, 15), DataEmissao);

        // 2024 (concessão + manutenção), 2025 e 2026 (manutenção).
        Assert.Equal(4, linhas.Count);
        Assert.Equal(1, linhas.Count(l => l.Tipo == TipoLancamento.Concessao));
        Assert.Equal(100m, linhas.Single(l => l.Tipo == TipoLancamento.Manutencao && l.Exercicio == 2024).ValorDevido);
        // Quadra "3" (demais quadras): 6 m² × 366,55.
        Assert.Equal(2199.30m, linhas.Single(l => l.Tipo == TipoLancamento.Concessao).ValorDevido);
    }

    [Fact]
    public async Task Documento19_ExercicioPagoPreservaValorHistorico()
    {
        var b = await CriarBaseAsync(exercicioValor: 2024, valorM2: 100m);
        using (var db = b.Factory.CreateDbContext())
        {
            db.LancamentosFinanceiros.Add(new LancamentoFinanceiro
            {
                JazigoId = b.JazigoId,
                Exercicio = 2024,
                Tipo = TipoLancamento.Manutencao,
                Valor = 28m,
                Pago = true,
                DataPagamento = new DateTime(2024, 3, 10)
            });
            await db.SaveChangesAsync();
        }

        var jazigo = await RecarregarJazigoAsync(b);
        using var dbLeitura = b.Factory.CreateDbContext();
        var linhas = await FinanceiroJazigoComum.MontarLancamentosAsync(dbLeitura, jazigo, new DateTime(2024, 1, 15), DataEmissao);

        var pago2024 = linhas.Single(l => l.Tipo == TipoLancamento.Manutencao && l.Exercicio == 2024);
        Assert.Equal(28m, pago2024.ValorDevido);
        Assert.Equal(0m, pago2024.Saldo);

        // Exercício em aberto usa o valor vigente do tipo de jazigo, sem juros.
        var aberto2025 = linhas.Single(l => l.Tipo == TipoLancamento.Manutencao && l.Exercicio == 2025);
        Assert.Equal(100m, aberto2025.ValorDevido);
        Assert.Equal(100m, aberto2025.Saldo);
    }

    [Fact]
    public async Task Documento19_LancamentoPago_NaoGeraSaldo()
    {
        var b = await CriarBaseAsync(exercicioValor: DataEmissao.Year, valorM2: 100m);
        using (var db = b.Factory.CreateDbContext())
        {
            db.LancamentosFinanceiros.Add(new LancamentoFinanceiro
            {
                JazigoId = b.JazigoId,
                Exercicio = DataEmissao.Year,
                Tipo = TipoLancamento.Manutencao,
                Valor = 600m,
                Pago = true,
                DataPagamento = DataEmissao,
                Observacao = "REC-100"
            });
            await db.SaveChangesAsync();
        }

        var jazigo = await RecarregarJazigoAsync(b);
        using var dbLeitura = b.Factory.CreateDbContext();
        var linhas = await FinanceiroJazigoComum.MontarLancamentosAsync(dbLeitura, jazigo, DataEmissao, DataEmissao);

        var manutencao = linhas.Single(l => l.Tipo == TipoLancamento.Manutencao);
        Assert.Equal(600m, manutencao.ValorPago);
        Assert.Equal(0m, manutencao.Saldo);
        Assert.False(manutencao.EmAberto);
        Assert.Equal("REC-100", manutencao.Documento);
    }

    [Fact]
    public async Task Documento19_SemArea_Bloqueia()
    {
        var b = await CriarBaseAsync(largura: 0m, comprimento: 0m);
        using (var db = b.Factory.CreateDbContext())
        {
            var jazigo = await db.Jazigos.FirstAsync(j => j.Id == b.JazigoId);
            jazigo.Largura = null;
            jazigo.Comprimento = null;
            await db.SaveChangesAsync();
        }

        var gerador = new FolhaFinanceiraGerador(b.Factory);
        Assert.Contains("área", await gerador.ValidarPreCondicoesAsync(b.JazigoId));
    }

    [Fact]
    public async Task Documento20_GeraPdfEComposicaoDoTotal()
    {
        var b = await CriarBaseAsync(exercicioValor: 2024, valorM2: 100m);
        var gerador = new FolhaRegularizacaoGerador(b.Factory);

        AssertPdf(await gerador.GerarAsync(Ctx(b.JazigoId)));

        var jazigo = await RecarregarJazigoAsync(b);
        using var db = b.Factory.CreateDbContext();
        var linhas = await FinanceiroJazigoComum.MontarLancamentosAsync(db, jazigo, new DateTime(2024, 1, 15), DataEmissao);

        var area = jazigo.AreaM2!.Value;
        Assert.Equal(6m, area);

        var taxaConcessao = RegrasDocumentos.CalcularValor(area, 366.55m);
        Assert.Equal(2199.30m, taxaConcessao);

        var maxParcelas = RegrasDocumentos.MaxParcelasPermitidas(taxaConcessao);
        Assert.Equal(12, maxParcelas);

        var parcelas = RegrasDocumentos.SimularParcelamento(taxaConcessao, maxParcelas);
        Assert.Equal(taxaConcessao, parcelas.Sum(p => p.Valor));
        Assert.All(parcelas, p => Assert.True(p.Valor >= RegrasDocumentos.ParcelaMinima));

        var totalManutencoes = FinanceiroJazigoComum.FiltrarTipo(linhas, TipoLancamento.Manutencao).Sum(l => l.ValorDevido);
        Assert.Equal(300m, totalManutencoes);
        Assert.Equal(2499.30m, taxaConcessao + totalManutencoes);
    }

    [Fact]
    public async Task Documento20_ParcelamentoRespondeMinimoDeCemReais()
    {
        var b = await CriarBaseAsync(largura: 2m, comprimento: 3m, exercicioValor: DataEmissao.Year, valorM2: 100m);
        var gerador = new FolhaRegularizacaoGerador(b.Factory);

        AssertPdf(await gerador.GerarAsync(Ctx(b.JazigoId, ("parcelas", "3"))));
        AssertPdf(await gerador.GerarAsync(Ctx(b.JazigoId, ("parcelas", "50"))));
    }

    [Fact]
    public void Catalogo_CodigosUnicosESeisNovosRegistraveis()
    {
        var tipos = typeof(DocumentosServiceCollectionExtensions).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IDocumentoGerador).IsAssignableFrom(t))
            .Select(t => (IDocumentoGerador)Activator.CreateInstance(t, new object?[] { null })!)
            .ToList();

        var codigos = tipos.Select(t => t.Codigo).ToList();
        Assert.Equal(codigos.Count, codigos.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        foreach (var codigo in new[] { "11", "15", "16", "17", "19", "20" })
            Assert.Contains(codigo, codigos, StringComparer.OrdinalIgnoreCase);
    }
}
