# Recorridos completos · demostración visual

Unity 6000.6.7f1 · rama feat/unity-map. Esta guía amplía DemoRecorridoGuide.md: ahora la demostración también concatena mapas. No se integra con el motor de partidas.

## Ejecución y controles

Abrir `Assets/Scenes/DemoRecorrido.unity`, pulsar Play y maximizar Game View para ver cómodamente el fondo y los controles. El modo inicial sigue siendo MAPA ÚNICO, con los 16 mapas individuales. Pulsar RECORRIDO COMPLETO, seleccionar una ruta y Reproducir / Reanudar.

Las ocho opciones se derivan de las referencias `MapaVisual.siguientes` del catálogo existente. No se duplican definiciones ni se inventan asociaciones con los IDs del motor. Se comprueba que cada camino contenga cinco mapas, termine en DELIVERY y no tenga ciclos, referencias externas, conexiones duplicadas o continuaciones faltantes; si no hay ocho caminos válidos, se muestra un aviso y no se habilita su reproducción.

| Opción | Recorrido |
|---|---|
| R3A | S0 → R1A → R2A → R3A → DELIVERY |
| R3B | S0 → R1A → R2A → R3B → DELIVERY |
| R3C | S0 → R1A → R2B → R3C → DELIVERY |
| R3D | S0 → R1A → R2B → R3D → DELIVERY |
| R3E | S0 → R1B → R2C → R3E → DELIVERY |
| R3F | S0 → R1B → R2C → R3F → DELIVERY |
| R3G | S0 → R1B → R2D → R3G → DELIVERY |
| R3H | S0 → R1B → R2D → R3H → DELIVERY |

Seleccionar una ruta reinicia en S0 y pausa. Pausar detiene tanto la distancia como el reloj de fundido; Reanudar conserva ambos estados. Reiniciar, incluso durante una transición, cancela el fundido y vuelve al inicio de S0, pausado. Cambiar a MAPA ÚNICO conserva como selección el mapa visible, reiniciado y pausado. Se mantienen los controles de velocidad y desplazamiento de carril de la demo anterior.

La interfaz indica mapa actual, etapa 1–5, progreso del mapa y progreso total. El total se pondera por la longitud de los cinco recorridos laterales en memoria; los fundidos no incrementan distancia. No representa tiempo restante ni puntuación. Al terminar DELIVERY se detiene y muestra exactamente **Entrega completada**, sin victoria, resultados educativos ni consecuencias.

## Movimiento y transiciones

Se reutiliza RecorridoDemo sin modificarlo: misma interpolación por longitud, normal derecha local, redondeo provisional en memoria y giro progresivo. Escala inicial: 18 × 5.3467 píxeles lógicos; carril derecho 6 px; velocidad inicial 24 px/s. No se modifica ningún punto aprobado. Se mantiene una única referencia a CamionCarga.png y un único dibujo del camión sobre el fondo actual; no se instancian vehículos al cambiar de mapa.

Tras completar íntegramente un mapa, el camión queda inmóvil mientras el fondo y el vehículo se oscurecen durante 0.35 s. Se sustituye el mapa completamente a negro y se deja al menos una actualización con opacidad 1 antes del fundido de entrada, también de 0.35 s. El movimiento continúa al acabar ese fundido. La propiedad Duracion Fundido del controlador permite ajustar la duración de cada mitad en el Inspector. Los controles permanecen disponibles durante el cambio.

El cambio tiene cuantización de un fotograma: el remanente temporal del fotograma de sustitución no se aplica al fondo nuevo, para garantizar una presentación negra. No se descarta distancia de carretera ni se adelanta al camión al inicio del siguiente mapa. En una actualización muy larga se completa como máximo una sustitución antes de presentar negro; no pretende ser una simulación determinista independiente del framerate.

El fundido se aplica a todas las conexiones, incluida R2A → R3B. Es una transición de presentación entre escenarios completos, no un desplazamiento continuo de cámara ni una unión física de sus extremos. Los ocho R3 llegan al mismo DELIVERY mediante esa transición; no confluyen físicamente en la imagen.

## Archivos de esta etapa

Modificado:

- `Assets/Scripts/Mapas/Demo/DemoRecorrido.cs`: modo completo, reproducción secuencial, estados de fundido, progreso total, controles y mensaje final.

Creados:

- `Assets/Scripts/Mapas/Demo/RecorridosVisualesDemo.cs`
- `Assets/Scripts/Mapas/Demo/RecorridosVisualesDemo.cs.meta`
- `Assets/Mapas/RecorridosCompletosGuide.md`
- `Assets/Mapas/RecorridosCompletosGuide.md.meta`

