# Auditoría de experiencia y calidad del piloto V2

Fecha de ejecución: 7 de noviembre de 2026, hora local de Colombia, con cierre el 8 de noviembre UTC. Unity 6000.6.7f1. Rama `feat/unity-map`. Actividad PROMPT 37, alcance de Sofía y responsabilidades compartidas de calidad.

## Dictamen y alcance

El flujo técnico permanece funcional en un entorno temporal controlado. La regresión final ejecutada mediante Unity MCP dio **79/79 EditMode y 34/34 Play Mode**, sin fallos ni pruebas omitidas dentro de esas suites. Los ocho recorridos se completaron desde Inicio hasta Resultado mediante respuestas a las situaciones. Se corrigieron cuatro defectos reproducidos. Permanece un defecto visual del PDF con nombres de longitud máxima y varios pendientes de producto y aprobación educativa.

Este resultado permite continuar una revisión interna del equipo con datos sintéticos. No constituye aprobación educativa del candidato V2 ni certificación de usabilidad con usuarios. No se ejecutó una prueba de campo, un build distribuido ni interacción humana física con el ratón. Las pruebas Play Mode envían eventos de clic a los botones reales, recorren escenas reales y consultan el almacenamiento temporal. La revisión visual consiste en inspección de capturas de esas ejecuciones y del PDF renderizado por separado.

Se revisaron Inicio, Registro, PruebaIntegracionV2, Resultado, AdminLogin y Admin. Se conservaron las escenas y recursos aprobados. No se creó una nueva demostración ni otra base de datos.

## Seguridad del trabajo y estado de Git

- HEAD inicial y final: `23370c089492d374520ea107ec93dfd1f8c9ed73`, `Integrar ocho recorridos V2 con persistencia y flujo del jugador`.
- Ese commit ya contiene la integración inicial del jugador, el contenido de ocho rutas y la persistencia base. Administración integrada, gamificación real y personalización incluyen avances locales posteriores que no están todos confirmados en commits.
- Se conservó el estado local inicial, incluidos scripts, documentación, escena de integración, fuentes y ProjectSettings ya modificados. Se creó un checkpoint de bytes y hashes en `Library/Prompt37Checkpoint`, fuera de los entregables versionables.
- El Test Runner regeneró datos internos del atlas `FuenteInterfaz.asset`. Se devolvió únicamente ese archivo a sus bytes exactos del checkpoint de esta auditoría. No se restauró desde Git ni se descartaron los cambios previos de las otras fuentes.
- Existía antes una escena `Assets/InitTestScene8574c1eb-c122-4e71-b794-51110fac0430.unity` y su meta. Se conservó, no forma parte del entregable de esta auditoría y no debe incluirse al preparar cambios para revisión.
- No se hizo commit, push ni staging. No se incluyeron credenciales, bases ni jugadores reales. Las evidencias contienen exclusivamente identidades y partidas sintéticas.
- La comparación SHA-256 contra el checkpoint permite distinguir las modificaciones de esta auditoría de los cambios locales anteriores. `ruta.json`, el contenido candidato, los recursos originales de los mapas, camión, trayectorias, escenas, servicios centrales y ProjectSettings no recibieron cambios de esta tarea.

## Criterios vigentes revisados

Se leyeron `RedisenoBaseUI.md`, `FlujoJugadorIntegradoV2.md`, `IntegracionPartidaV2.md`, `IntegracionAdministrativaV2.md`, `GamificacionIntegradaS3-04.md`, `PersistenciaPersonajesS3-07.md`, `ContenidoOchoRecorridosV2.md` y `PersistenciaRecorridosV2.md`.

Los documentos describen etapas diferentes. Los pendientes históricos de integración se interpretan junto con los informes posteriores y el código ejecutado, sin asumir que una especificación académica sea una implementación.

