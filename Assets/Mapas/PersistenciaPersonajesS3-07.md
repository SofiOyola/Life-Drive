# Persistencia de personajes S3-07 en el piloto V2

7 de noviembre de 2026. Unity 6000.6.7f1. Rama `feat/unity-map`. Prompt 36.

## Resultado y uso

La apariencia del piloto ya se guarda y recupera en el mismo `lifedrive-db.json` temporal que usan el jugador y la administración. No se crea una base independiente, un archivo auxiliar de perfiles ni una migración del modelo de Javier.

En Registro se puede personalizar antes de **Iniciar / retomar V2**. Para recuperar visualmente una configuración existente, escribir el nombre y la cédula ficticios y pulsar **Recuperar apariencia**. Se usa RegistrarJugador para validar la identidad existente antes de consultar su perfil. Recuperar no crea usuarios nuevos ni guarda valores por defecto.

También se conserva automáticamente un perfil existente al iniciar desde un formulario nuevo sin cambios visuales. La configuración inicial de una partida queda fijada una sola vez. Si después cambia el perfil del jugador, esa partida mantiene su apariencia original al reanudar y en Resultado.

La personalización no añade un conductor dibujado sobre el camión: el recorrido cenital no tiene esa representación. El adaptador mantiene la configuración de la partida durante la navegación y Resultado reconstruye el retrato con AvatarPersonalizableUI. No se cambian personajes, sprites, ropa, fondos ni trayectorias.

## Representación explícita y variantes

El formato V1 contiene los siguientes campos:

| Campo | Valores o significado |
| --- | --- |
| `version_formato` | 1. Versión de apariencia, independiente de la versión del contenido educativo. |
| `personaje_id` | Uno de los cuatro IDs originales, validado contra el jugador registrado y el contenido existente. |
| `presentacion` | `hombre` o `mujer`. No sustituye ni se infiere del personaje_id. |
| `piel` | `marfil`, `arena`, `canela`, `cacao`, `ebano`. |
| `ojos` | `cafe`, `verde`, `azul`, `miel`, `gris`. |
| `cabello` | `negro`, `castano`, `cobre`, `rubio`, `plata`. |
| `traje` | `musgo`, `azul`, `terracota`, `crema`, `ciruela`. |

Ejemplo de datos sintéticos:

```json
{"version_formato":1,"personaje_id":3,"presentacion":"mujer","piel":"canela","ojos":"miel","cabello":"plata","traje":"terracota"}
```

Los identificadores de variantes se resuelven mediante una tabla V1 fijada a los veinte colores ya aprobados en Registro. No se guardan valores RGBA arbitrarios. Al aplicar la configuración en Registro se comprueba que sus paletas serializadas todavía coincidan con esa tabla. Si la paleta cambia, se informa que hace falta revisar el contrato visual antes de guardar. El retrato utiliza las mismas capas procedurales de AvatarPersonalizableUI. No se generan otros personajes.

## Extensión compatible sobre el guardado existente

La inspección confirmó que RegistroEventos.Registrar acepta un tipo de texto y un detalle, y que BaseDatos valida el tipo no vacío y las referencias de jugador/partida sin imponer un catálogo cerrado de tipos. El piloto usa ese punto de extensión existente. No modifica TipoEvento, Jugador, Partida, BaseDatos, RegistroEventos, ServicioUsuarios ni ServicioPartidas.

Se agregan exclusivamente desde el adaptador explícito del piloto:

- `PILOTO_APARIENCIA_PERFIL_V1`: configuración actual del jugador. El evento conserva jugador_id y lleva partida_id=0. Su detalle es el JSON del formato anterior.
- `PILOTO_APARIENCIA_PARTIDA_V1`: copia de la configuración fijada a una partida V2. El evento conserva las referencias originales de jugador y partida y el mismo JSON versionado.

La ruta sigue siendo `Application.temporaryCachePath/LifeDrive-FlujoJugadorV2/<sesion>/lifedrive-db.json`. La sesión manual usa `sesion-local`. Los registros se insertan mediante la transacción y el escritor atómico originales, con su archivo temporal y respaldo habituales. No se escribe directamente ese JSON desde la UI.

El perfil se agrega solamente si cambia su configuración válida. Repetir el mismo guardado no añade otro evento. Una partida que ya tiene configuración fijada devuelve esa copia sin generar otra ni reemplazarla por el perfil actual. Las lecturas tampoco escriben. El esquema de la base no se altera por esta extensión y la selección V1/V2 conserva su comportamiento anterior.

