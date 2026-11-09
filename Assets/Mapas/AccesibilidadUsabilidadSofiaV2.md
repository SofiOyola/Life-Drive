# Accesibilidad y usabilidad de las interfaces del piloto V2

8 de noviembre de 2026, hora de Colombia. Unity 6000.6.7f1. Rama `feat/unity-map`. PROMPT 39, Sofía, S3-07 y S4-03.

## Resultado y alcance

Se ejecutaron **79/79 EditMode y 48/48 Play Mode**, sin fallos ni omisiones en las suites finales. Las 48 pruebas incluyen las 41 previas y siete casos nuevos de accesibilidad. Se conservaron los ocho recorridos completos por decisiones reales, la personalización, la reanudación y la administración. Dos operaciones completas adicionales se recorrieron mediante teclado, en ambas resoluciones.

Esto verifica comportamientos de UI y muestras visuales. **No certifica accesibilidad universal, cumplimiento integral de WCAG ni compatibilidad con lectores de pantalla.** No se realizaron sesiones con personas. La guía al final está preparada para ejecutarlas, sin participantes ni resultados inventados.

Se revisaron Inicio, Registro, PruebaIntegracionV2, Resultado, AdminLogin y Admin, los componentes de respuesta/retroalimentación y el EventSystem. Se leyeron NavegacionUXSofiaV2.md, AuditoriaExperienciaPilotoV2.md y RedisenoBaseUI.md. Los pendientes históricos de los documentos se interpretan junto con los adaptadores y las pruebas actuales.

HEAD inicial: `bd9de07202369b76389527108a6012663e34a062`. Había cambios locales del PROMPT 38, fuentes, ProjectSettings, una escena temporal anterior y evidencias sin seguimiento. Se conservaron mediante un checkpoint de archivos y hashes en `Library/Prompt39Checkpoint`. No se hizo commit, push ni staging.

## Hallazgos y cambios

| ID | Severidad | Hallazgo | Corrección dentro de UI |
| --- | --- | --- | --- |
| A-01 | Alto | No había orden explícito de Tab/Mayús+Tab ni foco inicial consistente en las seis pantallas. La navegación automática de Unity no garantizaba un recorrido comprensible | Orden de foco por pantalla, omisión de controles ocultos/deshabilitados, foco inicial y activación por Enter/Espacio mediante EventSystem |
| A-02 | Alto | El diálogo necesitaba confinar el foco y evitar que una pulsación sostenida alcanzara el siguiente botón o una respuesta detrás | Recorrido circular dentro de la confirmación, Escape para cancelar, foco inicial en Seguir jugando y bloqueo de activación hasta soltar Enter/Espacio. Se mantienen los bloqueos de acciones detrás del diálogo |
| A-03 | Medio | Hover y selección persistente no daban un indicador de foco independiente, especialmente en muestras de personalización | Borde dorado visible para el control enfocado, además de escala/hover. En áreas con máscara el borde queda dentro del viewport, para que no se recorte |
| A-04 | Medio | Texto crema sobre hover secundario tenía contraste aproximado 3,33:1 | Ajuste de luminosidad del hover en ejecución dentro de la paleta bosque/oliva. En el secundario comprobado sube a aproximadamente 4,62:1. Etiquetas eligen crema o bosque según el fondo. No se recolorean sprites ni atlas |
| A-05 | Medio | El tipo de situación no se indicaba en la ficha y la consecuencia no repetía la alternativa elegida | Etiqueta de tipo con el valor real, alternativas numeradas y “Tu elección” seguida del texto real guardado. Se conservan veredicto, signo de puntos, retraso y explicación exactos |
| A-06 | Medio | Un párrafo largo podía quedar truncado o reducirse demasiado en el área fija de lectura | ScrollRect y máscara de UI existentes, tipografía de cuerpo fija en 25 unidades y viewport más alto para consecuencia. Lectura con rueda o RePág/AvPág al enfocar el área |
| A-07 | Medio | Historial administrativo sobre la carretera perdía contraste. El scroll no tenía acceso explícito por teclado | Fondo verde del tema solo cuando hay detalle, foco de lectura y desplazamiento por teclado. En la selección vacía vuelve a transparente y conserva el volante y su composición |
| A-08 | Bajo | Resultado permitía paginar mientras su historial estaba cerrado | Paginación habilitada únicamente al abrir el historial. Orden de teclado: revisar, paginación disponible, nueva operación y volver |

