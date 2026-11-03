# Life Drive · validación final visual del camión

Esta sección es la conclusión vigente. Las tablas anteriores de color se conservan más abajo como historial automatizado y no representan por sí solas fallos visuales confirmados.

## Sprite realmente utilizado y escala recomendada

Se inspeccionó de nuevo **Assets/Mapas/Vehiculos/CamionCarga.png**: predomina el techo rectangular de la caja y de la cabina, ruedas como pequeñas protuberancias, frente/cabina a la derecha. Es una vista superior/cenital, no la variante lateral descartada en la generación anterior. No se sustituyó ni sobrescribió el recurso.

Para el prototipo se recomienda conservar **18 × 5.3467 píxeles lógicos**, escala 1, corrección angular 0°, con proporción de silueta 3.3666:1. Es una recomendación visual conservadora para revisión, no una escala física aprobada. Ampliarlo para ocupar más calzada no corrige un error de trayectoria. A esta escala algunos detalles se pierden; el zoom de la ventana no cambia el tamaño relativo al fondo.

El desplazamiento lateral utilizado fue cero para revisar las trayectorias aprobadas. Por ello el camión suele quedar sobre la línea amarilla: los recorridos son ejes de carretera, no carriles finales. **No se aprueba todavía la ocupación de un solo carril**. Antes de gameplay se deberá definir una trayectoria/desplazamiento por carril y validar nuevamente la envolvente sin cambiar el arte para disimular problemas.

## Evidencia visual y alcance exacto

Se generaron láminas diagnósticas externas al proyecto a partir de los PNG reales y del alfa del sprite, con recortes ampliados por nearest-neighbor. Se superpuso la misma silueta de 18×5.3467 y tangente de Route Preview; el rectángulo magenta indica su envolvente, no forma parte del camión ni del fondo. Estas láminas sí fueron inspeccionadas visualmente: no son únicamente una lista de coordenadas o resultados geométricos. El recorte de silueta usado en las láminas es aproximadamente el UV de previsualización; no modifica el PNG o su pivot.

Se inspeccionaron:
- Los 16 mapas en 0/25/50/75/100%: 80 posiciones visibles.
- R2A/R2B/R2C/R2D/R3F también en 12.5/37.5/62.5/87.5%: 20 posiciones adicionales.
- Tres vértices de mayor cambio de tangente por cada mapa prioritario: 15 posiciones adicionales. Son vértices de polilínea, no mediciones de radio físico.
- Los cinco bordes en recortes apareados a igual escala.

Total: **115 superposiciones inspeccionadas en láminas**. Se abrió Route Preview con el sprite y se seleccionaron los cinco mapas prioritarios, recorriendo sus estados de progreso. La captura nativa final de la ventana apareció parcialmente fuera del área de captura; por ello no se presenta como prueba visual de las 115 posiciones. La evidencia completa es la de las láminas, complementada por las pruebas de ventana y el barrido numérico.

Láminas de revisión (fuera de Assets, no recursos del juego):
- [Cinco mapas prioritarios](C:/Users/angil/.codex/visualizations/2026/11/02/01a19a47-2fd1-7f80-ab93-68b7734d228e/TruckVisual-0.png)
- [S0, R1A, R1B y R3A](C:/Users/angil/.codex/visualizations/2026/11/02/01a19a47-2fd1-7f80-ab93-68b7734d228e/TruckVisual-1.png)
- [R3B, R3C, R3D y R3E](C:/Users/angil/.codex/visualizations/2026/11/02/01a19a47-2fd1-7f80-ab93-68b7734d228e/TruckVisual-2.png)
- [R3G, R3H y DELIVERY](C:/Users/angil/.codex/visualizations/2026/11/02/01a19a47-2fd1-7f80-ab93-68b7734d228e/TruckVisual-3.png)
- [Curvas prioritarias](C:/Users/angil/.codex/visualizations/2026/11/02/01a19a47-2fd1-7f80-ab93-68b7734d228e/TruckVisual-TightCurves.png)
- [Comparación de bordes](C:/Users/angil/.codex/visualizations/2026/11/02/01a19a47-2fd1-7f80-ab93-68b7734d228e/TruckVisual-Borders.png)

## Resultado por mapa

