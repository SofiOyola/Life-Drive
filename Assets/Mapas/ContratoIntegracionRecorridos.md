# Contrato de decisiones y ocho recorridos educativos

Life-Drive · Unity 6000.6.7f1 · rama `feat/unity-map` · 5 de noviembre de 2026.

El contrato técnico define la integración futura. La organización debe aprobar los escenarios, sus efectos y las correspondencias antes de habilitar una partida de ocho recorridos. La línea base verificable consta de una prueba focalizada aprobada y 27 pruebas EditMode aprobadas, sin fallidas ni omitidas. Esa evidencia cubre el cursor visual y la regresión del motor actual. Esta etapa es de diseño e inspección estática. [Evidencia del Test Runner](MotorOchoRecorridos.md#validación-en-unity--5-de-noviembre-de-2026).

## Inventario del contenido actual

Fuente: [ruta.json](../StreamingAssets/Contenido/ruta.json). Las siete situaciones tienen `validada: false` y `fecha_validacion` vacía en el archivo. Los IDs de situación, alternativa y tramo son espacios distintos.

| Situación | Tramo | Orden | Tipo | Tema | Enunciado |
| ---: | ---: | ---: | --- | --- | --- |
| 1 | 1 | 1 | PREGUNTA | MANTENIMIENTO | ¿Qué debe hacerse antes de iniciar una operación de transporte? |
| 2 | 1 | 2 | DECISION_RUTA | SEGURIDAD_VIAL | El reporte vial indica un derrumbe parcial en la vía principal, con paso a un solo carril. La vía alterna es más larga. |
| 3 | 2 | 1 | RIESGO | NORMAS_EMPRESA | Tienes una entrega programada y llevas retraso. |
| 4 | 2 | 2 | DECISION_RUTA | SEGURIDAD_VIAL | Cae neblina densa y la visibilidad baja a pocos metros. Más adelante hay un retorno hacia la vía alterna. |
| 5 | 3 | 1 | RIESGO | FATIGA_PAUSAS | Llevas cuatro horas conduciendo y sientes los ojos pesados. |
| 6 | 4 | 1 | EMERGENCIA | EMERGENCIAS | A pocos kilómetros del destino se presenta una falla mecánica. ¿Cuál es el procedimiento adecuado? |
| 7 | 4 | 2 | PREGUNTA | MANEJO_CARGA | Al llegar al destino, ¿qué se debe verificar antes de iniciar el descargue? |

En la columna «Destino JSON», cero conserva el tramo como destino de `Decision`. El estado posterior incluye lo que hace `Avanzar` después. Cada fila aplica los puntos, el retraso y la retroalimentación configurados por su estrategia. Las alternativas 1, 4–5, 6–8, 9–11 y 12–14 se analizan por contexto en la propuesta, con el inventario completo como línea base.

| Situación | Alternativa | Respuesta literal | Puntos | Retraso min | Destino JSON | Tramo y situación posteriores |
| ---: | ---: | --- | ---: | ---: | ---: | --- |
| 1 | 1 | Realizar la inspección preoperacional del vehículo | 20 | 0 | 0 | 1 / s2 |
| 1 | 2 | Salir de inmediato para evitar retrasos | 0 | 0 | 0 | 1 / s2 |
| 1 | 3 | Revisar solo el combustible | 0 | 0 | 0 | 1 / s2 |
| 2 | 4 | Tomar la vía principal para ahorrar tiempo | -20 | 0 | 2 | 2 / s3 (explícito) |
| 2 | 5 | Tomar la vía alterna, más larga pero más segura | 15 | 40 | 3 | 3 / s5 (explícito) |
| 3 | 6 | Aumentar la velocidad para recuperar tiempo | -20 | 0 | 0 | 2 / s4 |
| 3 | 7 | Mantener una conducción segura y reportar el retraso | 20 | 10 | 0 | 2 / s4 |
| 3 | 8 | Ignorar las condiciones de la vía | -20 | 0 | 0 | 2 / s4 |
| 4 | 9 | Continuar sin reducir la velocidad | -20 | 0 | 0 | 4 / s6 (implícito) |
| 4 | 10 | Reducir la velocidad, encender luces y continuar | 15 | 15 | 0 | 4 / s6 (implícito) |
| 4 | 11 | Tomar el retorno hacia la vía alterna | 15 | 25 | 3 | 3 / s5 (explícito) |
| 5 | 12 | Detenerse en un lugar seguro y hacer una pausa activa | 20 | 15 | 0 | 4 / s6 (implícito) |
| 5 | 13 | Bajar la ventana y seguir conduciendo | -20 | 0 | 0 | 4 / s6 (implícito) |
| 5 | 14 | Acelerar para llegar antes y descansar allá | -20 | 0 | 0 | 4 / s6 (implícito) |
| 6 | 15 | Detenerse en un lugar seguro, señalizar y reportar | 15 | 20 | 0 | 4 / s7 |
| 6 | 16 | Seguir conduciendo hasta el siguiente pueblo | -20 | 0 | 0 | 4 / s7 |
| 6 | 17 | Detenerse en plena vía sin señalizar | -20 | 0 | 0 | 4 / s7 |
| 7 | 18 | El estado de los amarres y la estabilidad de la carga | 10 | 0 | 0 | 4 / finaliza |
| 7 | 19 | Solo que el cliente firme la remisión | 0 | 0 | 0 | 4 / finaliza |
| 7 | 20 | Nada, el descargue lo hace el cliente | 0 | 0 | 0 | 4 / finaliza |

Los cambios de tramo causados por una alternativa son s2/a4 (1→2), s2/a5 (1→3) y s4/a11 (2→3). En s4, a9 y a10 representan una sola salida topológica, 2→4 por defecto. En s5, todas las respuestas comparten el avance por defecto 3→4. Las respuestas de s1, s3 y s6 cambian efectos y dejan la siguiente situación en el mismo tramo. Las de s7 cambian efectos y cierran la partida al agotarse el destino.

Las dos bifurcaciones reales son s2 y s4, con s4 accesible solo desde la vía principal. Producen **tres secuencias distintas de tramos**:

- 1→2→4 con s2/a4 y s4/a9 o a10.
- 1→2→3→4 con s2/a4 y s4/a11.
- 1→3→4 con s2/a5.

El grafo lógico converge en 3 y 4. El visual tiene siete nodos de decisión, tres niveles y ocho hojas que convergen solo en `DELIVERY`. Contar variaciones de puntos o respuestas como secuencias de tramos inflaría la cobertura.

## Correspondencias confirmadas y propuestas

**Confirmadas en el repositorio.** El tramo 1 inicia la partida y el 4 es el destino lógico. `S0` es la raíz visual y `DELIVERY` es el terminal visual. Los 16 registros de asociación del catálogo tienen los IDs numéricos en cero. Las relaciones confirmadas son internas a cada grafo. **Hay cero asociaciones activas entre una respuesta y un mapa.** El nombre de un fondo aporta evidencia temática para revisar una propuesta, y la organización debe aprobar la correspondencia concreta. [Catálogo](CatalogoMapas.asset) · [Análisis de integración](AnalisisIntegracion.md).

| Mapa actual | Hijos | Situación y alternativas candidatas o faltantes | Estado y fundamento |
| --- | --- | --- | --- |
| S0 | R1A / R1B | s1/a1 → R1A. s1/a2 → R1B. s1/a3 → R1B solo si se aprueba que una revisión parcial cuenta como salida apresurada. | Afinidad fuerte con «Salida preparada» y «Salida apresurada». s1 es una pregunta de conocimiento. Para que su elección represente la acción de salir, aprobar su encuadre en la versión nueva y el tratamiento explícito de a3. Si se conserva como pregunta independiente, crear una decisión operativa de alistamiento. |
| R1A | R2A / R2B | Candidata s2/a4 → R2A. s2/a5 → R2B queda pendiente. | «Vía principal» coincide con R2A. El texto de a5 habla de una vía alterna más larga y el fondo R2B se llama «Atajo rural». Aprobar una variante de s2 que identifique un desvío rural habilitado y explique el tiempo extra, o usar una nueva decisión contextual. Sin esa revisión, la fila completa sigue deshabilitada. |
| R1B | R2C / R2D | Nueva `SALIDA_SERVICIO`. Entrar al corredor que lleva a una zona de servicio para verificar el vehículo y coordinar con despacho → R2C. Seguir por el corredor rural industrial abierto sin hacer esa verificación → R2D. | R2C tiene zona de servicio y R2D es un corredor industrial. La elección trata de una revisión pendiente tras la salida, y ninguna opción debe ordenar circular con una falla conocida. La trayectoria existente se mantiene en la calzada. |
| R2A | R3A / R3B | Nueva `MONTAÑA`. Tomar el corredor de curvas amplias con visibilidad confirmada → R3A. Elegir el corredor montañoso habilitado de curvas más cerradas → R3B. | s4 permite continuar o retornar, pero no identifica el corredor de mayor exposición como su vía alterna. Puede inspirar el enunciado sobre visibilidad. Mantener s4 íntegra en la ruta clásica. |
| R2B | R3C / R3D | Nueva `RURAL`. Elegir la vía abierta de mejor visibilidad → R3C. Elegir el tramo rural de doble curva y visibilidad limitada → R3D. | El contraste se apoya en los fondos. El reporte ficticio debe confirmar que ambos están habilitados para el vehículo. El objetivo es comparar visibilidad y geometría; la alternativa exigente sigue sujeta a la demarcación y a una velocidad adecuada. |
| R2C | R3E / R3F | s5/a12 → R3E. s5/a13 y s5/a14 → R3F. | «Descenso después del descanso» y «Descenso con fatiga» coinciden con la decisión de parar o seguir. Es una ampliación: hoy todas avanzan al mismo destino por defecto. Mantener diferenciadas las consecuencias de a13 y a14 aunque compartan fondo. La simulación debe identificar ambas conductas como inseguras. |
| R2D | R3G / R3H | Nueva `INSPECCION_CARGA`. Detenerse en un punto seguro, comprobar amarres y condición del vehículo antes de continuar → R3G. Aplazar la revisión y continuar por el tramo de mayor exposición → R3H. | Los conceptos visuales distinguen inspección y exposición. Es una verificación en ruta. s7 ocurre al llegar a la entrega y conserva ese contexto. Si la revisión detecta una falla, el texto debe contemplar reportarla y resolverla antes de continuar. |

La propuesta base requiere **cuatro situaciones nuevas** para R1B, R2A, R2B y R2D, y **una adaptación contextual** de s2 para R1A o una quinta situación nueva si la adaptación se descarta. La reutilización de s1 y s5 como bifurcaciones exige aprobar su nuevo efecto de navegación. Si s1 conserva únicamente su función de conocimiento, se añade una decisión operativa en S0. Los códigos simbólicos de esta tabla son borradores. Los IDs numéricos se asignan al aprobar el paquete. s3 puede reutilizarse antes de la bifurcación de R2A como situación de consecuencias sobre presión de tiempo. s6 y s7 pueden reutilizarse para aproximación y descargue, respectivamente. Sus respuestas conservan efectos propios y carecen de selección de hoja.

Las cuatro situaciones nuevas son propuestas didácticas para transporte de carga en Colombia. Antes de definir puntajes, retrasos o publicar, revisar enunciados, respuesta segura y retroalimentación con la organización. Usar reportes ficticios de habilitación y restricciones coherentes con el contexto colombiano, y revisar las consignas de visibilidad y demarcación con la fuente oficial. [Ministerio de Transporte, Sistema de Información Vial de Invías](https://mintransporte.gov.co/publicaciones/11542/invias-relanza-el-sistema-de-informacion-vial/) · [Código Nacional de Tránsito, arts. 73 y 74](https://www.funcionpublica.gov.co/eva/gestornormativo/norma.php?769=&i=5557).

La alternativa técnica para una correspondencia pendiente es dejar la versión de ocho recorridos inactiva y mantener el modo clásico. Si la situación actual sigue siendo útil pero su elección no encaja con los fondos, conservarla como situación de efectos y añadir una decisión contextual explícita. Las ramas representan acciones, condiciones o corredores. Las métricas y el resumen se calculan a partir de las decisiones guardadas; una hoja jamás clasifica el resultado.

## Esquema y reglas del contrato

Cada publicación enlaza una versión inmutable de contenido con una revisión del grafo visual y un manifiesto de asociaciones. La versión clásica conserva su grafo de cuatro tramos. La versión nueva propone tramos lógicos de rama propios, con IDs aprobados en su paquete, para que el selector actual de situación por tramo siga funcionando. Los tramos son contexto lógico; el manifiesto vincula explícitamente cada contexto de bifurcación con su fondo.

| Registro | Campos mínimos | Regla |
| --- | --- | --- |
| Paquete | `contenido_version`, `contrato_version`, `catalogo_revision` | La partida fija una publicación inmutable. La carga comprueba que el manifiesto, contenido y catálogo correspondan. |
| Contexto de decisión | `mapa_actual_id`, `tramo_origen_id`, `situacion_id` | Una situación navegacional aprobada por nodo interno. Las situaciones de efectos se declaran con navegación visual `NINGUNA`. |
| Fila de asociación | `mapa_actual_id`, `situacion_id`, `alternativa_id`, `mapa_siguiente_id` | Clave única dentro del paquete. La alternativa pertenece a la situación. Cada hijo está permitido por el cursor y cuenta con una opción alcanzable. Varias alternativas pueden conducir al mismo hijo. |
| Entrada y entrega | mapa raíz `S0`; arista `R3x → DELIVERY` al terminar la aproximación y confirmar la llegada visual | El motor conserva su estado lógico. La pregunta de descargue se presenta en DELIVERY y Resultado se habilita tras finalizar, llegar y confirmar la última consecuencia. |

El validador de publicación comprueba IDs y pertenencias, cobertura de todas las alternativas, coherencia entre destino lógico y siguiente contexto, ausencia de duplicados, destinos inválidos o ciclos y los ocho caminos completos. Debe simular el orden de situaciones, destinos explícitos y avances por defecto, incluidos tramos sin preguntas. `ValidadorRuta` sigue siendo la comprobación base; su prueba de que existe una salida desde cada tramo no garantiza por sí sola esta cobertura. Una fila pendiente, una versión desconocida o una incoherencia detienen la activación. En ejecución, una asociación ausente indica un error de configuración y el coordinador no elige un fondo sustituto.

### Confirmación y transiciones implícitas

1. El coordinador bloquea un segundo envío mientras tramita la respuesta. La fachada autorizada copia valores del estado anterior: partida, versión, tramo, estado, situación actual, puntos, retraso y contexto visual. Una referencia a `Partida` no sirve como copia porque el motor la modifica.
2. `ServicioPartidas.Decidir` valida, llama una vez al evaluador y guarda `Decision`, efectos, eventos y avance lógico en la transacción existente. Para v2, el retorno o la fachada recibe un delta inmutable que contiene `Decision.id`, los IDs realmente guardados, origen, destino explícito efectivo, efectos, `Repetida`, secuencia ordenada de IDs alcanzados y estado/situación posterior. Registrar los cambios estructurados dentro de `CambiarTramo` durante esa llamada y publicar el delta tras confirmar. La traza incluye el cambio explícito, si existe, y cada salto de `Avanzar`; una secuencia vacía es válida. Los eventos de auditoría se conservan.
3. Ejemplo clásico: s4/a10 guarda una decisión 2→2 y devuelve `CambioRuta=false`, pero la traza debe contener 4 y el estado posterior señala s6. Una cadena hipotética 2→X→4 requiere registrar ambos pasos aun si las dos capturas solo muestran 2 y 4. Los nombres de tramo y los eventos textuales carecen de la estructura necesaria para recuperar esa traza.
4. El resolutor consume la respuesta **guardada** y la fila de su contexto. Deduplica por `(partida_id, decision_id)` y ordena la presentación. Si `Repetida` devuelve la decisión previa ante otro clic, ignora el ID de alternativa solicitado y reconcilia con el historial. El estado actual devuelto en un reintento puede estar adelantado respecto de esa decisión. Una transición visual fallida se reintenta desde el hecho guardado; un error de escritura conserva el cursor y recarga el estado autorizado tras el rollback.
5. El reproductor reutiliza la geometría y los fundidos, espera al jugador y aplica cada arista con `RecorridoVisualValidado`. En R3 se presenta s6 en un hito de aproximación; su tramo lógico puede ser ya el destino compartido. Después de mostrar la consecuencia y recibir Continuar, se cruza a `DELIVERY`. Al llegar se presenta s7. El hook final exige llegada confirmada, `FINALIZADA` y última consecuencia reconocida. Un `null` con partida `EN_CURSO` fuera de una espera prevista es diagnóstico.

## Guardado, reanudación y compatibilidad

Hoy `Sesion` carga un solo `ServicioJuego` y conserva `PartidaId` en memoria. `IniciarPartida` retoma la única partida `EN_CURSO` del jugador y registra el evento, sin llamar de nuevo a `Avanzar`. La base local `Application.persistentDataPath/lifedrive-db.json` guarda partidas, decisiones y eventos mediante `BaseDatos.Transaccion`, reemplazo desde temporal, copia `.bak` y rollback de las tablas en memoria ante fallo. Las decisiones conservan una copia de sus efectos; los reportes consultan los nombres de tramo en el contenido cargado. Los registros actuales carecen de versión de contenido, traza estructurada y estado visual. [Registros](../Scripts/Datos/Registros.cs) · [BaseDatos](../Scripts/Datos/BaseDatos.cs) · [ServicioDesempeno](../Scripts/Motor/ServicioDesempeno.cs).

- **Versión de contenido.** Conservar `ruta.json` como v1 inmutable y publicar la ampliación en un archivo v2 separado. Un registro sin `contenido_version` se interpreta como v1, tanto en curso como finalizado. Resolver por partida para reanudar, consultar tramos y renderizar históricos y PDF. Mantener personajes e IDs de v1 y no mover una partida en curso entre paquetes. Una nueva publicación se aplica a nuevas partidas cuando se habilite. Retener los paquetes usados por partidas guardadas y fallar con diagnóstico recuperable si falta alguno.
- **Versión de almacenamiento.** La primera escritura que incorpora registros v2 debe usar un esquema de BD explícitamente actualizado y seguir leyendo el esquema 1. El binario anterior rechaza un esquema superior según su control actual. Esto evita que un ejecutable antiguo abra una partida v2 con los tramos v1. Probar lectura de originales y copias de respaldo, además de la restauración transaccional. Conservar las reglas de una partida activa por jugador y una decisión por partida/situación.
- **Checkpoint mínimo.** En la misma fila de partida, guardar `contenido_version` y un checkpoint opcional con `ultima_decision_presentada_id`, `mapa_id` y `fase` en un hito discreto. Fases persistibles: `LISTO` en un punto de espera aprobado, `ENTREGA` al confirmar llegada y `CIERRE_RECONOCIDO` tras la última consecuencia. No guardar posición por fotograma. El checkpoint apunta a la última consecuencia y transición completadas. El servicio verifica propiedad, versión, pertenencia del ID, fase y avance monotónico. Guarda con la transacción de la misma base; si esa escritura falla, conserva el hito previo.
- **Reconstrucción.** Leer por una fachada autorizada las decisiones guardadas en orden de ID y aplicar el manifiesto de la versión fijada desde S0 hasta reconstruir la ruta confirmada. Usar el checkpoint para decidir dónde reanudar la presentación. Los saltos implícitos de diagnóstico se capturan en el delta en vivo; las aristas visuales reanudables dependen de decisiones guardadas y de la fase de entrega. Reanudar desde el último hito si se cerró durante un fundido. Si el motor guardó una decisión y la animación no terminó, volver a presentar el paso pendiente sin llamar a `Decidir`. Validar el checkpoint contra el prefijo reproducido; ante inconsistencia, recuperar desde un hito verificable o detener con diagnóstico. La animación puede repetirse tras un cierre; el efecto lógico se confirma una vez.
- **Cierre interrumpido.** Una partida v2 `FINALIZADA` sin `CIERRE_RECONOCIDO` debe poder retomarse como presentación pendiente, antes de crear otra partida cuando no exista una `EN_CURSO`. Consultarla por propietario mediante una entrada explícita en el flujo de inicio. Al marcar el cierre se habilita Resultado. Si la app se cierra entre esa escritura y la navegación, el resumen sigue disponible con la partida finalizada; la siguiente entrada no reproduce la entrega. Las partidas clásicas finalizadas carecen de ese flujo pendiente.

## Impacto mínimo previsto

| Área | Cambio futuro |
| --- | --- |
| Contenido | Conservar `ruta.json`. Añadir paquete v2 con nodos, situaciones, alternativas y destinos aprobados, más manifiesto visual versionado. Las alternativas nuevas usan tipos existentes. La transición estructural de s1, si se aprueba conservarla como `PREGUNTA`, usa el destino explícito del contenido v2 bajo una regla restringida a ese contexto; `EvaluacionPregunta` sigue dando puntos y explicación. Si se aprueba una decisión operativa separada, su tipo de ruta existente elimina esa excepción. |
| Servicios | Añadir resolución de contenido por partida en inicio, consultas y reportes. Mantener el punto de evaluación y la transacción de `Decidir`; exponer captura antes/después y traza, una lectura autorizada del historial y actualización del checkpoint. Validar el destino efectivo antes de confirmar para v2. Mantener auditoría, permisos, puntuación y retraso de v1. |
| Datos | Añadir versión y checkpoint en `Partida`, con lectura del esquema previo. `Decision` ya tiene las claves y efectos para reproducir el camino visual. Los eventos existentes conservan su función de auditoría. |
| Presentación | Integrar en la pantalla real los hitos de situación, consecuencia, Continuar y llegada. Conectar el reproductor incremental y el cursor. Verificar el punto de montaje de `PantallaPartida`: la rama declara la escena `Partida` en código, pero el inventario de escenas versionadas consultado no contiene `Partida.unity`. La demo de recorridos sigue disponible como prueba visual independiente. |

## Riesgos y orden de implementación posterior

| Riesgo observado | Control |
| --- | --- |
| Afinidad temática insuficiente, sobre todo s2/a5 y s4/a11 | Bloquear la publicación hasta aprobar la alternativa y verificar la geografía del fondo. |
| El motor adelanta varios tramos antes de devolver el control | Capturar la traza dentro de la operación y ordenar la presentación por decisión confirmada. |
| Doble clic, escritura fallida o cierre durante animación | Mantener la unicidad transaccional, deduplicar por decisión y reconstruir desde un hito. |
| Partida antigua abierta con reglas nuevas o nombres incorrectos en PDF | Fijar versión por partida y resolver también el contenido de los reportes por esa versión. |
| Confundir exposición del trayecto con resultado o usar un fondo para sugerir que una maniobra insegura está autorizada | Mantener evaluación y retroalimentación explícitas, con revisión pedagógica y visual de cada rama. |
| Falta de escena montada o hitos de la demo incompatibles con el flujo real | Verificar el punto de montaje y las referencias antes de aprobar el piloto; revisar visualmente los enlaces con arte y trayectorias existentes. |

1. Aprobar la tabla por nodo, tratamiento de respuestas adicionales, contenido y consecuencias. Cerrar s1/a3, s2/a5 y el momento de s6/s7. Asignar los IDs definitivos.
2. Preparar fixtures v1 a partir de guardados representativos y registrar la línea base de motor, datos y reportes. Verificar la escena de partida y el punto de integración.
3. Crear y validar en aislamiento el paquete v2 y el manifiesto. Enumerar las ocho rutas con un simulador de pruebas que use las reglas reales del servicio.
4. Introducir resolución por versión, lectura de guardados v1, guardado de checkpoint y delta confirmado. Ejecutar regresión de v1 antes de conectar el reproductor.
5. Integrar un piloto con situación, consecuencia, arista, reanudación y entrega. Probar fallos de guardado, doble clic y cierres. Extender a los siete nodos de bifurcación una vez validado el piloto.
6. Ejecutar los ocho escenarios en la experiencia real y verificar Console, reportes y continuidad visual. Habilitar v2 solo al cumplir los criterios siguientes.

## Pruebas y criterios de aceptación

- **Unitarias del contrato.** Validar una fila por contexto y respuesta, alternativas agrupadas deliberadamente, pertenencia de IDs, hijos faltantes, mapas inexistentes, ciclos, saltos de nivel, versiones desconocidas y ausencia de salida desde una rama. Reproducir un prefijo de decisiones con alternativas que comparten hijo y detectar un checkpoint incompatible.
- **Unitarias de avance y reintento.** Cubrir cambio explícito, s4/a10 con avance implícito, cadena de varios saltos, respuesta de efectos sin movimiento y finalización. Comprobar snapshots independientes de las referencias mutables. Reintentar con otra alternativa para la misma situación y exigir la decisión original sin repetir efectos, eventos ni transición.
- **Integración de las ocho rutas.** Desde una partida nueva y eligiendo respuestas reales, comprobar S0→R1A→R2A→R3A/B, S0→R1A→R2B→R3C/D, S0→R1B→R2C→R3E/F y S0→R1B→R2D→R3G/H, cada hoja con llegada a DELIVERY. En cada una comprobar orden de paneles, respuesta/efectos persistidos exactamente una vez, transición solo después del guardado, s6 antes de llegar, s7 al llegar y Resultado tras el cierre. Probar todas las alternativas que comparten hijo, además de una combinación canónica por hoja. Confirmar que el resumen cambia por los efectos guardados aunque dos respuestas lleven al mismo mapa.
- **Integración de recuperación.** Cerrar antes de responder, tras el commit y antes de animar, durante un fundido, antes de s7 y tras finalizar antes de reconocer la consecuencia. Recuperar el mismo camino y estado lógico, con reanudación desde un hito seguro. Inyectar error de escritura en decisión y checkpoint, y verificar rollback y reintento sin duplicación. Probar respaldo, paquete ausente, acceso de otro jugador y rechazo de datos v2 por un lector de esquema 1.
- **Regresión.** Mantener las 27 pruebas EditMode de la línea base y agregar los casos nuevos. Retomar guardados v1 en las tres secuencias, comparar puntos, retraso, estados, decisiones, eventos, permisos, registro y PDF antes y después del cambio. Verificar convivencia de partidas v1 y v2, nombres históricos correctos y apertura del reporte de una partida finalizada. Conservar la validación visual de las ocho rutas y revisar Console tras las ejecuciones.

Este documento es el único cambio de la etapa. Las asignaciones propuestas siguen sujetas a aprobación y las pruebas de integración quedan especificadas para la etapa de implementación.