Las etiquetas envolventes y la numeración son presentación. **No se reescribieron enunciados, alternativas, respuestas correctas, explicaciones, valores o tipos de situación.** La consecuencia y la elección se leen de la decisión confirmada del motor. No se suman puntos desde la UI.

Las muestras de piel, ojos, cabello y traje conservan los recursos y colores originales del avatar. El foco de una muestra se identifica por borde y una ayuda textual de grupo/opción. Su activación mantiene el nombre del color seleccionado ya disponible. La comprensión de las cuatro fichas técnicas y cualquier ambigüedad educativa siguen pendientes de evaluación humana y del equipo. No se mapearon colores o género a IDs.

## Teclado y orden de lectura

| Contexto | Recorrido y acciones |
| --- | --- |
| Inicio | Jugar → acceso administrativo. Mayús+Tab permite retroceder de forma circular |
| Registro | Nombre → documento → fichas 1–4 → Hombre/Mujer → piel → ojos → cabello → traje → recuperar apariencia → confirmar → volver |
| Partida | Nueva situación enfoca la primera alternativa. Consecuencia enfoca Continuar. La lectura desplazable y las demás acciones visibles forman parte del recorrido de Tab |
| Confirmación | Seguir jugando ↔ Volver a Inicio. El foco no sale del diálogo. Escape equivale a seguir jugando |
| Reconocimientos | Foco en Volver al recorrido. Escape cierra el panel |
| Resultado | Revisar decisiones → paginación habilitada → iniciar nueva operación → volver a Inicio |
| AdminLogin | Usuario → contraseña → ingresar → volver. Enter dentro de un campo termina su edición y pasa al siguiente control, sin enviar automáticamente credenciales |
| Admin | Búsqueda → buscar → jugadores visibles → paginación disponible → partidas visibles → paginación disponible → desempeño → lectura → PDF → abrir PDF cuando existe → cerrar sesión |

Tab avanza y Mayús+Tab retrocede. Enter activa botones o avanza desde un campo. Espacio activa botones y se conserva como carácter cuando se edita texto. RePág/AvPág desplazan el área de lectura enfocada. Los controles deshabilitados u ocultos se omiten.

Se reutilizan los EventSystem, InputSystemUIInputModule, TMP_InputField y handlers submit de Unity. El módulo sigue procesando puntero y edición nativa de TMP. Solo durante el piloto se desactiva su navegación automática para evitar doble submit o movimientos de foco paralelos. Se restaura el valor anterior al destruir el adaptador.

No se agregaron controles WASD ni conducción por flechas. El camión sigue avanzando automáticamente y las decisiones siguen resolviéndose en el motor. La navegación nueva se activa exclusivamente al crear el adaptador explícito del piloto. Fuera del piloto V1 conserva su entrada y sus servicios.

## Comprobaciones visuales y límites

Se inspeccionaron capturas de 1920x1080 y 1280x720 con dimensiones reales de Game View verificadas. En las muestras revisadas se conservan la tipografía pixel, la fuente de lectura, los verdes bosque/musgo/oliva, el crema, los acentos cálidos y los paneles y botones redondeados. El mapa y camión permanecen visibles.

La ficha separa tipo, título, enunciado, opciones y consecuencia. La retroalimentación incluye palabras y signos, por lo que no depende solamente del color para indicar acierto o variación de puntos. Las opciones conservan el texto original y agregan números para facilitar la referencia. Continuar aparece después de registrar la respuesta.

