using Mastermind.BusinessLogic;

namespace Mastermind.Tests;

/// <summary>
/// Generación del patrón secreto para la partida interactiva.
/// </summary>
public class GeneradorPatronSecretoTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Generar_DevuelvePatronValidoDeLaLongitudIndicada(int longitud)
    {
        var configuracion = new ConfiguracionJuego();
        var generador = new GeneradorPatronSecreto(configuracion, new Random(1234));

        string patron = generador.Generar(longitud);

        Assert.Equal(longitud, patron.Length);
        Assert.All(patron, c => Assert.True(configuracion.EsColorValido(c)));
        // El patrón generado es aceptado por el evaluador.
        Assert.Equal(longitud, new EvaluadorMastermind().EvaluarIntento(patron, patron).BienPosicionados);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    public void Generar_LongitudFueraDeRango_SeRechaza(int longitud)
    {
        var generador = new GeneradorPatronSecreto(new ConfiguracionJuego());

        Assert.Throws<ArgumentOutOfRangeException>(() => generador.Generar(longitud));
    }
}
