using System;
using System.Collections.Generic;

namespace Sakrus.Core.Helpers;

/// <summary>
/// Compara identificadores de lote/gaveta respeitando a ordem numérica.
/// "2" vem antes de "10" e "10-A" vem antes de "10-B", mesmo sem zeros à esquerda.
/// </summary>
public static class OrdenacaoNatural
{
    public static int Comparar(string? a, string? b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (string.IsNullOrEmpty(a)) return -1;
        if (string.IsNullOrEmpty(b)) return 1;

        int i = 0, j = 0;

        while (i < a.Length && j < b.Length)
        {
            if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
            {
                int inicioA = i, inicioB = j;

                while (i < a.Length && char.IsDigit(a[i])) i++;
                while (j < b.Length && char.IsDigit(b[j])) j++;

                var numeroA = a.AsSpan(inicioA, i - inicioA).TrimStart('0');
                var numeroB = b.AsSpan(inicioB, j - inicioB).TrimStart('0');

                if (numeroA.Length != numeroB.Length)
                    return numeroA.Length.CompareTo(numeroB.Length);

                var comparacaoNumerica = numeroA.SequenceCompareTo(numeroB);
                if (comparacaoNumerica != 0)
                    return comparacaoNumerica;
            }
            else
            {
                var comparacao = char.ToUpperInvariant(a[i]).CompareTo(char.ToUpperInvariant(b[j]));
                if (comparacao != 0)
                    return comparacao;

                i++;
                j++;
            }
        }

        return (a.Length - i).CompareTo(b.Length - j);
    }

    public static IOrderedEnumerable<T> OrdenarPor<T>(this IEnumerable<T> origem, Func<T, string?> seletor)
        => origem.OrderBy(seletor, Comparador);

    public static IComparer<string?> Comparador { get; } =
        Comparer<string?>.Create(Comparar);
}
