using Mastermind.BusinessLogic;
using Mastermind.Tests.Dobles;

namespace Mastermind.Tests;

/// <summary>
/// Configuración dinámica de colores: paleta por defecto, paletas personalizadas y lectura desde JSON.
/// </summary>
public class ConfiguracionColoresTests
{
    [Fact]
    public void PaletaPorDefecto_TieneLosNueveColoresDelEnunciado()
    {
        var configuracion = new ConfiguracionJuego();

        Assert.Equal(
            new[] { "Yellow", "Cyan", "Black", "Purple", "White", "Pink", "Red", "Brown", "Green" },
            configuracion.Colores.Select(c => c.Nombre));
        Assert.Equal("YCBPWKRNG", new string(configuracion.Colores.Select(c => c.Codigo).ToArray()));
    }

    [Fact]
    public void PaletaNumerica_SeUsaParaValidarYEvaluar()
    {
        var colores = Enumerable.Range(1, 9).Select(n => new ColorJuego((char)('0' + n), $"Color {n}"));
        var evaluador = new EvaluadorMastermind(new ConfiguracionJuego(colores), new RegistroEvaluacionesEnMemoria());

        ResultadoEvaluacion resultado = evaluador.EvaluarIntento("1234", "1243");

        Assert.Equal(2, resultado.BienPosicionados);
        Assert.Equal(2, resultado.ColorCorrecto);
    }

    [Fact]
    public void PaletaNumerica_RechazaLetrasQueNoEstanConfiguradas()
    {
        var colores = Enumerable.Range(1, 9).Select(n => new ColorJuego((char)('0' + n), $"Color {n}"));
        var evaluador = new EvaluadorMastermind(new ConfiguracionJuego(colores), new RegistroEvaluacionesEnMemoria());

        var ex = Assert.Throws<EntradaInvalidaException>(() => evaluador.EvaluarIntento("1234", "RGBY"));

        Assert.Equal(ErrorValidacion.ColorNoPermitido, ex.Error);
    }

    [Fact]
    public void PaletaHexadecimal_AceptaDigitosYLetrasAaF()
    {
        var colores = "0123456789ABCDEF".Select(c => new ColorJuego(c, $"Hex {c}"));
        var evaluador = new EvaluadorMastermind(new ConfiguracionJuego(colores), new RegistroEvaluacionesEnMemoria());

        ResultadoEvaluacion resultado = evaluador.EvaluarIntento("A0F9", "a09f");

        Assert.Equal(2, resultado.BienPosicionados);
        Assert.Equal(2, resultado.ColorCorrecto);
    }

    [Fact]
    public void CodigosEnMinuscula_SeNormalizanAMayuscula()
    {
        var configuracion = new ConfiguracionJuego(new[] { new ColorJuego('a', "Azul"), new ColorJuego('v', "Verde") });

        Assert.Equal(new[] { 'A', 'V' }, configuracion.Colores.Select(c => c.Codigo));
        Assert.True(configuracion.EsColorValido('a'));
        Assert.True(configuracion.EsColorValido('A'));
        Assert.Equal("Verde", configuracion.ObtenerColor('v').Nombre);
    }

    [Fact]
    public void PaletaNula_SeRechaza()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfiguracionJuego(null!));
    }

    [Fact]
    public void PaletaConCodigosRepetidos_SeRechaza()
    {
        var colores = new[] { new ColorJuego('a', "Azul"), new ColorJuego('A', "Amarillo") };

        var ex = Assert.Throws<ArgumentException>(() => new ConfiguracionJuego(colores));
        Assert.Contains("repetido", ex.Message);
    }

    [Theory]
    [InlineData('#')]
    [InlineData(' ')]
    [InlineData('Ñ')]
    public void PaletaConCodigoNoAlfanumerico_SeRechaza(char codigo)
    {
        var colores = new[] { new ColorJuego(codigo, "Raro"), new ColorJuego('R', "Red") };

        Assert.Throws<ArgumentException>(() => new ConfiguracionJuego(colores));
    }

    [Fact]
    public void PaletaConNombreVacio_SeRechaza()
    {
        var colores = new[] { new ColorJuego('R', " "), new ColorJuego('G', "Green") };

        Assert.Throws<ArgumentException>(() => new ConfiguracionJuego(colores));
    }

    [Fact]
    public void PaletaConUnSoloColor_SeRechaza()
    {
        Assert.Throws<ArgumentException>(() => new ConfiguracionJuego(new[] { new ColorJuego('R', "Red") }));
    }

    [Fact]
    public void CargadorJson_LeeLaPaletaDelArchivo()
    {
        using var archivo = new ArchivoTemporal("""
            { "colores": [
                { "codigo": "1", "nombre": "Uno" },
                { "codigo": "2", "nombre": "Dos" },
                { "codigo": "x", "nombre": "Equis" }
            ] }
            """);

        ConfiguracionJuego configuracion = new CargadorConfiguracionJuego().Cargar(archivo.Ruta);

        Assert.Equal("12X", new string(configuracion.Colores.Select(c => c.Codigo).ToArray()));
        Assert.Equal("Equis", configuracion.ObtenerColor('x').Nombre);
    }

    [Fact]
    public void CargadorJson_ArchivoDeLaConsola_CoincideConLaPaletaPorDefecto()
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, "ArchivosConsola", "configuracion-colores.json");

        ConfiguracionJuego desdeArchivo = new CargadorConfiguracionJuego().Cargar(ruta);

        Assert.Equal(new ConfiguracionJuego().Colores, desdeArchivo.Colores);
    }

    [Theory]
    [InlineData("""{ "colores": [ { "codigo": "RR", "nombre": "Doble" }, { "codigo": "G", "nombre": "Green" } ] }""")]
    [InlineData("""{ "colores": [ { "nombre": "SinCodigo" }, { "codigo": "G", "nombre": "Green" } ] }""")]
    [InlineData("""{ "otros": [] }""")]
    [InlineData("""{ "colores": [ { "codigo": "R", "nombre": "Red" }, { "codigo": "r", "nombre": "Rojo" } ] }""")]
    [InlineData("""esto no es json""")]
    public void CargadorJson_ContenidoInvalido_LanzaInvalidDataException(string contenido)
    {
        using var archivo = new ArchivoTemporal(contenido);

        Assert.Throws<InvalidDataException>(() => new CargadorConfiguracionJuego().Cargar(archivo.Ruta));
    }

    [Fact]
    public void CargadorJson_ArchivoInexistente_LanzaFileNotFoundException()
    {
        using var archivo = new ArchivoTemporal(contenido: null);

        Assert.Throws<FileNotFoundException>(() => new CargadorConfiguracionJuego().Cargar(archivo.Ruta));
    }
}
