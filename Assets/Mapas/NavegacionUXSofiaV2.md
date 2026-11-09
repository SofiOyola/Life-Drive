# Navegación y experiencia del jugador del piloto V2

8 de noviembre de 2026, hora de Colombia. Unity 6000.6.7f1. Rama `feat/unity-map`. PROMPT 38, responsabilidades de interfaz de Sofía en S3-07 y S4-03.

Resultado final ejecutado: **79/79 EditMode y 41/41 Play Mode**, cero fallos y cero omisiones en las suites finales. Los ocho recorridos completos continúan aprobados. Las siete pruebas nuevas cubren navegación, integridad, versiones y conservación de apariencia.

## Alcance y conservación del trabajo

Se leyeron `AuditoriaExperienciaPilotoV2.md`, `FlujoJugadorIntegradoV2.md` y `PersistenciaRecorridosV2.md`. Se revisaron las interfaces y adaptadores de Inicio, Registro, Partida y Resultado, y los contratos de servicios únicamente para comprender su comportamiento.

HEAD inicial: `bd9de07202369b76389527108a6012663e34a062`, `Integrar administracion gamificacion personalizacion y auditoria V2`. Los avances anteriores ya están confirmados en ese commit. Al comenzar había modificaciones en FuenteLectura, FuentePixel, EditorSettings y ProjectSettings, una escena temporal InitTestScene con su meta y evidencias locales. Se conservaron. El checkpoint de esta tarea está en `Library/Prompt38Checkpoint`, fuera del entregable.

Al terminar se recuperaron únicamente los bytes de los tres atlas de fuentes desde ese checkpoint, conservando también las modificaciones que ya tenían las dos fuentes BaseNaturaleza. No se restauró desde Git. ProjectSettings mantiene sus bytes iniciales. La comparación SHA-256 final de los archivos previos registra cambios solo en los cuatro scripts de UI indicados abajo. La escena temporal que ya existía no forma parte del entregable de esta tarea.

No se modificaron servicios de Javier, fórmulas de Jorge, autenticación, PDF, administración, mapas, camión, trayectorias, IDs de personaje, `ruta.json` o paquetes educativos. Las escenas originales se reutilizan mediante sus adaptadores en ejecución, sin duplicarlas ni guardarlas con cambios. No se hizo commit, push ni staging.

V1 permanece como entrada predeterminada del motor. Los controles nuevos se activan solo con el piloto explícito. V2 sigue siendo contenido candidato pendiente de aprobación educativa.

## Acciones visibles y comportamiento

| Pantalla | Botón o mensaje | Acción y límites |
| --- | --- | --- |
| Inicio | Jugar | Abre Registro. Después de salir de una operación, un mensaje explica que debe ingresar los mismos datos para retomarla |
| Registro | Iniciar operación V2 | Para datos sin operación V2 anterior y sin una partida activa. El registro y el servicio conservan sus validaciones originales |
| Registro | Retomar operación | Al introducir la cédula de un jugador con operación en curso o entrega pendiente de presentación, indica la acción esperada antes de confirmar el formulario. No crea otra partida |
| Registro | Ver entrega guardada | Si la última V2 terminó y su presentación se reconoció, abre esa entrega y su resultado. No inicia automáticamente otra operación |
| Registro | Operación V1 guardada | Explica que una V1 activa debe retomarse en su flujo original. El piloto no ofrece una migración ni reemplazo |
| Registro | Recuperar apariencia | Conserva el comportamiento existente de recuperar perfil, ficha original y variantes. El snapshot de una partida ya abierta no cambia por los valores iniciales del formulario |
| Registro | Volver | Regresa a Inicio con el control existente |
| Partida | Volver a Inicio | Abre una confirmación en el panel lateral. Suspende la presentación mientras se lee, sin responder, confirmar hitos o escribir estado de partida |
| Confirmación | Seguir jugando | Cierra el diálogo y conserva la fase y la pausa manual previas. No crea decisiones, eventos ni partidas |
| Confirmación | Volver a Inicio | Cierra la sesión mediante el servicio existente y regresa a Inicio. Las decisiones y los hitos ya guardados permanecen. No hay abandono, borrado ni reinicio de la operación |
| Partida | Continuar recorrido | Presenta la consecuencia ya registrada y la transición antes de reconocer el hito. Una consecuencia restaurada no se evalúa de nuevo |
| DELIVERY | Ver resultado de la operación | Mantiene el requisito existente de terminar primero la presentación de entrega |
| Resultado | Ver / ocultar decisiones y paginación | Conserva la consulta del historial de la operación seleccionada |
| Resultado | Volver a Inicio | Conserva el cierre de sesión y regreso existentes |
| Resultado | Iniciar nueva operación | Reutiliza el tercer control visual de la fila superior. Solo se ofrece en un Resultado válido después de cierre visual confirmado. Invoca el comando existente del motor, conserva el histórico y abre la nueva operación en S0 |