La lectura larga usa un viewport que recorta deliberadamente la ventana visible y permite acceder al resto del texto mediante desplazamiento. Esto se distingue de perder texto por truncamiento. El cuerpo no reduce su tamaño para comprimir todo el párrafo. La prueba de desbordamiento usa texto sintético explícito solo en la vista, sin cambiar contenido educativo o persistencia.

Los valores de contraste citados corresponden a pares de color concretos y al botón secundario comprobado. No se certificó el contraste de cada píxel, estado animado, muestra de color, transparencia o imagen de fondo. Las capturas no demuestran por sí solas legibilidad para personas con baja visión.

Límites pendientes:
- No se probó un lector de pantalla ni se implementó un puente de accesibilidad del sistema operativo.
- No se certificaron IME, teclados internacionales, dictado, teclados de asistencia, lectores de braille, gamepad o interruptores.
- La edición automatizada usa eventos públicos de TMP, no escritura física de un participante. Debe validarse en una sesión con teclado real.
- No hay ajuste global de tamaño de texto o contraste por usuario. Las ayudas secundarias y los detalles pequeños del avatar requieren revisión humana.
- La recuperación de partida conserva hitos confirmados, no cada fotograma del camión. No cambia el contrato de reanudación del PROMPT 38.
- Abrir el visor PDF del sistema y guardar desde él no se ejecutó en esta tarea. Se verificaron generación real, archivo y foco del botón, sin modificar el generador.
- El candidato V2, las insignias, las categorías finales y los KPI mantienen sus dependencias de aprobación. No se activaron reglas nuevas.

## Pruebas ejecutadas

Las carpetas y cuentas son temporales y sintéticas. Las fixtures previas se reutilizan y limpian sus bases al terminar. No se usaron cédulas, contraseñas o jugadores reales.

| Casos nuevos | Cantidad | Evidencia funcional |
| --- | --- | --- |
| AccesibilidadJugadorTecladoRegistroEntregaYReanudacion | 2 resoluciones | Tab/Mayús+Tab en Inicio, edición TMP, selección de ficha y personalización, respuesta Enter/Espacio, pulsación sostenida sin doble evaluación, consecuencia con elección real, diálogo confinado, Escape, retorno/reanudación y Resultado con paginación |
| AccesibilidadTextoLargoDesplazableSinEvaluar | 2 resoluciones | Cuerpo a 25 unidades, sin truncamiento, desplazamiento hasta final y regreso al principio, foco dentro de máscara, partida idéntica y cero decisiones |
| AccesibilidadAdministracionTecladoPermisosYLectura | 2 resoluciones | Login inválido y válido por el servicio real, avance seguro desde contraseña, búsqueda visible, selección de jugador/partida, desempeño real, lectura con teclado, PDF real y cierre de sesión. Conserva volante |
| AccesibilidadContrasteHoverYFocoVisible | 1 | Contraste del secundario enfocado >= 4,5:1 y borde de foco visible |

Las pruebas de teclas introducen estados de un teclado virtual de Unity Input System y pasan por la ruta de producción. No llaman a los listeners de los botones para sustituir la navegación. La escritura de caracteres usa TMP_InputField.ProcessEvent y ForceLabelUpdate para reproducir la fase de edición de TMP. Los clics y las comprobaciones de persistencia de las 41 pruebas anteriores siguen ejecutándose.

