# Integración de partida V2 con mapas e interfaz

Life-Drive · Unity 6000.6.7f1 · `feat/unity-map`.

## Abrir el piloto

Abrir [PruebaIntegracionV2.unity](../Scenes/PruebaIntegracionV2.unity) en el editor, entrar en Play Mode y elegir **Iniciar / retomar V2**. La escena queda fuera de la navegación y del perfil de compilación del producto. El aviso del encabezado identifica el contenido educativo como candidato pendiente de validación. **Reiniciar vista** restaura el hito confirmado. **Iniciar otra prueba** se ofrece tras la entrega.

La escena usa un jugador ficticio y el mismo formato y servicios de guardado que el producto. Su carpeta es `Application.temporaryCachePath/LifeDrive-PruebaIntegracionV2/sesion-local`. Cada prueba automatizada usa un identificador único dentro de ese directorio. La caché permite reabrir el piloto local, sujeta a la limpieza de archivos temporales del sistema.

## Arquitectura y flujo

- [PruebaIntegracionV2.cs](../Scripts/IntegracionV2/PruebaIntegracionV2.cs) conecta la sesión aislada, el motor, la reanudación y el reproductor. [PruebaIntegracionV2.Vista.cs](../Scripts/IntegracionV2/PruebaIntegracionV2.Vista.cs) monta el Canvas con TextMeshPro, las fuentes de la identidad aprobada, la paleta natural, los paneles redondeados de `FormaRedondeadaUI` y el hover de `RespuestaBotonUI`. La escena muestra puntos reales y progreso visual. Las insignias figuran como pendientes.
- [DemoRecorrido.cs](../Scripts/Mapas/Demo/DemoRecorrido.cs) conserva geometría, camión y fundidos. Se amplió con carga explícita de un mapa y reproducción de una arista confirmada para el control externo. Las dos escenas demo conservan su configuración y su recorrido automático.
- [LifeDrive.IntegracionV2.asmdef](../Scripts/IntegracionV2/LifeDrive.IntegracionV2.asmdef) mantiene el montaje y la entrada del piloto separados del ensamblado de producción. [PruebaIntegracionV2Tests.cs](../Tests/PlayMode/PruebaIntegracionV2Tests.cs) y su [ensamblado de pruebas](../Tests/PlayMode/LifeDrive.Tests.PlayMode.asmdef) cargan la escena real en Play Mode.

El camión se mueve automáticamente y pausa en hitos normalizados de la trayectoria del piloto. La interfaz toma el enunciado y las alternativas de `ServicioPartidas.SituacionActual`. Al responder, bloquea los botones y llama una vez a `ServicioPartidas.Decidir`. Presenta el veredicto, la explicación, los puntos y el retraso de la decisión guardada. El origen sigue visible hasta que el jugador pulsa **Continuar recorrido**.

El destino de una respuesta se obtiene de `ReanudacionRecorridosV2`, que usa las 14 asociaciones del paquete fijado por versión y huella. El reproductor acepta solo la arista indicada y el adaptador guarda el hito al completar el fundido. Las respuestas sin cambio de mapa reconocen el hito al continuar. La aproximación s6 habilita la transición a DELIVERY aunque el motor ya se encuentre en el tramo lógico terminal. Tras responder s7, el piloto muestra la consecuencia y luego **Entrega completada**, usando la finalización ya persistida.

Al reabrir, se reconstruyen historial, mapa y presentación desde las decisiones guardadas. Si se cerró durante una transición, se restaura el origen y se vuelve a mostrar la consecuencia guardada. El jugador puede repetir la animación sin aplicar de nuevo la decisión. Si ya se reconoció la última consecuencia en DELIVERY, se muestra la entrega completada. Si el paquete o la versión no coinciden, la interfaz muestra un error controlado. [Contrato de persistencia](PersistenciaRecorridosV2.md).

## Ocho recorridos cubiertos en la escena

| Recorrido | Alternativas navegables |
| --- | --- |
| S0 → R1A → R2A → R3A → DELIVERY | 101, 103, 107 |
| S0 → R1A → R2A → R3B → DELIVERY | 101, 103, 108 |
| S0 → R1A → R2B → R3C → DELIVERY | 101, 104, 109 |
| S0 → R1A → R2B → R3D → DELIVERY | 101, 104, 110 |
| S0 → R1B → R2C → R3E → DELIVERY | 102, 105, 12 |
| S0 → R1B → R2C → R3F → DELIVERY | 102, 105, 13 |
| S0 → R1B → R2D → R3G → DELIVERY | 102, 106, 111 |
| S0 → R1B → R2D → R3H → DELIVERY | 102, 106, 112 |

Las pruebas Play Mode envían clics a los botones de la escena mediante eventos de Unity. Comparan la consecuencia visible con la guardada, el origen antes de continuar, los destinos y el cierre. Reintentan clics, verifican la cantidad de decisiones y eventos de puntuación, contrastan puntos y retrasos y exigen una única finalización. El noveno caso abre de nuevo la escena en S0, tras cada bifurcación y en DELIVERY, pausa y reinicia la vista durante una transición pendiente. La cobertura de servicios y reanudación estructural sigue en EditMode.

## Activación posterior

V1 sigue siendo la opción predeterminada. El contenido activo y las escenas del producto conservan sus archivos. Para integrar el piloto al flujo definitivo, aprobar las situaciones y sus efectos, acordar los hitos de la experiencia de producción y validar la navegación de registro a partida y resultado con datos aislados. La escena candidata conserva un único mensaje de entrega. La clasificación de finales, las insignias reales y los indicadores de Jorge quedan fuera del piloto.

## Evidencia de Unity Test Runner

| Ejecución | Aprobadas | Fallidas | Omitidas | ID |
| --- | ---: | ---: | ---: | --- |
| EditMode antes de editar | 59 | 0 | 0 | `5091656aff31470aa22c3011896477d6` |
| Play Mode final · interfaz y reanudación | 9 | 0 | 0 | `dccf27ada15f4832a56e1680989ca730` |
| EditMode final · servicios y regresión | 59 | 0 | 0 | `c8fcfb2bb5594180bd4a08c9223c7b6b` |

El primer intento de Play Mode se quedó en la escena de arranque del ejecutor y no inició casos. Habilitar temporalmente la recarga de dominio permitió ejecutar la suite. Durante el ajuste de fuentes, un intento adicional no inició pruebas por un error de compilación al pasar el tamaño tipográfico como flotante a una API que recibe un entero. Se corrigió y se repitieron ambas suites. Los ajustes de Play Mode y ejecución en segundo plano volvieron a su valor inicial.

La observación directa en Game View cubrió la pantalla de arranque, con S0, el botón de inicio y el aviso de candidatura. Sirvió para corregir el ajuste de línea. Las ocho entregas se verificaron mediante los botones automatizados en Play Mode. Console, consultada mediante Unity MCP al terminar, devolvió cero errores y cero advertencias.

Durante la primera validación, TextMeshPro agregó glifos a los atlas de las dos fuentes compartidas, que ya tenían cambios locales. Para evitar nuevas escrituras, el piloto final genera atlas temporales a partir de las mismas fuentes base. El SHA-256 de ambos recursos se mantuvo idéntico durante la validación final y la inspección visual posterior. Los atlas ya ampliados por el primer ensayo siguen en los archivos de trabajo.