“Pasa en muestras” significa que en las imágenes inspeccionadas el sprite permanece sobre el pavimento y orientado hacia el avance, sin un solape claro con construcciones, taludes, vegetación o guardarraíles. No significa aprobación continua de todos los porcentajes, ocupación correcta de carril ni cabida física certificada.

| Mapa | Resultado visual y advertencias |
|---|---|
| S0 | Pasa en muestras. Recta; vehículo centrado sobre demarcación. Recorte de sprite en 0/100% esperado. |
| R1A | Pasa en muestras de curvas y meseta; el aviso automático al 50% no se confirma como salida de carretera. Pendiente carril y bordes entrante/saliente. |
| R1B | Pasa en muestras; sin inversión de frente en el arco inferior. Pendiente carril. |
| R2A | Pasa en las 12 muestras inspeccionadas. No se observa contacto con guardarraíles en puente ni salida al talud. Revisar aproximación alrededor del 40.7%, 50%, 57.2% al definir carril. |
| R2B | Pasa en las 12 muestras. No se confirma invasión de berma en 50/75%. Cambios discretos de tangente cerca de 38.2/61.7/65.4%; requieren tratamiento de orientación posterior, no aumento de escala. |
| R2C | Pasa en las 12 muestras; la huella no entra en la bahía ni toca la construcción de servicio. En 33.3/37% la orientación por segmento no es una curva suave continua. Pendiente carril. |
| R2D | Pasa en las 12 muestras; sin solape visible con vegetación/talud en 25/75%. En torno a 20.2% la tangente cambia de forma discreta: advertencia de trazado/orientación, no defecto confirmado de tamaño. |
| R3A | Pasa en muestras. Distancia visible a guardarraíl, sin contacto evidente; persisten avisos geométricos locales históricos. |
| R3B | Pasa en muestras; sin contacto evidente con roca o guardarraíl. Pendiente borde de entrada desde R2A y revisión de carril. |
| R3C | Pasa en muestras de arco rural, sin solapes evidentes. |
| R3D | Pasa en muestras de S; cabina sigue el avance, sin inversión. El encajonamiento requiere revisión de carril. |
| R3E | Pasa en muestras; no se observa contacto con drenaje/guardarraíl. Pendiente borde de entrada desde R2C. |
| R3F | Pasa en las 12 muestras; las sombras de 25/75% no son obstáculos ni prueban una salida de vía. Revisados 24.3/71.3/85.9%; no se ve contacto con talud. Conservar advertencia de orientación por segmentos y entrada desde R2C. |
| R3G | Pasa en muestras tras la corrección de zigzags; sin contacto evidente con guardarraíl o instalaciones. |
| R3H | Pasa en muestras de S; sin solape evidente con talud o guardarraíl. El pavimento reparado produce falsas alarmas por color. |
| DELIVERY | Pasa en muestras; final interior sobre concreto vacío. No se observa solape con bodegas ni portería. Falta validar maniobra de descarga con tamaño final. |

**Salidas de carretera confirmadas en estas muestras:** ninguna, exceptuando el recorte de la silueta en el límite del lienzo en 0% de todos los mapas y 100% de los 15 mapas de carretera. Esto último es una transición de pantalla, no que el punto de trayectoria salga de la carretera. No se afirma ausencia de solapes entre muestras.

**Advertencia de carril:** los 15 mapas de carretera necesitan revisión del desplazamiento lateral, ya que la ruta central ocupa frecuentemente la demarcación y no un carril. Es un problema pendiente del uso de la trayectoria, no prueba de que el sprite sea demasiado grande. No se modificaron los puntos aprobados.

## Orientación: visual frente a geométrica

En las láminas se observa que la cabina permanece en el extremo de avance; no se encontraron inversiones de 180°. Como comprobación complementaria se evaluaron 1001 posiciones por mapa: componente X de tangente positiva en todos. Esto comprueba avance de la polilínea, no por sí solo que se haya usado el sprite correcto; esa parte se confirmó al inspeccionar el PNG y las superposiciones.

Máximos saltos de tangente muestreados: R2A 21.0°, R2B 28.3°, R2C 25.3°, R2D 19.4° y R3F 17.7°. Son cambios en vértices de las polilíneas; no son inversiones. Una futura interpolación de orientación debe validarse para no sacar la huella del asfalto. No se implementó en esta etapa.

