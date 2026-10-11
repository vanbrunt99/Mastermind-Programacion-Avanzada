using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mastermind.BusinessLogic;

/// <summary>
/// Lee la paleta de colores desde un archivo JSON, lo que permite cambiar los colores
/// sin recompilar. Formato esperado:
/// <code>
/// { "colores": [ { "codigo": "Y", "nombre": "Yellow" }, { "codigo": "1", "nombre": "Uno" } ] }
/// </code>
/// </summary>
public class CargadorConfiguracionJuego
{
    /// <summary>Carga la configuración desde la ruta indicada.</summary>
    /// <exception cref="FileNotFoundException">El archivo no existe.</exception>
    /// <exception cref="InvalidDataException">El contenido no tiene el formato esperado.</exception>
    public ConfiguracionJuego Cargar(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException("No se encontró el archivo de configuración de colores.", rutaArchivo);
        }

        ArchivoConfiguracion? archivo;
        try
        {
            archivo = JsonSerializer.Deserialize<ArchivoConfiguracion>(File.ReadAllText(rutaArchivo));
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"El archivo de colores '{rutaArchivo}' no es un JSON válido.", ex);
        }

        if (archivo?.Colores is null)
        {
            throw new InvalidDataException($"El archivo de colores '{rutaArchivo}' no contiene la lista \"colores\".");
        }

        var colores = new List<ColorJuego>();
        foreach (ColorArchivo? color in archivo.Colores)
        {
            if (color?.Codigo is null || color.Codigo.Length != 1)
            {
                throw new InvalidDataException("Cada color debe tener un \"codigo\" de exactamente un carácter.");
            }

            colores.Add(new ColorJuego(color.Codigo[0], color.Nombre ?? string.Empty));
        }

        try
        {
            return new ConfiguracionJuego(colores);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException($"La paleta del archivo '{rutaArchivo}' no es válida: {ex.Message}", ex);
        }
    }

    private sealed class ArchivoConfiguracion
    {
        [JsonPropertyName("colores")]
        public List<ColorArchivo?>? Colores { get; set; }
    }

    private sealed class ColorArchivo
    {
        [JsonPropertyName("codigo")]
        public string? Codigo { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }
    }
}