Guardar y fijar apariencia exige el rol jugador y, para una partida, la propiedad de esa partida mediante los permisos existentes. Leer el retrato de una partida utiliza la autorización de ServicioDesempeno.Resumen. No se confía en un jugador_id introducido por la interfaz.

El personaje_id de una cédula ya registrada sigue siendo el que devuelve ServicioUsuarios. Si se solicita otra ficha en el formulario, la personalización guarda el ID original devuelto. El color y la presentación nunca modifican ese dato.

## Reanudación, usuarios antiguos y errores

- Un formulario recién abierto comienza con su previsualización base. Si no se editó su apariencia y existe un perfil guardado, Iniciar / retomar lo conserva. El botón Recuperar apariencia permite verlo antes de continuar.
- Los clics en las variantes cuentan como edición explícita, incluso si se elige volver a los colores base. Las llamadas de reconstrucción no se consideran edición del usuario.
- Al cerrar y abrir el adaptador se reconstruye todo desde eventos persistidos. No se depende del diccionario de colores en memoria que existía antes.
- Un usuario o partida anterior sin eventos de apariencia sigue siendo válido. Leer ese caso devuelve ausencia, sin guardar un valor por defecto. Registro permite configurar y guardar una apariencia por primera vez. Resultado admite una base visual claramente marcada como sin guardar cuando no existe configuración fijada.
- No es posible recuperar la apariencia perdida de sesiones anteriores a esta función si nunca fue guardada. No se inventa una selección histórica. Si se guarda un perfil al retomar una partida antigua sin copia propia, su primer snapshot disponible corresponde a ese momento, no a una apariencia histórica recuperada.
- Una versión futura, una variante desconocida, un JSON dañado o un personaje_id discrepante producen un error controlado. No se sobrescribe el perfil con valores por defecto para forzar la continuación.
- Si falla la escritura, la transacción original conserva el perfil y el archivo previos. El error se muestra y no se anuncia un guardado exitoso.

El registro del jugador y el guardado de apariencia son operaciones sucesivas del motor. No se creó una transacción central nueva que combine ambas. Si falla el segundo paso, el jugador puede haber quedado registrado, pero la UI no avanza como si hubiera guardado la apariencia. El reintento puede recuperar ese jugador.

V1 fuera del piloto conserva sus controladores y sus cuatro personajes. No se cambia ruta.json ni se activa V2 por defecto. Las pruebas de compatibilidad corresponden al lector actual y a datos sin atributos adicionales, no a ejecutar un binario antiguo del equipo.

## Archivos de esta entrega

| Archivo | Trabajo |
| --- | --- |
| `Assets/Scripts/Mapas/InterfazVisual/AparienciaJugadorPilotoV1.cs` | DTO versionado, IDs de variantes, colores aprobados, validación y reconstrucción con componentes existentes. |
| `Assets/Scripts/Mapas/InterfazVisual/PersistenciaAparienciaPilotoV2.cs` | Extensión aislada que guarda perfiles y configuraciones por partida mediante eventos y transacciones existentes. |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.Apariencia.cs` | Recuperación en Registro y protección contra la sobrescritura accidental por valores iniciales. |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs` | Composición de la extensión, retirada del diccionario volátil, uso de la copia guardada en Resultado y textos de estado. |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.cs` | Callback opcional del adaptador al abrir o restaurar una partida, para fijar o reintentar su configuración cuando su ID ya existe. No cambia la evaluación ni el registro de decisiones. |
| `Assets/Tests/EditMode/PersistenciaAparienciaPilotoV2Tests.cs` | Nueve pruebas nuevas de variantes, reapertura, compatibilidad, permisos y fallos de escritura. |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.cs` | Cuatro casos nuevos: recuperación completa en dos resoluciones, usuario antiguo sin apariencia y reintento tras fallo al fijar la configuración. Mantiene los ocho recorridos anteriores. |
| `Assets/Mapas/FlujoJugadorIntegradoV2.md` | Actualiza el estado antes volátil de la apariencia y enlaza este contrato. |
| `Assets/Mapas/PersistenciaPersonajesS3-07.md` | Este informe. |
| `TestEvidence/PersonalizacionV2` | Capturas de Registro recuperado y Resultado en 1920×1080 y 1280×720. |

Los archivos nuevos de Assets tienen .meta. Las escenas Registro y Resultado no se reescriben. El botón y el retrato se enlazan en tiempo de ejecución reutilizando los componentes aprobados. No se modifica la lógica administrativa, los indicadores de Jorge, la gamificación ni el motor de evaluación. Los eventos de apariencia son metadatos, no decisiones educativas, puntos o buenas prácticas.