Las etiquetas de Registro son orientación visual basada en datos disponibles. El servicio sigue validando nombre, cédula, ficha y permisos al confirmar. Cambiar de cédula actualiza la etiqueta y la explicación para evitar que quede un mensaje de reanudación de otro jugador.

## Contratos reutilizados y seguridad de la navegación

`ServicioUsuarios.RegistrarJugador` conserva la identidad y el personajeId original para una cédula existente con nombre compatible. `CerrarSesion` mantiene la separación de roles al volver a Inicio. No se añadió autenticación.

`RecorridosV2.IniciarPartida()` ya delega al motor que prioriza la operación en curso, recupera un cierre pendiente y crea una nueva operación solo cuando corresponde. No fue necesario un nuevo contrato para iniciar otra operación tras DELIVERY y Resultado. La UI verifica el cierre visual y la ausencia de otra operación activa antes de llamar al comando. Un bloqueo local impide repetir el callback del botón tras una solicitud exitosa.

La selección del adaptador visual prioriza una partida activa. Para consultar una entrega anterior elige la última V2, sin interpretar una V1 finalizada como si fuera V2. Si solo existe historial V1 finalizado, el comando existente puede iniciar V2 sin alterar aquel historial. Se probó este caso.

El nuevo parámetro opcional de `ConectarJugador` comunica únicamente la acción de volver a Inicio entre adaptadores de UI. No cambia el contrato de servicios ni la persistencia. La escena independiente de pruebas conserva su funcionamiento sin ese callback.

Mientras el diálogo está abierto, se suspende el avance visual y se bloquean las acciones de respuesta, continuar, reinicio visual, pausa, reconocimientos y resultado de la pantalla que queda detrás. El fondo recibe los clics y el foco inicial va a Seguir jugando. No se añade un estado persistido de abandono o pausa.

## Estado recuperado y límites reales

El piloto sigue utilizando su misma base temporal y sus servicios actuales. Al salir no hay una operación adicional de guardado, porque cada decisión y confirmación de presentación ya usa la transacción existente. No se agrega un evento de evaluación desde la navegación.

Se recuperan mapa, situaciones resueltas, puntos, decisiones, apariencia fijada a la partida y progreso derivado de los hitos confirmados. Las pruebas comparan el registro completo de Partida, IDs de decisiones, eventos de puntos y apariencia antes de salir y después de retomar. El retorno a Inicio sí puede registrar el cierre de sesión y el ingreso posterior, conforme al servicio existente. Esos eventos no son decisiones ni puntos duplicados.

Si la salida ocurre durante una transición pendiente, se vuelve al mapa origen del último hito confirmado y se muestra la consecuencia guardada. Continuar reproduce la transición y confirma el hito cuando termina. No llama otra vez al evaluador.

**No se conserva cada fotograma ni la posición exacta del camión.** Una reanudación puede repetir una parte visual desde un punto estable dentro del mismo mapa. La confirmación lo explica antes de salir. Añadir posición continua persistida requeriría un contrato con Javier y queda fuera de esta tarea.

La apariencia recuperada sigue separada del personajeId original. Una operación nueva recibe su propio snapshot mediante el adaptador existente. La operación anterior, sus decisiones y su apariencia no se sobrescriben.

## Identidad visual y corrección durante la revisión

Se conservan paneles redondeados, tipografía pixel y de lectura, verdes bosque, musgo y oliva, crema y acento cálido. Los botones usan los componentes de hover y estado deshabilitado ya aprobados. Se reutiliza el espacio de cabecera para Volver a Inicio y Pausar, y el tercer control de Resultado para la nueva operación.

La primera versión del diálogo funcionaba en las pruebas de eventos pero una captura mostró que el mapa lo tapaba parcialmente. `DemoRecorrido` dibuja mediante OnGUI por encima del canvas. Se corrigió **solo la disposición del diálogo**, colocándolo en la columna lateral existente, fuera del área de dibujo del mapa. No se modificó el renderer, el fondo o el camión.

