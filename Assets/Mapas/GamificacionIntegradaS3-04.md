# Gamificación integrada S3-04 en el piloto V2

7 de noviembre de 2026. Unity 6000.6.7f1, rama `feat/unity-map`. Prompt 35.

## Elementos integrados y alcance

La partida que abre el flujo piloto existente incorpora puntos reales, progreso visual del recorrido, contador de situaciones resueltas y reconocimiento descriptivo de buenas prácticas guardadas. Se reutiliza PruebaIntegracionV2 y su navegación real. No se creó otra demostración ni otra base de datos.

Las tres insignias de la demostración conservan su catálogo, nombres e iconos. **Ninguna dispone de una regla real aprobada y ninguna se concede en V2.** El panel muestra expresamente que siguen pendientes. El reconocimiento factual de una buena práctica no equivale a desbloquear Conductor preventivo, Ruta responsable o Respuesta ante emergencias.

Para usarlo: **Life Drive > Piloto V2 > Abrir flujo del jugador**, registrar una identidad ficticia y comenzar o retomar la partida. El botón de reconocimientos está en el pie de la interfaz. Su panel ocupa el espacio de la bitácora, conserva el mapa y el camión visibles y suspende la presentación mientras está abierto. Al cerrarlo recupera el estado anterior, incluida una pausa manual previa. Abrir o cerrar el panel no evalúa, guarda ni recompensa nada.

## 1. Puntos de seguridad

El acumulado procede de `Partida.puntos`. La última variación procede de `Decision.puntos_aplicados`, que el evaluador y ServicioPartidas ya guardaron. La UI distingue ganancia, pérdida y cambio neutro por texto, signo y colores naturales. La consecuencia educativa sigue siendo `Decision.consecuencia`, junto con el retraso real.

La presentación no suma deltas al acumulado. No llama otra vez a Decidir para animar, pausar, abrir el panel, consultar datos o restaurar la vista. La última variación permanece identificada como un valor guardado, por lo que se puede volver a mostrar al reanudar sin conceder puntos de nuevo.

`GamificacionPartidaV2.Consultar` utiliza `ServicioDesempeno.Resumen`, con su autorización existente, para proyectar la partida seleccionada. No cambia las estrategias de evaluación ni las transacciones de Javier.

## 2. Progreso visual, aprendizaje y entrega

Se conservan las ocho rutas, sus cinco mapas y todas las trayectorias. La barra representa etapas visuales, no kilómetros, tiempo restante, seguridad del jugador ni un KPI.

El cálculo visual usa:

- `k`: cantidad de mapas ya cruzados, obtenida del historial reconstruido de V2.
- `p`: fracción actual de la trayectoria del mapa.
- `h`: hito de salida ya utilizado por esta escena, 0.56 en los mapas anteriores a DELIVERY y 0.82 para el punto de llegada en DELIVERY.
- `u`: `Clamp01(p / h)`, o 1 durante el fundido de salida.
- Progreso: `Clamp01((k + u) / 5)`. El cierre visual confirmado devuelve 1.

Cada mapa ocupa un quinto de la barra. Al salir de un mapa, su tramo visual llega al límite de esa etapa. Al reconocer el destino, la etapa siguiente empieza en cero local, con el mismo valor global. Así desaparece el retroceso que producía el cambio entre coordenadas locales de dos mapas. Los hitos 0.24 de las preguntas iniciales del mismo mapa se mantienen intactos.

El pie distingue el número real de situaciones resueltas, la llegada a DELIVERY y la entrega presentada. El número de decisiones depende de la ruta y no se impone un denominador ficticio. Alcanzar el 100% del recorrido visual no responde la última pregunta ni adelanta el cierre de la partida. Entrega presentada se muestra únicamente cuando `CierrePresentado` lo confirma.

La reanudación usa el historial y los hitos persistidos que ya reconstruye ReanudacionRecorridosV2. Se verificó su equivalencia antes y después de reabrir. No se agregó una posición continua del camión al esquema. Cerrar a mitad de un movimiento puede repetir la fracción visual posterior al último hito guardado. No pierde decisiones, puntos, reconocimientos registrados ni progreso confirmado. No se promete restaurar el fotograma exacto de una animación.

