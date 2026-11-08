# Integración administrativa del piloto V2

7 de noviembre de 2026. Unity 6000.6.7f1. Rama `feat/unity-map`. Prompt 34, alcance S3-08 y contribuciones a S3-10 y S4-01.

## Uso del piloto

1. Fuera de Play Mode abrir **Life Drive > Piloto V2 > Configurar administradores de prueba**. Se abre la misma herramienta de cuentas existente, dirigida explícitamente a la carpeta temporal del piloto y con un título propio.
2. Crear una cuenta exclusiva de prueba, con usuario y contraseña elegidos por la persona que prueba. No hay credenciales predeterminadas. La herramienta aplica la validación y `Contrasenas.Crear` existentes y escribe únicamente sal, hash PBKDF2 e iteraciones, nunca la contraseña.
3. Cerrar la ventana de cuentas y elegir **Life Drive > Piloto V2 > Abrir flujo del jugador**. Si se cambian las cuentas con el piloto abierto, detener y volver a abrir para recargarlas.
4. En Inicio, **Acceso para administradores** cierra cualquier sesión previa y abre AdminLogin. Ingresar autentica con el servicio original. No hay autorregistro desde el juego.
5. Buscar por nombre o prefijo de cédula, elegir jugador, elegir partida y pulsar **Ver desempeño**. Las listas se paginan. El detalle se desplaza con rueda o arrastre para leer todo el historial y los eventos.
6. Para una partida finalizada, **PDF** genera y guarda el reporte. **Abrir PDF** se habilita después de escribirlo correctamente y solicita su apertura con el visor del sistema. Ese visor permite guardar otra copia o imprimir. No hay visor PDF incrustado.
7. **Cerrar sesión** vuelve a Inicio. Salir de Play Mode destruye el adaptador. La siguiente apertura usa la misma carpeta, pero comienza sin sesión administrativa autenticada.

## Escenas y composición

Se conectaron `Inicio.unity`, `AdminLogin.unity` y `Admin.unity` dentro de la activación explícita del piloto. No se guardaron cambios en esas escenas ni se agregaron escenas al perfil de compilación. Los adaptadores enlazan las referencias ya serializadas. Los controladores DEMO y sus snapshots quedan desactivados antes de Start en este modo.

AdminLogin conserva el diseño, los campos de usuario y contraseña enmascarada y el feedback visual aprobado. Admin conserva **Seleccionado: [jugador]**, el volante pixel art centrado y **Pulsa Ver desempeño para consultar**. El volante es originalmente hijo del texto Detalle. Durante el piloto se mantiene su mismo marco de coordenadas en un soporte independiente para que el desplazamiento del detalle no lo oculte. No se eliminó ni sustituyó su arte. Se reutilizan fuentes, paneles redondeados y botones con RespuestaBotonUI. Solo se reorganiza la lista izquierda para incorporar partidas y paginación.

La previsualización fuera del piloto conserva su comportamiento anterior. Esta entrega no convierte esa maqueta en la administración de producción ni activa V2 globalmente.

## Servicios y permisos

Se utiliza exactamente el `ServicioJuego` que mantiene `FlujoJugadorPilotoV2`. No se usa `Sesion.Juego`, ni se abre otra base para la administración.

- `ServicioUsuarios.IniciarSesionAdministrador` verifica las cuentas mediante `Contrasenas.Verificar`. La presentación limpia la contraseña tras cada intento. Antes de intentar autenticarse cierra la sesión previa para impedir que un intento fallido conserve privilegios anteriores.
- `ServicioUsuarios.Exigir` protege la entrada y las acciones del panel. Admin se oculta antes de su primer render si el rol no es administrativo y redirige a AdminLogin. Si el rol deja de ser administrativo con el panel abierto, se oculta el contenido y se redirige.
- `ServicioDesempeno.ListarJugadores` y `PartidasDe` alimentan las listas. No hay usuarios ficticios serializados usados como fuente del piloto. Los nombres y cédulas visibles proceden de sus registros.
- `ServicioDesempeno.Resumen` proporciona puntos, retraso, estado, ruta lógica, decisiones y consecuencias de la partida seleccionada. Se comprueba que el resumen corresponda al jugador seleccionado. Cambiar jugador, partida o búsqueda limpia el resumen y el PDF anterior.
- `ServicioDesempeno.Eventos` filtra el historial de la partida. Los eventos y decisiones no se reconstruyen a partir de animaciones ni se recalculan sus efectos en UI.
- `ServicioDesempeno.ReportePdf` verifica permisos y finalización y delega en `GeneradorReportes` y `DocumentoPdf`, sin modificaciones en estos generadores.