| Ejecución Unity MCP | Job ID | Resultado |
| --- | --- | --- |
| Base EditMode | 8b38afb50b0c40afadc76e4267935ad4 | 79 aprobadas, 0 fallidas, 0 omitidas, 17,64 s |
| Base Play Mode | dc9d0a08579e45ffaf4e55d4b1099381 | 41 aprobadas, 0 fallidas, 0 omitidas, 141,66 s |
| Primer ensayo de accesibilidad | 01fa53a5c69d446288ddd0cb47480636 | 4 aprobadas y 3 fallidas, 0 omitidas, 15,60 s |
| Repetición dirigida | 6a5ee82e96de470aa76df66399e0e56d | 4 aprobadas, 0 fallidas, 0 omitidas, 16,02 s |
| Suite interrumpida | ba42e0ea88694b8cb28aa06c06b14ce3 | Interrumpida tras 19 de 48, sin resultado final. No se cuenta como aprobada |
| Diagnóstico y ocho rutas | b9a675a4e3b3476f859276a84cee7804 | 11 aprobadas, 2 fallidas, 0 omitidas, 34,61 s |
| Play Mode final | 1fcd105d47c7459c92276ed419ba95a4 | 48 aprobadas, 0 fallidas, 0 omitidas, 136,82 s |
| EditMode final | cea4d8a89b22417bb07698630f2885f8 | 79 aprobadas, 0 fallidas, 0 omitidas, 9,88 s |

Incidencias de desarrollo conservadas:
- La primera compilación usó conversiones de SceneHandle obsoletas y una sobrecarga de búsqueda de objetos obsoleta de esta versión de Unity. Se sustituyeron antes de ejecutar los casos nuevos.
- Los fallos del primer ensayo correspondían a inspeccionar el foco o el control de lectura antes de completarse el pase de UI. Se ajustó la espera en el test y se mejoró su diagnóstico.
- Se corrigió una referencia de fuente de la ayuda que seguía viva al liberar las fuentes temporales del piloto. Las ayudas y bordes ahora se limpian con su anfitrión. Los resultados individuales finales no contienen esos avisos.
- La comprobación posterior detectó que el simulador ProcessEvent no refrescaba la etiqueta de TMP como hace el ciclo nativo. Dos tests fallaron al comprobar el texto visible de búsqueda. Se añadió ForceLabelUpdate al simulador, conservando el caso que detectó el problema.
- El pase interrumpido registró desconexión del puente y NullReferenceException en PlayModeRunTask, seguida de errores del Test Runner. Su causa no se determinó. Se repitió y se obtuvo el resultado completo de 48/48. No se ocultó ni se atribuyó a una prueba humana.
- Tras Play Mode final Console devolvió cero errores y cero advertencias. La lectura final y la conservación de archivos se registran en las evidencias de cierre. La advertencia de transporte WebSocket is not initialised ya observada en recargas debe distinguirse de incidencias del producto.

## Archivos de esta tarea

| Archivo | Cambio |
| --- | --- |
| Assets/Scripts/IntegracionV2/TecladoPilotoV2.cs | Nuevo adaptador de foco, teclado, contraste de etiquetas, ayuda y superficies de lectura. Sin servicios de negocio |
| Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs | Activación explícita de teclado y habilitación contextual de paginación del historial |
| Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Vista.cs | Lectura desplazable, tipo real, numeración y elección guardada en la consecuencia |
| Assets/Tests/PlayMode/TeclasPilotoPruebas.cs | Teclado virtual y edición de TMP para las pruebas |
| Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.Accesibilidad.cs | Cinco casos nuevos del jugador, texto largo y contraste |
| Assets/Tests/PlayMode/AdministracionPilotoV2Tests.Accesibilidad.cs | Dos casos nuevos administrativos |
| Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.Auditoria.cs | Se separa foco de teclado y hover de puntero en el test previo |
| Assets/Tests/PlayMode/LifeDrive.Tests.PlayMode.asmdef | Referencia InputSystem para el dispositivo virtual de pruebas |
| Assets/Mapas/AccesibilidadUsabilidadSofiaV2.md y metadatos nuevos | Informe y guía |

