using System.Text.Json.Serialization;

namespace Mastermind.BusinessLogic;

/// <summary>
/// Entrada del archivo JSON de cheat codes:
/// <code>{ "hash": "A1B2C3D4E5F6G7H8I9J0ABCDE", "usosDisponibles": 1 }</code>
/// </summary>
public sealed class CheatCode
{
    /// <summary>Código que debe enviar el jugador (se compara sin distinguir mayúsculas).</summary>
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    /// <summary>Usos restantes. Cada uso aplicado lo reduce en uno; en cero ya no se acepta.</summary>
    [JsonPropertyName("usosDisponibles")]
    public int UsosDisponibles { get; set; }
}
