namespace Mastermind.BusinessLogic;

/// <summary>
/// Evalúa intentos del juego Mastermind (código secreto de colores).
/// Cada carácter del patrón representa una ficha de color.
/// Una instancia del evaluador representa una partida: el control de
/// "un cheat code por partida" vive en la instancia.
/// </summary>
public class EvaluadorMastermind
{
    private readonly ConfiguracionJuego _configuracion;
    private readonly IRegistroEvaluaciones _registro;
    private readonly ICheatCodeRepositorio? _cheatCodes;
    private readonly Random _aleatorio;
    private readonly TimeProvider _reloj;

    // Cheat code aplicado en esta partida; null mientras no se haya usado ninguno.
    private string? _cheatCodeUsadoEnPartida;

    /// <summary>
    /// Crea un evaluador con la paleta por defecto, historial en memoria y sin cheat codes.
    /// Conserva el uso original: <c>new EvaluadorMastermind().EvaluarIntento(patron, intento)</c>.
    /// </summary>
    public EvaluadorMastermind()
        : this(new ConfiguracionJuego(), new RegistroEvaluacionesEnMemoria())
    {
    }

    /// <summary>
    /// Crea un evaluador con todas sus dependencias configurables.
    /// </summary>
    /// <param name="configuracion">Paleta de colores permitidos.</param>
    /// <param name="registro">Destino del historial de evaluaciones.</param>
    /// <param name="cheatCodes">Origen de cheat codes; <c>null</c> si no se habilitan.</param>
    /// <param name="aleatorio">Generador para elegir la posición de la pista.</param>
    /// <param name="reloj">Fuente de fecha y hora para el historial.</param>
    public EvaluadorMastermind(
        ConfiguracionJuego configuracion,
        IRegistroEvaluaciones registro,
        ICheatCodeRepositorio? cheatCodes = null,
        Random? aleatorio = null,
        TimeProvider? reloj = null)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        ArgumentNullException.ThrowIfNull(registro);

