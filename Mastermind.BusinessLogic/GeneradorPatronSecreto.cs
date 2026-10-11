namespace Mastermind.BusinessLogic;

/// <summary>
/// Genera patrones secretos aleatorios con los colores de la configuración.
/// Los colores pueden repetirse dentro del patrón.
/// </summary>
public class GeneradorPatronSecreto
{
    private readonly ConfiguracionJuego _configuracion;
    private readonly Random _aleatorio;

    public GeneradorPatronSecreto(ConfiguracionJuego configuracion, Random? aleatorio = null)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        _configuracion = configuracion;
        _aleatorio = aleatorio ?? new Random();
    }

    /// <summary>Genera un patrón de la longitud indicada (entre 4 y 6).</summary>
    /// <exception cref="ArgumentOutOfRangeException">La longitud está fuera del rango permitido.</exception>
    public string Generar(int longitud)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(longitud, ConfiguracionJuego.LongitudMinima);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(longitud, ConfiguracionJuego.LongitudMaxima);

        var fichas = new char[longitud];
        for (int i = 0; i < longitud; i++)
        {
            fichas[i] = _configuracion.Colores[_aleatorio.Next(_configuracion.Colores.Count)].Codigo;
        }

        return new string(fichas);
    }
}