Evidencia:
- TestEvidence/AccesibilidadV2: JSON de base, intentos, interrupción y suites finales. Capturas Inicio-foco, Registro-foco, Ficha-enunciado, Ficha-consecuencia, Resultado-foco, Contraste-foco y Texto-sintetico-final en las resoluciones correspondientes.
- TestEvidence/AdministracionV2/Teclado-seleccion-* y Teclado-desempeno-*: selección con volante y desempeño con lectura enfocada. Las evidencias anteriores de las ocho rutas también se regeneran con la suite.
- El texto sintético de desbordamiento y las identidades de prueba son datos de prueba. Los puntos, decisiones, rutas, consecuencias y reportes de los casos de integración provienen del motor real.
- Los recursos de fuentes que Unity amplíe durante las pruebas se recuperan únicamente desde el checkpoint de esta tarea, preservando sus cambios locales anteriores. No se entregan atlas generados ni escenas del Test Runner.

## Guía breve para evaluación con personas

**Estado: preparada, no ejecutada.** Reclutar personas ajenas al proyecto y registrar si conocen videojuegos, seguridad vial y navegación por teclado, sin usar nombres o documentos reales. Asignar códigos P01, P02, etc. Obtener su consentimiento para observar y, solo si aceptan, grabar pantalla. La primera sesión debe ser interna y con el candidato identificado como no aprobado.

Preparación de Sofía:
1. Abrir el piloto explícito en un equipo del ensayo, con el mismo almacenamiento temporal aislado y un tamaño de pantalla registrado.
2. Crear identidades ficticias de prueba, por ejemplo “Participante de prueba” y documentos sintéticos acordados para el ensayo. No solicitar cédula real.
3. Provisionar una cuenta administrativa temporal mediante el mecanismo existente, sin escribir la contraseña en la guía, capturas o repositorio.
4. Preparar dos jugadores sintéticos y dos partidas de un jugador para las tareas administrativas.
5. Verificar que las ocho rutas y la sesión temporal están disponibles. No activar V2 globalmente ni introducir datos personales.
6. Leer: “Estamos evaluando la interfaz, no tus conocimientos. Cuenta en voz alta qué ves, qué entiendes y qué esperas que ocurra. Puedes detenerte cuando quieras”. No explicar de antemano cuál alternativa educativa es correcta.

Tareas del jugador:
- Encontrar la opción para jugar, completar datos ficticios y elegir una ficha y apariencia.
- Realizar la personalización primero con el medio habitual y después intentar los controles con Tab, Mayús+Tab, Enter y Espacio.
- Leer una situación y explicar con sus palabras qué se le pregunta y cómo elegir una alternativa.
- Responder y explicar qué cambió, qué eligió, cuál fue la consecuencia y cuándo puede continuar.
- Distinguir puntos acumulados, variación de puntos y progreso del recorrido.
- Regresar a Inicio y retomar la misma operación. Preguntar qué esperaba recuperar y si entiende la repetición de una presentación pendiente.
- Completar DELIVERY, consultar Resultado y su historial. Encontrar cómo iniciar otra operación sin perder la anterior.

Tareas del administrador:
- Intentar una contraseña de prueba incorrecta y explicar el mensaje.
- Ingresar con la cuenta temporal, buscar al jugador sintético por nombre y luego por documento de prueba.
- Elegir una partida concreta, consultar puntos, elección y consecuencias, y llegar al final del historial.
- Distinguir dos partidas y explicar a cuál pertenecen los datos.
- Generar el reporte real. Si el entorno lo permite, abrirlo con el visor del equipo y registrar cualquier problema, incluido el pendiente conocido de nombre largo. No presentar un archivo ficticio como reporte.
- Cerrar sesión y verificar que el panel no queda disponible sin autenticación.

