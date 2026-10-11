using Mastermind.BusinessLogic;

namespace Mastermind.Tests;

/// <summary>
/// Pruebas del algoritmo básico de EvaluarIntento (sin cheat code).
/// Estas pruebas se conservan sin cambios para verificar la retrocompatibilidad.
/// </summary>
public class EvaluarIntentoBasicoTests
{
    private readonly EvaluadorMastermind _evaluador = new();

    [Theory]
    [InlineData("RGBY")]
    [InlineData("YCBPW")]
    [InlineData("RRGGBB")]
    public void CoincidenciaCompleta_TodasBienPosicionadas(string patron)
    {
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(patron, patron);

        Assert.Equal(patron.Length, resultado.BienPosicionados);
        Assert.Equal(0, resultado.ColorCorrecto);
    }

    [Fact]
    public void CoincidenciaParcial_EjemploDelEnunciado_RGBYcontraRBYG()
    {
        // Ejemplo del enunciado: R está bien posicionada; B, Y y G tienen color correcto.
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento("RGBY", "RBYG");

        Assert.Equal(1, resultado.BienPosicionados);
        Assert.Equal(3, resultado.ColorCorrecto);
    }

    [Theory]
    [InlineData("RGBY", "RGYP", 2, 1)]
    [InlineData("RGBY", "YRGB", 0, 4)]
    [InlineData("RGBY", "RPWC", 1, 0)]
    [InlineData("RGBYCP", "PGBYCR", 4, 2)]
    public void CoincidenciaParcial_CuentaExactasYColores(
        string patron, string intento, int bienEsperado, int colorEsperado)
    {
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(patron, intento);

        Assert.Equal(bienEsperado, resultado.BienPosicionados);
        Assert.Equal(colorEsperado, resultado.ColorCorrecto);
    }

    [Theory]
    [InlineData("RGBY", "CPWK")]
    [InlineData("RRRR", "GGGG")]
    [InlineData("RGBYCP", "WWKKNN")]
    public void NingunaCoincidencia_DevuelveCeros(string patron, string intento)
    {
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(patron, intento);

        Assert.Equal(0, resultado.BienPosicionados);
        Assert.Equal(0, resultado.ColorCorrecto);
    }

    [Fact]
    public void Duplicados_IntentoRepiteColor_SoloCuentaUnaVez()
    {
        // El patrón tiene una sola R: una R del intento queda bien posicionada
        // y las demás se descartan; no cuentan como color correcto.
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento("RGBY", "RRRR");

        Assert.Equal(1, resultado.BienPosicionados);
        Assert.Equal(0, resultado.ColorCorrecto);
    }

    [Fact]
    public void Duplicados_CoincidenciaExactaTienePrioridad()
    {
        // La G de la posición 2 empareja exacto con la G del patrón;
        // la G de la posición 1 ya no tiene con quién emparejar.
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento("RGBY", "GGRR");

        Assert.Equal(1, resultado.BienPosicionados);
        Assert.Equal(1, resultado.ColorCorrecto);
    }

    [Theory]
    [InlineData("RRGB", "YYYR", 0, 1)]
    [InlineData("RRGG", "GGRR", 0, 4)]
    [InlineData("RRGG", "RGRG", 2, 2)]
    [InlineData("RRBB", "BRRR", 1, 2)]
    [InlineData("RRRGGG", "GGGRRR", 0, 6)]
    public void Duplicados_EnPatronEIntento_SinDobleConteo(
        string patron, string intento, int bienEsperado, int colorEsperado)
    {
        ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(patron, intento);

        Assert.Equal(bienEsperado, resultado.BienPosicionados);
        Assert.Equal(colorEsperado, resultado.ColorCorrecto);
    }
}
