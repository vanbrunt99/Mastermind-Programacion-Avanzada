namespace Mastermind.BusinessLogic;

/// <summary>
/// Configuración dinámica del juego: colores permitidos y longitudes válidas del patrón.
/// Los códigos de color no distinguen mayúsculas de minúsculas y se normalizan a mayúscula.
/// </summary>
public sealed class ConfiguracionJuego
{
    /// <summary>Longitud mínima permitida para un patrón.</summary>
    public const int LongitudMinima = 4;

    /// <summary>Longitud máxima permitida para un patrón.</summary>
    public const int LongitudMaxima = 6;

    private readonly Dictionary<char, ColorJuego> _coloresPorCodigo;

    /// <summary>
    /// Crea la configuración con la paleta por defecto (los nueve colores del enunciado).
    /// Cuando la inicial ya está ocupada se usa la última letra del nombre (pinK, browN).
    /// </summary>
    public ConfiguracionJuego()
        : this(new[]
        {
            new ColorJuego('Y', "Yellow"),
            new ColorJuego('C', "Cyan"),
            new ColorJuego('B', "Black"),
            new ColorJuego('P', "Purple"),
            new ColorJuego('W', "White"),
            new ColorJuego('K', "Pink"),
            new ColorJuego('R', "Red"),
            new ColorJuego('N', "Brown"),
            new ColorJuego('G', "Green"),
        })
    {
    }

    /// <summary>
    /// Crea la configuración con una paleta personalizada.
    /// </summary>
    /// <param name="colores">Colores permitidos. Se requieren al menos dos, con códigos únicos.</param>
    /// <exception cref="ArgumentException">La paleta no es válida.</exception>
    public ConfiguracionJuego(IEnumerable<ColorJuego> colores)
    {
        ArgumentNullException.ThrowIfNull(colores);

        _coloresPorCodigo = new Dictionary<char, ColorJuego>();
        var lista = new List<ColorJuego>();

        foreach (ColorJuego color in colores)
        {
            if (color is null)
            {
                throw new ArgumentException("La paleta contiene un color nulo.", nameof(colores));
            }

            if (!char.IsAsciiLetterOrDigit(color.Codigo))
            {
                throw new ArgumentException(
                    $"El código '{color.Codigo}' no es válido: solo se permiten letras (A-Z, a-z) o dígitos (0-9).",
                    nameof(colores));
            }

            if (string.IsNullOrWhiteSpace(color.Nombre))
            {
                throw new ArgumentException(
                    $"El color con código '{color.Codigo}' no tiene nombre.", nameof(colores));
            }

            var normalizado = new ColorJuego(char.ToUpperInvariant(color.Codigo), color.Nombre.Trim());
            if (!_coloresPorCodigo.TryAdd(normalizado.Codigo, normalizado))
            {
                throw new ArgumentException(
                    $"El código '{normalizado.Codigo}' está repetido en la paleta (no se distinguen mayúsculas de minúsculas).",
                    nameof(colores));
            }

            lista.Add(normalizado);
        }

        if (lista.Count < 2)
        {
            throw new ArgumentException("La paleta debe tener al menos dos colores.", nameof(colores));
        }

        Colores = lista.AsReadOnly();
    }

    /// <summary>Colores permitidos, en el orden en que fueron configurados.</summary>
    public IReadOnlyList<ColorJuego> Colores { get; }

    /// <summary>Indica si el carácter corresponde a un color de la paleta (sin distinguir mayúsculas).</summary>
    public bool EsColorValido(char codigo) => _coloresPorCodigo.ContainsKey(char.ToUpperInvariant(codigo));

    /// <summary>Obtiene el color asociado a un código.</summary>
    /// <exception cref="KeyNotFoundException">El código no pertenece a la paleta.</exception>
    public ColorJuego ObtenerColor(char codigo) =>
        _coloresPorCodigo.TryGetValue(char.ToUpperInvariant(codigo), out ColorJuego? color)
            ? color
            : throw new KeyNotFoundException($"El código '{codigo}' no pertenece a la paleta configurada.");

    /// <summary>Códigos permitidos separados por coma, útil para mensajes de error.</summary>
    public string DescribirCodigos() => string.Join(", ", Colores.Select(c => c.Codigo));
}
