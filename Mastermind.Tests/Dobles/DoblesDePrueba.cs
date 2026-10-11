using Mastermind.BusinessLogic;

namespace Mastermind.Tests.Dobles;

/// <summary>
/// Generador "aleatorio" que siempre devuelve el mismo valor, para fijar la posición de la pista.
/// También guarda el último máximo solicitado.
/// </summary>
internal sealed class RandomFijo : Random
{
    private readonly int _valor;

    public RandomFijo(int valor)
    {
        _valor = valor;
    }

    public int UltimoMaximo { get; private set; } = -1;

    public override int Next(int maxValue)
    {
        UltimoMaximo = maxValue;
        return _valor;
    }
}

/// <summary>
/// Reloj con una fecha fija y zona horaria UTC, para verificar el formato del historial.
/// </summary>
internal sealed class RelojFijo : TimeProvider
{
    private readonly DateTimeOffset _ahora;

    public RelojFijo(DateTimeOffset ahora)
    {
        _ahora = ahora;
    }

    public override DateTimeOffset GetUtcNow() => _ahora.ToUniversalTime();

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

/// <summary>
/// Repositorio de cheat codes que solo cuenta cuántas veces lo consultan.
/// </summary>
internal sealed class CheatCodeRepositorioEspia : ICheatCodeRepositorio
{
    public int Llamadas { get; private set; }

    public int? ObtenerUsosDisponibles(string codigo)
    {
        Llamadas++;
        return 5;
    }

    public void ConsumirUso(string codigo)
    {
        Llamadas++;
    }
}

/// <summary>
/// Archivo temporal que se elimina al terminar la prueba.
/// </summary>
internal sealed class ArchivoTemporal : IDisposable
{
    /// <param name="contenido">Contenido inicial; si es <c>null</c> el archivo no se crea.</param>
    public ArchivoTemporal(string? contenido, string extension = ".json")
    {
        Ruta = Path.Combine(Path.GetTempPath(), $"mastermind-{Guid.NewGuid():N}{extension}");
        if (contenido is not null)
        {
            File.WriteAllText(Ruta, contenido);
        }
    }

    public string Ruta { get; }

    public void Dispose()
    {
        if (File.Exists(Ruta))
        {
            File.Delete(Ruta);
        }
    }
}
