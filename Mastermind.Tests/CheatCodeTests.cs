using Mastermind.BusinessLogic;
using Mastermind.Tests.Dobles;

namespace Mastermind.Tests;

/// <summary>
/// Cheat code integrado: válido, inútil (sin suerte), repetido, agotado e inválido.
/// Cada prueba usa su propia copia temporal del archivo JSON.
/// </summary>
public class CheatCodeTests : IDisposable
{
    private const string CodigoUnUso = "A1B2C3D4E5F6G7H8I9J0ABCDE";
    private const string CodigoTresUsos = "Z9Y8X7W6V5U4T3S2R1Q0LMNOP";
    private const string CodigoAgotado = "AGOTADO00000000000000000X";

    private readonly ArchivoTemporal _archivo = new($$"""
        [
          { "hash": "{{CodigoUnUso}}", "usosDisponibles": 1 },
          { "hash": "{{CodigoTresUsos}}", "usosDisponibles": 3 },
          { "hash": "{{CodigoAgotado}}", "usosDisponibles": 0 }
        ]
        """);

    private readonly CheatCodeRepositorioJson _repositorio;

    public CheatCodeTests()
    {
        _repositorio = new CheatCodeRepositorioJson(_archivo.Ruta);
    }

    public void Dispose() => _archivo.Dispose();

    /// <summary>Crea una partida nueva cuya pista siempre cae en el índice indicado (base 0).</summary>
    private EvaluadorMastermind NuevaPartida(int indicePista = 0) =>
        new(new ConfiguracionJuego(), new RegistroEvaluacionesEnMemoria(), _repositorio, new RandomFijo(indicePista));

    /// <summary>Usos guardados en el archivo, leídos con una instancia nueva del repositorio.</summary>
    private int? UsosEnArchivo(string codigo) => new CheatCodeRepositorioJson(_archivo.Ruta).ObtenerUsosDisponibles(codigo);

    // ---------- Cheat válido ----------

    [Fact]
    public void CheatValido_RevelaElColorDeLaPosicionElegida()
    {
        ResultadoEvaluacion resultado = NuevaPartida(indicePista: 2).EvaluarIntento("RGBY", "RRRR", CodigoTresUsos);

        Assert.NotNull(resultado.Pista);
        Assert.Equal(3, resultado.Pista.Posicion);
        Assert.Equal('B', resultado.Pista.CodigoColor);
        Assert.Equal("Black", resultado.Pista.NombreColor);
        Assert.False(resultado.Pista.SinSuerte);
        Assert.Equal("Pista: la posición 3 tiene el color Black (B).", resultado.Pista.Mensaje);
    }

    [Fact]
    public void CheatValido_DescuentaUnUsoEnElArchivoJson()
    {
        NuevaPartida().EvaluarIntento("RGBY", "CCCC", CodigoTresUsos);

        Assert.Equal(2, UsosEnArchivo(CodigoTresUsos));
        Assert.Equal(1, UsosEnArchivo(CodigoUnUso));
    }

    [Fact]
    public void CheatValido_NoCambiaElConteoDelIntento()
    {
        ResultadoEvaluacion conCheat = NuevaPartida(indicePista: 1).EvaluarIntento("RGBY", "RBYG", CodigoTresUsos);
        ResultadoEvaluacion sinCheat = new EvaluadorMastermind().EvaluarIntento("RGBY", "RBYG");

        Assert.Equal(sinCheat.BienPosicionados, conCheat.BienPosicionados);
        Assert.Equal(sinCheat.ColorCorrecto, conCheat.ColorCorrecto);
    }

    [Fact]
    public void CheatValido_NoDistingueMayusculasNiEspacios()
    {
        ResultadoEvaluacion resultado = NuevaPartida().EvaluarIntento("RGBY", "CCCC", "  " + CodigoTresUsos.ToLowerInvariant() + " ");

        Assert.NotNull(resultado.Pista);
        Assert.Equal(2, UsosEnArchivo(CodigoTresUsos));
    }

    [Fact]
    public void CheatValido_EligeLaPosicionEntreTodasLasDelPatron()
    {
        var aleatorio = new RandomFijo(5);
        var evaluador = new EvaluadorMastermind(
            new ConfiguracionJuego(), new RegistroEvaluacionesEnMemoria(), _repositorio, aleatorio);

        ResultadoEvaluacion resultado = evaluador.EvaluarIntento("RGBYCP", "CCCCCC", CodigoTresUsos);

        Assert.Equal(6, aleatorio.UltimoMaximo);
        Assert.Equal(6, resultado.Pista!.Posicion);
        Assert.Equal("Purple", resultado.Pista.NombreColor);
    }

    // ---------- Cheat inútil ----------

    [Fact]
    public void CheatInutil_PosicionYaAcertada_DevuelveSinSuerte()
    {
        ResultadoEvaluacion resultado = NuevaPartida(indicePista: 0).EvaluarIntento("RGBY", "RCCC", CodigoTresUsos);

        Assert.NotNull(resultado.Pista);
        Assert.True(resultado.Pista.SinSuerte);
        Assert.Equal(1, resultado.Pista.Posicion);
        Assert.Equal("Sin suerte: la posición 1 ya estaba acertada.", resultado.Pista.Mensaje);
    }

    [Fact]
    public void CheatInutil_IgualSeConsume()
    {
        var partida = NuevaPartida(indicePista: 0);

        partida.EvaluarIntento("RGBY", "RCCC", CodigoTresUsos);

        Assert.Equal(2, UsosEnArchivo(CodigoTresUsos));
        var ex = Assert.Throws<EntradaInvalidaException>(() => partida.EvaluarIntento("RGBY", "RCCC", CodigoTresUsos));
        Assert.Equal(ErrorValidacion.CheatCodeRepetido, ex.Error);
    }

