using System.Globalization;

namespace Mastermind.BusinessLogic;

/// <summary>
/// Fila del historial de evaluaciones.
/// </summary>
/// <param name="Fecha">Fecha y hora local de la evaluación.</param>
/// <param name="Intento">Intento evaluado (normalizado a mayúsculas).</param>
/// <param name="Patron">Patrón secreto contra el que se evaluó.</param>
/// <param name="BienPosicionados">Fichas con color y posición correctos.</param>
/// <param name="ColorCorrecto">Fichas con color correcto en otra posición.</param>
public sealed record RegistroEvaluacion(
    DateTime Fecha,
    string Intento,
    string Patron,
    int BienPosicionados,
    int ColorCorrecto)
{
    /// <summary>Formato de fecha solicitado: dd-mm-yyyy hh:mm:ss (hora en formato de 24 horas).</summary>
    public const string FormatoFecha = "dd-MM-yyyy HH:mm:ss";

    /// <summary>
    /// Texto del registro con el formato del enunciado:
    /// "En la fecha dd-mm-yyyy hh:mm:ss se evaluó el intento XXXX contra el patrón YYYY.
    /// Resultado: x bien posicionados – z con color correcto."
    /// </summary>
    public string Mensaje =>
        $"En la fecha {Fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture)} " +
        $"se evaluó el intento {Intento} contra el patrón {Patron}. " +
        $"Resultado: {BienPosicionados} bien posicionados – {ColorCorrecto} con color correcto.";
}
