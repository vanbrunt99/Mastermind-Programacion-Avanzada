using Mastermind.BusinessLogic;
using Mastermind.Tests.Dobles;

namespace Mastermind.Tests;

/// <summary>
/// Historial de evaluaciones con fecha (logging).
/// </summary>
public class RegistroEvaluacionesTests
{
    private static readonly DateTimeOffset FechaFija = new(2026, 10, 10, 15, 30, 45, TimeSpan.Zero);

    private readonly RegistroEvaluacionesEnMemoria _registro = new();
    private readonly EvaluadorMastermind _evaluador;

    public RegistroEvaluacionesTests()
    {
        _evaluador = new EvaluadorMastermind(
            new ConfiguracionJuego(), _registro, reloj: new RelojFijo(FechaFija));
    }

    [Fact]
    public void Mensaje_TieneElFormatoDelEnunciado()
    {
        _evaluador.EvaluarIntento("RGBY", "RBYG");

        RegistroEvaluacion registro = Assert.Single(_registro.Registros);
        Assert.Equal(
            "En la fecha 10-10-2026 15:30:45 se evaluó el intento RBYG contra el patrón RGBY. " +
            "Resultado: 1 bien posicionados – 3 con color correcto.",
            registro.Mensaje);
    }

    [Fact]
    public void Registro_GuardaFechaIntentoPatronYResultado()
    {
        _evaluador.EvaluarIntento("RGBY", "GGRR");

        RegistroEvaluacion registro = Assert.Single(_registro.Registros);
        Assert.Equal(new DateTime(2026, 10, 10, 15, 30, 45), registro.Fecha);
        Assert.Equal("GGRR", registro.Intento);
        Assert.Equal("RGBY", registro.Patron);
        Assert.Equal(1, registro.BienPosicionados);
        Assert.Equal(1, registro.ColorCorrecto);
    }

    [Fact]
    public void Fecha_UsaFormatoDe24Horas()
    {
        var evaluador = new EvaluadorMastermind(
            new ConfiguracionJuego(), _registro,
            reloj: new RelojFijo(new DateTimeOffset(2026, 1, 5, 21, 5, 9, TimeSpan.Zero)));

        evaluador.EvaluarIntento("RGBY", "RGBY");

        Assert.StartsWith("En la fecha 05-01-2026 21:05:09 ", Assert.Single(_registro.Registros).Mensaje);
    }

    [Fact]
    public void Registro_GuardaLasEntradasNormalizadasAMayuscula()
    {
        _evaluador.EvaluarIntento("rgby", "rbyg");

        RegistroEvaluacion registro = Assert.Single(_registro.Registros);
        Assert.Equal("RBYG", registro.Intento);
        Assert.Equal("RGBY", registro.Patron);
    }

    [Fact]
    public void CadaEvaluacion_AgregaUnRegistroEnOrden()
    {
        _evaluador.EvaluarIntento("RGBY", "CCCC");
        _evaluador.EvaluarIntento("RGBY", "RBYG");
        _evaluador.EvaluarIntento("RGBY", "RGBY");

        Assert.Equal(new[] { "CCCC", "RBYG", "RGBY" }, _registro.Registros.Select(r => r.Intento));
    }

    [Fact]
    public void EvaluacionInvalida_NoSeRegistra()
    {
        Assert.Throws<EntradaInvalidaException>(() => _evaluador.EvaluarIntento("RGBY", "RGB"));

        Assert.Empty(_registro.Registros);
    }

    [Fact]
    public void RegistroEnArchivo_AgregaUnaLineaPorEvaluacion()
    {
        using var archivo = new ArchivoTemporal(contenido: null, extension: ".log");
        var evaluador = new EvaluadorMastermind(
            new ConfiguracionJuego(), new RegistroEvaluacionesArchivo(archivo.Ruta), reloj: new RelojFijo(FechaFija));

        evaluador.EvaluarIntento("RGBY", "RBYG");
        evaluador.EvaluarIntento("RGBY", "RRRR");

        string[] lineas = File.ReadAllLines(archivo.Ruta);
        Assert.Equal(2, lineas.Length);
        Assert.Equal(
            "En la fecha 10-10-2026 15:30:45 se evaluó el intento RBYG contra el patrón RGBY. " +
            "Resultado: 1 bien posicionados – 3 con color correcto.",
            lineas[0]);
        Assert.EndsWith("Resultado: 1 bien posicionados – 0 con color correcto.", lineas[1]);
    }
}
