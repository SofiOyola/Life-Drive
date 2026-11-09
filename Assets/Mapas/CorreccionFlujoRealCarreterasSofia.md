# Corrección del flujo real: JUGAR muestra las carreteras intermedias

Prompt 45. Unity 6000.6.7f1. Rama `feat/unity-map`. Commit `3d08667`.
Documento redactado el 9 de noviembre de 2026 a partir del código del commit y de `DiagnosticoProgresionVisibleSofia.md`. No recoge conteos nuevos de pruebas.

## Problema

Al jugar `JUGAR → Registro → COMENZAR RECORRIDO` se veían preguntas en `S0` y un salto directo a `DELIVERY`. El flujo normal entraba a V1, que no tiene correspondencia visual con el árbol V2 (ver `ConexionMenuJuegoRealSofia.md` y `DiagnosticoProgresionVisibleSofia.md`). Las pruebas pasaban porque usaban un lanzador del Editor que activaba V2.

## Qué cambió

| Archivo | Cambio |
|---|---|
| `Scripts/IntegracionV2/EntradaJuego.cs` | En Editor y `DEVELOPMENT_BUILD` activa el flujo V2 con el catálogo de mapas. En compilación final activa `ActivarVigente` (V1, `Sesion.Juego`, `persistentDataPath`). |
| `Editor/InterfazVisual/FlujoJugadorPilotoV2Editor.cs` | El menú *Abrir flujo del jugador* solo abre `Inicio.unity`. Se eliminó el activador propio del lanzador. Ya no hay un camino distinto del que usa el jugador. |
| `Scripts/Mapas/PaqueteRecorridos.cs` | Nueva validación: la situación de aproximación (id 6) abre el tramo de entrega, y ningún mapa presenta más de dos situaciones. |
| `Scripts/Mapas/ReanudacionRecorridosV2.cs` | Usa la constante compartida `PaqueteRecorridos.SituacionAproximacion`. |
| `Tests/PlayMode/FlujoJugadorPilotoV2Tests.EntradaNormal.cs` | Ocho rutas V2 desde Inicio por el botón real, con interrupción y reanudación en R3A y R3H. |

## Resultado

Sofía confirmó visualmente en el Editor que JUGAR ya muestra `S0 → R1 → R2 → R3 → DELIVERY`. Última suite conocida: 81 EditMode + 64 Play Mode, 0 fallos.

## Límites

- **Una compilación final (sin Development) arranca en V1** y no muestra las ocho rutas. Es deliberado: V2 sigue siendo candidato sin aprobación educativa. Si se quiere distribuir V2 hay que cambiar `EntradaJuego` tras esa aprobación.
- No se generó un build final para comprobar la ocultación del botón de piloto.
- V1 no tiene correspondencia de fondos para sus tramos. Depende de Javier y el equipo.
