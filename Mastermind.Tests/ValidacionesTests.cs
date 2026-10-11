using Mastermind.BusinessLogic;

namespace Mastermind.Tests;

/// <summary>
/// Escenarios de error de las entradas: vacías, longitudes y colores permitidos.
/// </summary>
public class ValidacionesTests
{
    private readonly EvaluadorMastermind _evaluador = new();

    [Fact]
    public void PatronNulo_LanzaArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _evaluador.EvaluarIntento(null!, "RGBY"));
        Assert.Equal("patronSecreto", ex.ParamName);
    }

    [Fact]
    public void IntentoNulo_LanzaArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _evaluador.EvaluarIntento("RGBY", null!));
        Assert.Equal("intentoJugador", ex.ParamName);
    }

    [Theory]
    [InlineData("", "RGBY", "patronSecreto")]
    [InlineData("   ", "RGBY", "patronSecreto")]
    [InlineData("RGBY", "", "intentoJugador")]
    [InlineData("RGBY", "  ", "intentoJugador")]
    public void EntradaVacia_SeRechaza(string patron, string intento, string parametro)
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => _evaluador.EvaluarIntento(patron, intento));

        Assert.Equal(ErrorValidacion.EntradaVacia, ex.Error);
        Assert.Equal(parametro, ex.ParamName);
    }

    [Theory]
    [InlineData("RGB", "RGB", "patronSecreto")]
    [InlineData("RGBYCPW", "RGBYCPW", "patronSecreto")]
    [InlineData("RGBY", "RGB", "intentoJugador")]
    [InlineData("RGBY", "RGBYCPW", "intentoJugador")]
    public void LongitudFueraDeRango_SeRechaza(string patron, string intento, string parametro)
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => _evaluador.EvaluarIntento(patron, intento));

        Assert.Equal(ErrorValidacion.LongitudFueraDeRango, ex.Error);
        Assert.Equal(parametro, ex.ParamName);
    }

    [Theory]
    [InlineData("RGBY")]
    [InlineData("RGBYC")]
    [InlineData("RGBYCP")]
    public void LongitudesPermitidas_4a6_SeAceptan(string patron)
    {
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(patron, patron);

        Assert.Equal(patron.Length, resultado.BienPosicionados);
    }

    [Theory]
    [InlineData("RGBY", "RGBYC")]
    [InlineData("RGBYCP", "RGBY")]
    public void LongitudesDistintas_SeRechaza(string patron, string intento)
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => _evaluador.EvaluarIntento(patron, intento));

        Assert.Equal(ErrorValidacion.LongitudesDistintas, ex.Error);
        Assert.Equal("intentoJugador", ex.ParamName);
    }

    [Theory]
    [InlineData("RGBX")]
    [InlineData("RGB1")]
    [InlineData("RG-Y")]
    [InlineData("RG Y")]
    [InlineData("RGBÑ")]
    public void ColorNoPermitido_EnIntento_SeRechaza(string intento)
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => _evaluador.EvaluarIntento("RGBY", intento));

        Assert.Equal(ErrorValidacion.ColorNoPermitido, ex.Error);
        Assert.Equal("intentoJugador", ex.ParamName);
        Assert.Contains("Y, C, B, P, W, K, R, N, G", ex.Message);
    }

    [Fact]
    public void ColorNoPermitido_EnPatron_SeRechaza()
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => _evaluador.EvaluarIntento("RGZY", "RGBY"));

        Assert.Equal(ErrorValidacion.ColorNoPermitido, ex.Error);
        Assert.Equal("patronSecreto", ex.ParamName);
        Assert.Contains("posición 3", ex.Message);
    }

    [Theory]
    [InlineData("rgby", "rbyg")]
    [InlineData("RgBy", "rByG")]
    [InlineData("RGBY", "rbyg")]
    public void MayusculasYMinusculas_SeTratanIgual(string patron, string intento)
    {
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(patron, intento);

        Assert.Equal(1, resultado.BienPosicionados);
        Assert.Equal(3, resultado.ColorCorrecto);
    }

    [Fact]
    public void EspaciosAlInicioYAlFinal_SeIgnoran()
    {
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(" RGBY ", "RBYG  ");

        Assert.Equal(1, resultado.BienPosicionados);
        Assert.Equal(3, resultado.ColorCorrecto);
    }

    [Fact]
    public void EntradaInvalidaException_EsUnaArgumentException()
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => _evaluador.EvaluarIntento("RGBY", "RGBX"));

        Assert.IsAssignableFrom<ArgumentException>(ex);
        Assert.DoesNotContain("Parameter", ex.Detalle);
    }
}
