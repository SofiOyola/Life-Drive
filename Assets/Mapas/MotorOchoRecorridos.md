# Motor de ocho recorridos · etapa independiente

4 de noviembre de 2026. Rama feat/unity-map; git status inicial limpio.

## Resultado y límite de esta entrega

Se añadió RecorridoVisualValidado: un cursor sobre los ScriptableObjects existentes, con validación exacta del árbol, destinos permitidos e historial en memoria. Reutiliza RecorridosVisualesDemo.Obtener; no duplica geometría, evaluación, puntuación, eventos ni base de datos. No está conectado al gameplay. No se modificaron servicios ni contenido. Esta es una entrega parcial conforme a la condición de calidad del encargo, no la integración de las ocho rutas educativas.

S0 → R1A/R1B; R1A → R2A/R2B; R1B → R2C/R2D; R2A → R3A/R3B; R2B → R3C/R3D; R2C → R3E/R3F; R2D → R3G/R3H; cada R3 → DELIVERY. Se rechazan destinos inexistentes, saltos de nivel, ciclos o conexiones distintas de ese árbol. El historial se devuelve como copia. No hay persistencia visual ni reanudación implementada.

## Diagnóstico actualizado

Se revisaron ServicioJuego, ServicioPartidas, Evaluacion, ValidadorRuta, Sesion, PantallaPartida, ruta.json, las pruebas existentes, RecorridosVisualesDemo, RecorridoDemo, MapaVisual y CatalogoMapas. El contenido conserva 4 tramos, 7 situaciones y 20 alternativas. No se han confirmado asociaciones decisión-mapa. Los IDs visuales permanecen separados de los numéricos. Las alternativas existentes no representan tres niveles binarios independientes para todas las ramas.

ServicioPartidas.Decidir registra una Decision, aplica puntos/retraso, cambia de tramo y ejecuta Avanzar antes de devolver ResultadoDecision. La respuesta puede contener un tramo posterior distinto del destino explícito de Decision. El reintento devuelve Repetida sin reevaluar; este comportamiento debe conservarse. PantallaPartida.Decidir/Continuar son métodos privados y navegan a Resultado cuando no hay situación, sin esperar una llegada visual.

## Contrato pendiente antes de modificar servicios

1. Aprobar tres niveles de bifurcaciones educativas, sus enunciados, alternativas, efectos y referencias explícitas; no asignar la revisión existente arbitrariamente a R1A/R1B. Decidir si se incorpora una versión nueva de contenido o un itinerario opcional conservando partidas anteriores.
2. Resolver la versión de contenido asociada a cada partida: hoy la sesión carga un único ruta.json. Cambiar el grafo sin versionado puede dejar partidas antiguas a mitad de una semántica diferente. Hace falta política de compatibilidad y prueba con registros anteriores antes de sustituir contenido.
3. Definir una asociación validada por contexto de mapa actual, situación y alternativa confirmada; con cobertura de las ocho ramas y rechazo de ambigüedades. No hay tabla numérica aprobada en esta entrega.
4. Capturar antes de Decidir los IDs y campos primitivos del estado anterior; después, copiar Decision, efectos, Repetida y tramo posterior. Una cola visual presenta ese hecho confirmado sin volver a ejecutar Decidir. CambioRuta no es señal suficiente.
5. Acordar hooks de PantallaPartida para detener el reproductor, presentar situación/consecuencia, continuar, cambiar fondo y llegar a DELIVERY. El cursor solo valida destinos suministrados por ese adaptador futuro; AvanzarConfirmado no verifica por sí mismo una transacción del motor.
6. La persistencia visual debe usar un contrato compatible de checkpoint y versión, no otro archivo de puntuaciones ni una segunda base. Los eventos/decisiones seguirán bajo los servicios actuales. Los finales definitivos quedan fuera del alcance.

## Archivos

Nuevos: Assets/Scripts/Mapas/RecorridoVisualValidado.cs, Assets/Tests/EditMode/RecorridoVisualValidadoTests.cs, Assets/Mapas/MotorOchoRecorridos.md y sus meta. Ningún archivo previo modificado intencionalmente. No se cambiaron escenas, PNG, trayectorias, JSON, motor, registro o autenticación.