## Cinco conexiones y gravedad

Se conservan las diferencias estimadas de RoutePreviewReview.md. La gravedad siguiente es una clasificación **visual/artística**, no de seguridad física. El detector por color puede exagerar la variación de ancho al confundir marcas y sombras; la diferencia numérica no equivale exactamente a distancia entre líneas blancas.

| Conexión | Estimación previa: Δcentro / Δancho | Gravedad visual | Observación |
|---|---|---|---|
| S0 → R1A | 2 / 4 px | Baja | Pequeño escalón de líneas y distinta fase de demarcación. No obliga a cambiar trayectoria. |
| R1A → R2A | 1 / 6 px | Baja–media | Cambio visible de textura/demarcación y aparición de guardarraíl; no se confirma a ojo toda la diferencia de ancho estimada. |
| R2A → R3B | 4 / 6 px | Media, prioridad mayor | Escalón de altura entre los extremos claramente visible. Corregir arte o definir transición antes de exigir encaje continuo. |
| R2C → R3E | 1 / 8 px | Baja–media | Cambio de berma/guardarraíl y demarcación; las líneas blancas parecen más próximas entre sí de lo que sugiere la métrica de color. Requiere medir contornos antes de retocar. |
| R2C → R3F | .5 / 5 px | Baja | Cambio de sombras y demarcación; no se confirma un salto grave de calzada en los recortes. |

Los datos nominales de las cinco conexiones mantienen altura .5, dirección este y ancho 32 sobre 640×360. Esas coincidencias geométricas no corrigen los píxeles del borde. No se forzaron extremos para ocultar arte distinto.

## Clasificación de problemas y decisión de prototipo

- **Trayectoria/uso de carril:** eje central en los 15 mapas de carretera y orientación por segmentos en curvas; falta validar desplazamiento y envolvente continua. No se observó una trayectoria invertida.
- **Escala del vehículo:** conservar 18×5.35 como escala visual de prototipo. No hay un caso de tamaño excesivo confirmado en las muestras a escala 1. Falta una referencia métrica acordada; no puede afirmarse cabida física.
- **Arte del fondo:** cinco bordes pendientes, especialmente R2A→R3B; son independientes de la escala del camión.
- **Perspectiva y luces:** el camión cenital contrasta moderadamente con fachadas oblicuas. Una sola orientación rotada conserva sombras pintadas que giran con el vehículo. La fuente generada y su alfa requieren acabado nativo pixel art para entrega final.
- **Pendiente para gameplay:** escoger carril, validar radio de giro y pivote de SpriteRenderer, máscaras semánticas de vía/obstáculos o revisión equivalente, transiciones y maniobra en DELIVERY. No se han implementado.

Se mantuvieron intactos el sprite, los fondos, las miniaturas y los ScriptableObjects. Solo se actualiza este documento dentro del proyecto. Las láminas son archivos auxiliares externos y no recursos de gameplay.

---

## Historial de la primera validación (automatizada y visual parcial)

## Recurso y perspectiva

Se consultaron RoutePreviewGuide.md y CorreccionTrayectorias.md y se inspeccionaron los PNG reales S0, R1A, R1B, R2A, R3B y R3G. Los fondos usan una cámara elevada con fachadas parcialmente visibles. Se eligió un camión principalmente cenital, frente hacia el este, caja cerrada marfil, cabina blanca/azul y sombreado contenido en la silueta. Es un vehículo genérico sin logos, texto o conductor visible. El techo predomina para poder girarlo sobre las carreteras; no se dibujó una sombra exterior permanente.

Creado mediante la herramienta integrada de generación de imágenes con S0/R1A/R3G como referencias PNG de estilo. El primer resultado era demasiado lateral y fue sustituido por una edición cenital. Prompt final: vista ortográfica desde arriba del techo, caja rectangular izquierda y cabina derecha, ruedas como pequeñas protuberancias, colores claros, pixel art 16-bit, alfa real, sin suelo, sombra exterior, marcas, texto ni personajes. Solo el resultado cenital se conserva en el proyecto.

