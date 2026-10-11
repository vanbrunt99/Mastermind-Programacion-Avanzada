# Mastermind — Evaluador con cheat code integrado

Solución en C# / .NET 10 para el juego Mastermind (código secreto de colores).
Curso BSI-19 Programación Avanzada.

## Estructura

| Proyecto | Descripción |
|---|---|
| `Mastermind.BusinessLogic` | Biblioteca con toda la lógica: evaluación, validaciones, paleta de colores, historial y cheat codes. |
| `Mastermind.AppConsole` | Aplicación de consola (partida interactiva o evaluación de un intento). Solo lee y muestra datos. |
| `Mastermind.Tests` | Pruebas unitarias con xUnit. |

Archivos principales de `Mastermind.BusinessLogic`:

| Archivo | Responsabilidad |
|---|---|
| `EvaluadorMastermind.cs` | `EvaluarIntento(patronSecreto, intentoJugador, cheatCode = null)`. |
| `ResultadoEvaluacion.cs` | Resultado: `BienPosicionados`, `ColorCorrecto` y `Pista` (solo con cheat code). |
| `ConfiguracionJuego.cs` / `ColorJuego.cs` | Paleta de colores y longitudes válidas (4 a 6). |
| `CargadorConfiguracionJuego.cs` | Lee la paleta desde un JSON (configuración dinámica). |
| `EntradaInvalidaException.cs` / `ErrorValidacion.cs` | Escenarios de error de validación. |
| `RegistroEvaluacion.cs`, `IRegistroEvaluaciones.cs`, `RegistroEvaluacionesEnMemoria.cs`, `RegistroEvaluacionesArchivo.cs` | Historial de evaluaciones con fecha. |
| `CheatCode.cs`, `ICheatCodeRepositorio.cs`, `CheatCodeRepositorioJson.cs`, `PistaCheat.cs` | Cheat codes y pistas. |
| `GeneradorPatronSecreto.cs` | Patrón aleatorio para la partida interactiva. |

## Algoritmo

`EvaluarIntento` devuelve:

- `BienPosicionados`: fichas con color y posición correctos.
- `ColorCorrecto`: fichas con un color del patrón, pero en otra posición.

1. Se cuentan primero las coincidencias exactas.
2. Con las fichas que no coincidieron, se cuentan los colores correctos en otra posición.
3. Cada ficha del patrón se empareja una sola vez, por lo que no hay doble conteo.

Ejemplo del enunciado: patrón `RGBY`, intento `RBYG` → 1 bien posicionado, 3 con color correcto.

El resultado solo informa cantidades; nunca indica cuáles fichas acertaron ni muestra el patrón.

## Colores (configuración dinámica)

La paleta por defecto contiene los nueve colores del enunciado. Cuando la inicial ya está ocupada
se usa la última letra del nombre:

| Código | Color |
|---|---|
| Y | Yellow |
| C | Cyan |
| B | Black |
| P | Purple |
| W | White |
| K | Pink |
| R | Red |
| N | Brown |
| G | Green |

La consola lee la paleta de `Mastermind.AppConsole/configuracion-colores.json`, por lo que se puede cambiar
sin recompilar. Los códigos son de un carácter (letra o dígito) y no distinguen mayúsculas de minúsculas.
Ejemplo de paleta numérica:

```json
{
  "colores": [
    { "codigo": "1", "nombre": "Yellow" },
    { "codigo": "2", "nombre": "Cyan" },
    { "codigo": "3", "nombre": "Black" }
  ]
}
```

Reglas de la paleta: al menos dos colores, códigos únicos y alfanuméricos, nombres no vacíos.

## Validaciones

`EvaluarIntento` lanza `ArgumentNullException` si el patrón o el intento es `null`, y
`EntradaInvalidaException` (deriva de `ArgumentException`) con la propiedad `Error` en estos casos:

| `ErrorValidacion` | Caso |
|---|---|
| `EntradaVacia` | Patrón o intento vacío. |
| `LongitudFueraDeRango` | Longitud menor que 4 o mayor que 6. |
| `LongitudesDistintas` | El intento no mide lo mismo que el patrón. |
| `ColorNoPermitido` | Carácter que no está en la paleta. |
| `CheatCodeVacio` | Cheat code vacío o con solo espacios. |
| `CheatCodeInexistente` | El código no está en `cheatcodes.json`. |
| `CheatCodeRepetido` | El mismo código ya se usó en la partida. |
| `CheatCodeLimitePorPartida` | Ya se usó otro código en la partida (solo uno por partida). |
| `CheatCodeAgotado` | El código tiene `usosDisponibles` en 0. |

