# R3E–R3H · continuidad de prototipos

Estos conceptos sustituyen únicamente los conceptos preliminares R3E–R3H de ContinuidadVisual.md. Se consultaron ese documento, R2Continuidad.md, R3ABCDContinuidad.md y los cuatro ScriptableObjects. Se inspeccionaron R2C/R2D y los cuatro R3 anteriores. No se modifican fondos ni trayectorias anteriores, asociaciones del motor, escenas ni gameplay.

## Creación

Herramienta integrada de generación de imágenes con PNG de referencia: R2C + R3D para R3E/R3F; R2D + R3A para R3G/R3H. Prompts: mismo universo pixel art 16-bit, vegetación colombiana, verdes oliva, escala ambiental y perspectiva elevada con fachadas parcialmente visibles; asfalto oscuro, centro amarillo y bordes blancos, luz natural del noroeste. Entrada izquierda y salida derecha a media altura, carriles despejados, sin vehículos, personas, animales, texto, interfaz, accidentes ni clima dinámico. Cada prompt incorpora el paisaje descrito abajo.

Los cuatro fondos se normalizaron a cuadrícula 480×270 y exportaron a 1920×1080 con nearest-neighbor; miniaturas 480×270. Importación Sprite individual, Point, Uncompressed, sin mipmaps, NPOT None, máximo 2048.

## Fondos

| Mapa | Paisaje y vía | Predecesor | Trayectoria guardada |
|---|---|---|---:|
| R3E · Descenso después del descanso | Descenso hacia valle verde expresado por terrazas y taludes; curva moderada, guardarraíles, drenajes y vegetación andina. | R2C | 29 puntos |
| R3F · Descenso con fatiga | Curvas repetidas, vegetación cerrada, sombras, taludes y señales preventivas sin texto. La fatiga no está dibujada como objeto permanente. | R2C | 33 puntos |
| R3G · Corredor después de la inspección | Curva suave, bodegas rurales e infraestructura de servicio fuera de la calzada; entorno de operación controlada. | R2D | 60 puntos |
| R3H · Corredor de mayor exposición | Doble curva, pavimento envejecido, bermas estrechas, bodegas dispersas y terreno irregular fuera de la vía; asfalto despejado. | R2D | 39 puntos |

## Enlaces y trayectorias

Todos conservan entrada (0,0.5), salida (1,0.5) y tangentes horizontales rumbo este. Ancho nominal conservado: 32 px sobre 640×360, equivalente a 24 px sobre 480×270. El único siguiente es DELIVERY, mediante futura transición de escena; no hay convergencias físicas entre R3. El descenso se sugiere por el terreno, no por un cambio de proyección o escala.

Las posiciones exactas se guardan en trayectoria de cada .asset, normalizadas con Y hacia arriba. Se trazaron guías visuales sobre los PNG y se refinaron con las líneas laterales; en R3F se usó la línea amarilla para evitar el efecto de las sombras y se interpolaron los huecos entre marcas. Se suavizó el muestreo y se simplificó adaptativamente con error vertical máximo de 0.6 px lógicos respecto al eje muestreado; no se impuso un número fijo de puntos. La cantidad mayor de R3G refleja variaciones locales del raster y no una mayor dificultad de gameplay.

Las polilíneas son ejes de carretera, no carriles ni movimiento ejecutable. La futura animación deberá elegir carril, escala del sprite y una interpolación que no sobrepase los bordes. No hay colliders ni programación de movimiento.

## Comprobación raster y límites

Detección final de ambas líneas blancas por columna en la miniatura: R3E 480/480, R3F 327/480, R3G 476/480 y R3H 473/480. Distancia mínima detectada eje-borde: 10, 11, 10 y 12 px lógicos, respectivamente. R3F tiene menor cobertura automática porque las sombras oscurecen las líneas; se afinó usando 248 columnas de demarcación amarilla y se inspeccionó visualmente el PNG. Esta comprobación de color no certifica una máscara transitable ni el gálibo de un camión futuro.

Son prototipos generados con IA y posprocesados para exportación pixel art. Conservan la perspectiva parcialmente oblicua aprobada, no una proyección cenital matemática. Persisten pequeñas diferencias de centro/ancho del raster en los bordes respecto al contrato nominal; no deben tratarse como un mosaico exacto píxel a píxel. El enlace nominal permanece dentro de la vía. Las pendientes y montañas se representan como terrazas y relieve local sin horizonte; no existe altura física ni longitud métrica del descenso. La escala de bodegas y vegetación es visual y deberá contrastarse con el sprite final. Ningún PNG anterior se ajustó para compensar esas diferencias.