Archivo: Assets/Mapas/Vehiculos/CamionCarga.png. Lienzo fuente 2022×778 px. Silueta con alfa >8: 1249×371 px (relación 3.3666:1). Alfa comprobado: 1 140 637 píxeles totalmente transparentes, 1 309 totalmente opacos y 431 170 con alfa intermedio. El fondo no es blanco ni un damero dibujado. El alfa intermedio incluye contorno y superficies casi opacas: conviene revisar el halo y limpiar píxeles antes de arte final. La fuente generada no equivale a una cuadrícula nativa artesanal de 52×20; su reducción es una prueba visual.

## Importación y previsualización

Sprite Single, Point, Uncompressed, sin mipmaps, NPOT None, máximo 2048, alfa FromInput y alphaIsTransparency activado. Pivot centrado en el lienzo fuente: (1011,389) px, equivalente a (.5,.5). No se crearon prefab, GameObject, material o controlador de gameplay.

Route Preview admite selector de Sprite, escala, corrección de orientación base, activación del vehículo, huella diagnóstica opcional y ancho/largo editables. El botón de conservar proporción ajusta ancho a partir de la silueta visible. El margen transparente se excluye solo del dibujo de Editor mediante UV: no se recorta ni reescribe el PNG ni su pivot. El dibujo se centra sobre la trayectoria y rota con su tangente; 0° es el frente hacia la derecha. Si se cambia la corrección angular, la huella usa también esa orientación.

Hipótesis principal: largo 18 px y ancho 5.3467 px en cuadrícula lógica 480×270; escala 1, orientación base 0°, desplazamiento lateral 0. El sprite conserva su proporción visible. Equivale a una silueta aproximada 72×21.4 px sobre un fondo exportado 1920×1080. El carril nominal es 12 px lógicos (vía 24); la relación de anchos es 44.6%. Esta comparación de píxeles NO valida escala física. El vehículo puede parecer pequeño respecto a ciertas construcciones; se debe acordar una referencia de ancho de carril o longitud real antes de aprobar tamaño definitivo.

Prueba adicional de sensibilidad: escala 3 y corrección 45° producen 54×16.04 px lógicos y una invasión visible de bermas en S0. Es deliberadamente inadecuada y se restauró escala 1 / 0°. Se inspeccionó el dibujo sobre S0 y R3H en la UI de Unity, y se comprobó que el slider desplaza el sprite real (por ejemplo 50%→28.1% en S0), no solo la huella provisional.

## Pruebas de los 16 mapas

Se seleccionaron los 16 mapas en Route Preview y se comprobaron 0%, 25%, 50%, 75% y 100%: 80 estados. En los 80 coincide la posición del vehículo con la evaluación por distancia de la polilínea; la orientación procede de la tangente y conserva frente hacia avance con base 0°. La cobertura automatizada no debe confundirse con 80 aprobaciones visuales humanas.

Tabla de muestreo de huella y color: OK = sin alerta de color en esa posición; C = posible solape por color (no confirmado); B = huella parcial fuera de pantalla en la entrada/salida. No representa una máscara semántica de obstáculos.

| Mapa | 0% | 25% | 50% | 75% | 100% | Orientación 25/50/75 (°) |
|---|---|---|---|---|---|---|
| S0 | B | OK | OK | OK | B | 0.0 / 0.0 / 0.0 |
| R1A | B | OK | C | OK | B | 31.7 / 0.0 / -22.4 |
| R1B | B | OK | OK | OK | B | -33.3 / 0.0 / 34.0 |
| R2A | B | OK | C | OK | B | 38.2 / -34.0 / 0.0 |
| R2B | B | OK | C | C | B | -29.4 / 44.5 / -32.3 |
| R2C | B | C | C | C | B | 35.1 / -44.5 / 29.4 |
| R2D | B | C | OK | C | B | -40.2 / 12.7 / 15.7 |
| R3A | B | OK | OK | OK | B | 26.6 / 0.0 / -27.7 |
| R3B | B | OK | OK | OK | B | 32.0 / 0.8 / -27.2 |
| R3C | B | OK | OK | OK | B | -30.0 / -0.7 / 30.5 |
| R3D | B | OK | OK | OK | B | 36.9 / -51.4 / 29.4 |
| R3E | B | OK | OK | OK | B | 33.1 / -26.9 / 3.2 |
| R3F | B | C | OK | C | B | -37.9 / 47.5 / -35.2 |
| R3G | B | OK | OK | OK | B | -20.1 / 3.2 / 18.6 |
| R3H | B | OK | OK | OK | B | 29.4 / -42.6 / 22.4 |
| DELIVERY | B | OK | OK | OK | OK | 0.0 / 2.0 / 6.0 |