## 3. Buenas prácticas e insignias

El motor ya marca `Decision.es_buena_practica` y emite `BUENA_PRACTICA` en la misma transacción. El piloto muestra una buena práctica registrada únicamente cuando encuentra ambos datos para la misma partida, jugador y situación. Conserva el ID de decisión y el ID del evento como evidencia. Una decisión se cuenta una sola vez, aunque se vuelva a consultar o presentar. No se crea una tabla de recompensas, otro evento ni un punto adicional.

Este es un reconocimiento descriptivo de un hecho del motor. No se define una nueva condición educativa, un umbral, un rango o una certificación. Los datos del paquete V2 siguen siendo candidatos pendientes de validación educativa.

| Insignia existente | Condición DEMO anterior | Posible evidencia real para discutir | Estado en V2 |
| --- | --- | --- | --- |
| Conductor preventivo | Señal `demo.confirmacion`, primera respuesta de S0 | Buenas prácticas de inspección guardadas y sus eventos. Falta acordar qué decisiones y cuántas evidencias son elegibles. | Icono conservado. Desbloqueo pendiente, no otorgada. |
| Ruta responsable | Señal `demo.ruta.coordinada`, primera respuesta de R2 | Conductas seguras en decisiones de ruta, según sus decisiones y eventos. Falta aprobar alcance y requisitos acumulativos. | Icono conservado. Desbloqueo pendiente, no otorgada. |
| Respuesta ante emergencias | Señal `demo.aviso.emergencia`, primera respuesta de R3 | Respuestas a emergencias evaluadas por el motor y sus eventos. Falta aprobar la condición educativa y su relación con la insignia. | Icono conservado. Desbloqueo pendiente, no otorgada. |

Estas propuestas no son reglas implementadas. Entrar en un mapa no concede nada. El piloto no interpreta las señales `demo.*` como eventos reales y no ejecuta EstadoGamificacionDemo para evaluar una partida.

## Resultado y administración

Resultado conserva el total real de seguridad y añade el número de buenas prácticas respaldadas por decisiones y eventos. Los tres iconos aparecen en estado pendiente, con texto explícito. No se asignan categorías finales ni se usan las recompensas ficticias de los ejemplos.

Las pruebas administrativas contrastan el resumen real seleccionado con la proyección usada por el jugador: puntos y buenas prácticas coinciden. Las decisiones y eventos siguen consultándose con los servicios existentes. No se agregó un cálculo administrativo alternativo ni se modificaron los generadores PDF.

TDS, IEE, PPS y PCRS permanecen fuera de esta presentación. No se modifican sus fórmulas ni servicios. Se conservan tipo de situación, acierto, buena práctica, puntos, consecuencias, eventos, tramos, fechas y versión necesarios para cálculos posteriores.

## Archivos creados y modificados