    // ---------- Cheat repetido / ya utilizado ----------

    [Fact]
    public void CheatRepetido_MismoCodigoEnLaMismaPartida_SeRechaza()
    {
        var partida = NuevaPartida();
        partida.EvaluarIntento("RGBY", "CCCC", CodigoTresUsos);

        var ex = Assert.Throws<EntradaInvalidaException>(() => partida.EvaluarIntento("RGBY", "PPPP", CodigoTresUsos));

        Assert.Equal(ErrorValidacion.CheatCodeRepetido, ex.Error);
        Assert.Equal("cheatCode", ex.ParamName);
        Assert.Equal(2, UsosEnArchivo(CodigoTresUsos)); // solo se descontó el primer uso
    }

    [Fact]
    public void CheatRepetido_OtroCodigoEnLaMismaPartida_SeRechazaPorLimite()
    {
        var partida = NuevaPartida();
        partida.EvaluarIntento("RGBY", "CCCC", CodigoTresUsos);

        var ex = Assert.Throws<EntradaInvalidaException>(() => partida.EvaluarIntento("RGBY", "PPPP", CodigoUnUso));

        Assert.Equal(ErrorValidacion.CheatCodeLimitePorPartida, ex.Error);
        Assert.Equal(1, UsosEnArchivo(CodigoUnUso)); // el segundo código no se tocó
    }

    [Fact]
    public void CheatRepetido_SinCheat_LaPartidaContinuaNormalmente()
    {
        var partida = NuevaPartida();
        partida.EvaluarIntento("RGBY", "CCCC", CodigoTresUsos);

        ResultadoEvaluacion resultado = partida.EvaluarIntento("RGBY", "RBYG");

        Assert.Equal(1, resultado.BienPosicionados);
        Assert.Equal(3, resultado.ColorCorrecto);
        Assert.Null(resultado.Pista);
    }

    // ---------- Cheat agotado ----------

    [Fact]
    public void CheatAgotado_SinUsosDisponibles_SeRechaza()
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => NuevaPartida().EvaluarIntento("RGBY", "CCCC", CodigoAgotado));

        Assert.Equal(ErrorValidacion.CheatCodeAgotado, ex.Error);
        Assert.Equal(0, UsosEnArchivo(CodigoAgotado));
    }

    [Fact]
    public void CheatAgotado_TrasUsarseEnOtraPartida_SeRechaza()
    {
        NuevaPartida().EvaluarIntento("RGBY", "CCCC", CodigoUnUso);
        Assert.Equal(0, UsosEnArchivo(CodigoUnUso));

        var ex = Assert.Throws<EntradaInvalidaException>(() => NuevaPartida().EvaluarIntento("RGBY", "CCCC", CodigoUnUso));

        Assert.Equal(ErrorValidacion.CheatCodeAgotado, ex.Error);
    }

    [Fact]
    public void CheatConVariosUsos_SeUsaUnaVezPorPartidaHastaAgotarse()
    {
        for (int partida = 1; partida <= 3; partida++)
        {
            Assert.NotNull(NuevaPartida().EvaluarIntento("RGBY", "CCCC", CodigoTresUsos).Pista);
            Assert.Equal(3 - partida, UsosEnArchivo(CodigoTresUsos));
        }

        var ex = Assert.Throws<EntradaInvalidaException>(() => NuevaPartida().EvaluarIntento("RGBY", "CCCC", CodigoTresUsos));
        Assert.Equal(ErrorValidacion.CheatCodeAgotado, ex.Error);
    }

    // ---------- Cheat inválido ----------

    [Theory]
    [InlineData("NOEXISTE")]
    [InlineData("A1B2C3D4E5F6G7H8I9J0ABCD")]
    [InlineData("Machillo tricampeón 2026-2028**")]
    public void CheatInexistente_SeRechaza(string cheatCode)
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => NuevaPartida().EvaluarIntento("RGBY", "CCCC", cheatCode));

        Assert.Equal(ErrorValidacion.CheatCodeInexistente, ex.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CheatVacio_SeRechaza(string cheatCode)
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => NuevaPartida().EvaluarIntento("RGBY", "CCCC", cheatCode));

        Assert.Equal(ErrorValidacion.CheatCodeVacio, ex.Error);
    }

    [Fact]
    public void CheatRechazado_NoGastaElCheatDeLaPartida()
    {
        var partida = NuevaPartida();
        Assert.Throws<EntradaInvalidaException>(() => partida.EvaluarIntento("RGBY", "CCCC", "NOEXISTE"));

        ResultadoEvaluacion resultado = partida.EvaluarIntento("RGBY", "CCCC", CodigoTresUsos);

        Assert.NotNull(resultado.Pista);
    }

    [Fact]
    public void IntentoInvalido_ConCheatValido_NoConsumeElCheat()
    {
        var partida = NuevaPartida();

        var ex = Assert.Throws<EntradaInvalidaException>(() => partida.EvaluarIntento("RGBY", "RGB", CodigoUnUso));

        Assert.Equal(ErrorValidacion.LongitudFueraDeRango, ex.Error);
        Assert.Equal(1, UsosEnArchivo(CodigoUnUso));
        Assert.NotNull(partida.EvaluarIntento("RGBY", "CCCC", CodigoUnUso).Pista);
    }

    [Fact]
    public void SinRepositorioDeCheatCodes_CualquierCodigoEsInexistente()
    {
        var ex = Assert.Throws<EntradaInvalidaException>(() => new EvaluadorMastermind().EvaluarIntento("RGBY", "CCCC", CodigoUnUso));

        Assert.Equal(ErrorValidacion.CheatCodeInexistente, ex.Error);
    }
}
