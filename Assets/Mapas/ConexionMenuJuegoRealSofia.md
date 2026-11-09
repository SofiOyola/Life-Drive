# Conexión del menú visible con el juego real

8 de noviembre de 2026. Prompt 40. Unity 6000.6.7f1. Rama `feat/unity-map`.

## Diagnóstico reproducido

La escena abierta normalmente era `Assets/Scenes/Inicio.unity`. Inicio y Registro tenían `PersonalizacionRegistroUI`, cuyo `Start` conectaba botones de previsualización. JUGAR cargaba Registro, pero su botón VER MI PERSONAJE solo validaba el formulario para mostrar el avatar. No llamaba a ServicioUsuarios, no iniciaba una partida y no tenía una continuación al motor. Los controladores originales PantallaInicio y PantallaRegistro no estaban instalados en esas escenas rediseñadas.

El comando Life Drive > Piloto V2 > Abrir flujo del jugador creaba antes de cargar Inicio un adaptador persistente, `FlujoJugadorPilotoV2`. Ese adaptador desactivaba la demostración y sustituía los eventos de las mismas pantallas por llamadas reales. Por eso los ensayos del lanzador pasaban mientras el JUGAR normal seguía mostrando una maqueta. No eran dos motores diferentes, sino dos formas de entrar en las mismas interfaces.

La configuración de compilación también estaba incompleta. Solo incluía SampleScene, cuya ruta no existía. Tampoco existe una escena Partida.unity en este repositorio. La escena funcional que ya contiene el camión y el reproductor es PruebaIntegracionV2.unity.

Se agregaron primero seis casos que abrían Inicio directamente sin activar el adaptador. Los seis fallaron por ausencia del flujo normal. Evidencia: `TestEvidence/EntradaNormal/Desconexion-reproducida.json`, trabajo MCP `bd29bf427a1648f0bf9c013fc8dd5f96`. Ese fallo reproduce el problema reportado, no un fallo del motor.

## Entrada y pantallas conectadas

Inicio y Registro incorporan ahora un objeto no visual con `EntradaJuego`. Su Awake selecciona el servicio V1 original y crea el adaptador común antes de los Start de previsualización. Si ya existe un adaptador explícito, lo conserva. Así sigue funcionando el lanzador anterior, aunque ya no es necesario para entrar desde Inicio.

JUGAR abre Registro. COMENZAR RECORRIDO valida y registra mediante ServicioUsuarios. Después se carga PruebaIntegracionV2 con el mismo servicio y jugador. Esa escena se reutiliza para V1 y V2, sin duplicar escenas, recursos ni formularios. La entrega animada precede al acceso a Resultado. Volver a Inicio cierra la sesión mediante el servicio existente. El menú de partida permite regresar y retomar las decisiones ya guardadas.

Acceso para administradores abre AdminLogin y después Admin con el mismo servicio y almacenamiento de la versión seleccionada. Se conserva el login original, sus permisos y el volante de la interfaz administrativa. No se incluyen cuentas ni contraseñas en escenas o código. Las cuentas de ensayo se aprovisionan con el mecanismo original sobre carpetas aisladas.

Las seis escenas habilitadas en EditorBuildSettings, en orden, son Inicio, Registro, PruebaIntegracionV2, Resultado, AdminLogin y Admin. Inicio queda como primera escena del ejecutable. No se agregan las demostraciones al flujo del producto.

## Contenido vigente V1

La entrada normal utiliza Sesion.Juego. Sin parámetros de ensayo, el guardado sigue en Application.persistentDataPath, según el contrato original. El contenido activo sigue siendo StreamingAssets/Contenido/ruta.json. Iniciar, retomar, decidir y crear otra operación después de finalizar usan ServicioPartidas. Resultado y administración consultan ServicioDesempeno. La interfaz no vuelve a evaluar ni a sumar puntos.

Los datos reales que llegan al motor son nombre, documento y uno de los cuatro personaje_id originales. Para una identidad ya registrada, se conserva el ID devuelto por ServicioUsuarios. El formulario conserva la personalización Hombre/Mujer y las paletas aprobadas.

