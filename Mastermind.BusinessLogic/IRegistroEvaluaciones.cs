namespace Mastermind.BusinessLogic;

/// <summary>
/// Destino donde se guarda el historial de evaluaciones.
/// </summary>
public interface IRegistroEvaluaciones
{
    /// <summary>Agrega una evaluación al historial.</summary>
    void Registrar(RegistroEvaluacion registro);
}