| Criterio | Resultado de auditoría |
| --- | --- |
| Piloto explícito y V1 por defecto | Conservado. Menú del Editor para abrir el piloto, sin activar V2 globalmente |
| Un solo motor de evaluación y persistencia | Conservado. La UI lee decisiones, consecuencias y puntos del servicio |
| Inicio → Registro → Partida → Resultado | Verificado por ocho casos completos Play Mode |
| Ocho rutas sin salidas inválidas | Verificado por suites de motor, contenido, recorrido visual y UI |
| Entrega visual antes de Resultado | Verificado por las aserciones del flujo completo |
| Administración con roles y misma base temporal | Verificado por permisos, búsqueda, selección de partidas y reapertura |
| Personalización independiente del personajeId | Verificado con perfiles nuevos, antiguos, snapshots y rechazo de variantes inválidas |
| Identidad natural y pixel art | Conservada en capturas de ambas resoluciones, sin modificar mapas o personajes |
| TDS, IEE, PPS y PCRS | Pendientes del servicio de indicadores. No se implementaron ni recalcularon fórmulas |
| Tres insignias y categorías de final | Recursos preparados, sin concesión ni clasificación inventada |
| Contenido educativo V2 | Candidato pendiente de validación del equipo |

## Hallazgos reproducidos y correcciones

No se detectó un bloqueante en los casos ejecutados. Un hallazgo alto y tres medios quedaron corregidos.

| ID | Severidad | Reproducción y efecto anterior | Corrección y evidencia |
| --- | --- | --- | --- |
| UX-01 | Alto | Alterar la huella de contenido de una partida sintética y pulsar Reiniciar vista. El servicio rechaza correctamente la incompatibilidad, pero la presentación del error vuelve a consultar el mismo resumen y lanza otra excepción sin gestionar | En fase Error, la UI no vuelve a consultar gamificación. Indica puntos y recorrido no disponibles, inhabilita reconocimientos y permite reintentar. Restaurada la huella sintética, recupera la consecuencia sin sumar puntos ni decisiones. `AuditoriaErrorDeHuellaNoFallaOtraVezAlPresentarlo` |
| UX-02 | Medio | Registrar un nombre válido de 60 caracteres W y completar una partida. Resultado recorta el nombre en 1920x1080 y 1280x720 | Se amplió la altura del nombre y separó la etiqueta de ficha dentro del panel existente. Se comprueba ausencia de truncamiento y presencia de los 60 caracteres. `AuditoriaNombreLargoSinRecortes`, dos resoluciones |
| UX-03 | Medio | Registrar literalmente `<color=red>Auditora</color>`. TMP interpreta el nombre como formato y oculta parte de la identidad escrita | La presentación del piloto trata texto y datos como literales mediante `richText=false`. El nombre completo se conserva y muestra sin controlar el estilo. No era ejecución de código. `AuditoriaNombreSeMuestraLiteralSinInterpretarEtiquetas` |
| UX-04 | Medio | Deshabilitar un botón después de probar hover. Su color era prácticamente idéntico al de un control disponible | El componente visual existente oscurece el estado deshabilitado con verde bosque y limpia el estado presionado. Conserva hover, escala y restauración del color al habilitar. Se comprueba que el clic deshabilitado no navega. `AuditoriaHoverYDeshabilitadoSonDistinguibles` |

Las nuevas regresiones se ejecutaron primero contra la implementación anterior: **0 aprobadas y 5 fallidas**, porque UX-02 tiene dos resoluciones. Después de corregir, las mismas cinco pasaron. Las fallas iniciales se registran como evidencia del diagnóstico, no se ocultan ni se cuentan como fallos pendientes de la versión final.

## Hallazgos abiertos y funciones pendientes

