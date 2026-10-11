using Mastermind.BusinessLogic;
using Mastermind.Tests.Dobles;

namespace Mastermind.Tests;

/// <summary>
/// Comprueba que la versión con cheat code se comporta como la versión básica
/// cuando el cheat code se omite o vale null.
/// </summary>
public class RetrocompatibilidadTests
{
    public static TheoryData<string, string, int, int> CasosBasicos => new()
    {
        { "RGBY", "RGBY", 4, 0 },
        { "RGBY", "RBYG", 1, 3 },
        { "RGBY", "CPWK", 0, 0 },
        { "RGBY", "RRRR", 1, 0 },
        { "RRGG", "RGRG", 2, 2 },
        { "RGBYCP", "PGBYCR", 4, 2 },
    };

    [Theory]
    [MemberData(nameof(CasosBasicos))]
    public void CheatCodeOmitido_DevuelveElMismoResultadoQueLaVersionBasica(
        string patron, string intento, int bienEsperado, int colorEsperado)
    {
        ResultadoEvaluacion resultado = new EvaluadorMastermind().EvaluarIntento(patron, intento);

        Assert.Equal(new ResultadoEvaluacion(bienEsperado, colorEsperado), resultado);
        Assert.Null(resultado.Pista);
    }

    [Theory]
    [MemberData(nameof(CasosBasicos))]
    public void CheatCodeNulo_EsEquivalenteAOmitirlo(
        string patron, string intento, int bienEsperado, int colorEsperado)
    {
        var evaluador = new EvaluadorMastermind();

        ResultadoEvaluacion omitido = evaluador.EvaluarIntento(patron, intento);
        ResultadoEvaluacion nulo = evaluador.EvaluarIntento(patron, intento, cheatCode: null);

        Assert.Equal(omitido, nulo);
        Assert.Equal(bienEsperado, nulo.BienPosicionados);
        Assert.Equal(colorEsperado, nulo.ColorCorrecto);
        Assert.Null(nulo.Pista);
    }

    [Fact]
    public void CheatCodeNulo_NoConsultaNiConsumeCheatCodes()
    {
        var espia = new CheatCodeRepositorioEspia();
        var evaluador = new EvaluadorMastermind(new ConfiguracionJuego(), new RegistroEvaluacionesEnMemoria(), espia);

        evaluador.EvaluarIntento("RGBY", "RBYG");
        evaluador.EvaluarIntento("RGBY", "RBYG", null);

        Assert.Equal(0, espia.Llamadas);
    }

    [Fact]
    public void CheatCodeNulo_PuedeRepetirseSinLimiteEnLaPartida()
    {
        var evaluador = new EvaluadorMastermind();

        for (int i = 0; i < 20; i++)
        {
            Assert.Null(evaluador.EvaluarIntento("RGBY", "RBYG", null).Pista);
        }
    }

    [Fact]
    public void ConstructorSinParametros_SigueFuncionandoSinArchivos()
    {
        var evaluador = new EvaluadorMastermind();

        ResultadoEvaluacion resultado = evaluador.EvaluarIntento("RGBY", "RBYG");

        Assert.Equal(1, resultado.BienPosicionados);
        Assert.Equal(3, resultado.ColorCorrecto);
    }

    [Fact]
    public void LongitudesDistintas_SigueSiendoArgumentExceptionConElMismoParametro()
    {
        // En la versión básica este caso lanzaba ArgumentException con ParamName "intentoJugador".
        ArgumentException ex = Assert.ThrowsAny<ArgumentException>(
            () => new EvaluadorMastermind().EvaluarIntento("RGBY", "RGBYC"));

        Assert.Equal("intentoJugador", ex.ParamName);
    }

    [Fact]
    public void PatronNulo_SigueLanzandoArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EvaluadorMastermind().EvaluarIntento(null!, "RGBY"));
    }
}