Se agregó una regresión que comprueba que el panel queda fuera del rectángulo del mapa y que el botón de confirmación recibe el primer impacto de raycast. También se comprueba ausencia de truncamiento del texto y del botón de nueva operación. Las capturas posteriores muestran el diálogo completo y el mapa y camión visibles en 1920x1080 y 1280x720.

## Pruebas automatizadas y evidencias

Antes de modificar código se ejecutaron las suites con Unity MCP: **79 EditMode y 34 Play Mode**, todas aprobadas, sin fallos ni omisiones. Se usaron únicamente jugadores sintéticos y carpetas temporales por GUID que las fixtures limpian al terminar.

| Prueba nueva | Casos | Verificaciones |
| --- | --- | --- |
| `NavegacionMenuReanudaMapasTransicionYDelivery` | 1920x1080 y 1280x720 | Regresa y retoma desde S0, R1A, R2A, R3A y DELIVERY. Cubre pregunta, consecuencia guardada, transición pendiente y entrega presentada. Compara Partida completa, decisiones, puntos, eventos de puntos, apariencia e hito. Comprueba diálogo sin recorte, raycast y bloqueo de respuesta residual |
| `NavegacionNuevaOperacionTrasResultadoSinPerderAnterior` | 1920x1080 y 1280x720 | Completa dos operaciones. Ejecuta dos veces el callback de nueva operación, comprueba una sola nueva partida activa, S0, cero puntos iniciales, snapshot de apariencia y preservación exacta del registro e historial anteriores. Regresa desde Resultado a Inicio |
| `NavegacionV1ActivaExplicaLimiteSinReemplazarPartida` | 1 | Informa límite de navegación, conserva partida V1 e ID original, no crea otra operación ni guarda una apariencia por defecto |
| `NavegacionV1FinalizadaPermiteV2SinModificarHistorico` | 1 | Completa V1 con el motor, ingresa por Registro y abre una nueva V2, sin cambiar el registro V1 ni su personajeId |
| `NavegacionCancelarMenuRespetaPausaManual` | 1 | Abrir y cancelar el diálogo no libera la pausa manual, no mueve el camión ni agrega eventos |

Además se ejecuta la regresión existente de ocho recorridos completos desde Inicio hasta Resultado y las ocho rutas de la escena independiente, sin menús de selección de ruta. La administración conserva pruebas de autenticación, permisos, dos jugadores, varias partidas, coincidencia de puntos e historial y generación de PDF. Ejecutar sus pruebas existentes no implica modificar esas áreas. El defecto PDF de nombres largos de la auditoría anterior sigue pendiente de Javier.

### Registro de ejecuciones

| Ejecución | Job ID Unity MCP | Resultado |
| --- | --- | --- |
| Línea base EditMode | `6de82023de0446149730cbc6b975a7ae` | 79 aprobadas, 0 fallidas, 0 omitidas, 20.49 s |
| Línea base Play Mode | `438cde836d764bbb9a0c72bcc8241c06` | 34 aprobadas, 0 fallidas, 0 omitidas, 103.43 s |
| Primer intento nuevo | `0e7c87c22c8344bb84dcd156b4fa130b` | No inició. Faltaba el using del namespace Motor en el archivo nuevo de tests. Se corrigió, recompiló y salió de Play Mode antes de repetir |
| Navegación inicial | `ec0c9904ebfe47df9609ed88fe08daa8` | 5 aprobadas, 0 fallidas, 0 omitidas, 33.69 s. Revisión visual posterior detectó la superposición del diálogo |
| Suite antes de ajuste visual | `70140655ea134405bc8507ec306d6553` | 39 aprobadas, 0 fallidas, 0 omitidas, 119.00 s. Este pase funcional no certifica el diálogo visual inicial |
| Ajuste visual y raycast | `04248f0b175a425ea9bd7472b4cc3f71` | 2 aprobadas, 0 fallidas, 0 omitidas, 24.68 s |
| Play Mode final | `314d995db9b345158fbebbd8c5caa4bd` | 41 aprobadas, 0 fallidas, 0 omitidas, 109.73 s |
| EditMode final | `03ddaf2f855c467ba7800e64fd64bd99` | 79 aprobadas, 0 fallidas, 0 omitidas, 7.72 s |

El intento sin inicialización no se contabiliza como prueba aprobada ni como caso ejecutado. La omisión de un using fue un defecto del código de pruebas durante el desarrollo, no una dependencia del motor. No hubo casos de test ejecutados que fallaran en estas ejecuciones, pero sí ese intento fallido de inicialización y el defecto visual intermedio descrito. No se presentan los pases funcionales previos como aprobación visual de la primera versión.