**Limitación espacial de V1:** sus tramos lógicos convergen. Hay tres recorridos posibles: 1→2→4, 1→2→3→4 y 1→3→4. El catálogo aprobado de dieciséis fondos corresponde al árbol V2, que no converge. No hay un contrato vigente que asocie los cuatro tramos V1 a esos fondos y sus salidas. No se inventó esa correspondencia ni se cambió el motor para adaptarlo al árbol V2.

La integración V1 presenta las preguntas y consecuencias reales en su orden original, muestra el nombre del tramo lógico y mueve automáticamente el camión sobre S0 como referencia. El encabezado dice V1 · RUTA CLÁSICA · VISTA DE REFERENCIA. Después de la última consecuencia presenta el desplazamiento en DELIVERY y el resultado real. El avance educativo se expresa como número de situaciones resueltas, sin un porcentaje ficticio de ruta intermedia. Esta conexión permite terminar la operación, pero no certifica una representación geográfica ramificada de V1.

Javier y el equipo deben definir la asociación visual de los tramos V1, sus salidas legales y sus hitos si se necesita una representación fiel de esas bifurcaciones. No se modificaron asociaciones, mapas, trayectorias ni cálculos para forzarla.

La apariencia V1 se conserva durante la navegación de la sesión en memoria. El formulario y Resultado indican esa limitación. No se escriben eventos PILOTO_APARIENCIA en datos de producción. La persistencia definitiva de esos atributos para V1 sigue dependiendo del contrato de Javier. El personaje_id original sí se guarda mediante el servicio existente.

## Acceso explícito V2 dentro de la aplicación

En el Editor y en compilaciones de desarrollo, Inicio ofrece MODO PILOTO V2. Al pulsarlo se cambia explícitamente al entorno temporal existente `Application.temporaryCachePath/LifeDrive-FlujoJugadorV2/sesion-local`. Se copia solo el contenido candidato cuando todavía no existe en esa carpeta. No se copian cuentas administrativas reales. El aviso identifica contenido candidato y datos temporales. Para volver, Inicio ofrece Volver al contenido V1.

El botón no se crea en una compilación final. No hay activación global de V2 y JUGAR sigue comenzando en V1. El menú del Editor queda como herramienta adicional.

V2 usa las ocho rutas reales, las respuestas y el guardado ya existentes. El nombre histórico de la escena PruebaIntegracionV2 no implica que el flujo V1 seleccione el paquete candidato. La selección depende del servicio inyectado y de la versión persistida, no del nombre de la escena.

V2 reutiliza PersistenciaAparienciaPilotoV2 y AvatarPersonalizableUI. Guarda presentación y variantes válidas de piel, ojos, cabello y traje en los eventos versionados de la misma base temporal. No cambia los cuatro IDs. Los puntos, decisiones, consecuencias, eventos y progreso siguen consultándose al motor. No se otorgan insignias sin reglas ni se calculan categorías finales o KPI.

## Aislamiento de pruebas

Las pruebas Editor abren Inicio directamente mediante carga de escena y asignan antes una carpeta GUID con EntradaJuego.CarpetaPruebas. No invocan Activar ni el menú del piloto. Se copia ruta.json a una carpeta de ensayo sin usuarios originales. La selección visible V2 usa otra carpeta GUID del piloto, con el mismo motor y formato de persistencia. Se verifica que los jugadores V2 no aparecen en la base V1 de ensayo al regresar.

Una compilación de desarrollo admite `--lifedrive-test-dir <carpeta>` para utilizar un directorio de ensayo explícito. Una compilación final ignora ese parámetro. No se utiliza almacenamiento de jugadores reales en la validación.

EnsayoEntradaEjecutable es una comprobación optativa que solo se activa fuera del Editor, en un ejecutable de desarrollo, con `--lifedrive-smoke` y una carpeta de ensayo. Utiliza los botones visibles y el motor original para terminar una operación V1. No crea una autenticación alternativa. Produce smoke-result.json y termina el proceso con el código correspondiente. Sin ambos parámetros no ejecuta la prueba.

## Archivos de esta tarea

Nuevos:

- Scripts/IntegracionV2/EntradaJuego.cs y su meta.
- Scripts/IntegracionV2/FlujoJugadorPilotoV2.Producto.cs y su meta.
- Scripts/IntegracionV2/PruebaIntegracionV2.Clasica.cs y su meta.
- Scripts/IntegracionV2/EnsayoEntradaEjecutable.cs y su meta.
- Tests/PlayMode/FlujoJugadorPilotoV2Tests.EntradaNormal.cs y su meta.
- Tests/PlayMode/AdministracionPilotoV2Tests.EntradaNormal.cs y su meta.
- Este documento y su meta. Evidencias sintéticas en TestEvidence/EntradaNormal.

Modificados sobre sus versiones locales anteriores:

- Scenes/Inicio.unity y Scenes/Registro.unity, entrada no visual y textos conectados.
- Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs y FlujoJugadorPilotoV2.Navegacion.cs, selección e integración comunes.
- Scripts/IntegracionV2/PruebaIntegracionV2.cs, PruebaIntegracionV2.Vista.cs y PruebaIntegracionV2.Gamificacion.cs, presentación V1 con llamadas originales.
- Scripts/IntegracionV2/AdministracionPilotoV2.cs, textos según entorno.
- Scripts/IntegracionV2/TecladoPilotoV2.cs, foco sobre la selección explícita del piloto.
- ProjectSettings/EditorBuildSettings.asset, punto de entrada y seis escenas funcionales.

Los cambios anteriores de los prompts 38 y 39 permanecen. Se tomó un inventario de estado y hashes antes de editar en Library/Prompt40Checkpoint. No se hizo commit ni push. HEAD inicial: bd9de07202369b76389527108a6012663e34a062.

## Validación

Línea base ejecutada por Unity MCP antes de modificar código: 79 EditMode y 48 Play Mode aprobadas. Trabajos d1f6d455da3a499f891292e4290a7371 y 896529195890492eb3fda0912448b1a5. Dos solicitudes EditMode anteriores no llegaron a ejecutar casos porque el Editor seguía en Play. Se detuvo Play y se obtuvo la línea base real.

Los nuevos dieciséis casos comprueban seis operaciones V1, correspondientes a sus tres rutas en ambas resoluciones, ocho rutas V2 seleccionadas desde Inicio y dos accesos administrativos V1 desde Inicio. Incluyen formulario inválido, doble clic, conservación del personaje original, retorno y reanudación sin nuevas decisiones, puntos guardados, llegada visual a DELIVERY y Resultado. Las ocho rutas V2 alternan las dos resoluciones. Los otros 48 casos mantienen cobertura de reanudación, apariencia, aislamiento, permisos, PDF y accesibilidad del piloto.

Se detectó y corrigió en la primera conexión un catálogo no serializado en EntradaJuego. Antes de corregirlo pasaban los seis casos V1 y los dos administrativos, y fallaban los ocho V2. El ensayo dirigido posterior pasó los ocho. También se corrigió una aserción de rutas de pruebas que comparaba separadores Windows sin normalizarlos. No se contabilizan esas ejecuciones como aprobadas.

Resultado final mediante Unity MCP:

| Tipo | Aprobadas | Fallidas | Omitidas | Evidencia |
| --- | ---: | ---: | ---: | --- |
| EditMode | 79 | 0 | 0 | EditMode-final.json, trabajo 2dfc9f92d7374d41bd138a762833d00f, 13,70 s |
| Play Mode | 64 | 0 | 0 | PlayMode-final.json, trabajo 4dfceb81c97446468fb4258f5e4281fd, 200,39 s |
| Ejecutable Windows de desarrollo, recorrido V1 | 1 | 0 | 0 | Ejecutable-V1.json y Ejecutable-V1.log |

Las capturas de Inicio, entrega y Resultado V1 se guardan con dimensiones verificadas en TestEvidence/EntradaNormal, en 1920×1080 y 1280×720. Se revisaron visualmente esos estados y el panel administrativo. Las imágenes administrativas están en TestEvidence/AdministracionV2/Entrada-normal-V1-*.png. No son pruebas humanas de usabilidad.

