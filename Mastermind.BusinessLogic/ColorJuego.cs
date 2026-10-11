namespace Mastermind.BusinessLogic;

/// <summary>
/// Color disponible en el juego.
/// </summary>
/// <param name="Codigo">Carácter que representa el color dentro de un patrón (letra o dígito).</param>
/// <param name="Nombre">Nombre legible del color.</param>
public sealed record ColorJuego(char Codigo, string Nombre);
