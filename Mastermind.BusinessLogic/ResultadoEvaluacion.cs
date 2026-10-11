namespace Mastermind.BusinessLogic;

/// <summary>
/// Resultado de comparar un intento del jugador contra el patrón secreto.
/// Solo expone cantidades: nunca revela cuáles fichas acertaron ni el patrón.
/// </summary>
/// <param name="BienPosicionados">Fichas con el color correcto en la posición correcta.</param>
/// <param name="ColorCorrecto">Fichas con un color presente en el patrón, pero en otra posición.</param>
public sealed record ResultadoEvaluacion(int BienPosicionados, int ColorCorrecto)
{
    /// <summary>
    /// Pista del cheat code aplicado en este intento; <c>null</c> cuando no se envió cheat code.
    /// </summary>
    public PistaCheat? Pista { get; init; }
}
