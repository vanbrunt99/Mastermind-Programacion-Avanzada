namespace Mastermind.BusinessLogic;

/// <summary>
/// Escenarios de error de validación de EvaluarIntento.
/// </summary>
public enum ErrorValidacion
{
    /// <summary>El patrón o el intento está vacío o solo tiene espacios.</summary>
    EntradaVacia,

    /// <summary>La longitud está fuera del rango permitido (4 a 6).</summary>
    LongitudFueraDeRango,

    /// <summary>El intento no tiene la misma longitud que el patrón secreto.</summary>
    LongitudesDistintas,

    /// <summary>Se usó un carácter que no pertenece a la paleta configurada.</summary>
    ColorNoPermitido,

    /// <summary>Se envió un cheat code vacío o con solo espacios.</summary>
    CheatCodeVacio,

    /// <summary>El cheat code no existe en el archivo JSON de cheat codes.</summary>
    CheatCodeInexistente,

    /// <summary>El cheat code ya no tiene usos disponibles.</summary>
    CheatCodeAgotado,

    /// <summary>El mismo cheat code ya se utilizó en esta partida.</summary>
    CheatCodeRepetido,

    /// <summary>En esta partida ya se usó un cheat code (solo se permite uno por partida).</summary>
    CheatCodeLimitePorPartida,
}
