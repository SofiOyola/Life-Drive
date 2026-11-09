# Diagnóstico de la progresión visible desde JUGAR

Unity 6000.6.7f1 · Rama `feat/unity-map` · Prompt 44

## Causa comprobada

Al iniciar esta revisión, el Editor tenía abierta `Assets/Scenes/Inicio.unity` y **Usar V2 desde JUGAR** estaba desmarcado. Se reprodujo el arranque en ese estado. Un clic sobre el botón visible abrió Registro con el flujo V1 y sin el servicio de recorridos V2. Las pruebas aisladas de la entrada normal completaron las tres rutas lógicas V1 y comprobaron que la partida guardada tenía `contenido_version = 1`.

`EntradaJuego` decide qué flujo instalar al cargar Inicio. El botón JUGAR dirige a Registro en ambos modos. V1 usa el servicio vigente y `ruta.json`. Su presentación coloca las situaciones en el fondo de referencia S0 y cambia a DELIVERY al cerrar. Los cambios de tramo lógico de V1 se guardan, pero carecen de una correspondencia visual aprobada. Esto reproduce la única carretera observada. No hay una traza histórica de la partida de Sofía para comprobar qué sesión del Editor o ejecutable abrió en ese momento.

Con el selector marcado antes de entrar en Play Mode, la opción sobrevivió al inicio de Play y al cambio a Registro. El controlador que se conserva entre escenas abrió el servicio de recorridos V2 en el almacenamiento aislado del piloto. Se confirmó que la copia de `ruta-v2-candidata.json` de esa sesión coincidía con la del proyecto y que la partida nueva guardó `contenido_version = 2`.

| Escenario investigado | Resultado |
| --- | --- |
| A. V1 en el arranque normal | Reproducido con la opción desmarcada. Explica las preguntas sobre S0. |
| B. V2 solicitado con una partida V1 en curso | Si se encuentra en el mismo almacenamiento, Registro bloquea la entrada y conserva el histórico. Una partida V1 cerrada permite abrir una V2 independiente. |
| C. V2 activo con un único fondo | No se reprodujo. El fondo capturado coincidió con el destino de las decisiones guardadas. |
| D. La transición solo sucede en el estado interno | No se reprodujo. El fondo anterior se mantuvo durante la consecuencia y el nuevo se mostró tras pulsar Continuar recorrido. |
| E. JUGAR diverge del lanzador | Ambos abrieron Inicio y Registro con el mismo flujo V2 al activar el piloto. Se completó una ruta distinta desde cada entrada. |

Los informes de los Prompts 40, 42 y 43 documentan respectivamente la entrada del producto, los recorridos V2 mediante pruebas en Play Mode y la regresión general. Esta revisión añadió dos arranques reales del Editor, uno con el selector y otro con el lanzador, y comprobó los fondos que se renderizaron.

## Reproducción con la interfaz visible

Con Play Mode detenido y `Inicio.unity` abierta, se marcó la opción desde el menú real del Editor y se pulsó Play. Se utilizó Game View de 1280×720. Se registró un jugador ficticio nuevo en el almacenamiento aislado `sesion-local`. La automatización ubicó los controles mediante raycast del EventSystem y les envió eventos de puntero, incluyendo JUGAR, COMENZAR RECORRIDO, las alternativas y Continuar recorrido. La segunda reproducción usó **Abrir flujo del jugador** y otro jugador ficticio en el mismo entorno aislado.

| Entrada y partida ficticia | Versión solicitada / guardada | Mapas efectivamente presentados | Situaciones por mapa |
| --- | --- | --- | --- |
| JUGAR con el selector marcado, partida 2 | 2 / 2 | S0 → R1A → R2A → R3A → DELIVERY | 2 / 1 / 2 / 1 / 1 |
| Lanzador técnico y después JUGAR, partida 3 | 2 / 2 | S0 → R1B → R2D → R3H → DELIVERY | 2 / 1 / 1 / 1 / 1 |

| Entrada | Mapa anterior | Situación | Alternativa guardada | Destino guardado en el recorrido y fondo presentado |
| --- | --- | ---: | ---: | --- |
| Selector | S0 | 101 | 101 | R1A |
| Selector | R1A | 102 | 103 | R2A |
| Selector | R2A | 104 | 107 | R3A |
| Selector | R3A | 6 | 15 | DELIVERY |
| Lanzador | S0 | 101 | 102 | R1B |
| Lanzador | R1B | 103 | 106 | R2D |
| Lanzador | R2D | 106 | 112 | R3H |
| Lanzador | R3H | 6 | 15 | DELIVERY |

Las trazas separan la situación visible, la alternativa confirmada, el destino lógico, el mapa anterior, el destino visual, el fondo del reproductor y el estado de presentación. Las preguntas sin salida visual conservan el mapa. La emergencia de aproximación se presenta sobre R3 y la situación de descargue sobre DELIVERY.