Todas las validaciones se hacen antes de cambiar cualquier estado: un intento rechazado no consume
cheat codes ni se registra en el historial.

## Historial con fecha

Cada evaluación válida se registra con este formato:

```
En la fecha 11-10-2026 09:38:36 se evaluó el intento RBYG contra el patrón RGBY. Resultado: 1 bien posicionados – 3 con color correcto.
```

- Por defecto se guarda en memoria (`RegistroEvaluacionesEnMemoria`).
- La consola lo guarda en `registro-evaluaciones.log`, junto al ejecutable. Este archivo es interno:
  el jugador no lo ve durante la partida y está excluido del repositorio (`*.log` en `.gitignore`).

## Cheat code

Firma: `EvaluarIntento(string patronSecreto, string intentoJugador, string? cheatCode = null)`.

- Se valida que el código exista en `cheatcodes.json`, que tenga usos disponibles y que no se haya usado en la partida.
- Solo se permite un cheat code por partida (una instancia de `EvaluadorMastermind` = una partida).
- Se elige una posición aleatoria y se revela su color: `Pista: la posición 3 tiene el color Black (B).`
- Si el jugador ya había acertado esa posición: `Sin suerte: la posición 3 ya estaba acertada.`
- El código siempre se consume cuando se aplica: `usosDisponibles` baja en uno y se guarda en el JSON.
- Si `cheatCode` se omite o es `null`, el método se comporta igual que la versión básica.

Archivo `cheatcodes.json` (formato del enunciado):

```json
[
  { "hash": "A1B2C3D4E5F6G7H8I9J0ABCDE", "usosDisponibles": 1 },
  { "hash": "Z9Y8X7W6V5U4T3S2R1Q0LMNOP", "usosDisponibles": 3 }
]
```

El valor de `hash` es el código que escribe el jugador (se compara sin distinguir mayúsculas).
La consola usa la copia que queda en `bin/.../cheatcodes.json`; para restablecer los usos, borre esa copia
y vuelva a compilar.

## Ejecución

Requisito: SDK de .NET 10.

```bash
dotnet build Mastermind.sln

# Partida interactiva (4 fichas por defecto, o 5 / 6)
dotnet run --project Mastermind.AppConsole
dotnet run --project Mastermind.AppConsole -- jugar 5

# Evaluar un solo intento
dotnet run --project Mastermind.AppConsole -- RGBY RBYG
dotnet run --project Mastermind.AppConsole -- RGBY RRRR A1B2C3D4E5F6G7H8I9J0ABCDE
```

En la partida interactiva cada línea es un intento; para usar un cheat code se escribe después del intento,
separado por un espacio (`RGBY Z9Y8X7W6V5U4T3S2R1Q0LMNOP`). `salir` termina la partida.

## Pruebas

```bash
dotnet test Mastermind.sln
```

| Clase de pruebas | Qué verifica |
|---|---|
| `EvaluarIntentoBasicoTests` | Coincidencia completa, parcial (ejemplo `RGBY`/`RBYG`), ninguna coincidencia y duplicados. Se conserva sin cambios desde la versión básica. |
| `ValidacionesTests` | Entradas nulas o vacías, longitudes 4-6, longitudes distintas, colores no permitidos, mayúsculas/minúsculas. |
| `ConfiguracionColoresTests` | Paleta por defecto, paletas numérica y hexadecimal, paletas inválidas y lectura desde JSON. |
| `RegistroEvaluacionesTests` | Formato exacto del mensaje con fecha, datos guardados y registro en archivo. |
| `CheatCodeTests` | Cheat válido, inútil ("sin suerte"), repetido, límite por partida, agotado, inexistente y vacío. |
| `CheatCodeRepositorioJsonTests` | Lectura, validación y persistencia de `cheatcodes.json`. |
| `RetrocompatibilidadTests` | Con `cheatCode` omitido o `null` el resultado es igual al de la versión básica. |
| `GeneradorPatronSecretoTests` | Patrones aleatorios válidos para la partida interactiva. |

Las pruebas de cheat codes trabajan sobre copias temporales del JSON, por lo que no modifican el archivo del proyecto.

## Flujo de trabajo en Git

1. Algoritmo básico → commit.
2. Pruebas unitarias → commit.
3. Requisitos avanzados → commit (las pruebas básicas no se tocaron y siguen pasando).
4. Pruebas actualizadas → commit.
5. Push al repositorio remoto.
