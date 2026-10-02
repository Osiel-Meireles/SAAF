using System;
using System.Collections.Generic;

namespace Sakrus.Core.Helpers;

public static class RegrasDocumentos
{
    public const int AnosParaExumacao = 5;
    public const int MaxParcelas = 12;
    public const decimal ParcelaMinima = 100m;

    public static DateTime DataLiberacaoExumacao(DateTime dataSepultamento)
        => dataSepultamento.AddYears(AnosParaExumacao);

    public static decimal? CalcularAreaM2(decimal? largura, decimal? comprimento)
    {
        if (largura == null || comprimento == null || largura <= 0 || comprimento <= 0)
            return null;

        return Math.Round(largura.Value * comprimento.Value, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal CalcularValor(decimal areaM2, decimal valorM2)
        => Math.Round(areaM2 * valorM2, 2, MidpointRounding.AwayFromZero);

    public static int MaxParcelasPermitidas(decimal total)
    {
        if (total <= 0)
            return 0;

        return Math.Clamp((int)Math.Floor(total / ParcelaMinima), 1, MaxParcelas);
    }

    public static IReadOnlyList<ParcelaSimulada> SimularParcelamento(decimal total, int parcelas)
    {
        var maxParcelas = MaxParcelasPermitidas(total);
        if (parcelas < 1 || parcelas > maxParcelas)
            throw new ArgumentOutOfRangeException(nameof(parcelas));

        var valorBase = Math.Floor(total / parcelas * 100) / 100;
        var resultado = new List<ParcelaSimulada>(parcelas);
        var soma = 0m;

        for (var numero = 1; numero <= parcelas; numero++)
        {
            var valor = numero == parcelas ? total - soma : valorBase;
            resultado.Add(new ParcelaSimulada(numero, valor));
            soma += valor;
        }

        return resultado;
    }

    public static IReadOnlyList<OpcaoParcelamento> OpcoesParcelamento(decimal total)
    {
        var maxParcelas = MaxParcelasPermitidas(total);
        var opcoes = new List<OpcaoParcelamento>(maxParcelas);

        for (var parcelas = 1; parcelas <= maxParcelas; parcelas++)
        {
            var simulacao = SimularParcelamento(total, parcelas);
            opcoes.Add(new OpcaoParcelamento(parcelas, simulacao[0].Valor, simulacao[^1].Valor));
        }

        return opcoes;
    }

    public static string FormatarNumeroAnual(int sequencia, int ano)
        => $"{sequencia:000}/{ano}";

    public static int? CalcularIdade(DateTime? nascimento, DateTime referencia)
    {
        if (nascimento == null)
            return null;

        var idade = referencia.Year - nascimento.Value.Year;
        if (nascimento.Value.Date > referencia.AddYears(-idade))
            idade--;

        return idade;
    }
}

public record ParcelaSimulada(int Numero, decimal Valor);

public record OpcaoParcelamento(int Parcelas, decimal ValorParcela, decimal ValorUltimaParcela);
