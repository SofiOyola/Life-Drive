# Flujo del jugador integrado V2

7 de noviembre de 2026. Unity 6000.6.7f1, rama `feat/unity-map`. Integración piloto del prompt 33.

## Activación y pantallas

En Unity elegir **Life Drive > Piloto V2 > Abrir flujo del jugador**. El comando solicita guardar escenas que tengan cambios, entra en Play Mode y abre Inicio. Solo este comando o la activación explícita de las pruebas crea el adaptador persistente. Salir de Play Mode desactiva el piloto.

Se reutilizan sin duplicar ni guardar modificaciones en sus escenas:

1. `Inicio.unity`: Jugar abre Registro. Un aviso identifica el piloto, los datos temporales y el uso exclusivo de identidades ficticias. El acceso administrativo se conecta en el piloto según [IntegracionAdministrativaV2.md](IntegracionAdministrativaV2.md).
2. `Registro.unity`: solicita nombre, cédula y elección explícita de una de las cuatro fichas existentes. Conserva Hombre/Mujer y los cinco tonos de cada grupo: piel, ojos, cabello y traje. Iniciar / retomar V2 llama a `ServicioUsuarios.RegistrarJugador` y abre la partida. Los errores del servicio aparecen en el formulario.
3. `PruebaIntegracionV2.unity`: recibe el mismo ServicioJuego y la identidad registrada. Inicia o retoma automáticamente, usa las respuestas del paquete y las decisiones guardadas para navegar. Mantiene camión, mapas, pausas, consecuencias y reconocimiento de hitos. Al completar la última presentación en DELIVERY aparece **Ver resultado de la operación**. Ese botón abre Resultado, sin adelantar el cierre visual.
4. `Resultado.unity`: usa las referencias de la presentación existente con su controlador DEMO desactivado solo en el piloto. Muestra nombre, ficha, apariencia de la sesión, puntos, decisiones, retraso, recorrido visual e historial paginado de alternativas y consecuencias. Volver a Inicio cierra la sesión de usuario del piloto. No asigna una de las tres categorías finales. Esas categorías y sus recursos siguen preparados en la escena original.

`DemoInterfazPartida.unity` se inspeccionó como referencia. No alimenta la puntuación ni el historial del piloto y no se modificó.

La navegación piloto usa carga de escenas del Editor. No se agregó al perfil de compilación ni se verificó un ejecutable distribuido. La entrada normal, Sesion.Juego y la selección V1 del motor conservan su contrato. Abrir Inicio normalmente sigue mostrando la previsualización anterior. No se está certificando aquí un flujo V1 de pantallas que previamente era una maqueta.

## Contrato de personaje y apariencia

Los botones **Ficha 1**, **Ficha 2**, **Ficha 3** y **Ficha 4** se generan a partir de los IDs reales del contenido. No se muestran sus nombres u ocupaciones como identidad del jugador. Ningún color o silueta se traduce automáticamente a esos IDs. El registro exige elegir una ficha, que funciona como dato técnico compatible independiente.

Una cédula ya registrada con nombre compatible recupera el jugador existente. El servicio conserva su personaje_id original aunque se elija otra ficha en el formulario. El piloto respeta esa conducta y usa el ID devuelto. No modifica Usuarios, Datos, esquema ni validaciones de Javier.

La apariencia del piloto ahora se conserva mediante eventos versionados en la misma base temporal, con variantes visuales válidas y una configuración fijada a cada partida. Registro permite recuperarla y Resultado reconstruye la apariencia guardada, sin reemplazar el personaje_id original. El contrato, las pruebas y los límites están en [PersistenciaPersonajesS3-07.md](PersistenciaPersonajesS3-07.md). La incorporación al modelo definitivo de producción sigue pendiente de coordinación con Javier.

## Datos reales y provisionales

El guardado utiliza el formato y las transacciones existentes en `Application.temporaryCachePath/LifeDrive-FlujoJugadorV2/sesion-local/lifedrive-db.json`. No usa la carpeta persistente de jugadores de producción. La caché puede ser limpiada por el sistema.

