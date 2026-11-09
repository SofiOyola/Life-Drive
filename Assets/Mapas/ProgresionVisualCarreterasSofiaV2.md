# Progresión visual de carreteras de Life Drive

Unity 6000.6.7f1 · Rama `feat/unity-map` · Prompt 42

## Diagnóstico

La entrada normal de Inicio y Registro pasa por `EntradaJuego`. Antes del cambio, esta entrada activaba siempre el servicio V1. Su presentación, en `PruebaIntegracionV2.Clasica.cs`, fija S0 como mapa de referencia y coloca allí todos los hitos educativos. Al finalizar abre DELIVERY. El motor sí guarda las alternativas y cambia de tramo lógico. Lo que faltaba era una relación entre esos tramos y los fondos. La causa se encontró en la presentación V1, sin indicios de un fundido V2 pendiente ni de una decisión guardada en el mapa equivocado.

V1 tiene cuatro tramos lógicos y tres itinerarios posibles, 1→2→4, 1→2→3→4 y 1→3→4. Sus ramificaciones convergen. La configuración existente de los 16 mapas define otro grafo. Las asociaciones declaradas corresponden al candidato V2 y usan sus tramos 101 a 116. Por eso V1 conserva la vista de referencia y puede mostrar de cinco a siete situaciones sobre S0. El límite de dos se valida para V2.

## Probar V2 desde la entrada normal

1. Con Play Mode detenido, abre `Assets/Scenes/Inicio.unity`.
2. En el menú del Editor, marca **Life Drive → Piloto V2 → Usar V2 desde JUGAR**.
3. Pulsa **Play** en Unity.
4. En el juego pulsa **JUGAR**, llena Registro con datos de prueba y pulsa **COMENZAR RECORRIDO**.
5. Responde, lee la consecuencia y pulsa **Continuar recorrido** para seguir hacia la carretera correspondiente.
6. Para volver a V1, detén Play Mode y desmarca la misma opción.

La opción se conserva durante la sesión del Editor. Al abrir una sesión nueva, su valor predeterminado es V1. El selector y su carga de catálogo están excluidos de las compilaciones del juego. El menú del jugador conserva sus dos acciones habituales. El atajo previo **Abrir flujo del jugador** abre Inicio con la misma selección V2 y el mismo arranque.

El piloto usa su carpeta de pruebas separada, con la instantánea del contenido candidato y sus partidas. El mismo flujo la conserva al regresar a Inicio y retomar. Las pruebas automatizadas usan carpetas independientes con identificadores únicos.

## Destinos declarados por el candidato

Los números son identificadores de situación y alternativa del paquete. Se consultan aquí para revisar el contrato y no se agregaron a la interfaz de juego.

| Origen | Situación | Alternativa y destino | Alternativa y destino |
| --- | ---: | --- | --- |
| S0 | 101 | 101 → R1A | 102 → R1B |
| R1A | 102 | 103 → R2A | 104 → R2B |
| R1B | 103 | 105 → R2C | 106 → R2D |
| R2A | 104 | 107 → R3A | 108 → R3B |
| R2B | 105 | 109 → R3C | 110 → R3D |
| R2C | 5 | 12 → R3E | 13 → R3F |
| R2D | 106 | 111 → R3G | 112 → R3H |

Las preguntas de conocimiento 1 y 7 conservan su mapa. La situación de riesgo 3 también permanece en R2A. Cada R3 tiene una salida única a DELIVERY. La emergencia 6 pertenece al tramo lógico de entrega y la presentación existente la muestra sobre la carretera de aproximación R3. Sus alternativas cambian el resultado educativo. Ambas permiten continuar por la misma salida a DELIVERY.

## Presentación y reanudación

`ServicioPartidas.Decidir` guarda situación, alternativa, destino lógico, consecuencia y puntos. `ReanudacionRecorridosV2` coteja esa decisión con las asociaciones y el grafo de `RecorridoVisualValidado`. La consecuencia se muestra sobre la carretera actual. Al continuar, `DemoRecorrido` lleva el camión hasta la salida, aplica el fundido y coloca el camión al comienzo de la trayectoria del nuevo fondo. Una vez terminada la entrada, se confirma el hito de presentación y se muestra la siguiente situación al llegar a su punto de parada.

La partida guarda por separado la decisión y el último hito presentado. Si se interrumpe el fundido, al retomar se reconstruye el último mapa confirmado y aparece la consecuencia que ya estaba guardada. Al continuar se repite la presentación de la transición. La alternativa y los puntos se conservan y el motor no los evalúa otra vez.

## Cantidad de situaciones visibles

| Mapa | Situaciones visibles | Cantidad |
| --- | --- | ---: |
| S0 | 1 y 101 | 2 |
| R1A | 102 | 1 |
| R1B | 103 | 1 |
| R2A | 3 y 104 | 2 |
| R2B | 105 | 1 |
| R2C | 5 | 1 |
| R2D | 106 | 1 |
| Cada R3A a R3H | 6, aproximación | 1 |
| DELIVERY | 7, descargue | 1 |

La validación del paquete usa la distribución visible. Cuenta la aproximación en cada carretera que desemboca en DELIVERY y la descuenta de DELIVERY. Si un mapa supera dos, rechaza el paquete antes de abrir una partida V2. El candidato actual cumple la regla con todas sus situaciones y puntuaciones originales.

