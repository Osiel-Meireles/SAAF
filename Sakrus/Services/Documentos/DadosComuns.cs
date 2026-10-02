using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

public record PessoaQualificacao(
    string Nome,
    string? Cpf,
    string? Rg,
    string? OrgaoEmissor,
    string? Telefone,
    string? Endereco,
    string? Profissao,
    string? EstadoCivil,
    string? Parentesco);

public record FalecidoQualificacao(
    string Nome,
    string? EstadoCivil,
    string? Sexo,
    int? Idade,
    string? Profissao,
    string? Endereco,
    string? LocalObito,
    string? LocalCorpo,
    DateTime? DataObito,
    string? NumeroDeclaracaoObito,
    string? NomePai,
    string? NomeMae,
    string? Naturalidade,
    string? CausaObito,
    string? Cartorio,
    string? MunicipioCartorio,
    string? MatriculaObito,
    DateTime? DataSepultamento);

public record LocalCemiterial(
    string Tipo,
    int Id,
    string Cemiterio,
    string? Municipio,
    string? Quadra,
    string? Ala,
    string? Lote,
    string? Gaveta,
    string? Classificacao,
    DateTime? DataSepultamento);

public static class DadosComuns
{
    public static async Task<PessoaQualificacao> ResponsavelAsync(ApplicationDbContext db, int responsavelId, string? parentesco = null)
    {
        var responsavel = await db.Responsaveis.FindAsync(responsavelId);
        if (responsavel == null)
            throw new DocumentoBloqueadoException("Responsável não encontrado");

        return new PessoaQualificacao(
            responsavel.Nome,
            FormatarCpf(responsavel.CPF),
            string.IsNullOrWhiteSpace(responsavel.RG) ? null : responsavel.RG,
            string.IsNullOrWhiteSpace(responsavel.OrgaoEmissor) ? null : responsavel.OrgaoEmissor,
            string.IsNullOrWhiteSpace(responsavel.Telefone) ? null : responsavel.Telefone,
            string.IsNullOrWhiteSpace(responsavel.Endereco) ? null : responsavel.Endereco,
            string.IsNullOrWhiteSpace(responsavel.Profissao) ? null : responsavel.Profissao,
            string.IsNullOrWhiteSpace(responsavel.EstadoCivil) ? null : responsavel.EstadoCivil,
            parentesco);
    }

    public static async Task<FalecidoQualificacao> FalecidoAsync(ApplicationDbContext db, int falecidoId)
    {
        var falecido = await db.Falecidos.FindAsync(falecidoId);
        if (falecido == null)
            throw new DocumentoBloqueadoException("Falecido não encontrado");

        return new FalecidoQualificacao(
            falecido.Nome,
            string.IsNullOrWhiteSpace(falecido.EstadoCivil) ? null : falecido.EstadoCivil,
            string.IsNullOrWhiteSpace(falecido.Sexo) ? null : falecido.Sexo,
            falecido.Idade,
            string.IsNullOrWhiteSpace(falecido.Profissao) ? null : falecido.Profissao,
            string.IsNullOrWhiteSpace(falecido.Endereco) ? null : falecido.Endereco,
            null,
            string.IsNullOrWhiteSpace(falecido.LocalCorpo) ? null : falecido.LocalCorpo,
            falecido.DataFalecimento,
            null,
            string.IsNullOrWhiteSpace(falecido.NomePai) ? null : falecido.NomePai,
            string.IsNullOrWhiteSpace(falecido.NomeMae) ? null : falecido.NomeMae,
            string.IsNullOrWhiteSpace(falecido.Naturalidade) ? null : falecido.Naturalidade,
            CausaMorteTexto(falecido.CausaMorte),
            string.IsNullOrWhiteSpace(falecido.CartorioRegistro) ? null : falecido.CartorioRegistro,
            string.IsNullOrWhiteSpace(falecido.MunicipioCartorio) ? null : falecido.MunicipioCartorio,
            string.IsNullOrWhiteSpace(falecido.MatriculaObito) ? null : falecido.MatriculaObito,
            falecido.DataSepultamento);
    }

    public static async Task<Jazigo> JazigoCompletoAsync(ApplicationDbContext db, int id)
    {
        var jazigo = await db.Jazigos
            .Include(j => j.Cemiterio)
            .Include(j => j.ClassificacaoEspaco)
            .Include(j => j.ModeloJazigo)
            .Include(j => j.Gavetas).ThenInclude(g => g.Falecido)
            .Include(j => j.Proprietarios).ThenInclude(p => p.Responsavel)
            .Include(j => j.Falecidos)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (jazigo == null)
            throw new DocumentoBloqueadoException("Jazigo não encontrado");

        return jazigo;
    }

    public static JazigoProprietario? TitularAtivo(Jazigo jazigo)
        => jazigo.Proprietarios.FirstOrDefault(p => p.Ativo && p.TipoVinculo == TipoVinculoJazigo.Titular);