Los 64 casos incluyen los 48 de regresión y los dieciséis nuevos. Las ocho rutas V2 entran por el botón visible, responden realmente y consultan Resultado. No se atribuye esa verificación a un ejecutable V2: sus ocho rutas se ejecutaron en Play Mode.

## Compilación y ejecución independiente

Se generó `Builds/Prompt40Development/LifeDrive.exe` para StandaloneWindows64, con seis escenas y BuildOptions.Development. Resultado Succeeded, cero errores, tres advertencias y 87,07 s. El primer intento fue rechazado porque Unity prohíbe colocar el build dentro de Library. La salida se corrigió a Builds. El informe válido está copiado en TestEvidence/EntradaNormal/Build-Windows.json.

Advertencias del build: una sobre cambios pendientes de compilación de código de importación o posprocesamiento y dos desconexiones WebSocket del conector MCP durante el proceso. Se conservan literalmente en el informe. El runtime del jugador sí fue compilado por BuildPipeline y después ejecutado. No se presenta el build como libre de advertencias.

Se inició el ejecutable con `-batchmode -nographics --lifedrive-test-dir <carpeta GUID> --lifedrive-smoke -logFile <registro>`. El proceso finalizó con código 0. La evidencia confirma Inicio > Registro > PruebaIntegracionV2 > Resultado, contenido_version 1, personaje_id 3, seis decisiones y -15 puntos, iguales a la suma del historial guardado. El usuario de ensayo fue Ensayo ejecutable, documento 99004004. El JSON de base quedó exclusivamente en la carpeta GUID de Builds, fuera de los datos de producción.

El registro del ejecutable muestra un fallo DNS de los servicios cloud de Unity y un mensaje de liberación de ComputeBuffer al cerrar el proceso. No impidieron el recorrido. Se mantienen como observaciones del runtime, sin cambiar paquetes, servicios cloud o motor fuera del alcance.

La ejecución fue automatizada y sin gráficos. No certifica interacción humana, renderizado de escritorio, ocho rutas V2 en binario, ni una compilación final sin Development. La ocultación del botón piloto en compilación final se implementa mediante Application.isEditor y Debug.isDebugBuild, pero no se generó un segundo build final.

Console estuvo sin errores ni advertencias al terminar las 64 pruebas Play Mode. Los mensajes posteriores del intento de build y del conector se registran por separado. No hubo errores de compilación de scripts. Los atlas de fuente generados durante las pruebas y el ajuste automático de UnityConnectSettings del build se excluyeron devolviendo esos archivos a sus bytes exactos del inventario inicial, que conserva también las modificaciones locales anteriores. La escena InitTestScene8574c1eb-c122-4e71-b794-51110fac0430 ya existía antes de esta tarea y no se eliminó ni se incorporó al build.

Después del build, el último refresco de Assets agotó el tiempo de espera del conector. Las lecturas posteriores de Console devolvieron sesión no preparada y ping sin respuesta. El registro Editor.log muestra pérdida de keep-alive y cierre WebSocket de MCP. No se pudo completar otra lectura final de Console ni recuperar la conexión mediante control de escritorio, cuyo servidor no estaba disponible. Esta limitación posterior se distingue de las suites y del ejecutable ya terminados. Console-build.json conserva el historial que sí se pudo consultar. Antes de una nueva sesión de validación será necesario restablecer MCP en Unity.

## Pendientes antes de usuarios

- Javier y equipo: contrato visual V1 para sus tramos y persistencia definitiva de apariencia fuera del piloto.
- Equipo educativo: aprobación del paquete V2. El piloto conserva su identificación de candidato.
- Sofía: evaluación humana de comprensión, teclado y legibilidad en equipos de destino. Una prueba automatizada de botones no sustituye esa evaluación.
- Equipo: provisión autorizada de cuentas y política de tratamiento de datos antes de usar identidades reales. Las pruebas aquí usan nombres y documentos sintéticos.
- Jorge: indicadores y reglas aprobadas de clasificación o reconocimientos pendientes, sin fórmulas nuevas en esta tarea.

No se alteraron ruta.json activo, motor, autenticación central, generación PDF, indicadores, fondos, camión ni recursos originales de personajes.
