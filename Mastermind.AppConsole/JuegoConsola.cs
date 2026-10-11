using Mastermind.BusinessLogic;

namespace Mastermind.AppConsole;

/// <summary>
/// Interfaz de consola del juego. Solo se encarga de leer y mostrar datos;
/// toda la lógica vive en Mastermind.BusinessLogic.
/// </summary>
public class JuegoConsola
{
    /// <summary>Intentos disponibles en una partida interactiva.</summary>
    public const int MaximoIntentos = 10;

    private readonly EvaluadorMastermind _evaluador;
    private readonly GeneradorPatronSecreto _generador;
    private readonly TextReader _entrada;
    private readonly TextWriter _salida;

    public JuegoConsola(
        EvaluadorMastermind evaluador,
        GeneradorPatronSecreto generador,
        TextReader entrada,
        TextWriter salida)
    {
        _evaluador = evaluador;
        _generador = generador;
        _entrada = entrada;
        _salida = salida;
    }

    /// <summary>
    /// Evalúa un único intento recibido por línea de comandos.
    /// </summary>
    public int EvaluarUnaVez(string patronSecreto, string intentoJugador, string? cheatCode)
    {
        try
        {
            ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(patronSecreto, intentoJugador, cheatCode);
            MostrarResultado(resultado);
            return 0;
        }
        catch (EntradaInvalidaException ex)
        {
            _salida.WriteLine($"Error: {ex.Detalle}");
            return 2;
        }
    }

    /// <summary>
    /// Juega una partida: genera un patrón secreto que no se muestra hasta el final.
    /// Cada línea es un intento; opcionalmente se agrega un cheat code separado por espacio.
    /// </summary>
    public int Jugar(int longitud)
    {
        string patronSecreto = _generador.Generar(longitud);

        _salida.WriteLine("=== Mastermind: código secreto ===");
        _salida.WriteLine("Colores disponibles: " +
            string.Join(", ", _evaluador.Configuracion.Colores.Select(c => $"{c.Codigo}={c.Nombre}")));
        _salida.WriteLine($"Longitud del patrón: {longitud}. Tienes {MaximoIntentos} intentos.");
        _salida.WriteLine("Escribe tu intento (opcional: un espacio y un cheat code). Escribe 'salir' para terminar.");

        int intentosUsados = 0;
        while (intentosUsados < MaximoIntentos)
        {
            _salida.Write($"Intento {intentosUsados + 1}/{MaximoIntentos}> ");
            string? linea = _entrada.ReadLine();

            if (linea is null || linea.Trim().Equals("salir", StringComparison.OrdinalIgnoreCase))
            {
                _salida.WriteLine();
                _salida.WriteLine($"Partida terminada. El patrón era: {patronSecreto}");
                return 0;
            }

            string[] partes = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length is 0 or > 2)
            {
                _salida.WriteLine("Formato: <intento> [cheatCode]");
                continue;
            }

            try
            {
                ResultadoEvaluacion resultado = _evaluador.EvaluarIntento(
                    patronSecreto, partes[0], partes.Length == 2 ? partes[1] : null);
                intentosUsados++;
                MostrarResultado(resultado);

                if (resultado.BienPosicionados == longitud)
                {
                    _salida.WriteLine($"¡Adivinaste el patrón en {intentosUsados} intento(s)!");
                    return 0;
                }
            }
            catch (EntradaInvalidaException ex)
            {
                // Un intento inválido no consume intentos ni cheat codes.
                _salida.WriteLine($"Error: {ex.Detalle}");
            }
        }

        _salida.WriteLine($"Se acabaron los intentos. El patrón era: {patronSecreto}");
        return 0;
    }

    private void MostrarResultado(ResultadoEvaluacion resultado)
    {
        _salida.WriteLine($"Bien posicionados: {resultado.BienPosicionados}");
        _salida.WriteLine($"Color correcto (otra posición): {resultado.ColorCorrecto}");
        if (resultado.Pista is not null)
        {
            _salida.WriteLine(resultado.Pista.Mensaje);
        }
    }
}
