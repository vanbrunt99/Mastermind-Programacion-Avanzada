using System.Text;

namespace Mastermind.BusinessLogic;

/// <summary>
/// Historial persistente: agrega una línea por evaluación a un archivo de texto (UTF-8).
/// </summary>
public class RegistroEvaluacionesArchivo : IRegistroEvaluaciones
{
    private readonly string _rutaArchivo;
    private readonly object _bloqueo = new();
    private readonly Encoding _codificacion = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public RegistroEvaluacionesArchivo(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        _rutaArchivo = rutaArchivo;
    }

    public void Registrar(RegistroEvaluacion registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        lock (_bloqueo)
        {
            File.AppendAllText(_rutaArchivo, registro.Mensaje + Environment.NewLine, _codificacion);
        }
    }
}
