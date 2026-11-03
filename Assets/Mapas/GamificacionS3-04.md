# S3-04 · tres elementos de gamificación de prueba

Life Drive · Unity 6000.6.7f1 · feat/unity-map. Actividad de Sofía, limitada a la presentación propia de DemoInterfazPartida. Todos los puntos, señales y reconocimientos descritos son ficticios. No existe integración con el motor ni con su base de datos.

## Cómo probar

Abrir `Assets/Scenes/DemoInterfazPartida.unity`, pulsar Play y maximizar Game View. El camión se conduce automáticamente. Responder con clic y pulsar Continuar después de cada retroalimentación. Se detiene en tres puntos: S0 al 35%, el R2 de la rama elegida al 40% y el R3 al 55%. Los porcentajes son de la trayectoria del mapa, no del recorrido completo.

El botón **Insignias** abre el panel lateral y pausa la presentación sin tapar el mapa. Cerrar lo devuelve a su estado anterior: si ya había pausa manual, conserva esa pausa; si estaba en marcha, continúa. Situación y retroalimentación permanecen detenidas. Pausar/Reanudar funciona durante marcha y fundidos. Reiniciar restablece las tres situaciones, puntos, progreso, insignias y notificaciones de esta demostración; vuelve a S0 y arranca otra vez. No borra ninguna partida real.

## Elemento 1 · puntos de seguridad simulados

Se muestra el total actual y el último cambio con signo. La retroalimentación explica brevemente ese cambio. El total inicial es cero y se permite que sea negativo para comprobar las penalizaciones.

Cada resultado se acepta una sola vez por `idDemo` de situación. La pantalla ignora clics posteriores al primero y el estado temporal también rechaza el mismo ID, aunque se intente introducir por otra llamada. Una segunda respuesta no cambia puntos, contadores, insignias ni avisos. Movimiento, fundidos y animación de notificaciones no tienen una operación que conceda puntos.

| Situación ficticia | Primera respuesta | Segunda respuesta |
|---|---|---|
| demo.radio · S0 | Confirmar aviso: +10 | Seguir sin confirmación: −5 |
| demo.coordinacion · R2 | Confirmar corredor: +5 | Omitir coordinación: −5 |
| demo.apoyo · R3 | Confirmar aviso del ejercicio: +10 | Omitir aviso del ejercicio: −10 |

Los valores y explicaciones están almacenados en los ScriptableObjects propios, no calculados a partir de acierto, de tipo de situación del motor o de velocidad. Son ejemplos de presentación, no nuevas reglas educativas aprobadas. La primera respuesta de S0 conserva la continuación ficticia R1A→R2A→R3A y la segunda R1B→R2D→R3H. Las otras dos situaciones no cambian la ruta.

## Elemento 2 · progreso de la misión

La barra superior **TRAYECTO DE LA MISIÓN** usa la longitud del recorrido visual lateral de cinco fondos. Se actualiza al avanzar el camión; los fundidos no añaden distancia. El estado conserva el máximo alcanzado, evitando una pequeña regresión cuando la elección ficticia inicial cambia a una ruta más larga. Puede quedar estable brevemente hasta que la nueva longitud normalizada alcance ese máximo: no se presenta como tiempo restante.

Al completar la trayectoria de DELIVERY se establece 100%. Bajo el mapa aparece un contador independiente **SITUACIONES 0/3–3/3**, que aumenta únicamente al aceptar una respuesta. Es posible tener las tres situaciones resueltas mientras el camión todavía se aproxima a DELIVERY. Puntos e insignias no incrementan la barra de trayecto; recorrer carretera no responde situaciones.

El progreso no modifica estado, fechas ni puntuación de Partida. No cambia las coordenadas ni los fondos aprobados.

## Elemento 3 · insignias y reconocimientos

Las definiciones incluyen ID propio estable, nombre, descripción, icono, texto de condición y señal ficticia requerida. Los estados bloqueado/desbloqueado se mantienen solo en memoria. Los iconos son texturas Unity de 16×16 generadas en memoria, con alfa y FilterMode.Point: escudo, camino y símbolo de apoyo. Bloqueadas se muestran grises; desbloqueadas, doradas. No se generó ni editó ningún PNG.

| Nombre | Descripción | Condición determinista de prueba | Señal ficticia |
|---|---|---|---|
| Conductor preventivo | Confirmación del aviso de prueba | Primera respuesta en S0 | demo.confirmacion |
| Ruta responsable | Coordinación del corredor ficticio | Primera respuesta en R2 | demo.ruta.coordinada |
| Respuesta ante emergencias | Recepción del aviso simulado | Primera respuesta en R3 | demo.aviso.emergencia |