## Pruebas y evidencia

### Validación en Unity · 5 de noviembre de 2026

Se verificó la presencia de `RecorridoVisualValidado.cs` y `RecorridoVisualValidadoTests.cs` en `feat/unity-map`. Unity MCP por HTTP local solicitó una compilación real en el editor abierto de Unity 6000.6.7f1. El estado del editor confirmó el inicio y fin de la compilación, la recarga de dominio y la vuelta al estado listo. El editor operó fuera de modo batch.

| Ejecución del Unity Test Runner | Aprobadas | Fallidas | Omitidas | ID de ejecución |
| --- | ---: | ---: | ---: | --- |
| `RecorridoVisualValidadoTests.OchoRutasAlcanzanDeliverySinSaltos` | 1 | 0 | 0 | `967170bbf4e54257b9429a6fc37056b0` |
| EditMode completo | 27 | 0 | 0 | `0073e70181b64eefad76a7c4e24f626e` |

La ejecución completa incluye la prueba de recorridos y las 26 pruebas existentes de `BaseDatosTests`, `DatosTests` y `MotorTests`. Ambas ejecuciones devolvieron `Passed`.

La prueba recorrió los ocho caminos del catálogo, uno por cada hoja de `R3A` a `R3H`. Todos llegaron a `DELIVERY` en cinco etapas. La construcción del cursor validó las conexiones de los 16 mapas contra el árbol esperado y la enumeración descartó conexiones externas y ciclos. La prueba verificó también el rechazo de un destino desconocido, del salto directo `S0 → DELIVERY` y del regreso a `S0` desde `DELIVERY`.

Console no devolvió errores de compilación ni de pruebas. Antes y después de ejecutar el Test Runner figuró una advertencia de Unity MCP: `[WebSocket] Unexpected receive error: WebSocket is not initialised`. La conexión siguió respondiendo y devolvió ambos resultados completos. Las pruebas no requirieron correcciones de código.

### Evidencia histórica · 4 de noviembre de 2026

Se escribió una prueba NUnit que enumera las ocho rutas del catálogo, avanza las cinco etapas, verifica DELIVERY y rechaza un destino desconocido, el salto S0→DELIVERY y salir de DELIVERY. Estas son pruebas del cursor visual, no de registro o puntuación.

Unity MCP no está disponible en esta sesión. Se intentó iniciar el Test Runner por línea de comandos en Unity 6000.6.7f1 con resultados en Temp/OchoRecorridosTests.xml y log en Temp/OchoRecorridosTests.log; no se obtuvo archivo de resultados durante la revisión. Por tanto NO se declara ninguna prueba NUnit ejecutada o aprobada, ni compilación/Console comprobadas. La línea base de MotorTests, DatosTests y BaseDatosTests está pendiente de ejecución. No se modificó el motor antes de obtenerla.

No están probados en esta entrega: registro único de decisiones/eventos, efectos sin duplicación, reanudación, compatibilidad de registros anteriores o integración visual educativa. Su implementación requiere el contrato anterior. No se sustituyen esas verificaciones por una prueba del árbol.

La ausencia de una correspondencia segura y versionada impide garantizar ocho recorridos reales sin cambiar contratos relevantes. Se detiene aquí dejando los servicios afectados intactos, como permite la condición de calidad. No se hace commit ni push.

## Reintento de validación · 4 de noviembre de 2026

Cursor y prueba revisados sin modificaciones. MCP continúa ausente. Dos arranques de Test Runner (uno fuera del entorno restringido) terminaron con código 1 antes de generar XML. El log confirma el projectPath Life-Drive y registra LicenseClient-angil rechazado y aserciones WMI/Windows durante el arranque. Cero pruebas ejecutadas verificables; compilación y Console no comprobadas. No hay rutas certificadas por NUnit ni regresión certificada. Se detiene la implementación según la instrucción expresa. Véase ContratoIntegracionRecorridos.md para evidencia y pasos de recuperación; el contrato detallado queda pendiente, sin asociaciones inventadas.
