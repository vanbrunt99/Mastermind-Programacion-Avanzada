namespace Mastermind.BusinessLogic;

/// <summary>
/// Error de validación de las entradas de EvaluarIntento.
/// Hereda de <see cref="ArgumentException"/> para que el código que ya capturaba
/// <see cref="ArgumentException"/> siga funcionando igual.
/// </summary>
public class EntradaInvalidaException : ArgumentException
{
    public EntradaInvalidaException(ErrorValidacion error, string mensaje, string? nombreParametro)
        : base(mensaje, nombreParametro)
    {
        Error = error;
        Detalle = mensaje;
    }

    /// <summary>Escenario de error que provocó la excepción.</summary>
    public ErrorValidacion Error { get; }

    /// <summary>Mensaje sin el sufijo con el nombre del parámetro, apto para mostrar al jugador.</summary>
    public string Detalle { get; }
}