La lista incluye partidas en curso. Su resumen puede consultarse, con un estado vacío si todavía no tiene decisiones. Su PDF queda deshabilitado y el servicio rechaza la generación incluso al invocar directamente el callback. Los errores de consulta, autenticación y escritura se muestran sin declarar un éxito ficticio.

## Aislamiento y datos

La base sigue siendo `Application.temporaryCachePath/LifeDrive-FlujoJugadorV2/<sesion>/lifedrive-db.json`. El modo manual usa `sesion-local`. Las pruebas generan una sesión por GUID.

El motor carga contenido y cuentas desde una misma carpeta. Para aislar también las cuentas, el piloto prepara una instantánea de `ruta.json` y `ruta-v2-candidata.json` en `<sesion>/Contenido`. **No copia administradores.json de producción**. La cuenta debe aprovisionarse explícitamente en esa carpeta. Si no existe el archivo de cuentas, el servicio carga una lista vacía y no permite ingresar. Las instantáneas de rutas existentes no se reemplazan automáticamente, para conservar el paquete asociado a las partidas guardadas. No se crea una base paralela ni se migra la base real.

Los PDF se guardan en `<sesion>/Reportes/lifedrive-partida-<id>.pdf`. Las rutas temporales pueden ser limpiadas por el sistema. Las pruebas eliminan sus carpetas al finalizar y conservan únicamente evidencias con datos sintéticos en `TestEvidence/AdministracionV2`.

Los registros son reales del motor dentro del entorno piloto. Los sujetos son de prueba y el contenido V2 sigue siendo candidato. No se muestran puntuaciones DEMO ni se concede una clasificación final. Las contraseñas de las pruebas se generan aleatoriamente en memoria, se derivan mediante el mecanismo existente y no aparecen en código, prefabs, capturas ni mensajes de prueba.

## PDF y evidencias

Se generó el PDF de la partida finalizada seleccionada, se verificaron cabecera PDF 1.4, cierre EOF, identidad, ID de partida y puntos, y se comprobó que no incluya al otro jugador. Poppler confirmó un documento carta válido de una página y se inspeccionó su render. Los generadores existentes conservaron su implementación. El renderizador emitió avisos locales sobre fuentes de sustitución Symbol/ArialUnicode, sin impedir el render legible.

Evidencias: AdminLogin, Seleccion, Desempeno y Eventos a 1920×1080 y 1280×720. Incluyen la composición aprobada y el detalle desplazado hasta los eventos finales. Hay PDF sintéticos de evidencia y un PNG de su render. La apertura con un visor externo es una acción ofrecida por el botón y no se automatizó durante las pruebas para evitar abrir aplicaciones ajenas al ejecutor. No se certifica una vista previa incrustada ni un build distribuido.

## Archivos de esta entrega

