using Mastermind.AppConsole;
using Mastermind.BusinessLogic;

// Uso:
//   Mastermind.AppConsole                                    -> partida interactiva de 4 fichas
//   Mastermind.AppConsole jugar [4|5|6]                      -> partida interactiva
//   Mastermind.AppConsole <patronSecreto> <intento> [cheat]  -> evalúa un solo intento
// Ejemplo: dotnet run --project Mastermind.AppConsole -- RGBY RBYG

string carpeta = AppContext.BaseDirectory;
string rutaColores = Path.Combine(carpeta, "configuracion-colores.json");
string rutaCheatCodes = Path.Combine(carpeta, "cheatcodes.json");
string rutaRegistro = Path.Combine(carpeta, "registro-evaluaciones.log");

ConfiguracionJuego configuracion;
ICheatCodeRepositorio cheatCodes;
try
{
    configuracion = new CargadorConfiguracionJuego().Cargar(rutaColores);
    cheatCodes = new CheatCodeRepositorioJson(rutaCheatCodes);
}
catch (Exception ex) when (ex is FileNotFoundException or InvalidDataException)
{
    Console.WriteLine($"Error de configuración: {ex.Message}");
    return 3;
}

var evaluador = new EvaluadorMastermind(configuracion, new RegistroEvaluacionesArchivo(rutaRegistro), cheatCodes);
var juego = new JuegoConsola(evaluador, new GeneradorPatronSecreto(configuracion), Console.In, Console.Out);

if (args.Length == 0)
{
    return juego.Jugar(ConfiguracionJuego.LongitudMinima);
}

if (args[0].Equals("jugar", StringComparison.OrdinalIgnoreCase) && args.Length <= 2)
{
    int longitud = ConfiguracionJuego.LongitudMinima;
    if (args.Length == 2 &&
        (!int.TryParse(args[1], out longitud) ||
         longitud < ConfiguracionJuego.LongitudMinima ||
         longitud > ConfiguracionJuego.LongitudMaxima))
    {
        Console.WriteLine($"La longitud debe ser un número entre {ConfiguracionJuego.LongitudMinima} y {ConfiguracionJuego.LongitudMaxima}.");
        return 1;
    }

    return juego.Jugar(longitud);
}

if (args.Length is 2 or 3)
{
    return juego.EvaluarUnaVez(args[0], args[1], args.Length == 3 ? args[2] : null);
}

Console.WriteLine("Uso:");
Console.WriteLine("  Mastermind.AppConsole jugar [4|5|6]");
Console.WriteLine("  Mastermind.AppConsole <patronSecreto> <intentoJugador> [cheatCode]");
Console.WriteLine("Ejemplo: Mastermind.AppConsole RGBY RBYG");
return 1;