Al aceptar una respuesta que aporta la señal correspondiente, se desbloquea la insignia una sola vez. Una señal repetida en otro resultado tampoco vuelve a conceder la misma insignia ni genera otra notificación de ese reconocimiento. El pie muestra **INSIGNIA DE PRUEBA DESBLOQUEADA** y su nombre durante cuatro segundos; si hay varias notificaciones, se encolan. El temporizador usa tiempo no escalado para que el aviso pueda desaparecer también durante pausa de la demo. El panel conserva el detalle después de que desaparezca el aviso. Reiniciar limpia la cola y todos los estados de desbloqueo.

No se comprueban umbrales reales de puntos ni se emiten TipoEvento.BuenaPractica, PuntosActualizados u otros registros de la base de datos. El icono de apoyo no representa una certificación ni una competencia real del jugador.

## Arquitectura

| Parte | Clases y responsabilidades |
|---|---|
| Definiciones ficticias | GamificacionDemo / ReconocimientoDemo: catálogo de tres insignias; SituacionInterfazDemo / RespuestaInterfazDemo: textos, deltas y señales prefijadas. |
| Entrada de presentación | ResultadoPresentacionDemo: ID de situación, cambio ya determinado, explicación y señales. No evalúa una alternativa. |
| Estado temporal | EstadoGamificacionDemo: total, último cambio, respuestas aceptadas, desbloqueos y máximo de progreso; sin persistencia ni referencias a Motor/Datos/UI. |
| Presentación | DemoInterfazPartida: paneles, clics, espera por hitos, notificaciones y coordinación con el reproductor existente. IconosReconocimientoDemo: caché y liberación de texturas nativas en memoria. |
| Movimiento reutilizado | DemoRecorrido, RecorridoDemo y RecorridosVisualesDemo siguen llevando geometría, velocidad, orientación, carril y transiciones; no calculan puntos ni reconocimientos. |
| Adaptador futuro | Solo un punto de entrada propuesto mediante resultados de presentación. No se creó un adaptador conectado a servicios reales. |

Las tres situaciones tienen IDs `demo.*` y prefijos visuales, sin números de ruta.json. El anfitrión subdivide el avance temporal en pasos pequeños y recorta al hito para no saltarse una situación al terminar un fundido. Solo el primer Continuar elige una continuación ficticia y conserva el progreso de S0. Los demás reanudan el mismo mapa.

La revisión de solo lectura de Evaluacion.cs, ServicioPartidas.cs, Decision, Partida y RegistroEventos confirma que el motor ya determina puntos y guarda consecuencias transaccionalmente. Esta implementación no replica esas estrategias. Un futuro adaptador debería recibir un resultado confirmado, con una clave idempotente de su contexto autorizado, y presentar sus valores. Las señales y condiciones actuales son únicamente el arnés de prueba: no deben asociarse a eventos reales por similitud de nombres.

## Archivos creados y modificados

Modificados en esta actividad:

- Assets/Scripts/Mapas/Demo/DemoInterfazPartida.cs
- Assets/Scripts/Mapas/Demo/SituacionInterfazDemo.cs
- Assets/Scenes/DemoInterfazPartida.unity
- Assets/Mapas/DemoInterfaz/SituacionRadioDemo.asset
- Assets/Mapas/DemoInterfazPartidaGuide.md, con aviso de la ampliación vigente

Creados, cada uno con su `.meta`:

- Assets/Scripts/Mapas/Demo/GamificacionDemo.cs
- Assets/Scripts/Mapas/Demo/IconosReconocimientoDemo.cs
- Assets/Mapas/DemoInterfaz/GamificacionS304Demo.asset
- Assets/Mapas/DemoInterfaz/SituacionCoordinacionDemo.asset
- Assets/Mapas/DemoInterfaz/SituacionApoyoDemo.asset
- Assets/Mapas/GamificacionS3-04.md

Los metadatos de archivos existentes conservan sus referencias. No se cambia DemoRecorrido.unity ni el código de sus tres clases de movimiento en esta actividad. No se cambian EditorBuildSettings, el catálogo de mapas o sus dieciséis definiciones.

## Evidencias y resultados de validación

Se ejecutó DemoInterfazPartida en Play Mode. Se inspeccionaron capturas nativas de Game View con el panel bloqueado, la retroalimentación positiva, la penalización negativa, el aviso del pie y las tres insignias desbloqueadas con sus iconos. Se pulsaron por mouse Insignias, Cerrar y respuestas positivas de los dos primeros hitos. Para inspeccionar el aviso de cuatro segundos sin que caducara durante la revisión se pausó temporalmente el Editor; después se restauró la ejecución. Esta pausa diagnóstica no sustituye las pruebas de los controles de pausa de la demo.

