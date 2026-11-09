# Corrección de teclado y foco de Life Drive

Unity 6000.6.7f1 · Rama `feat/unity-map` · Prompt 43 · S3-07 y S4-03

## Casos e historial verificable

Los casos pertenecen a `LifeDrive.Tests.FlujoJugadorPilotoV2Tests`.

| Caso | Foco esperado | Foco que quedó | Punto de la aserción |
| --- | --- | --- | --- |
| `AccesibilidadJugadorTecladoRegistroEntregaYReanudacion(1920)` | Administradores | Jugar | Después de `Pulsar(Key.Tab, true)` en Inicio |
| `AccesibilidadJugadorTecladoRegistroEntregaYReanudacion(1280)` | Administradores | Jugar | Después de `Pulsar(Key.Tab, true)` en Inicio |
| `AccesibilidadTextoLargoDesplazableSinEvaluar(1920)` | Lectura de ficha | Respuesta 1 | `TeclasPilotoPruebas.IrA` al recorrer los controles con Tab |
| `AccesibilidadTextoLargoDesplazableSinEvaluar(1280)` | Lectura de ficha | Respuesta 1 | `TeclasPilotoPruebas.IrA` al recorrer los controles con Tab |

El Prompt 41 registró 64 de 64 pruebas Play Mode aprobadas. En el Prompt 42, estos cuatro casos fallaron en la suite final y volvieron a fallar al ejecutarlos como grupo aislado. Sus mensajes originales están en [Verificacion.json del Prompt 42](../../TestEvidence/ProgresionVisualV2/Verificacion.json). El trabajo histórico de MCP conserva los mensajes de aserción y el progreso. La traza de pila completa de esa corrida ya no está disponible en el resultado retenido. Los puntos de la aserción se localizaron en `FlujoJugadorPilotoV2Tests.Accesibilidad.cs` y `TeclasPilotoPruebas.cs`.

En esta revisión, las cuatro ejecuciones individuales previas a la edición aprobaron. Esto muestra que el fallo dependía de las condiciones de ejecución. Una sonda separada reprodujo la pérdida de la entrada bajo esas condiciones de manera controlada.

## Causa y traza de diagnóstico

Las pruebas crean un `Keyboard` virtual y encolan estados con `InputSystem.QueueStateEvent`. El controlador real, `TecladoPilotoV2.LateUpdate`, lee `Keyboard.current.tabKey.wasPressedThisFrame` antes de llamar a `Mover`. Las aserciones que preceden al movimiento muestran que el EventSystem había seleccionado correctamente el control inicial.

En una escena vacía, se instrumentaron los eventos del dispositivo y las actualizaciones del Input System. Con foco, Tab se registró en el ciclo `Dynamic`. Cambiar de ventana dentro de Unity manteniendo el foco de la aplicación también permitió recibir Tab. Al simular el evento de pérdida de foco de la aplicación, la configuración original `ResetAndDisableNonBackgroundDevices` produjo `HardReset` y `Disabled` en el teclado virtual. Con `PointersAndKeyboardsRespectGameViewFocus`, la pulsación ya no apareció en el ciclo del juego. Esto explica que Mayús+Tab deje el foco en Jugar y que los intentos de Tab no alcancen Lectura de ficha.

Con la ayuda corregida, el mismo evento de pérdida de foco y la misma pulsación produjeron Tab en `Dynamic`, con el dispositivo virtual como teclado actual. Al cerrar la ayuda, la sonda comprobó que las opciones de entrada y la ejecución en segundo plano recuperaron sus valores originales y el dispositivo virtual se retiró. [Traza de la sonda](../../TestEvidence/CorreccionTecladoFocoSofia/SondaFoco.json)

## Corrección

El cambio se aplicó a `Assets/Tests/PlayMode/TeclasPilotoPruebas.cs`. La ayuda captura la configuración de entrada, permite la ejecución en segundo plano durante su ámbito y configura temporalmente `IgnoreFocus` y `AllDeviceInputAlwaysGoesToGameView` antes de crear el teclado virtual. Al disponer de la ayuda, retira el dispositivo, restablece el teclado anterior y restaura las propiedades originales. La restauración también se ejecuta si falla la preparación.

El dispositivo sigue enviando eventos al Input System. Las pruebas siguen atravesando el controlador real, la selección del EventSystem y los manejadores de los botones. Las expectativas y los casos existentes se conservaron. Las opciones de entrada se modifican en memoria y se restauran dentro de Play Mode. Este archivo pertenece al ensamblado de pruebas.

## Incidencia de la primera regresión

Los cuatro casos de teclado pasaron en la primera corrida completa. El único fallo fue `InicioNormalV1CompletaTresRutas(1)`, en la aserción de movimiento del volante. La traza apunta a `FlujoJugadorPilotoV2Tests.EntradaNormal.cs`, línea 97 antes de ajustar esa comprobación. Se midieron 0° entre dos muestras separadas por 0,16 s y la aserción exigía más de 0,01°.

El giro de `AnimarVolante` sigue una senoide. Dos muestras simétricas alrededor de un extremo pueden devolver la misma orientación. Con la fórmula del juego se comprobó un ejemplo de 8,87329° al principio, 9° en el extremo y 8,87329° al final. La prueba ahora observa fotogramas durante un máximo de 0,35 s y conserva la exigencia de detectar un giro mayor de 0,01°. El cambio se limita al muestreo de la prueba.

## Resultados y regresión

| Ejecución | Aprobadas | Fallidas | Omitidas |
| --- | ---: | ---: | ---: |
| Casos individuales antes de editar | 4 | 0 | 0 |
| Casos individuales con el ajuste | 4 | 0 | 0 |
| Repetición conjunta con el ajuste | 4 | 0 | 0 |
| Primera regresión Play Mode | 63 | 1 | 0 |
| EditMode final | 81 | 0 | 0 |
| Play Mode final | 64 | 0 | 0 |

La última revisión de Console reportó cero errores y cero advertencias. [Resultados de las ejecuciones](../../TestEvidence/CorreccionTecladoFocoSofia/Verificacion.json)

Los casos a 1920×1080 y 1280×720 comprueban Inicio, personalización de Registro, respuestas en Partida, confinamiento y regreso de foco del diálogo, reanudación, desplazamiento de textos largos y controles de Resultado. La suite completa incluye además navegación por clic, panel administrativo, V1 como entrada predeterminada, las ocho rutas V2 desde JUGAR, movimiento automático, transiciones y llegada a DELIVERY.

La validación automatizada se hizo en el Editor con un teclado virtual. Queda a cargo de Sofía una comprobación manual con teclado físico y ratón en ambas resoluciones, incluyendo la recuperación de foco después de cambiar de aplicación, el indicador visual del control seleccionado y el regreso desde los diálogos.

## Archivos

- Modificado `Assets/Tests/PlayMode/TeclasPilotoPruebas.cs`.
- Modificado `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.EntradaNormal.cs` en la comprobación del giro.
- Creado `Assets/Mapas/CorreccionTecladoFocoSofia.md`, con su metadato de Unity.
- Guardadas la traza y las ejecuciones en `TestEvidence/CorreccionTecladoFocoSofia`.