    public static PessoaQualificacao Qualificacao(JazigoProprietario? vinculo)
        => vinculo?.Responsavel == null
            ? new PessoaQualificacao(string.Empty, null, null, null, null, null, null, null, null)
            : new PessoaQualificacao(
                vinculo.Responsavel.Nome,
                FormatarCpf(vinculo.Responsavel.CPF),
                string.IsNullOrWhiteSpace(vinculo.Responsavel.RG) ? null : vinculo.Responsavel.RG,
                string.IsNullOrWhiteSpace(vinculo.Responsavel.OrgaoEmissor) ? null : vinculo.Responsavel.OrgaoEmissor,
                string.IsNullOrWhiteSpace(vinculo.Responsavel.Telefone) ? null : vinculo.Responsavel.Telefone,
                string.IsNullOrWhiteSpace(vinculo.Responsavel.Endereco) ? null : vinculo.Responsavel.Endereco,
                string.IsNullOrWhiteSpace(vinculo.Responsavel.Profissao) ? null : vinculo.Responsavel.Profissao,
                string.IsNullOrWhiteSpace(vinculo.Responsavel.EstadoCivil) ? null : vinculo.Responsavel.EstadoCivil,
                TipoVinculoTexto(vinculo.TipoVinculo));

    public static string TipoVinculoTexto(TipoVinculoJazigo tipo)
        => tipo switch
        {
            TipoVinculoJazigo.Titular => "Titular",
            TipoVinculoJazigo.CoUsuario => "Co-usuário",
            TipoVinculoJazigo.Beneficiario => "Beneficiário",
            TipoVinculoJazigo.Herdeiro => "Herdeiro",
            _ => tipo.ToString()
        };

    public static string FormatarLocalJazigo(Jazigo jazigo, string? gaveta = null)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(jazigo.Cemiterio?.Nome))
            partes.Add(jazigo.Cemiterio.Nome);
        if (!string.IsNullOrWhiteSpace(jazigo.Quadra))
            partes.Add($"Quadra {jazigo.Quadra}");
        if (!string.IsNullOrWhiteSpace(jazigo.Ala))
            partes.Add($"Ala {jazigo.Ala}");
        if (!string.IsNullOrWhiteSpace(jazigo.NumeroLote))
            partes.Add($"Lote {jazigo.NumeroLote}");
        if (!string.IsNullOrWhiteSpace(gaveta))
            partes.Add($"Gaveta {gaveta}");
        if (!string.IsNullOrWhiteSpace(jazigo.ClassificacaoEspaco?.Nome))
            partes.Add($"({jazigo.ClassificacaoEspaco.Nome})");

        return partes.Count > 0 ? string.Join(" / ", partes) : "—";
    }

    public static string FormatarMoeda(decimal valor)
        => valor.ToString("C", CultureInfoPtBr);

    public static string FormatarArea(decimal? areaM2)
        => areaM2.HasValue ? $"{areaM2.Value.ToString("0.##", CultureInfoPtBr)} m²" : "—";

    public static string FormatarData(DateTime? data)
        => data?.ToString("dd/MM/yyyy", CultureInfoPtBr) ?? "—";

    private static readonly System.Globalization.CultureInfo CultureInfoPtBr = new("pt-BR");

    public static async Task<Atendimento> AtendimentoCompletoAsync(ApplicationDbContext db, int id)
    {
        var atendimento = await db.Atendimentos
            .Include(a => a.Responsavel)
            .Include(a => a.Falecido)
            .Include(a => a.Funeraria)
            .Include(a => a.EmpresaExecutora)
            .Include(a => a.ServicosAuxilio).ThenInclude(s => s.ServicoAuxilio)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (atendimento == null)
            throw new DocumentoBloqueadoException("Atendimento não encontrado");

        return atendimento;
    }

    public static string FormatarCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return string.Empty;

        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11)
            return cpf;

        return $"{digitos[..3]}.{digitos.Substring(3, 3)}.{digitos.Substring(6, 3)}-{digitos[9..]}";
    }

    public static string CausaMorteTexto(CausaMorte causa)
        => causa switch
        {
            CausaMorte.Natural => "Natural",
            CausaMorte.Acidente => "Acidente",
            CausaMorte.Homicidio => "Homicídio",
            CausaMorte.Pandemia => "Pandemia",
            CausaMorte.Desconhecida => "Desconhecida",
            _ => causa.ToString()
        };

    public static string TipoAtendimentoTexto(TipoAtendimentoFunerario tipo)
        => tipo switch
        {
            TipoAtendimentoFunerario.Particular => "Particular",
            TipoAtendimentoFunerario.AuxilioFuneral => "Auxílio Funeral",
            TipoAtendimentoFunerario.Assistencia => "Assistência",
            TipoAtendimentoFunerario.Associado => "Associado",
            TipoAtendimentoFunerario.PlanoFuneral => "Plano Funeral",
            _ => tipo.ToString()
        };
}