También se probaron tres sesiones sobre los componentes vivos en Play Mode con el mismo método Avanzar usado por Update, a pasos de 1/60 s. Es una prueba funcional acelerada, no una inspección visual de cada fotograma.

| Caso | Respuestas en S0/R2/R3 | Total simulado | Insignias | Situaciones | Trayecto final |
|---|---|---:|---:|---:|---:|
| Todas positivas | primera / primera / primera | +25 | 3/3 | 3/3 | 100% |
| Todas negativas | segunda / segunda / segunda | −20 | 0/3 | 3/3 | 100% |
| Mixto, rama B | segunda / primera / primera | +10 | 2/3 | 3/3 | 100% |

Las sesiones recorrieron respectivamente S0/R1A/R2A/R3A/DELIVERY y S0/R1B/R2D/R3H/DELIVERY. Pasaron 3129, 3166 y 3166 pasos de presentación. No se detectaron retrocesos del progreso, respuestas contadas dos veces ni recompensas duplicadas. Se rechazaron reenvíos con el mismo ID incluso cambiando el delta a 999, además del doble clic de interfaz.

Comprobaciones adicionales realizadas:

- Reiniciar: puntos cero, trayecto cero, situaciones cero, insignias bloqueadas y avisos vacíos.
- Pausa/reanudación en marcha y durante un fundido: distancia, progreso y opacidad inmóviles mientras hay pausa y continúan al reanudar.
- Abrir/cerrar panel de insignias: pausa sin perder posición y reanudación de la marcha anterior.
- Señal repetida en dos resultados distintos: una sola insignia y una sola concesión de ese reconocimiento.
- Actualizar solo progreso/animación: no cambia puntos ni insignias. Actualizar progreso 0.9→0.2 conserva 0.9.
- DemoRecorrido anterior: los ocho recorridos llegaron a DELIVERY y se detuvieron; ControlExterno permanece false en esa escena.

Unity compiló las clases y la revisión final de Console devolvió cero errores y cero advertencias. Los 119 archivos protegidos comprobados por SHA-256 (Motor, Datos, UI, StreamingAssets, fondos, miniaturas y camión, con sus metadatos) permanecieron idénticos. No existían archivos lifedrive-db.json* en el persistentDataPath del proyecto antes de la prueba y tampoco aparecieron después. No se invocaron servicios de partidas ni registro de eventos. Se cerró Play Mode, sin pausa del Editor, y quedó abierta DemoInterfazPartida en edición.

## Reutilización futura y coordinación con Javier

Reutilizables: aspecto de los tres indicadores, panel e iconos, prevención de reenvíos, estado de presentación, notificaciones y separación entre distancia y situaciones. El adaptador deberá respetar ResultadoDecision.Repetida y la diferencia entre destino explícito y estado posterior de Partida descrita en AnalisisIntegracion.md; no volverá a ejecutar Decidir al terminar una animación.

Necesitan acuerdo: valores de puntos reales, límites del total, qué efectos conceden cada reconocimiento, si existen requisitos acumulativos, identidad/versionado del resultado y persistencia de reconocimientos reales. También siguen pendientes los hooks de PantallaPartida, marcadores de situaciones y correspondencias entre el grafo lógico y el visual. No se implementa ninguna de esas decisiones aquí.

## Limitaciones

- Las reglas son explícitas pero solo demostrativas; las insignias se conceden por señales prefijadas de respuestas ficticias. No certifican conducción preventiva, ruta responsable o respuesta real ante emergencias.
- Hay tres situaciones de prueba y dos secuencias visuales desde la interfaz. La demo independiente conserva sus ocho rutas.
- Progreso de trayecto monotónico por máximo histórico: al seleccionar la rama más larga puede permanecer estable brevemente. No es un reloj, una medida física ni el estado de una partida real.
- UI IMGUI proporcional, fuente estándar y panel diseñado para tres insignias; un catálogo mayor requeriría paginación/desplazamiento y revisión de accesibilidad. Maximizar Game View.
- No se guarda nada al salir de Play Mode. No hay sincronización con cuentas, servidor, reportes, eventos reales o finales. No se implementaron las tres categorías de final.
- Persisten los límites de escala del camión, curvas y cinco discrepancias artísticas entre mapas; el avance y las insignias no validan cabida física ni corrigen PNG o trayectorias.

No se hace commit ni push. Se conservan los cambios locales previos.
