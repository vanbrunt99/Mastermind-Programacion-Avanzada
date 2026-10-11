using System.Text.Json;
using Mastermind.BusinessLogic;
using Mastermind.Tests.Dobles;

namespace Mastermind.Tests;

/// <summary>
/// Lectura, validación y persistencia del archivo JSON de cheat codes.
/// </summary>
public class CheatCodeRepositorioJsonTests
{
    private const string JsonEnunciado = """
        [
          { "hash": "A1B2C3D4E5F6G7H8I9J0ABCDE", "usosDisponibles": 1 },
          { "hash": "Z9Y8X7W6V5U4T3S2R1Q0LMNOP", "usosDisponibles": 3 }
        ]
        """;

    [Fact]
    public void ObtenerUsos_CodigoExistente_DevuelveLosUsosDelArchivo()
    {
        using var archivo = new ArchivoTemporal(JsonEnunciado);
        var repositorio = new CheatCodeRepositorioJson(archivo.Ruta);

        Assert.Equal(1, repositorio.ObtenerUsosDisponibles("A1B2C3D4E5F6G7H8I9J0ABCDE"));
        Assert.Equal(3, repositorio.ObtenerUsosDisponibles("Z9Y8X7W6V5U4T3S2R1Q0LMNOP"));
    }

    [Fact]
    public void ObtenerUsos_CodigoInexistente_DevuelveNull()
    {
        using var archivo = new ArchivoTemporal(JsonEnunciado);

        Assert.Null(new CheatCodeRepositorioJson(archivo.Ruta).ObtenerUsosDisponibles("NOEXISTE"));
    }

    [Fact]
    public void ConsumirUso_GuardaElArchivoConElMismoFormato()
    {
        using var archivo = new ArchivoTemporal(JsonEnunciado);
        var repositorio = new CheatCodeRepositorioJson(archivo.Ruta);

        repositorio.ConsumirUso("Z9Y8X7W6V5U4T3S2R1Q0LMNOP");

        using JsonDocument documento = JsonDocument.Parse(File.ReadAllText(archivo.Ruta));
        JsonElement segundo = documento.RootElement[1];
        Assert.Equal(2, documento.RootElement.GetArrayLength());
        Assert.Equal("Z9Y8X7W6V5U4T3S2R1Q0LMNOP", segundo.GetProperty("hash").GetString());
        Assert.Equal(2, segundo.GetProperty("usosDisponibles").GetInt32());
        Assert.Equal(1, documento.RootElement[0].GetProperty("usosDisponibles").GetInt32());
    }

    [Fact]
    public void ConsumirUso_CodigoAgotado_LanzaInvalidOperationException()
    {
        using var archivo = new ArchivoTemporal(JsonEnunciado);
        var repositorio = new CheatCodeRepositorioJson(archivo.Ruta);
        repositorio.ConsumirUso("A1B2C3D4E5F6G7H8I9J0ABCDE");

        Assert.Throws<InvalidOperationException>(() => repositorio.ConsumirUso("A1B2C3D4E5F6G7H8I9J0ABCDE"));
        Assert.Equal(0, repositorio.ObtenerUsosDisponibles("A1B2C3D4E5F6G7H8I9J0ABCDE"));
    }

    [Fact]
    public void ConsumirUso_CodigoInexistente_LanzaInvalidOperationException()
    {
        using var archivo = new ArchivoTemporal(JsonEnunciado);

        Assert.Throws<InvalidOperationException>(() => new CheatCodeRepositorioJson(archivo.Ruta).ConsumirUso("NOEXISTE"));
    }

    [Fact]
    public void ArchivoDeLaConsola_TieneLosCodigosDelEnunciado()
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, "ArchivosConsola", "cheatcodes.json");
        var repositorio = new CheatCodeRepositorioJson(ruta);

        Assert.Equal(1, repositorio.ObtenerUsosDisponibles("A1B2C3D4E5F6G7H8I9J0ABCDE"));
        Assert.Equal(3, repositorio.ObtenerUsosDisponibles("Z9Y8X7W6V5U4T3S2R1Q0LMNOP"));
    }

    [Theory]
    [InlineData("esto no es json")]
    [InlineData("""{ "hash": "A1", "usosDisponibles": 1 }""")]
    [InlineData("""[ { "hash": "", "usosDisponibles": 1 } ]""")]
    [InlineData("""[ { "usosDisponibles": 1 } ]""")]
    [InlineData("""[ { "hash": "A1", "usosDisponibles": -1 } ]""")]
    [InlineData("""[ { "hash": "A1", "usosDisponibles": 1 }, { "hash": "a1", "usosDisponibles": 2 } ]""")]
    [InlineData("null")]
    public void ArchivoInvalido_LanzaInvalidDataException(string contenido)
    {
        using var archivo = new ArchivoTemporal(contenido);

        Assert.Throws<InvalidDataException>(() => new CheatCodeRepositorioJson(archivo.Ruta));
    }

    [Fact]
    public void ArchivoInexistente_LanzaFileNotFoundException()
    {
        using var archivo = new ArchivoTemporal(contenido: null);

        Assert.Throws<FileNotFoundException>(() => new CheatCodeRepositorioJson(archivo.Ruta));
    }
}
