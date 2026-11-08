# Persistencia y reanudación segura de recorridos V2

Life-Drive · Unity 6000.6.7f1 · rama `feat/unity-map` · 5 de noviembre de 2026.

## Selección y conservación de versión

La entrada normal `new ServicioJuego(carpetaContenido, carpetaDatos)` y `Partidas.IniciarPartida()` sigue creando V1. Para probar V2 se construye el mismo servicio con el catálogo como tercer argumento y se llama `RecorridosV2.IniciarPartida()`. La carga valida el paquete candidato y el árbol visual antes de exponer el adaptador. Si el jugador tiene una partida en curso, el servicio conserva la versión guardada aunque se haya solicitado otra para una partida nueva. Una partida V2 abierta por la entrada clásica sin paquete disponible devuelve un error y conserva sus datos. La entrada explícita V2 recupera también un cierre finalizado pendiente de presentar cuando el jugador carece de una partida activa.

| Dato guardado en `Partida` | Significado |
| --- | --- |
| `contenido_version` | 0 en archivos anteriores equivale a V1. Las nuevas partidas escriben 1 o 2. |
| `contenido_huella` | SHA-256 del texto UTF-8 del paquete candidato, codificado en Base64. V2 la fija al crear la partida y rechaza un paquete distinto aunque anuncie la versión 2. Los registros V1 pueden dejarla vacía. |
| `ultima_decision_presentada_id` | Hito visual reconocido. Cero equivale a iniciar en S0. Un valor distinto de cero debe referir una decisión de la misma partida V2. |

Se usa `lifedrive-db.json` y la transacción existente, con reemplazo desde el archivo temporal y copia de respaldo. El archivo conserva el esquema 1 mientras solo almacene partidas clásicas. Al guardar la primera V2 sube a 2 para que un lector anterior, cuyo máximo es 1, rechace el archivo completo. El nuevo lector acepta archivos del esquema 1, incluidos registros sin campos nuevos. Las pruebas usaron carpetas temporales. No se migró una base de usuario ni se verificó el comportamiento ejecutando un binario anterior.

## Respuesta, guardado y presentación

1. `ServicioPartidas.Decidir` copia el estado previo y usa el evaluador existente. En una sola transacción guarda la respuesta, sus puntos y retraso, los eventos y los cambios lógicos de tramo.
2. El resultado expone `Antes`, `Decision`, `Despues` y `TramosAlcanzados` en orden. La traza incluye el destino explícito y los tramos recorridos por avance implícito, sin depender de `CambioRuta`. En V1, s4/a10 guarda 2→2 como destino explícito y después avanza a 4. En V2, s5/a12 alcanza el tramo 112 y avanza a 116 en la misma llamada, con la aproximación aún por presentar en R3E.
3. `RecorridosV2.Consultar(partidaId)` revisa propiedad y versión. Reproduce las decisiones de esa partida en orden de ID desde S0, usa las asociaciones explícitas del paquete y aplica al cursor solo las aristas reconocidas en el hito. Devuelve mapa actual, historial, situación pendiente del motor, última decisión confirmada, destino visual asociado, presentación pendiente y estado de cierre. El estado lógico puede estar adelantado respecto al mapa.
4. La interfaz deberá mostrar la consecuencia y completar la transición antes de llamar `ConfirmarPresentacion(partidaId, decisionId)`. El adaptador acepta solo el siguiente hito o un reintento del hito ya guardado. La confirmación persiste en la misma base y no genera una segunda decisión. El servicio V2 impide responder a otra situación mientras exista una presentación pendiente.
5. Un cierre durante un fundido deja el origen en el último hito reconocido y conserva la decisión y su destino para reintentar la presentación. Un reintento de `Decidir` sobre la misma situación devuelve la decisión guardada, con `Repetida=true`, sin evaluación nueva, traza nueva ni snapshot histórico `Antes`. `Despues` refleja el estado lógico vigente.
6. La respuesta de aproximación s6 habilita R3→`DELIVERY` aunque no cambie el tramo lógico. Reconocer ese hito confirma la llegada. La respuesta s7 finaliza la partida lógica. Reconocer su presentación completa el cierre visual, sin crear otra partida ni otro evento de finalización.

El adaptador deriva el historial visual y el destino de la decisión de los datos ya guardados. Conserva un hito discreto, suficiente para repetir la presentación desde un punto estable. Las consultas y confirmaciones válidas de V2 no agregan eventos de reanudación. V1 conserva el evento `PARTIDA_RETOMADA` previsto por su flujo clásico. El historial y los reportes seleccionan nombres de tramos de la versión de la partida. Los datos existentes de acierto, tipo, buena práctica, puntos, retraso y finalización quedan disponibles para S3-09; su definición y cálculo siguen a cargo de ese trabajo.

