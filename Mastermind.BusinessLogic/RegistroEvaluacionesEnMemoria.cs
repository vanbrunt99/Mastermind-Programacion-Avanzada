namespace Mastermind.BusinessLogic;

/// <summary>
/// Historial en memoria (tabla de registros). Es el registro por defecto del evaluador.
/// </summary>
public class RegistroEvaluacionesEnMemoria : IRegistroEvaluaciones
{
    private readonly List<RegistroEvaluacion> _registros = new();
    private readonly object _bloqueo = new();

    /// <summary>Copia de los registros guardados, en orden cronológico.</summary>
    public IReadOnlyList<RegistroEvaluacion> Registros
    {
        get
        {
            lock (_bloqueo)
            {
                return _registros.ToList();
            }
        }
    }

    public void Registrar(RegistroEvaluacion registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        lock (_bloqueo)
        {
            _registros.Add(registro);
        }
    }
}