| ID | Severidad o estado | Evidencia, consecuencia y responsable |
| --- | --- | --- |
| UX-05 | Medio, no corregido | El PDF admite y contiene el nombre completo de 60 W y la cédula, pero el encabezado se escribe en una sola línea y rebasa el ancho de la página. La cédula queda fuera del área visible. Reproducido desde Admin con el generador real y confirmado al renderizar `TestEvidence/AuditoriaV2/Pdf-nombre-largo.pdf`. Javier: ajustar el encabezado en `GeneradorReportes` usando el soporte de párrafos existente. No se reescribió ni modificó ese servicio central |
| UX-06 | Medio, decisión de producto | El piloto integrado ofrece retomar la última partida. Tras DELIVERY se vuelve a la entrega y a su resultado, no aparece una elección explícita de nueva operación para el mismo jugador. El motor admite múltiples partidas y Admin las separa correctamente. Equipo y Javier: acordar el flujo antes de añadir un nuevo control |
| UX-07 | Medio, decisión de producto | Durante el recorrido no hay navegación visible para guardar y salir a Inicio. La reanudación técnica sí funciona al cerrar y volver a abrir el piloto. Equipo: definir una salida comprensible dentro del alcance siguiente, sin añadir ahora una funcionalidad nueva |
| UX-08 | Bajo, claridad pendiente | Registro exige elegir Ficha 1–4 por compatibilidad con los IDs originales, además de personalizar la apariencia. El texto explica que la ficha se conserva, pero el significado educativo no se puede deducir de la apariencia. Javier y equipo: acordar una etiqueta o explicación válida sin asociar género o color a un ID |
| EDU-01 | Pendiente de aprobación | Preguntas, alternativas, consecuencias y valores del candidato V2 requieren validación educativa. No se sustituyeron ni se corrigieron por criterio de UI |
| GAM-01 | Pendiente de aprobación | Las tres insignias siguen pendientes de reglas aprobadas. Solo se muestran buenas prácticas respaldadas por decisiones y eventos del motor, sin conceder insignias |
| RES-01 | Pendiente de aprobación | Las tres categorías finales siguen como diseños preparados. Resultado muestra resumen neutral, sin clasificar la operación |
| KPI-01 | Dependencia con Jorge | TDS, IEE, PPS y PCRS no están disponibles como resultados reales en esta integración. Se conserva el historial necesario. No se añadieron fórmulas visuales |

UX-06 a UX-08 se identificaron por inspección de la navegación y del código existente. No se presentan como nuevas pruebas automatizadas fallidas ni como solicitudes de cambio ya aprobadas.

## Matriz de casos funcionales ejecutados

Los nombres siguientes corresponden a pruebas existentes o añadidas realmente ejecutadas en las suites finales. Los prefijos de namespace se encuentran completos en los JSON de evidencia.

