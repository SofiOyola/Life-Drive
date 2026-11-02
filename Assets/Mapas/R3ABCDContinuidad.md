# R3A–R3D · prototipos visuales

Esta entrega sustituye únicamente los conceptos preliminares de R3A–R3D en ContinuidadVisual.md. Se leyeron también R2Continuidad.md y las definiciones de padres e hijos. No cambia los siete fondos anteriores, sus trayectorias, los otros R3, DELIVERY ni las asociaciones numéricas del motor.

## Creación y estilo

Generación con la herramienta integrada de imágenes, utilizando los PNG aprobados R2A y R2B como referencias visuales explícitas. Cada fondo se redujo a cuadrícula lógica 480×270 y se exportó a 1920×1080 con nearest-neighbor; miniatura 480×270. Importación Sprite individual, Point, sin compresión, sin mipmaps, sin reescalado NPOT, máximo 2048.

Prompts: misma escala, perspectiva cenital con fachadas parcialmente visibles, vegetación tropical/andina colombiana, verdes oliva, asfalto oscuro, centro amarillo y bordes blancos, iluminación natural del noroeste. Una carretera continua, entrada izquierda y salida derecha a media altura; excluir vehículos, personas, animales, texto, HUD, clima dinámico, accidentes y obstáculos móviles. Las particularidades de cada prompt corresponden a los cuatro conceptos siguientes.

## Conceptos y conexiones

- R3A — Corredor prudente: bosque húmedo y andino, curva amplia, guardarraíles e infraestructura de drenaje lateral. Padre R2A.
- R3B — Montaña de alta exposición: curva más pronunciada, paredes de roca estables y guardarraíles; vía despejada, sin derrumbes activos. Padre R2A.
- R3C — Vía rural abierta: fincas, cultivos, potreros, cercas, árboles y espacio lateral abierto para futuros sprites independientes. Padre R2B.
- R3D — Vía rural exigente: doble curva, vegetación más cerrada, taludes, bermas reducidas y línea central continua. La dificultad se expresa principalmente por el encajonamiento; el enlace conserva el ancho nominal de su padre. Padre R2B.

Los cuatro entran en (0,0.5) y salen en (1,0.5), rumbo este. Se conserva ancho nominal 32 sobre 640×360, equivalente a 24 sobre 480×270. Los extremos conservan tangentes horizontales. La única conexión posterior de cada mapa es DELIVERY, mediante futura transición de escena: no se dibujan convergencias físicas ni se implementa dicha transición.

## Trayectorias actualizadas

Las seis posiciones preliminares se sustituyeron en cada nuevo ScriptableObject por 61 puntos normalizados, Y hacia arriba. Se trazaron guías visuales del eje sobre el PNG y se ajustaron mediante detección de líneas laterales blancas; para R3D se afinó con la línea central amarilla continua. Los puntos exactos se conservan en el campo trayectoria de cada .asset. El primer y último enlace mantienen la posición nominal documentada.

La polilínea representa el centro de la carretera, no el centro de un carril. No contiene gameplay, física, colliders ni movimiento. Una futura interpolación deberá permanecer dentro del asfalto y evitar sobrepasar las curvas; no se ha validado ningún spline futuro.

Comprobación final de bordes, columna por columna sobre la miniatura:

| Mapa | Columnas con ambos bordes detectados | Distancia mínima eje-borde detectado |
|---|---:|---:|
| R3A | 480/480 | 11 px lógicos |
| R3B | 472/480 | 8 px lógicos |
| R3C | 475/480 | 11 px lógicos |
| R3D | 480/480 | 13 px lógicos |

Las columnas no clasificadas automáticamente en R3B/R3C se deben a variaciones de textura y demarcación; no constituyen una máscara certificada de transitabilidad. Se inspeccionaron visualmente los PNG completos. Los recorridos no presentan salidas visibles del asfalto. El espacio efectivo de un camión requiere verificar su sprite, escala, carril y giro cuando existan.

## Limitaciones

Son prototipos generados con IA, no arte final dibujado a mano. Mantienen la perspectiva parcialmente oblicua ya aprobada, no una proyección ortográfica estricta. Existen pequeñas diferencias de ancho y posición del raster respecto al enlace nominal; continuidad entre paneles, no un mosaico exacto píxel a píxel. La tolerancia de centro prevista es de hasta 3 píxeles sobre la cuadrícula lógica. La geometría y los PNG de los predecesores no se modifican para compensar variaciones. La escala ambiental es coherente visualmente, no métrica; ajustar el sprite futuro antes de certificar gálibo. R3D no debe interpretarse como un cambio de ancho del motor.
