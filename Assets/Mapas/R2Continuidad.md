# R2 · fondos prototipo y continuidad medida

Los conceptos de esta entrega sustituyen los conceptos preliminares R2 del documento ContinuidadVisual.md. Solo cambian los cuatro R2; referencias aprobadas S0/R1 intactas. Imágenes generadas con IA usando los tres PNG aprobados como referencias visuales. Normalizadas a 480×270 y exportadas por nearest-neighbor a 1920×1080; miniaturas 480×270. Son prototipos, no arte final ni gameplay.

## Enlaces

Todos entran por izquierda (0,.5), salen por derecha (1,.5), tangentes horizontales hacia este. Ancho nominal conservado: 32 sobre 640×360, equivalente a 24 sobre 480×270. El raster generado presenta diferencias de pocos píxeles en centro/ancho; continuidad visual entre paneles, no mosaico de precisión píxel a píxel. Tolerancia de centro de 3 píxeles lógicos respecto al enlace nominal. Antes de arte final será necesario igualar los bordes exactamente. No se alteraron PNG aprobados para compensar esas diferencias.

R1A → R2A/R2B; R1B → R2C/R2D.

Salidas: R2A → R3A/R3B; R2B → R3C/R3D; R2C → R3E/R3F; R2D → R3G/R3H. Los R3 deberán entrar por izquierda a media altura. Ninguna bifurcación dibujada es necesaria.

## Recorridos trazados sobre imágenes

Coordenadas normalizadas, Y hacia arriba. Las siguientes polilíneas reemplazan los seis puntos preliminares para estos cuatro mapas. Son guías del eje de vía, no un carril específico ni animación. La futura curva suavizada debe conservarse dentro del asfalto; no aplicar splines que sobrepasen los puntos en las curvas. No hay colliders ni movimiento.

### R2A · Corredor principal

(0, 0.5) → (0.1, 0.5) → (0.14, 0.5) → (0.2, 0.54) → (0.25, 0.61) → (0.3, 0.67) → (0.35, 0.695) → (0.4, 0.69) → (0.45, 0.65) → (0.5, 0.59) → (0.55, 0.535) → (0.6, 0.518) → (0.65, 0.51) → (0.7, 0.5) → (0.75, 0.5) → (0.8, 0.5) → (0.85, 0.5) → (0.9, 0.5) → (1, 0.5)

### R2B · Atajo rural

(0, 0.5) → (0.1, 0.5) → (0.17, 0.49) → (0.22, 0.465) → (0.27, 0.415) → (0.32, 0.365) → (0.36, 0.35) → (0.4, 0.355) → (0.44, 0.4) → (0.48, 0.47) → (0.52, 0.54) → (0.56, 0.6) → (0.6, 0.635) → (0.64, 0.64) → (0.68, 0.62) → (0.72, 0.575) → (0.76, 0.53) → (0.8, 0.505) → (0.85, 0.5) → (0.9, 0.5) → (1, 0.5)

### R2C · Zona de servicio

(0, 0.5) → (0.1, 0.5) → (0.17, 0.5) → (0.22, 0.54) → (0.26, 0.59) → (0.3, 0.64) → (0.34, 0.66) → (0.38, 0.655) → (0.42, 0.615) → (0.46, 0.545) → (0.5, 0.475) → (0.54, 0.42) → (0.58, 0.385) → (0.62, 0.375) → (0.66, 0.38) → (0.7, 0.4) → (0.74, 0.44) → (0.78, 0.48) → (0.82, 0.5) → (0.9, 0.5) → (1, 0.5)

### R2D · Corredor rural industrial

(0, 0.5) → (0.1, 0.5) → (0.17, 0.5) → (0.21, 0.48) → (0.25, 0.43) → (0.29, 0.37) → (0.33, 0.33) → (0.37, 0.315) → (0.41, 0.315) → (0.45, 0.325) → (0.5, 0.345) → (0.55, 0.365) → (0.6, 0.395) → (0.65, 0.43) → (0.7, 0.47) → (0.75, 0.495) → (0.8, 0.5) → (0.9, 0.5) → (1, 0.5)

## Características y comprobaciones

- R2A: carretera de montaña, guardarraíles y puente de ancho completo. El eje asciende, desciende suavemente al puente y termina horizontal.
- R2B: doble curva rural entre fincas, cercas y quebrada. La quebrada queda fuera de la carretera.
- R2C: doble curva con bahía de inspección y construcción de servicio; el patio queda fuera de los carriles principales. La trayectoria principal no entra en la bahía.
- R2D: curva baja con bodegas rurales, taludes y reparaciones discretas sobre el asfalto.

Validación geométrica por muestreo de las líneas blancas en 480 columnas: se reconocieron ambos bordes en 475/480 columnas de R2A, 479/480 de R2B, 475/480 de R2C y 477/480 de R2D. Distancia mínima detectada desde eje a borde: 5, 8, 9 y 11 píxeles lógicos respectivamente. Las columnas restantes no se clasifican automáticamente por textura/puente y requieren inspección visual; la prueba no es una máscara certificada de superficie transitable. Se revisaron visualmente los cuatro PNG. El ancho es coherente con un futuro sprite de camión ajustado a la escala; no existe todavía sprite ni prueba dinámica que certifique su gálibo. La perspectiva ligeramente oblicua de edificios conserva el lenguaje de S0.