| Área y casos positivos o negativos | Pruebas y comprobaciones |
| --- | --- |
| Selección de perfiles, Registro y navegación | `InicioAResultadoOchoRutas(0..7)`. Selección desde Inicio, ficha obligatoria, registro, camión automático, preguntas, alternativas, consecuencias, mapa correcto, DELIVERY, Resultado, historial y regreso a Inicio |
| Campos vacíos y cédula inválida | `AuditoriaNombreLargoSinRecortes(1920/1280)` rechaza nombre vacío y cédula de cuatro dígitos sin crear jugadores. `RegistroExigeNombreCedulaYPersonaje`, `CedulaSoloAceptaEntre5y15Digitos` y `LaCedulaInvalidaSeRechaza` cubren reglas de datos |
| Doble clic y respuestas repetidas | Cada una de las ocho rutas hace doble clic en registro y respuesta. Comprueba cantidad de decisiones, puntos, eventos de puntos y una sola finalización. `UnaDecisionRepetidaNoSumaDosVecesYLasInvalidasSeRechazan` valida el contrato central |
| Puntos positivos, negativos y neutros | `ProyectaPuntosYBuenasPracticasSinEscribir`, `PenalizacionRealSinReconocimientoYCompatibilidadV1`, `CambiosNeutroYNegativoSinInsigniaAplicable` y capturas del flujo. Las animaciones no evalúan ni guardan de nuevo |
| Progreso y ocho rutas | `OchoRutasConProgresoContinuoYReconstruccionDesdeHitos(0..7)`, `OchoRutasConBotonesReales(0..7)` y flujo completo. Se distinguen recorrido visual, situaciones resueltas y entrega presentada |
| Reanudación en transición | `CierreDelPilotoRetomaConsecuenciaSinEvaluarDeNuevo`, `RestauraHitosYPuedePausarYReiniciarLaPresentacion`, `ReproducePendienteSinReevaluarNiDuplicar`. Conserva puntos y decisiones, repite presentación pendiente sin reevaluar |
| Reapertura tras DELIVERY | `RestauraHitosYPuedePausarYReiniciarLaPresentacion` reabre la escena después de entrega y conserva la misma partida, seis decisiones y una sola finalización |
| Personalización y usuarios anteriores | `RecuperaAparienciaTrasCerrarSinSobrescribirDefaults(1920/1280)`, `JugadorAntiguoRecuperaBaseYConservaIdOriginal`, y pruebas EditMode de perfil, snapshot, versiones y variantes. Resultado usa la apariencia de esa partida aunque el perfil cambie después |
| Partidas V1 antiguas | `ReanudaArchivoV1SinCamposNuevosYRegistraElAvanceImplicito`, `ConservaAmbasVersionesYRechazaRetomarV2SinPaquete`, `JugadorAntiguoSinAtributosYV1SonValidosSinGuardarDefaults` y compatibilidad del candidato. Es comprobación de datos y motor, no una sesión manual completa de UI V1 |
| Login, permisos y aislamiento | `CredencialesIncorrectasJugadorBloqueadoYReapertura`, `PanelVacioYUsuarioInexistente`, `LaTablaDePermisosSeparaLosRoles`, `CadaRolSoloHaceLoSuyoYLosRechazosQuedanRegistrados`. Login inválido limpia contraseña y conserva rol Ninguno. Jugador y usuario sin sesión no consultan administración |
| Dos jugadores y varias partidas | `ConsultaPartidasPermisosYPdf(1920/1280)` crea Alfa, Beta y Sin Partidas, cuatro partidas, busca por nombre/cédula, pagina partidas y compara IDs, puntos, decisiones y eventos. No mezcla resultados |
| Vacíos y errores administrativos | Mismas pruebas: jugador sin partidas, búsqueda sin coincidencias, base vacía, selección nueva limpia resumen y PDF. PDF inhabilitado y callback directo rechazado para partida sin finalizar |
| Coincidencia jugador/administrador | `ValidarResumen` compara gamificación con ServicioDesempeno y con decisiones/puntos/eventos leídos de la base temporal, filtrados por partida y jugador |
| PDF y cierre de sesión | `ConsultaPartidasPermisosYPdf` genera archivo real, verifica cabecera/EOF/datos y ruta temporal, registra reporte, cierra sesión y regresa a Inicio. `AuditoriaPdfConNombreMaximoDesdePanelReal` verifica generación estructural y exporta la evidencia del defecto UX-05 |
| Integridad ante fallos simulados | `UnFalloDeEscrituraNoConfirmaRespuestaNiHito`, `FalloDeEscrituraConservaPerfilYArchivoPrevios`, `FalloAlFijarAparienciaSeReintentaAntesDeContinuar`, `UnaReglaRotaRevierteTodaLaTransaccion` y `UnArchivoDanadoSeApartaYSeUsaLaCopia`. No equivalen a pruebas de corte eléctrico o fallo físico de disco |

## Resultados de Unity MCP

| Ejecución | Job ID | Aprobadas | Fallidas | Omitidas | Duración de tests |
| --- | --- | --- | --- | --- | --- |
| Base EditMode antes de editar | `7abed9870bd247d5ae6a2c63df682423` | 79 | 0 | 0 | 12.55 s |
| Base Play Mode antes de editar | `0e962753c6d541dd87c4e4db3b4e5129` | 28 | 0 | 0 | 81.52 s |
| Reproducción de UX-01 a UX-04 | `f1fe789f456f46bba3336abc89317157` | 0 | 5 | 0 | 12.55 s |
| Regresión dirigida después de corregir | `bf024eefc8de44c39504904fb46eeeb9` | 5 | 0 | 0 | 12.25 s |
| Final Play Mode completo | `d959b58078e04459818dacce36ac4097` | 34 | 0 | 0 | 91.33 s |
| Final EditMode completo | `c3b079540e4847f486b947ff5924bfb1` | 79 | 0 | 0 | 11.70 s |

Hubo además un intento de filtro con nombres incompletos, job `9e580c6fcd3b401ea666f273e166241a`, que seleccionó cero tests. No se cuenta como verificación. Se repitió con nombres completos y cinco casos ejecutados.

Los 34 casos Play Mode son los 28 previos más cinco regresiones de UI y un caso de generación de PDF con nombre máximo. El último caso aprueba la generación y selección de datos, **no aprueba la composición visual del PDF**, que falló en la revisión independiente.