| Itinerario después de S0 y antes de DELIVERY | Cantidades en S0 / R1 / R2 / R3 / DELIVERY | Total |
| --- | --- | ---: |
| R1A → R2A → R3A | 2 / 1 / 2 / 1 / 1 | 7 |
| R1A → R2A → R3B | 2 / 1 / 2 / 1 / 1 | 7 |
| R1A → R2B → R3C | 2 / 1 / 1 / 1 / 1 | 6 |
| R1A → R2B → R3D | 2 / 1 / 1 / 1 / 1 | 6 |
| R1B → R2C → R3E | 2 / 1 / 1 / 1 / 1 | 6 |
| R1B → R2C → R3F | 2 / 1 / 1 / 1 / 1 | 6 |
| R1B → R2D → R3G | 2 / 1 / 1 / 1 / 1 | 6 |
| R1B → R2D → R3H | 2 / 1 / 1 / 1 / 1 | 6 |

## Contrato pendiente con Javier

- Identificar la revisión de V1 a la que corresponde la asociación visual y acordar cómo se reconocen las partidas V1 ya guardadas.
- Definir para cada tramo y situación obligatoria un mapa de presentación, su orden y el punto de parada. Si un tramo se presenta en varios fondos, el avance entre ellos requiere una regla explícita con un máximo de dos situaciones por fondo.
- Definir por cada arista lógica el origen y el destino visual, incluidos los avances automáticos del motor y las convergencias en los tramos 3 y 4. Las decisiones de ruta se deben resolver con la combinación de tramo de origen, situación, alternativa y destino guardado.
- Acordar los puntos de entrada y salida, la llegada a DELIVERY y el hito persistido para reanudar transiciones sin reevaluar respuestas.

El otro pendiente del equipo es la aprobación educativa del contenido V2. Su uso en desarrollo conserva el carácter de candidato.

## Archivos de esta revisión

| Archivo | Cambio |
| --- | --- |
| `Assets/Scripts/IntegracionV2/EntradaJuego.cs` | Selección explícita del piloto en el Editor y conexión a la escena ya abierta. |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs` | Creación del mismo piloto con opción de enlazar Inicio sin recargarlo. |
| `Assets/Editor/InterfazVisual/FlujoJugadorPilotoV2Editor.cs` | Selector marcado en el menú del Editor y atajo conectado al mismo flujo. |
| `Assets/Scripts/Mapas/PaqueteRecorridos.cs` | Validación de la distribución visible con máximo de dos. |
| `Assets/Scripts/Mapas/ReanudacionRecorridosV2.cs` | Comparte el identificador de aproximación con la validación del paquete. |
| `Assets/Tests/EditMode/PaqueteRecorridosTests.cs` | Casos de exceso directo y exceso al contar la aproximación. |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.EntradaNormal.cs` | Ocho itinerarios por la entrada normal, persistencia, fundido y reanudación. |
| `Assets/Tests/PlayMode/AdministracionPilotoV2Tests.EntradaNormal.cs` | Aísla la prueba V1 de la selección de desarrollo y la restaura al salir. |

La modificación se concentra en la integración y la validación visual. Los fondos, el camión, las preguntas y los servicios de evaluación se reutilizan.

## Verificación en Unity MCP

Las ocho pruebas dirigidas empiezan cargando Inicio en Play Mode con el selector de desarrollo activado y pulsan JUGAR y COMENZAR RECORRIDO. Comparan mapa, situación, alternativa y destino, consultan los datos guardados, intentan un segundo clic y comprueban el total de puntos. Cada cambio comprueba el mapa nuevo bajo el fundido y la posición inicial del camión. R3A y R3H interrumpen el primer fundido y reabren la partida desde Inicio con un nuevo servicio, usando los datos del disco. Las ocho rutas completan DELIVERY y abren Resultado con el retrato correspondiente.

Se usaron las resoluciones 1920×1080 y 1280×720, alternadas entre las ocho rutas. Las capturas representativas se guardaron en `TestEvidence/ProgresionVisualV2`.

| Ejecución | Aprobadas | Fallidas | Omitidas |
| --- | ---: | ---: | ---: |
| EditMode de referencia | 79 | 0 | 0 |
| Play Mode de referencia | 58 | 6 | 0 |
| EditMode con el cambio | 81 | 0 | 0 |
| Play Mode dirigido desde JUGAR en V2 | 8 | 0 | 0 |
| Play Mode completo con el cambio | 60 | 4 | 0 |
| Repetición aislada de teclado y foco | 0 | 4 | 0 |

Los cuatro casos de teclado y foco del jugador que fallan en la regresión ya fallaban antes de la edición. Son `AccesibilidadJugadorTecladoRegistroEntregaYReanudacion` y `AccesibilidadTextoLargoDesplazableSinEvaluar`, cada uno en ambas resoluciones. En la repetición no se desplazó el foco al control esperado. Las dos pruebas de teclado administrativo que fallaron en la línea base pasaron en la suite final. Los recorridos V2, las tres rutas lógicas de V1 y la entrada administrativa normal pasaron.

La validación se realizó en el Editor. La opción de menú se invocó directamente y se verificó que alterna el ajuste de la entrada. Console reportó cero errores y advertencias tras la última ejecución. Los resultados y mensajes de prueba están en [Verificacion.json](../../TestEvidence/ProgresionVisualV2/Verificacion.json).