No fue necesario modificar DemoRecorrido.unity, RecorridoDemo.cs ni EditorBuildSettings. Los componentes nuevos se compilan en el assembly propio ya existente.

## Validación realizada

Se ejecutó la escena real en Play Mode. Sobre el componente vivo se reprodujeron las ocho rutas con el mismo método Avanzar que llama Update, mediante pasos de 1/60 s a 24 px/s. Este barrido acelerado en una llamada no equivale a observar todos los fotogramas en tiempo real.

| Ruta | Pasos | Orden completo | Sustituciones a negro | Llega y se detiene en DELIVERY |
|---|---:|---|---:|---|
| R3A | 6085 | Correcto | 4 | Sí |
| R3B | 6129 | Correcto | 4 | Sí |
| R3C | 6134 | Correcto | 4 | Sí |
| R3D | 6211 | Correcto | 4 | Sí |
| R3E | 6153 | Correcto | 4 | Sí |
| R3F | 6236 | Correcto | 4 | Sí |
| R3G | 6081 | Correcto | 4 | Sí |
| R3H | 6159 | Correcto | 4 | Sí |

En cada cambio se comprobó que el mapa saliente había alcanzado toda su longitud. No hubo segmentos saltados, retrocesos de progreso total ni centros fuera del lienzo. En las ocho rutas el progreso terminó en 1, la reproducción quedó detenida y la posición final estuvo dentro del Rect zonaEntrega de DELIVERY. Se verificó una sola instancia de DemoRecorrido y la implementación conserva un solo dibujo de camión por fondo.

Pruebas adicionales: pausa y reanudación durante fundido de salida; pausa durante entrada; reinicio en ambas fases; restauración de S0 sin opacidad residual; modo individual R2B que termina sin pasar a otro mapa. Todas pasaron.

También se dejó avanzar Update en tiempo real, sin llamadas manuales a Avanzar, por S0 → R1A → R2A → R3B → DELIVERY a 160 px/s: entre Time.time 47.95 y 68.32 llegó a DELIVERY, etapa 5, total 100%, detenido. Se inspeccionaron la interfaz, un fundido visible y la llegada final con el mensaje Entrega completada y un único camión en el patio libre. Para R2A → R3B se inspeccionó además el fundido de salida detenido aproximadamente al 50% y la imagen completamente negra. No se afirma revisión visual exhaustiva de todos los fotogramas de las ocho rutas.

## Límites y advertencias pendientes

- Se mantienen las cinco discrepancias artísticas documentadas: S0→R1A, R1A→R2A, R2A→R3B, R2C→R3E y R2C→R3F. R2A→R3B sigue siendo la de mayor prioridad por posición y anchura. El fundido oculta el empalme directo, pero no corrige ni certifica el arte.
- Persisten las advertencias de proximidad a líneas laterales en curvas de R2A/R2B/R2C y las prioridades de revisión de R2D/R3F. No se certifica cabida física, radio de giro ni ausencia de solapes con obstáculos. Las comprobaciones de límites verifican el centro, no toda la huella del vehículo.
- En entradas/salidas sobre el borde, parte de la silueta puede recortarse; el fundido comienza después de alcanzar el extremo. No se añadieron trayectorias exteriores ficticias. DELIVERY conserva su final interior en el patio.
- Se conserva la proporción y el filtro Point. Rotaciones, escala de ventana no entera, detalle pequeño del sprite y fachadas oblicuas mantienen las limitaciones de perspectiva descritas en ValidacionCamion.md.
- La interfaz IMGUI es temporal; maximizar Game View. Una vista muy pequeña deja poco espacio para el fondo. Los cambios de selección y carril no son maniobras animadas.
- No hay carga de escenas entre mapas, física, colliders, persistencia de partida, decisiones ni integración con los servicios de Javier. Los fondos cambian dentro de la misma escena de demostración.

Resultado de cierre: compilación correcta; Console final sin errores ni advertencias; ValidadorContinuidad.Revisar sin incidencias. Se compararon por SHA-256 los 50 archivos PNG y assets de Assets/Mapas (fondos, miniaturas, camión, catálogo y definiciones): ninguno cambió. Los 49 188 pasos de la prueba final están resumidos en la tabla. Se salió de Play Mode y quedó abierta DemoRecorrido en edición.

Se conservaron los cambios locales previos. No se modificaron Motor, Datos, UI ni ruta.json. No se hizo commit ni push.