Después de las suites finales, Console consultada mediante MCP: **0 errores y 0 advertencias**. Durante recargas del dominio apareció una advertencia de infraestructura ya observada: `MCP-FOR-UNITY: [WebSocket] Unexpected receive error: WebSocket is not initialised`. Se separa de los defectos del producto. La excepción de UX-01 corresponde a la ejecución anterior a su corrección. Estado final consultado: `playing=false`, `compiling=false`, `scriptCompilationFailed=false`, versión 6000.6.7f1.

## Revisión visual y evidencia

Las pruebas fijan el tamaño real de Game View y verifican `Screen.width/height` antes de capturar. Se inspeccionaron Inicio, Registro, Partida, Resultado, AdminLogin y desempeño administrativo en 1920x1080 y 1280x720. También se inspeccionaron selección con volante, historial, apariencia recuperada y el estado de error.

En las capturas revisadas después de corregir se conservan verdes bosque/musgo/oliva, crema, acentos cálidos, tipografía pixel y de lectura, paneles redondeados, camión y mapas visibles. El nombre largo ya cabe sin superponerse a ficha y puntuación. El panel administrativo conserva `Seleccionado: [jugador]`, el volante centrado y `Pulsa Ver desempeño para consultar` antes de abrir el detalle. El detalle usa desplazamiento y las listas e historial usan paginación.

Hover y deshabilitado se comprobaron mediante eventos de puntero, color y escala. La accesibilidad táctil, lectores de pantalla, navegación completa por teclado, contraste de cada combinación dinámica y legibilidad con discapacidad visual no fueron certificados. Las capturas son muestras de estados ejecutados, no una afirmación de que se inspeccionó visualmente cada fotograma o cada alternativa de todo el contenido.

Evidencia relativa a la raíz del repositorio:

- `TestEvidence/AuditoriaV2/PlayMode-final.json` y `EditMode-final.json`: resultados individuales, estados y tiempos devueltos por Unity MCP.
- `TestEvidence/AuditoriaV2/Reproduccion-defectos.json`: fallas iniciales y mensajes del Test Runner.
- `TestEvidence/AuditoriaV2/Antes/`: capturas previas a las correcciones.
- `TestEvidence/AuditoriaV2/Resultado-nombre-largo-1920x1080.png` y `Resultado-nombre-largo-1280x720.png`: resultado corregido.
- `TestEvidence/AuditoriaV2/Registro-validado-*`, `Nombre-literal-1280x720.png`, `Control-deshabilitado-1280x720.png`, `Error-controlado-1280x720.png`.
- `TestEvidence/FlujoJugadorV2/`: Inicio, Registro, Partida, Resultado e Historial en las dos resoluciones, regenerados por la suite.
- `TestEvidence/PersonalizacionV2/`: Registro recuperado y Resultado con snapshot de apariencia en ambas resoluciones.
- `TestEvidence/GamificacionV2/`: variaciones y reconocimientos del motor, sin insignias concedidas.
- `TestEvidence/AdministracionV2/`: Login, selección con volante, desempeño, eventos y PDF de la partida seleccionada. `Nombre-largo-1280x720.png` corresponde al nuevo caso.
- `TestEvidence/AuditoriaV2/Pdf-normal.png`: render independiente del reporte normal generado desde Admin.
- `TestEvidence/AuditoriaV2/Pdf-nombre-largo.pdf` y `.png`: PDF real y render que muestran UX-05. `pdfinfo` confirmó PDF 1.4, una página Letter, no cifrado. Poppler emitió avisos de fuentes de visualización Symbol/ArialUnicode, pero produjo ambos renders.

## Archivos de esta auditoría