| Archivo | Trabajo |
| --- | --- |
| `Assets/Scripts/Mapas/GamificacionPartidaV2.cs` | Nueva proyección de lectura autorizada de puntos, decisiones y evidencias de buenas prácticas. Fórmula de presentación del progreso e hitos visuales compartidos. |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Gamificacion.cs` | Nuevo montaje del panel, contador, última variación y estados pendientes. Reutiliza los iconos existentes. |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.cs` | Suspende el avance cuando el panel está abierto y reutiliza los hitos sin cambiar sus valores. |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Vista.cs` | Indicadores, barra normalizada, consecuencia con evidencia de buena práctica y liberación de iconos temporales. |
| `Assets/Scenes/PruebaIntegracionV2.unity` | Referencia al catálogo de las tres insignias existente. Sin reconstrucción de la escena. |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs` | Resultado muestra el conteo real e iconos pendientes, sin otorgarlos. |
| `Assets/Tests/EditMode/GamificacionPartidaV2Tests.cs` | Once pruebas nuevas de las ocho rutas, progreso, reapertura, signos y compatibilidad V1. |
| `Assets/Tests/PlayMode/PruebaIntegracionV2Tests.cs` | Verificación por fotogramas del progreso en las ocho rutas y dos casos nuevos de panel, reanudación y cambios de puntos. |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.cs` | Capturas y comprobación de Resultado con datos guardados. Una ruta prueba una respuesta inicial neutra sin cambiar sus bifurcaciones. |
| `Assets/Tests/PlayMode/AdministracionPilotoV2Tests.cs` | Coincidencia de puntos y buenas prácticas con la proyección del jugador. |
| `Assets/Mapas/GamificacionIntegradaS3-04.md` | Este informe. |
| `TestEvidence/GamificacionV2` | Capturas de variaciones y reconocimientos en 1920×1080 y 1280×720. |

Los archivos nuevos de Assets cuentan con .meta. Se conservaron los cambios locales anteriores y se realizó un respaldo antes de implementar. Las fuentes temporales y los iconos se liberan al destruir las vistas. No se duplicaron fondos, camión, prefabs, catálogos ni sistemas de guardado.

## Pruebas mediante Unity MCP

| Ejecución | Aprobadas | Fallidas | Omitidas | ID |
| --- | ---: | ---: | ---: | --- |
| Línea base EditMode | 59 | 0 | 0 | `06bbfcc6ae5f4eb78d9c303a9fd7a4b2` |
| Línea base Play Mode | 22 | 0 | 0 | `d15c4afb2acb4ca9b2e095b5a3835ca7` |
| Gamificación EditMode específica | 11 | 0 | 0 | `44ddd4d7f648493f90f17d9468dc50ee` |
| Play Mode completo | 24 | 0 | 0 | `b3fef1967eee4f44a5242586bca2da99` |
| EditMode completo final | 70 | 0 | 0 | `ea9dae5d0e2e4a4299001ae5cf9910e5` |

Las pruebas ejercitan respuestas reales mediante el servicio o los botones de las escenas, sin escribir puntajes prefijados en UI. Verifican ganancias, pérdidas y cero puntos, buenas prácticas con evidencia, decisiones sin reconocimiento aplicable, contador sin duplicados, avance y fundidos en ocho rutas, 100% de trayecto, entrega, reanudación, pausa del panel, lectura en Resultado, concordancia administrativa y entrada V1 predeterminada.

Se inspeccionaron las capturas de ambos tamaños, incluido el panel pendiente, la penalización y Resultado. Los reconocimientos no tapan el camión. La falta inicial de un using produjo un error de compilación CS0103, corregido antes de ejecutar las pruebas nuevas. No hubo una prueba aprobada durante ese error.

Resultado final: 70 EditMode y 24 Play Mode aprobadas, 94 en total, cero fallidas y cero omitidas en las suites finales. Console tras Play Mode devolvió cero errores y cero advertencias. Tras EditMode final, Console mostró cero errores y una advertencia de transporte de Unity MCP: `[WebSocket] Unexpected receive error: WebSocket is not initialised`. Se documentó sin borrar el mensaje.

Unity amplió automáticamente los atlas FuenteInterfaz y FuenteLectura durante las verificaciones. Se recuperaron sus bytes exactos desde el respaldo de esta sesión, conservando las modificaciones anteriores del usuario. La revisión de hashes protegió motor, Datos, indicadores, contenido activo, fondos, camión, trayectorias y el resto de escenas. El único cambio previsto en PruebaIntegracionV2.unity es la referencia al catálogo existente de insignias.

## Pendientes para cerrar S3-04

Los tres aspectos de presentación tienen una integración útil en el piloto: puntos reales, avance derivado de la partida y reconocimiento de buenas prácticas registradas. Eso no equivale a integrar tres insignias como recompensas definitivas.

Falta la aprobación del equipo sobre las condiciones educativas, elegibilidad, posible acumulación y persistencia/versionado de cada insignia. Cualquier futura concesión deberá tener una identidad estable y respetar la idempotencia del motor. También faltan validación educativa del paquete candidato, aceptación visual del equipo y pruebas del ejecutable definitivo. No se activa una condición propuesta para completar artificialmente la actividad.

V1 sigue predeterminada. No se cambian motor, indicadores, ruta.json activo, ocho rutas, trayectorias, fondos, camión, identidad visual ni categorías finales. No se hizo commit ni push.
