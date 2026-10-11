namespace Mastermind.BusinessLogic;

/// <summary>
/// Origen de los cheat codes y de sus usos disponibles.
/// </summary>
public interface ICheatCodeRepositorio
{
    /// <summary>
    /// Devuelve los usos disponibles del código, o <c>null</c> si el código no existe.
    /// </summary>
    int? ObtenerUsosDisponibles(string codigo);

    /// <summary>Descuenta un uso del código y guarda el cambio.</summary>
    /// <exception cref="InvalidOperationException">El código no existe o no tiene usos disponibles.</exception>
    void ConsumirUso(string codigo);
}