## Archivos

| Archivo | Cambio |
| --- | --- |
| [Registros.cs](../Scripts/Datos/Registros.cs) y [BaseDatos.cs](../Scripts/Datos/BaseDatos.cs) | Campos compatibles con V1, verificación del hito y protección de esquema para V2. |
| [ServicioJuego.cs](../Scripts/Motor/ServicioJuego.cs) | Carga optativa del paquete y composición del adaptador sobre los servicios existentes. |
| [ServicioPartidas.cs](../Scripts/Motor/ServicioPartidas.cs) | Versión fija al crear y reanudar, snapshots, traza y control del hito pendiente. |
| [ServicioDesempeno.cs](../Scripts/Motor/ServicioDesempeno.cs) | Nombres de tramos de la partida V2 para resumen e informe. |
| [PaqueteRecorridos.cs](../Scripts/Mapas/PaqueteRecorridos.cs) | Huella del paquete leído. |
| [ReanudacionRecorridosV2.cs](../Scripts/Mapas/ReanudacionRecorridosV2.cs) | Adaptador nuevo de reconstrucción y reconocimiento de hitos. |
| [ReanudacionRecorridosV2Tests.cs](../Tests/EditMode/ReanudacionRecorridosV2Tests.cs) | Pruebas nuevas de compatibilidad, reapertura, guardado y reintento. |

## Evidencia de Unity Test Runner

Compilación y recarga de dominio confirmadas en Unity 6000.6.7f1 mediante Unity MCP, fuera de modo batch. Los dos primeros intentos de línea base no ejecutaron pruebas. El primero agotó el tiempo de inicialización y el segundo informó que el editor estaba en Play Mode. Tras poner Unity en primer plano y salir de Play Mode, se completó la línea base antes de editar código.

| Ejecución | Aprobadas | Fallidas | Omitidas | ID |
| --- | ---: | ---: | ---: | --- |
| Línea base EditMode | 45 | 0 | 0 | `2bd23764049144d1b894b17a5958fcc5` |
| Reanudación V2 y compatibilidad | 14 | 0 | 0 | `c5b3f1b00e4d460d9ea83ecd1ab3df22` |
| EditMode completo final | 59 | 0 | 0 | `d031d040f18247178f80d391d8debead` |

Las pruebas nuevas reabren V2 en S0, después de cada bifurcación y en DELIVERY. Reproducen interrupciones antes de reconocer la primera, segunda y tercera transición, la llegada y la consecuencia final. Verifican IDs, puntos, número de decisiones y eventos, huella alterada, autorización, coexistencia V1/V2, rechazo sin paquete y rollback de respuesta y de hito por un fallo de escritura. La prueba legada reabre un archivo de esquema 1 sin campos nuevos y continúa V1 hasta el avance implícito s4/a10. EditMode conserva también las pruebas de las ocho rutas del paquete, del cursor, de permisos, de datos y del motor.

Console después de las pruebas devolvió cero errores y una advertencia previa de Unity MCP, `[WebSocket] Unexpected receive error: WebSocket is not initialised`.

## Integración pendiente

1. Aprobar el contenido y publicar un paquete inmutable. Conservar los paquetes asociados a guardados existentes; si no se dispone de la huella esperada, ofrecer recuperación explícita antes de reanudar.
2. Conectar la entrada optativa en la interfaz del jugador. Si devuelve una partida V1 activa, usar el flujo clásico. Para V2, consultar antes de pintar el mapa, restaurar el último hito y mostrar la presentación pendiente antes de habilitar respuestas.
3. Confirmar el hito al terminar consecuencia y transición. En la entrega, mostrar s7 tras la llegada y navegar a Resultado cuando `CierrePresentado` sea verdadero. Los puntos exactos de pausa y el manejo visual de fallos se deben verificar en la escena real.
4. Probar cierres reales del reproductor, la entrega y la navegación de pantallas en Play Mode antes de habilitar V2. El almacenamiento mantiene el modelo local de un proceso escritor del motor. La prueba de interrupción cubre la reapertura desde el hito guardado y queda pendiente la posición exacta del camión dentro de una animación. Coordinar con Jorge los criterios y la aprobación de los datos de S3-09.

`ruta.json` y el paquete candidato existente conservaron sus datos. V2 sigue siendo optativo.
