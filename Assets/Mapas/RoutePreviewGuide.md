# Route Preview · revisión de los 16 mapas

Abrir Tools > Life Drive > Route Preview. Elegir mapa, ajustar largo/ancho en píxeles de la cuadrícula 480×270 y recorrer con el slider 0–100%. Cabina azul: frente del camión provisional. Huella amarilla: sin alerta local de color, no aprobación física. Huella roja: revisar posible solape o salida del lienzo. Usar zoom y barras de desplazamiento para inspeccionar toda la imagen. Trayectoria y puntos se pueden ocultar de forma independiente; entrada verde, puntos cian, final naranja y zona de entrega magenta.

El camión es una representación dibujada por el Editor, independiente del PNG; no crea GameObjects, escenas, colliders, sprites definitivos ni movimiento de gameplay. La posición avanza por distancia acumulada y la orientación usa la tangente de cada segmento. Se muestran deliberadamente los defectos del recorrido original: no se suavizan ni ocultan los giros ni se impide el slider por una clasificación incierta de color.

El control de desplazamiento lateral permite comparar carriles. Cero sigue el eje almacenado, habitualmente la línea central de la carretera; por sí solo no prueba que se circule dentro de un único carril. Las dimensiones iniciales 18×7 px son solo una hipótesis de revisión, no una escala de camión aprobada.

## Verificaciones realizadas

- Unity compiló los dos scripts nuevos sin errores.
- Se seleccionaron los 16 recursos en la ventana y se probaron 0%, 50% y 100%: entrada y final coinciden con los datos, posición intermedia cambia y tangente tiene longitud unitaria.
- Se probaron las capas de trayectoria y puntos. En UI nativa se comprobó el desplazamiento visible del camión al cambiar el slider y la desaparición de puntos al desmarcar Puntos; se inspeccionaron los overlays de R3F y R3G.
- Prueba sintética en memoria detectó punto fuera de imagen, segmento largo, curva cerrada y ancho excesivo. No se guardó ningún recurso de prueba.
- Se analizaron 201 posiciones por mapa con huella 18×7, desplazamiento lateral cero. Informe reproducible en RoutePreviewReview.md.
- Se revisaron las 22 conexiones: 14 enlaces de carretera y 8 transiciones de escena a DELIVERY. La ventana muestra los recortes finales/iniciales de cada conexión del mapa seleccionado, tanto entrantes como salientes. Las comparaciones raster son estimaciones por color, con tolerancia de 3 px lógicos en altura y ancho.
- ValidadorContinuidad mantiene resultado vacío. Console sin errores de compilación/GUI durante las pruebas; puede conservar la advertencia WebSocket del MCP.

## Prioridades de corrección y revisión

1. **Trayectorias R3G, R3E y R3H:** numerosos segmentos diminutos con cambios bruscos de dirección; R3G llega a un giro local aproximado de 93°. Requieren depurar sus puntos antes de animar un camión. En R3G se observó el zigzag superpuesto en Unity. No confundir ruido de trazado con curvas reales del fondo.
2. **R1A y R1B:** solo seis puntos para curvas amplias. Revisar el ajuste entre segmentos rectos y asfalto, especialmente con la huella completa. S0 tiene segmentos largos pero su carretera es recta: esa alerta aislada no justifica corregirlo.
3. **R3A, R3B y R3F:** alertas locales de radio/giro que requieren inspección con el tamaño final; no prueban una imposibilidad de paso.
4. **Bordes candidatos a corrección artística**, sujetos a confirmación visual: S0→R1A, R1A→R2A, R2A→R3B, R2C→R3E y R2C→R3F. La medición del raster supera 3 px en ancho o centro. No se modificaron PNG para compensar.
5. **R2A/R2B/R2C/R2D y resto de mapas:** revisar las posiciones sospechosas por color del informe, incluso cuando no existan alertas geométricas. Puentes, sombras, marcas y zonas grises pueden producir falsos positivos. DELIVERY necesita comprobar la huella completa y el punto de anclaje del sprite dentro del patio, no solo su punto final.

## Lo que esta herramienta no certifica

No existen máscaras semánticas de asfalto, carriles y obstáculos. Un techo, talud gris o guardarraíl puede compartir colores con la carretera; las muestras de huella son discretas y no analizan todos los píxeles de su interior. La herramienta señala posibles solapes, pero no evita automáticamente atravesar árboles, casas o guardarraíles. Eso requiere revisión humana o máscaras explícitas posteriores. La comprobación de puntos dentro del lienzo, la clasificación por color y la cabida física son tres afirmaciones distintas.

Los extremos de rutas pueden mostrar parte del camión fuera del lienzo por la entrada/salida. El radio estimado es diagnóstico de polilínea, no un modelo cinemático de tractocamión o remolque. Ningún fondo, trayectoria, asociación de motor o dato de partida se modifica al revisar. El botón de revisión completa solo actualiza RoutePreviewReview.md; no envía advertencias a Console para evitar contaminarla.
