namespace Mastermind.BusinessLogic;

/// <summary>
/// Pista obtenida al aplicar un cheat code.
/// </summary>
/// <param name="Posicion">Posición revelada, contada desde 1.</param>
/// <param name="CodigoColor">Código del color que hay en esa posición del patrón.</param>
/// <param name="NombreColor">Nombre del color revelado.</param>
/// <param name="SinSuerte">
/// <c>true</c> cuando el jugador ya había acertado esa posición: la pista no aporta nada,
/// pero el cheat code igual se consume.
/// </param>
public sealed record PistaCheat(int Posicion, char CodigoColor, string NombreColor, bool SinSuerte)
{
    /// <summary>Mensaje para mostrar al jugador.</summary>
    public string Mensaje => SinSuerte
        ? $"Sin suerte: la posición {Posicion} ya estaba acertada."
        : $"Pista: la posición {Posicion} tiene el color {NombreColor} ({CodigoColor}).";
}