Los B en 0/100% son esperables al centrar el camión justo en el borde del mapa. En DELIVERY el final es interior y no hay recorte final. No se cambió ninguna trayectoria para ocultar esos recortes.

### Barrido adicional y comparación de carril

201 posiciones por mapa, misma huella. La columna lateral usa −6 px como hipótesis de carril derecho, sin escribir esos valores a los ScriptableObjects. Los números son muestras sospechosas por color, no colisiones.

| Mapa | Centro de vía: huella sospechosa /201 | Lateral −6: huella sospechosa /201 |
|---|---:|---:|
| S0 | 15 | 8 |
| R1A | 39 | 8 |
| R1B | 44 | 9 |
| R2A | 75 | 33 |
| R2B | 60 | 30 |
| R2C | 60 | 16 |
| R2D | 48 | 8 |
| R3A | 35 | 11 |
| R3B | 32 | 11 |
| R3C | 29 | 8 |
| R3D | 37 | 8 |
| R3E | 27 | 8 |
| R3F | 52 | 10 |
| R3G | 51 | 13 |
| R3H | 64 | 9 |
| DELIVERY | 8 | 5 |

## Hallazgos y ajustes pendientes

- Prioridad de inspección: R2A alrededor del 50% (curva/puente), R2B 50% y 75%, R2C 25/50/75%, R2D 25/75% y R3F 25/75% (sombras). Hay posibles solapes de la huella con bordes, marcas y texturas; no se afirma invasión real de berma solo por la clasificación de color.
- R1A al 50% también da una alerta de color sobre la zona central. S0, R1B, R3A/B/C/D/E/G/H y DELIVERY no dan alertas de color en 25/50/75% en este muestreo de cinco posiciones, pero el barrido completo detecta otras posiciones que revisar.
- No hay una escala físicamente validada que permita declarar cualquier mapa aprobado o el sprite demasiado grande en él. La escala 3 sí excede el ancho nominal de un carril en todos los mapas de carretera; es solo una demostración de sensibilidad de la herramienta.
- Las trayectorias suelen recorrer la línea amarilla, no un carril. Validar el desplazamiento lateral definitivo y la huella completa en curvas. Las pruebas laterales reducen alertas, pero no certifican cabida ni ausencia de solapes con árboles, casas o guardarraíles.
- El sprite es cenital y los edificios tienen perspectiva parcialmente oblicua; al girar el vehículo una sola imagen no cambia de perspectiva. Las luces y sombras pintadas sobre el techo giran con el sprite, por lo que no mantienen una dirección mundial perfecta. Considerar sprites por orientación o una solución de iluminación posterior.
- A 18 px de largo lógico se pierden detalles de cabina y caja; revisar a zoom 1 o superior y decidir resolución nativa del arte final. El filtro Point evita desenfoque de textura, pero las rotaciones arbitrarias producen escalones y aliasing. No se aplicó suavizado a los fondos.
- El dibujo de UV recortado del Editor no sustituye el ajuste futuro de pivot al usar el recurso en un SpriteRenderer. La huella rectangular es conservadora y no usa el contorno alfa exacto; no modela remolque, ejes ni radio de giro.
- DELIVERY termina sobre el patio libre, pero se debe revisar la huella completa, la descarga y una futura maniobra con el vehículo definitivo.

## Alcance de la validación

Unity compila sin errores y ValidadorContinuidad pasa. Se comprobó alfa, dimensiones, importación, selección de sprite, recorrido del slider y rotación. La revisión de color es heurística: no distingue rocas o tejados grises del asfalto y puede confundir sombras o marcas. No hay máscaras semánticas, colisiones ni referencia métrica. Los PNG, miniaturas y 16 .asset aprobados se verificaron por hash antes/después; deben permanecer idénticos. No se programó conducción, movimiento de gameplay, preguntas, decisiones ni puntuación.