| Archivo | Cambio de esta tarea |
| --- | --- |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs` | Datos de UI literales y espacio para nombre completo en Resultado |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Gamificacion.cs` | Evitar consultar de nuevo el servicio que acaba de fallar al presentar Error |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Vista.cs` | Presentación de puntos/progreso no disponibles y texto literal |
| `Assets/Scripts/Mapas/InterfazVisual/RespuestaBotonUI.cs` | Estado visual deshabilitado y limpieza del estado presionado |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.cs` | Clase parcial para reutilizar la fixture, sin duplicar preparación |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.Auditoria.cs` | Cinco casos de regresión de interfaz |
| `Assets/Tests/PlayMode/AdministracionPilotoV2Tests.cs` | Clase parcial para reutilizar la fixture administrativa |
| `Assets/Tests/PlayMode/AdministracionPilotoV2Tests.Auditoria.cs` | Caso del PDF real con nombre máximo |
| `Assets/Mapas/AuditoriaExperienciaPilotoV2.md` | Este informe |
| Metadatos Unity de los archivos nuevos | Importación normal, sin escenas ni atlas nuevos |
| `TestEvidence/AuditoriaV2/` y capturas existentes regeneradas | Evidencias sintéticas, sin bases ni secretos |

## Compatibilidad y datos

V1 sigue predeterminada fuera del piloto. Se ejecutaron pruebas de datos V1 antiguos, coexistencia V1/V2 y ausencia de atributos de apariencia. No se reemplazó el motor, la autenticación ni los cuatro IDs originales. El único ajuste compartido fuera del adaptador piloto es la respuesta visual de botones deshabilitados, sin cambiar su lógica de navegación o permisos.

El piloto usa la misma carpeta temporal por sesión para jugador y administrador, `Application.temporaryCachePath/LifeDrive-FlujoJugadorV2/<sesion>/lifedrive-db.json`. Las pruebas crean sesiones GUID y limpian sus datos después. No se abrió la base real. Los administradores de prueba se provisionan por el mecanismo existente con credenciales aleatorias en memoria, no incluidas en scripts ni capturas.

Se conservan nombre, cédula sintética, personajeId original, partidas, versión y huella de contenido, hitos confirmados, decisiones, tipos de situación, alternativas, consecuencias, puntos, retrasos y eventos. La apariencia usa los eventos versionados ya implementados de perfil y snapshot de partida en la misma base, sin alterar contratos centrales. No se persiste cada fotograma del camión, se reconstruye la presentación desde hitos estables.

## Límites y recomendaciones para el equipo

1. Javier debe corregir UX-05 y ejecutar una regresión PDF con nombre máximo y reportes extensos. No se modificó `GeneradorReportes`, `DocumentoPdf`, autenticación, permisos o almacenamiento central.
2. El equipo debe acordar nueva partida y salida desde el recorrido, y explicar las cuatro fichas sin cambiar sus IDs. Son decisiones pendientes, no funcionalidades añadidas en esta auditoría.
3. Aprobar educativamente V2 antes de probar aprendizaje con usuarios. Validar preguntas, consecuencias y valores con el responsable del contenido. No confundir una ruta ejecutable con contenido aprobado.
4. Jorge debe exponer los indicadores cuando corresponda. La UI conserva los datos y muestra la dependencia, no calcula KPI alternativos.
5. Antes de incorporar usuarios, realizar una sesión observada con personas ajenas a la arquitectura y datos sintéticos. Medir si entienden fichas, iniciar/retomar, consecuencias, progreso, entrega, selección administrativa y desplazamiento del historial.
6. Comprobar físicamente `Abrir PDF` en el visor del sistema y su opción de guardar. Esta auditoría verificó archivo, contenido y render independiente, no abrió el visor nativo mediante el botón. Probar también empaquetado, reapertura del proceso completo y conservación de caché del sistema operativo. Aquí se simuló cierre del controlador/escena y reapertura con la misma carpeta, no el cierre abrupto del ejecutable de Unity.
7. Conservar piloto temporal y acceso explícito. No emplear información personal real ni promoverlo a contenido global hasta cerrar aprobaciones. Preparar el conjunto revisable de cambios locales sin escenas temporales, atlas generados o datos de ejecución. Esta tarea no hizo commit ni push.

El trabajo aporta evidencia funcional y de regresión para S3-10, S4-01 y los planes de calidad/caja negra/caja blanca/unitarias del documento académico. No declara completos S4-04, S4-05 o S4-06 únicamente por su presencia en el documento, ni cierra S3-04 mediante insignias sin reglas aprobadas.