- [Traza desde JUGAR con selector](../../TestEvidence/DiagnosticoProgresionVisibleSofia/JUGAR-antes/Traza.json)
- [Capturas S0](../../TestEvidence/DiagnosticoProgresionVisibleSofia/JUGAR-antes/S0.png), [R1A](../../TestEvidence/DiagnosticoProgresionVisibleSofia/JUGAR-antes/R1A.png), [R2A](../../TestEvidence/DiagnosticoProgresionVisibleSofia/JUGAR-antes/R2A.png), [R3A](../../TestEvidence/DiagnosticoProgresionVisibleSofia/JUGAR-antes/R3A.png), [DELIVERY](../../TestEvidence/DiagnosticoProgresionVisibleSofia/JUGAR-antes/DELIVERY.png) y [entrega completada](../../TestEvidence/DiagnosticoProgresionVisibleSofia/JUGAR-antes/Entrega.png)
- [Traza desde el lanzador](../../TestEvidence/DiagnosticoProgresionVisibleSofia/Lanzador/Traza.json)
- [Arranque real con el selector desmarcado](../../TestEvidence/DiagnosticoProgresionVisibleSofia/JUGAR-V1-arranque.json)

## Corrección de interfaz

Inicio identifica el flujo efectivo en su mensaje existente cuando se juega en el Editor. Muestra **Recorrido clásico activo** o **Piloto de carreteras activo**, según el controlador instalado. El texto habitual se conserva en las compilaciones. El mensaje de retorno desde una partida conserva sus indicaciones para retomarla.

Registro usa el mismo aviso al escribir el documento, escoger la ficha y confirmar si encuentra una operación en curso incompatible. Antes podía anunciar que la operación se retomaría y cambiar a un mensaje de conflicto al confirmar. La restricción existente sigue validándose antes de asociar la apariencia y cargar Partida.

| Archivo | Cambio |
| --- | --- |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs` | Etiqueta de modo en Inicio solo en el Editor y actualización del aviso de Registro. |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.Navegacion.cs` | Aviso coherente para una operación de otra versión y conservación del bloqueo. |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.EntradaNormal.cs` | Regresión de la etiqueta en la entrada real V1 y V2. |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.Navegacion.cs` | Regresión del aviso de conflicto antes y después de seleccionar la ficha. |

## Validación

| Suite completa por Unity MCP | Aprobadas | Fallidas | Omitidas |
| --- | ---: | ---: | ---: |
| EditMode | 81 | 0 | 0 |
| Play Mode | 64 | 0 | 0 |

Console al finalizar quedó con 0 errores y 0 advertencias. Durante la recarga del dominio se registró un aviso transitorio de conexión WebSocket de Unity MCP. [Resultados con los nombres de las pruebas](../../TestEvidence/DiagnosticoProgresionVisibleSofia/Verificacion.json).

Los ocho casos de `InicioNormalEnModoV2OchoRutas` cargan Inicio en Play Mode y pasan por Registro y Partida. Comprueban los mapas, los identificadores esperados, la alternativa persistida, el destino, el fondo anterior durante la consecuencia, el fundido con el camión en la entrada, las decisiones, los puntos, la llegada a DELIVERY y Resultado. Los extremos R3A y R3H interrumpen el primer fundido y reanudan desde Inicio con un controlador y un servicio nuevos. Comparan versión, mapa confirmado, apariencia y decisiones del disco. Los casos V1 cubren tres rutas en ambas resoluciones. EditMode valida los paquetes y contratos de servicios sin interfaz.

| Itinerario entre S0 y DELIVERY | Cantidades en S0 / R1 / R2 / R3 / DELIVERY | Total |
| --- | --- | ---: |
| R1A → R2A → R3A o R3B | 2 / 1 / 2 / 1 / 1 | 7 |
| R1A → R2B → R3C o R3D | 2 / 1 / 1 / 1 / 1 | 6 |
| R1B → R2C → R3E o R3F | 2 / 1 / 1 / 1 / 1 | 6 |
| R1B → R2D → R3G o R3H | 2 / 1 / 1 / 1 / 1 | 6 |

Las ejecuciones automáticas comprueban Game View a 1920×1080 y 1280×720 y actualizan las capturas y PDF de prueba de sus carpetas habituales. Las dos reproducciones del arranque real se hicieron a 1280×720. Esta revisión se ejecutó en Unity Editor.

## Cómo reproducirlo

1. Detén Play Mode y abre `Assets/Scenes/Inicio.unity`.
2. Marca **Life Drive → Piloto V2 → Usar V2 desde JUGAR**.
3. Pulsa Play y comprueba que Inicio indique **Piloto de carreteras activo**.
4. Pulsa **JUGAR**, registra un jugador ficticio nuevo y pulsa **COMENZAR RECORRIDO**.
5. Completa la situación inicial y responde la primera decisión de ruta. Después de leer su consecuencia, pulsa **Continuar recorrido**. La primera alternativa conduce a R1A y la segunda a R1B.
6. Al terminar la prueba, detén Play Mode y desmarca la opción para volver al recorrido clásico. Al iniciar una sesión nueva del Editor se parte del modo clásico. Las compilaciones siguen arrancando con V1.

## Dependencia con Javier

Para mostrar varias carreteras en V1 se requiere una correspondencia aprobada para la revisión vigente de `ruta.json`. Debe definir el fondo y el punto de parada de cada situación, el destino visual de cada transición lógica o avance automático, las convergencias, los puntos de entrada y salida y el hito persistido que permite retomar una transición sin reevaluar la respuesta. V2 sigue siendo un candidato de uso en desarrollo pendiente de aprobación educativa.