        _configuracion = configuracion;
        _registro = registro;
        _cheatCodes = cheatCodes;
        _aleatorio = aleatorio ?? new Random();
        _reloj = reloj ?? TimeProvider.System;
    }

    /// <summary>Configuración de colores que usa este evaluador.</summary>
    public ConfiguracionJuego Configuracion => _configuracion;

    /// <summary>
    /// Compara el intento del jugador contra el patrón secreto.
    /// Primero cuenta las coincidencias exactas (color y posición) y luego,
    /// con las fichas que sobran, los colores correctos en otra posición.
    /// Una ficha del patrón solo puede emparejarse una vez, lo que evita el doble conteo.
    /// </summary>
    /// <param name="patronSecreto">Combinación que el jugador intenta adivinar.</param>
    /// <param name="intentoJugador">Combinación propuesta por el jugador.</param>
    /// <param name="cheatCode">
    /// Código opcional que revela el color de una posición aleatoria del patrón.
    /// Si se omite o es <c>null</c>, el método se comporta igual que la versión básica.
    /// </param>
    /// <returns>Cantidad de fichas bien posicionadas y con color correcto, y la pista si hubo cheat code.</returns>
    /// <exception cref="ArgumentNullException">El patrón o el intento es <c>null</c>.</exception>
    /// <exception cref="EntradaInvalidaException">Alguna entrada no cumple las validaciones.</exception>
    public ResultadoEvaluacion EvaluarIntento(string patronSecreto, string intentoJugador, string? cheatCode = null)
    {
        ArgumentNullException.ThrowIfNull(patronSecreto);
        ArgumentNullException.ThrowIfNull(intentoJugador);

        // 1. Validar todas las entradas antes de cambiar cualquier estado.
        string patron = ValidarCombinacion(patronSecreto, nameof(patronSecreto));
        string intento = ValidarCombinacion(intentoJugador, nameof(intentoJugador));

        if (patron.Length != intento.Length)
        {
            throw new EntradaInvalidaException(
                ErrorValidacion.LongitudesDistintas,
                $"El intento tiene {intento.Length} fichas y el patrón secreto tiene {patron.Length}; deben tener la misma longitud.",
                nameof(intentoJugador));
        }

        string? codigo = cheatCode is null ? null : ValidarCheatCode(cheatCode);

        // 2. Evaluar con el algoritmo básico.
        ResultadoEvaluacion resultado = Comparar(patron, intento);

        // 3. Aplicar el cheat code (siempre se consume, aunque la pista no ayude).
        PistaCheat? pista = codigo is null ? null : AplicarCheatCode(codigo, patron, intento);

        // 4. Registrar la evaluación con fecha y hora.
        _registro.Registrar(new RegistroEvaluacion(
            _reloj.GetLocalNow().DateTime,
            intento,
            patron,
            resultado.BienPosicionados,
            resultado.ColorCorrecto));

        return resultado with { Pista = pista };
    }

    private ResultadoEvaluacion Comparar(string patronSecreto, string intentoJugador)
    {
        int bienPosicionados = 0;

        // Fichas del patrón que no tuvieron coincidencia exacta, agrupadas por color.
        var pendientesPatron = new Dictionary<char, int>();
        // Fichas del intento que no tuvieron coincidencia exacta.
        var pendientesIntento = new List<char>();

        // Paso 1: coincidencias exactas (tienen prioridad sobre cualquier otra).
        for (int i = 0; i < patronSecreto.Length; i++)
        {
            if (patronSecreto[i] == intentoJugador[i])
            {
                bienPosicionados++;
            }
            else
            {
                pendientesPatron[patronSecreto[i]] = pendientesPatron.GetValueOrDefault(patronSecreto[i]) + 1;
                pendientesIntento.Add(intentoJugador[i]);
            }
        }

        // Paso 2: colores correctos en otra posición, consumiendo cada ficha del patrón una sola vez.
        int colorCorrecto = 0;
        foreach (char color in pendientesIntento)
        {
            if (pendientesPatron.TryGetValue(color, out int disponibles) && disponibles > 0)
            {
                colorCorrecto++;
                pendientesPatron[color] = disponibles - 1;
            }
        }

        return new ResultadoEvaluacion(bienPosicionados, colorCorrecto);
    }

    private string ValidarCombinacion(string valor, string nombreParametro)
    {
        string combinacion = valor.Trim().ToUpperInvariant();

        if (combinacion.Length == 0)
        {
            throw new EntradaInvalidaException(
                ErrorValidacion.EntradaVacia,
                $"El valor de {nombreParametro} no puede estar vacío.",
                nombreParametro);
        }

        if (combinacion.Length < ConfiguracionJuego.LongitudMinima ||
            combinacion.Length > ConfiguracionJuego.LongitudMaxima)
        {
            throw new EntradaInvalidaException(
                ErrorValidacion.LongitudFueraDeRango,
                $"El valor de {nombreParametro} tiene {combinacion.Length} fichas; " +
                $"la longitud debe estar entre {ConfiguracionJuego.LongitudMinima} y {ConfiguracionJuego.LongitudMaxima}.",
                nombreParametro);
        }

        for (int i = 0; i < combinacion.Length; i++)
        {
            if (!_configuracion.EsColorValido(combinacion[i]))
            {
                throw new EntradaInvalidaException(
                    ErrorValidacion.ColorNoPermitido,
                    $"El carácter '{valor.Trim()[i]}' en la posición {i + 1} de {nombreParametro} no es un color permitido. " +
                    $"Colores válidos: {_configuracion.DescribirCodigos()}.",
                    nombreParametro);
            }
        }

        return combinacion;
    }

    private string ValidarCheatCode(string cheatCode)
    {
        string codigo = cheatCode.Trim();

        if (codigo.Length == 0)
        {
            throw new EntradaInvalidaException(
                ErrorValidacion.CheatCodeVacio,
                "El cheat code no puede estar vacío.",
                nameof(cheatCode));
        }

        int? usosDisponibles = _cheatCodes?.ObtenerUsosDisponibles(codigo);
        if (usosDisponibles is null)
        {
            throw new EntradaInvalidaException(
                ErrorValidacion.CheatCodeInexistente,
                "El cheat code no existe en la lista de códigos válidos.",
                nameof(cheatCode));
        }

        if (_cheatCodeUsadoEnPartida is not null)
        {
            bool esElMismo = string.Equals(_cheatCodeUsadoEnPartida, codigo, StringComparison.OrdinalIgnoreCase);
            throw new EntradaInvalidaException(
                esElMismo ? ErrorValidacion.CheatCodeRepetido : ErrorValidacion.CheatCodeLimitePorPartida,
                esElMismo
                    ? "Este cheat code ya se utilizó en esta partida."
                    : "Ya usaste un cheat code en esta partida; solo se permite uno por partida.",
                nameof(cheatCode));
        }

        if (usosDisponibles <= 0)
        {
            throw new EntradaInvalidaException(
                ErrorValidacion.CheatCodeAgotado,
                "El cheat code ya no tiene usos disponibles.",
                nameof(cheatCode));
        }

        return codigo;
    }

    private PistaCheat AplicarCheatCode(string codigo, string patron, string intento)
    {
        int indice = _aleatorio.Next(patron.Length);
        ColorJuego color = _configuracion.ObtenerColor(patron[indice]);
        bool sinSuerte = intento[indice] == patron[indice];

        // El código se consume aunque la pista no aporte información nueva.
        _cheatCodes!.ConsumirUso(codigo);
        _cheatCodeUsadoEnPartida = codigo;

        return new PistaCheat(indice + 1, color.Codigo, color.Nombre, sinSuerte);
    }
}