Preguntas neutrales:
- ¿Qué entendiste del enunciado y del tipo de situación? ¿Qué palabra te hizo dudar?
- ¿Cómo sabes cuál control tiene el foco y qué ocurrirá al activarlo?
- ¿Cómo identificas la alternativa que elegiste y su consecuencia?
- ¿Qué diferencia hay entre los puntos que aparecen arriba y el cambio de esta respuesta?
- ¿Qué representa la barra de progreso? ¿Qué esperas cuando llegas a DELIVERY?
- ¿Qué esperabas que pasara al volver a Inicio y al retomar?
- ¿Qué significan para ti las fichas 1–4? ¿Las confundiste con Hombre/Mujer o con un color?
- ¿Pudiste leer todo? ¿Descubriste cómo desplazar el texto o cambiar la página?
- ¿Qué cambiarías primero? Califica facilidad de uso y satisfacción de 1 (muy baja) a 5 (muy alta), explicando el motivo.

Registro por tarea, para completar durante la sesión:

| Código | Tarea / pantalla / situación ID | Resolución y método de entrada | Resultado autónomo / con ayuda / no logrado | Tiempo observado, si se mide | Dificultad o frase literal | Ayuda dada | Severidad / responsable |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Por completar | | | | | | | |

Registrar aparte satisfacción, sugerencias, interrupciones y cualquier necesidad de acceso declarada voluntariamente. No convertir puntuaciones de facilidad o satisfacción en KPI de Jorge. No incluir información personal en capturas.

Clasificación propuesta:
- Bloqueante: impide completar la tarea y no hay alternativa disponible.
- Alto: un grupo o método de entrada queda excluido, existe riesgo de acción involuntaria o se requiere ayuda constante.
- Medio: causa errores recuperables, lectura difícil o desorientación, pero permite continuar.
- Bajo: fricción menor de etiqueta, orden o presentación.
- Pendiente de validación educativa: confusión del contenido que no se resuelve con presentación. Registrar ID y texto observado para el equipo, sin reescribirlo.
- Dependencia técnica ajena: registrar contrato faltante y responsable. No reemplazar servicios o fórmulas para completar la demostración.

## Pendientes exclusivos de Sofía

Organizar y ejecutar las sesiones, revisar teclado físico y edición real de texto, observar baja visión y comprensión del foco/scroll, revisar tamaños de ayudas secundarias, analizar las dificultades de las fichas técnicas y priorizar mejoras de UI a partir de evidencias. Después debe repetir las regresiones correspondientes y registrar qué se validó con personas.

El contenido educativo candidato debe aprobarlo el equipo responsable. Las fórmulas de Jorge, PDF, autenticación y persistencia central siguen fuera de esta tarea. No se inventaron insignias ni categorías, no se cambiaron rutas o recursos gráficos y V1 sigue predeterminada.

## Cierre verificable

La comparación SHA-256 final revisó 506 archivos que existían al comenzar: cero faltantes. Solo cambiaron dos scripts runtime de UI, el test de hover y el asmdef de pruebas descritos arriba. Las fuentes y ProjectSettings recuperaron exactamente sus bytes del checkpoint, incluidos los cambios que ya estaban pendientes antes de esta tarea. Los servicios, contenido educativo, ruta.json, mapas, camión, personajes, escenas originales y demás recursos conservaron sus hashes.

HEAD continúa en bd9de07202369b76389527108a6012663e34a062. La escena temporal original InitTestScene8574c1eb-c122-4e71-b794-51110fac0430 y su meta se conservaron. Se retiró únicamente la escena temporal identificada que dejó la ejecución interrumpida de esta tarea. No se entrega una escena temporal nueva.

La comprobación final de archivos nuevos no encontró espacios al final de líneas. El cierre se verificó mediante Unity MCP y hashes porque el cliente de comandos comenzó a rechazar una ruta de permisos /workspace no válida en Windows. No se cambió la configuración de permisos para corregir ese problema del entorno.

Console final: **0 errores y 0 advertencias**. Unity quedó fuera de Play Mode, sin compilación pendiente ni errores de compilación, con Inicio abierto. No se limpió Console para ocultar mensajes. Evidencias de cierre: TestEvidence/AccesibilidadV2/Console-final.json y Conservacion-final.json. Las pruebas humanas siguen sin realizarse.