Se guardan nombre, cédula, personaje_id, fechas, partida, versión y huella del paquete, puntos, retraso, estado y último hito visual presentado. Cada decisión conserva IDs, tipo, alternativa, acierto, buena práctica, puntos, retraso, consecuencia y tramos. Se conservan los eventos originales. El recorrido visual se reconstruye desde esas decisiones y las asociaciones versionadas, sin una segunda fuente de verdad.

Los valores mostrados proceden del motor y de esa partida. Los escenarios, efectos y criterios del paquete siguen siendo **candidatos pendientes de validación educativa**, aunque se evalúen y guarden realmente. No se usan puntos ni insignias simulados de DemoInterfazPartida. Los tres reconocimientos aparecen como pendientes de reglas aprobadas. No se inventan clasificaciones de operación.

No se implementan ni reemplazan TDS, IEE, PPS o PCRS. Se preservan sus entradas disponibles para el trabajo de Jorge. La fórmula aprobada debe aplicarse posteriormente en su módulo correspondiente. IEE no adquiere una medición antes/después por esta integración.

## Archivos de esta integración

| Archivo | Trabajo |
| --- | --- |
| `Assets/Scripts/IntegracionV2/FlujoJugadorPilotoV2.cs` | Adaptador aislado de pantallas, sesión temporal, selección técnica explícita, persistencia visual aislada, resumen real e historial paginado. Crea fuentes TMP temporales para los textos del piloto. |
| `Assets/Editor/InterfazVisual/FlujoJugadorPilotoV2Editor.cs` | Activación explícita y navegación de Editor sin cambiar Build Profiles. |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.cs` | Inyección opcional del servicio del jugador y salida a Resultado limitada al cierre visual confirmado. |
| `Assets/Scripts/IntegracionV2/PruebaIntegracionV2.Vista.cs` | El botón final ofrece Resultado cuando hay sesión inyectada. El piloto independiente conserva Iniciar otra prueba. |
| `Assets/Tests/PlayMode/FlujoJugadorPilotoV2Tests.cs` | Ocho recorridos con pantallas, registro aislado, doble clic, datos reales, paginación, reanudación y ambas resoluciones. |
| `Assets/Mapas/FlujoJugadorIntegradoV2.md` | Sustituye el informe previo bloqueado por ausencia de MCP con el estado actual verificado. |
| `TestEvidence/FlujoJugadorV2/*.png` | Capturas de Inicio, Registro, Partida, Resultado e Historial en 1920×1080 y 1280×720. |

Los archivos nuevos de Assets tienen sus .meta. Los servicios del motor, Datos y DemoRecorrido ya tenían cambios locales al comenzar y se conservaron. No se hizo commit ni push. Tampoco se cambiaron el contenido activo, los 16 fondos ni el camión. SHA-256 de ruta.json: `E7AD6F40DDC679348A934CAECC052376AE0607A2BAB07487E07A7F156FE28A7A`.

## Pruebas y evidencias

Las pruebas usan exclusivamente Piloto Ficticio, cédula 990033, y carpetas por GUID que se eliminan al terminar. Los clics se envían a los botones reales mediante eventos UI de Unity, sin menús para seleccionar rutas ni escritura directa de decisiones.

| Ejecución mediante Unity MCP | Aprobadas | Fallidas | Omitidas | ID |
| --- | ---: | ---: | ---: | --- |
| EditMode antes de editar | 59 | 0 | 0 | `5b574694b17c4cf0a6628da931e0e03a` |
| Play Mode antes de editar | 9 | 0 | 0 | `56526b70cea4401fafeb7cde472796da` |
| Primer ensayo del nuevo flujo | 0 | 9 | 0 | `ad84e1b177eb41c8861f812f55d8fbf9` |
| Nuevo flujo corregido | 9 | 0 | 0 | `819e95674a814dccadeaf64e39f28943` |
| Suite completa tras ajustes de presentación | 18 | 0 | 0 | `1d70044642fa4c1887ea663ba40f908a` |
| Play Mode final | 18 | 0 | 0 | `99fc3b12e2d74f2cb627537c65e7ab09` |
| EditMode final | 59 | 0 | 0 | `08b429b862344ea6a160d3dea137f030` |

El primer ensayo detectó un conflicto de dos componentes Graphic al montar el avatar y una aserción que no contemplaba al jugador previamente registrado en la reapertura. Se corrigieron ambos. También se ajustaron las etiquetas de fichas y los textos largos de Resultado al inspeccionar las capturas. No se ocultaron esos intentos fallidos.

Las ocho rutas verifican S0, las tres bifurcaciones y DELIVERY mediante respuestas. Comprueban registro con ficha 3, bloqueo sin ficha, puntos iguales a la suma de decisiones, un evento de puntos por respuesta, una finalización, consecuencia antes de cambiar de mapa, aparición posterior de Resultado, recorrido correcto y todas las consecuencias a través de las páginas del historial. La prueba nueva de reapertura destruye el adaptador, abre de nuevo el mismo almacenamiento, vuelve a ingresar desde Inicio/Registro y retoma una transición pendiente desde S0 sin reevaluar ni perder puntos. Las nueve pruebas originales conservan además reanudación en los hitos, pausa y reinicio de presentación.

Las capturas se generan con tamaños fijos de Game View y se comprueban Screen.width/height reales. Se inspeccionaron visualmente las pantallas en ambas resoluciones. Esta evidencia no equivale a probar todas las combinaciones de hardware, formatos de pantalla ni accesibilidad.

Resultado final: 77 pruebas aprobadas, cero fallidas y cero omitidas en las suites finales. Console consultada tras Play Mode y tras la apertura por menú: cero errores y cero advertencias. La apertura real mediante el comando del Editor confirmó Play Mode, escena Inicio y adaptador activo. Al salir se comprobó adaptador ausente y scriptCompilationFailed=false. El registro y las ocho entregas se verificaron por UI automatizada, sin una segunda sesión manual completa.

Durante las recargas apareció una advertencia transitoria de transporte Unity MCP y se corrigieron advertencias de API obsoleta en las pruebas nuevas. No estuvieron presentes en Console final. Unity expandió un atlas FuenteInterfaz.asset que estaba limpio al inicio, por lo que se recuperaron sus bytes desde HEAD sin tocar los cambios previos. Las dos fuentes BaseNaturaleza ya estaban modificadas y se conservaron, con posibles ampliaciones automáticas de glifos. No se declara identidad binaria inicial de esos atlas. El ejecutor activó temporalmente la ejecución en segundo plano y volvió a false. Las opciones de entrada a Play Mode quedaron en DisableDomainReload y DisableSceneReload. El archivo temporal InitTestScene que ya existía al comenzar se conserva.

## Cobertura académica y dependencias

- S3-09: conserva decisiones, tipos, señales de seguridad, puntos, historial, versión y estado para el cálculo posterior. No calcula KPI.
- S4-04: aporta controles comprobados de navegación, integridad, reanudación, rutas válidas y aislamiento V1/V2. No certifica todo el plan de calidad del equipo.
- S4-05: aporta pruebas de comportamiento por las pantallas y verificaciones internas de estado, decisiones y eventos. No sustituye la totalidad de pruebas de caja negra y blanca documentadas.
- S4-06: ejecuta la suite EditMode existente y añade pruebas de integración Play Mode. No se afirma que toda especificación de pruebas unitarias del documento esté implementada.

La integración administrativa posterior queda documentada en [IntegracionAdministrativaV2.md](IntegracionAdministrativaV2.md), que actualiza la autenticación, las consultas y el PDF del piloto. No se debe mezclar automáticamente esta caché con producción. La persistencia de apariencia del piloto se documenta en [PersistenciaPersonajesS3-07.md](PersistenciaPersonajesS3-07.md). Siguen pendientes su incorporación a producción, las reglas definitivas de categorías e insignias, los cálculos de Jorge y la aprobación del contenido candidato. No se añadió autenticación administrativa.