## Pruebas ejecutadas mediante Unity MCP

Todas las pruebas usan nombres/cédulas sintéticos y carpetas temporales por GUID. No se utiliza la base real del usuario. Las cuentas administrativas de la regresión siguen siendo efímeras y aisladas.

| Ejecución | Aprobadas | Fallidas | Omitidas | ID |
| --- | ---: | ---: | ---: | --- |
| Línea base EditMode | 70 | 0 | 0 | `840ffc1942aa40caba1b259f5e5c1ac5` |
| Línea base Play Mode | 24 | 0 | 0 | `2275cd079e054474a2c8092f5611af58` |
| Persistencia de apariencia EditMode | 9 | 0 | 0 | `fc84a848a13e4444b3fbff6cc06e4dc2` |
| Play Mode completo | 27 | 0 | 0 | `d889dae74ec5442587ffd9687d2c60f6` |
| EditMode completo antes del último ajuste | 79 | 0 | 0 | `f173de95c0fc4f1b85a4f77220dcd089` |
| Play Mode final con reintento de escritura | 28 | 0 | 0 | `6b5b995338604db890ef724896f30266` |
| EditMode completo final | 79 | 0 | 0 | `5a38db15eea643c5894a3f5224fa4bab` |

Se probaron los cuatro IDs originales con distintas combinaciones de presentación y colores, reapertura desde disco, idempotencia de perfil/copia por partida, rechazo de cambio de ID y de formatos inválidos, protección de datos de otra partida y rollback por fallo de escritura.

En Play Mode se guardó una apariencia, se respondió una situación real y se destruyó/reabrió el adaptador sobre la misma carpeta. Se inició desde un formulario sin cambios y solicitando otra ficha: se preservaron el ID y la apariencia anteriores. Después se recuperó el formulario, se comprobó que no hubiera eventos de apariencia duplicados, se cambió el perfil y se terminó la partida. Resultado mantuvo los colores originales fijados a esa partida. También se probó un jugador antiguo y se ejecutaron las ocho rutas, la administración, los permisos y el PDF de la regresión existente. La revisión final añadió el reintento desde Reiniciar vista cuando falla el guardado de la copia por partida, antes de permitir continuar. La prueba provoca ese fallo de escritura y comprueba que el reintento guarda una sola copia, con cero decisiones y puntos intactos.

Se inspeccionaron las capturas de Registro y Resultado en ambas resoluciones. El retrato conserva las mismas selecciones después de reabrir y el nuevo botón no oculta los campos ni la personalización. La reapertura automatizada reconstruye servicios y vistas desde disco dentro del Editor. No se hizo un cierre forzado del sistema operativo ni se certificó un ejecutable distribuido.

Resultado final: 79 pruebas EditMode y 28 Play Mode aprobadas, 107 en total. Cero fallidas y cero omitidas en las suites finales. Console tras Play Mode devolvió cero errores y cero advertencias. Tras EditMode final devolvió cero errores y una advertencia de transporte de Unity MCP: `[WebSocket] Unexpected receive error: WebSocket is not initialised`. Se conserva documentada, sin borrar el mensaje.

Se verificó la integridad por SHA-256 de los archivos protegidos. Las escenas, el modelo de datos, los servicios originales, los indicadores, el contenido activo, el camión y las trayectorias conservaron sus bytes previos. Unity amplió automáticamente FuenteInterfaz.asset durante las pruebas, por lo que se recuperó su copia exacta del respaldo de esta sesión. Los cambios locales anteriores en fuentes y configuración se conservaron. Unity quedó fuera de Play Mode, sin fallo de compilación.

## Dependencias de Javier y límites

La persistencia del piloto está implementada con la extensión genérica ya disponible. **No fue necesario cambiar un contrato central ni queda una autorización bloqueando este piloto.** La adopción en producción debe acordarse con Javier: decidir si estos atributos pasan al modelo formal del jugador, si se mantienen las copias por partida, cómo se migran/versionan los perfiles y cuál es la política de conservación de metadatos.

La carpeta continúa siendo temporal y puede ser limpiada por el sistema. La solución no convierte la caché en un almacenamiento permanente de producción. Los colores del formato V1 no se deben reasignar a otros tonos sin una versión nueva y una política de compatibilidad. Las partidas sin apariencia histórica no pueden recuperar datos que nunca se guardaron.

No se implementa visualización de un conductor dentro del camión. No se agregan accesorios, otros peinados, nuevos trajes o personajes educativos. No se activan insignias ni categorías finales. Se conserva V1 predeterminada y no se hizo commit ni push.