| Archivo | Cambio |
| --- | --- |
| `Assets/Scripts/IntegracionV2/AdministracionPilotoV2.cs` | Nuevo adaptador de las escenas administrativas, permisos, listas, selección, resumen, eventos y escritura/apertura del PDF. |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs` | Preparación del contenido/cuentas aislados, enlace administrativo por escena y acceso desde Inicio. No cambia el registro ni la apariencia. |
| `Assets/Scripts/Editor/VentanaAdministradores.cs` | La herramienta existente admite una ventana explícitamente dirigida al piloto. Conserva el mecanismo de creación y validación. Su destino queda serializado para sobrevivir recargas del Editor. |
| `Assets/Editor/InterfazVisual/FlujoJugadorPilotoV2Editor.cs` | Menú para configurar las cuentas de prueba. |
| `Assets/Tests/PlayMode/AdministracionPilotoV2Tests.cs` | Pruebas nuevas con cuentas efímeras, tres jugadores y cuatro partidas, en dos resoluciones. |
| `Assets/Mapas/IntegracionAdministrativaV2.md` | Este informe. |
| `Assets/Mapas/FlujoJugadorIntegradoV2.md` | Enlaces al estado administrativo actualizado. |
| `TestEvidence/AdministracionV2` | Capturas y PDF de pruebas sin datos reales. |

Los archivos nuevos de Assets tienen .meta. Se conservaron los cambios previos de fuentes y configuración y se respaldaron los archivos antes de implementar. No se modificaron motor, Datos, reglas de permisos, generadores PDF, mapas, camión, personalización, contenido activo ni Build Profiles. No se hizo commit ni push.

## Pruebas ejecutadas mediante Unity MCP

| Ejecución | Aprobadas | Fallidas | Omitidas | ID |
| --- | ---: | ---: | ---: | --- |
| EditMode antes de editar | 59 | 0 | 0 | `7ef14f5b9e54448ca017a57264fe29db` |
| Play Mode antes de editar | 18 | 0 | 0 | `7d0e767d87b747fab212b1bfff3777d3` |
| Primer ensayo administrativo | 2 | 2 | 0 | `e5677651ed83457596f51fc0f3879fe0` |
| Administración corregida | 4 | 0 | 0 | `01de75e8222f4943883d979f2c0835e3` |
| Regresión con fallo de preparación de capturas | 20 | 2 | 0 | `267be30224314effaafb23b97bfb6b58` |
| Play Mode final, con regresión completa | 22 | 0 | 0 | `d3ea1067e063446e81b61683386576dc` |
| EditMode final | 59 | 0 | 0 | `63bd8064757a41b7aaaa0fdaf1f2ac65` |

El primer ensayo detectó el volante oculto por su dependencia del objeto Detalle. Se corrigió separando los contenedores sin alterar la composición. La prueba inicial de regresión completa detectó después que el fixture reutilizaba el ancho de una prueba anterior al capturar Login. Se restableció esa variable al preparar cada caso. Los intentos fallidos se conservan en este informe y no cuentan como pruebas aprobadas.

Se verifican credenciales correctas e incorrectas, usuario inexistente, permisos de jugador/anonimato, bloqueo al entrar directamente en Admin, revocación con el panel abierto, cierre de sesión, reapertura de la misma carpeta sin sesión heredada, búsqueda por nombre y cédula, selección de dos partidas distintas del mismo jugador y otra de un jugador diferente, limpieza al cambiar selección, estados vacíos, coincidencia con decisiones persistidas, PDF finalizado y rechazo en curso. La regresión del jugador incluye los ocho recorridos con pantallas, transiciones y reanudación.

Resultado final: 81 pruebas aprobadas, cero fallidas y cero omitidas en las suites finales. Console final: cero errores y una advertencia de conexión de Unity MCP, `[WebSocket] Unexpected receive error: WebSocket is not initialised`. Esa advertencia corresponde al transporte del plugin y se conservó visible. No hubo advertencias de compilación ni errores funcionales del piloto en las suites finales.

Se verificó además la apertura de la herramienta de cuentas del piloto desde su comando y que el destino pertenece a temporaryCachePath. Se cerró sin introducir ni guardar credenciales. La comparación SHA-256 contra el respaldo conservó las escenas, mapas, camión, contenido activo, motor, generadores y fuentes personalizadas previas. La ampliación automática del atlas FuenteInterfaz.asset se revirtió desde el respaldo exacto de esta sesión. El ejecutor restauró la ejecución en segundo plano a false y Unity quedó fuera de Play Mode.

## Dependencias y cierre de S3-08

S3-08 queda conectado y verificado en el piloto de Editor para autenticación, listas, selección de partidas, desempeño y generación de PDF. S3-10 recibe trazabilidad del registro y de los reportes, y S4-01 recibe pruebas de integración de estas pantallas con los servicios. Esto no certifica todos los requisitos académicos ni activa el módulo en producción.

Javier: acordar incorporación al flujo de producción/build, política de cuentas por entorno, conservación o publicación de paquetes y tratamiento operativo de caché/reportes. No hizo falta sustituir sus servicios ni cambiar sus contratos de datos. La personalización persistente sigue siendo una dependencia separada.

Jorge: el servicio todavía no expone TDS, IEE, PPS ni PCRS. La UI lo indica como pendiente y no calcula ni inventa valores. Los datos históricos disponibles se conservan para sus fórmulas.

Para cerrar S3-08 más allá del piloto faltan aprobación funcional y visual del equipo, operación de cuentas y reportes en el entorno definitivo y validación de un ejecutable distribuido. También siguen pendientes la aprobación educativa de V2 y las reglas de KPI, reconocimientos y categorías finales. V1 permanece predeterminada.
