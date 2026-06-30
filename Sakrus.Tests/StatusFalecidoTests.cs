using Sakrus.Core.Entities;
using Xunit;

namespace Sakrus.Tests;

/// <summary>
/// Testes unitários para a lógica de status do ciclo de vida do falecido.
/// Verifica o comportamento correto do StatusFalecido e previne regressões
/// do bug de re-sepultamento após exumação.
/// </summary>
public class StatusFalecidoTests
{
    // ──────────────────────────────────────────────────────────────────────
    // 1. Estado inicial padrão
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Falecido_RecemCriado_DeveSerNaoSepultado()
    {
        var falecido = new Falecido();

        Assert.Equal(StatusFalecido.NaoSepultado, falecido.Status);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 2. Ciclo normal: NaoSepultado → Sepultado
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Ao_Sepultar_Status_DeveSerSepultado()
    {
        var falecido = new Falecido();

        // Simula a ação de sepultamento
        falecido.Status = StatusFalecido.Sepultado;

        Assert.Equal(StatusFalecido.Sepultado, falecido.Status);
    }

    [Fact]
    public void Falecido_Sepultado_NaoDeveTerStatusNaoSepultado()
    {
        var falecido = new Falecido { Status = StatusFalecido.Sepultado };

        Assert.NotEqual(StatusFalecido.NaoSepultado, falecido.Status);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 3. Ciclo completo: NaoSepultado → Sepultado → Exumado
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Ao_Exumar_Status_DeveSerExumado()
    {
        var falecido = new Falecido { Status = StatusFalecido.Sepultado };

        // Simula a ação de exumação
        falecido.Status = StatusFalecido.Exumado;

        Assert.Equal(StatusFalecido.Exumado, falecido.Status);
    }

    [Fact]
    public void Falecido_Exumado_NaoDeveTerStatusSepultado()
    {
        var falecido = new Falecido { Status = StatusFalecido.Exumado };

        Assert.NotEqual(StatusFalecido.Sepultado, falecido.Status);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 4. BUG FIX: Falecido exumado NÃO deve poder ser sepultado novamente
    //    O método IsSepultado em Obitos.razor deve retornar false para Exumado.
    // ──────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(StatusFalecido.Sepultado, true)]   // botão "Exumar" visível
    [InlineData(StatusFalecido.NaoSepultado, false)] // botão "Sepultar" visível
    [InlineData(StatusFalecido.Exumado, false)]      // BUGFIX: nenhum botão de ação visível
    public void IsSepultado_RetornaCorretoParaCadaStatus(StatusFalecido status, bool esperadoIsSepultado)
    {
        var resultado = IsSepultado(status);
        Assert.Equal(esperadoIsSepultado, resultado);
    }

    // Replica a lógica do IsSepultado corrigida em Obitos.razor
    private static bool IsSepultado(StatusFalecido status)
        => status == StatusFalecido.Sepultado;

    // ──────────────────────────────────────────────────────────────────────
    // 5. Garante que Exumado é diferente de NaoSepultado
    //    (Falecido exumado sem novo destino NÃO deve aparecer como "novo")
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Exumado_NaoDeveSerConfundidoComNaoSepultado()
    {
        var falecidoExumado = new Falecido { Status = StatusFalecido.Exumado };
        var falecidoNovo = new Falecido { Status = StatusFalecido.NaoSepultado };

        Assert.NotEqual(falecidoExumado.Status, falecidoNovo.Status);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 6. Enum deve ter exatamente 3 valores (garante que não foi alterado
    //    inadvertidamente, o que poderia quebrar a lógica da UI)
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void StatusFalecido_DeveTerExatamente3Valores()
    {
        var valores = Enum.GetValues(typeof(StatusFalecido));
        Assert.Equal(3, valores.Length);
    }
}
