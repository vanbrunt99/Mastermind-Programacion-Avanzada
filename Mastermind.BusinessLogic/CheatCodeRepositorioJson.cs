using System.Text.Json;

namespace Mastermind.BusinessLogic;

/// <summary>
/// Repositorio de cheat codes basado en un archivo JSON (arreglo de objetos con
/// "hash" y "usosDisponibles"). Cada consumo se guarda de inmediato en el archivo,
/// por lo que los usos agotados se conservan entre ejecuciones del programa.
/// </summary>
public class CheatCodeRepositorioJson : ICheatCodeRepositorio
{
    private readonly string _rutaArchivo;
    private readonly object _bloqueo = new();
    private readonly JsonSerializerOptions _opcionesEscritura = new() { WriteIndented = true };

    /// <exception cref="FileNotFoundException">El archivo no existe.</exception>
    /// <exception cref="InvalidDataException">El contenido no tiene el formato esperado.</exception>
    public CheatCodeRepositorioJson(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException("No se encontró el archivo de cheat codes.", rutaArchivo);
        }

        _rutaArchivo = rutaArchivo;

        // Se valida el formato al crear el repositorio para detectar errores temprano.
        Leer();
    }

    public int? ObtenerUsosDisponibles(string codigo)
    {
        ArgumentNullException.ThrowIfNull(codigo);
        lock (_bloqueo)
        {
            return Buscar(Leer(), codigo)?.UsosDisponibles;
        }
    }

    public void ConsumirUso(string codigo)
    {
        ArgumentNullException.ThrowIfNull(codigo);
        lock (_bloqueo)
        {
            List<CheatCode> codigos = Leer();
            CheatCode entrada = Buscar(codigos, codigo)
                ?? throw new InvalidOperationException("El cheat code no existe.");

            if (entrada.UsosDisponibles <= 0)
            {
                throw new InvalidOperationException("El cheat code no tiene usos disponibles.");
            }

            entrada.UsosDisponibles--;
            Guardar(codigos);
        }
    }

    private CheatCode? Buscar(List<CheatCode> codigos, string codigo) =>
        codigos.FirstOrDefault(c => string.Equals(c.Hash, codigo.Trim(), StringComparison.OrdinalIgnoreCase));

    private List<CheatCode> Leer()
    {
        List<CheatCode?>? codigos;
        try
        {
            codigos = JsonSerializer.Deserialize<List<CheatCode?>>(File.ReadAllText(_rutaArchivo));
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"El archivo de cheat codes '{_rutaArchivo}' no es un JSON válido.", ex);
        }

        if (codigos is null)
        {
            throw new InvalidDataException($"El archivo de cheat codes '{_rutaArchivo}' debe contener un arreglo.");
        }

        var hashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resultado = new List<CheatCode>();
        foreach (CheatCode? codigo in codigos)
        {
            if (codigo is null || string.IsNullOrWhiteSpace(codigo.Hash))
            {
                throw new InvalidDataException("Cada cheat code debe tener un \"hash\" no vacío.");
            }

            if (codigo.UsosDisponibles < 0)
            {
                throw new InvalidDataException($"El cheat code '{codigo.Hash}' tiene usos disponibles negativos.");
            }

            if (!hashes.Add(codigo.Hash.Trim()))
            {
                throw new InvalidDataException($"El cheat code '{codigo.Hash}' está repetido en el archivo.");
            }

            resultado.Add(codigo);
        }

        return resultado;
    }

    private void Guardar(List<CheatCode> codigos)
    {
        // Se escribe primero en un archivo temporal y luego se reemplaza el original
        // para no dejar un JSON a medias si el proceso se interrumpe.
        string temporal = _rutaArchivo + ".tmp";
        File.WriteAllText(temporal, JsonSerializer.Serialize(codigos, _opcionesEscritura));
        File.Move(temporal, _rutaArchivo, overwrite: true);
    }
}