Console después de Play Mode final: cero errores y cero advertencias. En la lectura final después de EditMode y del refresco de recursos: cero errores y una advertencia de transporte ya observada `MCP-FOR-UNITY: [WebSocket] Unexpected receive error: WebSocket is not initialised`. Se registra por separado como advertencia de Unity MCP, sin atribuirla a las reglas o pantallas del jugador. No se limpió Console para ocultar incidencias.

Evidencias relativas a la raíz del repositorio:

- `TestEvidence/NavegacionV2/`: capturas con tamaño real de Game View comprobado mediante Screen.width/height.
- `Menu-<mapa>-<fase>-1920x1080.png` y `-1280x720.png`: confirmación de regreso en distintos mapas y estados.
- `Inicio-tras-regreso-*`, `Registro-retomar-*`, `Registro-entrega-*`, `Resultado-nueva-operacion-*`: orientación y acciones disponibles.
- `AntesAjusteVisual/Menu-S0-Transicion-1280x720.png`: defecto visual anterior al ajuste, conservado como evidencia del diagnóstico.
- `EditMode-base.json`, `PlayMode-base.json`, `Navegacion-inicial.json`, `PlayMode-antes-ajuste-visual.json` y `Navegacion-ajuste-visual.json`: respuestas del Test Runner.
- `PlayMode-final.json` y `EditMode-final.json`: resultados individuales de las suites finales, con sus IDs y tiempos reales.
- `Conservacion-archivos.json`: comparación de los archivos que existían al comenzar contra el checkpoint de esta tarea.

La navegación se verificó con eventos UI automatizados sobre escenas reales y revisión de sus capturas. No se atribuye a estas pruebas una sesión humana de usabilidad, un cierre abrupto del ejecutable, hardware distinto ni una build distribuida.

## Archivos creados y modificados

| Archivo | Cambio |
| --- | --- |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs` | Enlace de regreso a Inicio, mensaje de orientación, etiqueta contextual de Registro, comprobación visual de compatibilidad y enlace de nueva operación en Resultado |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.Navegacion.cs` | Nuevo parcial de navegación de pantallas. Reutiliza los servicios existentes |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.cs` | Callback opcional de UI, selección de operación guardada y suspensión/bloqueo de controles durante el diálogo |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Vista.cs` | Construcción optativa de los controles del jugador |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Navegacion.cs` | Nuevo parcial de diálogo lateral y foco de sus botones |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Gamificacion.cs` | Impide abrir reconocimientos detrás del diálogo. No cambia puntos, progreso o reglas |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.Navegacion.cs` | Siete casos nuevos, reutilizando la fixture y el almacenamiento temporal actuales |
| `Assets/Mapas/NavegacionUXSofiaV2.md` | Este informe |
| Metadatos Unity de archivos nuevos y `TestEvidence/NavegacionV2/` | Importación y evidencias sintéticas, sin nuevas escenas o atlas |

## Límites y responsables de los pendientes

**No hay un bloqueo de contrato para regresar, retomar V2 o crear otra operación después del cierre.** Esas acciones ya estaban soportadas y ahora se exponen desde las interfaces del piloto.

Siguen sin estar soportadas por esta navegación la conversión de una operación V1 activa a V2, la persistencia por fotograma del vehículo, eliminar o abandonar partidas, seleccionar simultáneamente otra operación en curso y migrar el piloto a producción. No se añadieron reglas para forzarlas. Si el equipo las solicita, Javier debe definir sus contratos antes de conectarlas visualmente. La navegación V1 original no se reimplementó dentro del adaptador V2.

Pendientes que corresponden a Sofía: realizar pruebas de usabilidad observadas con personas ajenas al proyecto, comprobar comprensión de iniciar/retomar/ver entrega, claridad del aviso de punto confirmado, significado de las fichas, percepción de la confirmación y facilidad para iniciar otra operación. También revisar navegación completa por teclado y accesibilidad con participantes, sin darla por certificada por asignar foco al diálogo.

El equipo debe aprobar educativamente el contenido candidato y las reglas de insignias y categorías cuando corresponda. Jorge mantiene la responsabilidad de indicadores. El PDF de nombres largos corresponde a Javier. La distribución y el comportamiento en una build requieren coordinación fuera del alcance exclusivo de interfaz de esta tarea. Ninguna de estas dependencias se sustituyó por datos, reglas o resultados ficticios.
