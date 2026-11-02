# Life Drive · revisión Route Preview

Huella 18×7 px lógicos; desplazamiento lateral 0 px. Progreso por longitud de polilínea.

## Geometría y estimación visual por mapa

- S0 · Geometría: segmento 0–1 largo (86px): revisar salto/escasez de puntos; segmento 1–2 largo (96px): revisar salto/escasez de puntos; segmento 2–3 largo (115px): revisar salto/escasez de puntos; segmento 3–4 largo (96px): revisar salto/escasez de puntos; segmento 4–5 largo (86px): revisar salto/escasez de puntos · Color PNG (estimación): centro 4/201, huella 15/201 sospechosos; 8 huellas fuera del lienzo.
- R1A · Geometría: sin alertas · Color PNG (estimación): centro 9/201, huella 31/201 sospechosos; 8 huellas fuera del lienzo.
- R1B · Geometría: sin alertas · Color PNG (estimación): centro 11/201, huella 36/201 sospechosos; 8 huellas fuera del lienzo.
- R2A · Geometría: sin alertas · Color PNG (estimación): centro 19/201, huella 63/201 sospechosos; 8 huellas fuera del lienzo.
- R2B · Geometría: sin alertas · Color PNG (estimación): centro 15/201, huella 57/201 sospechosos; 8 huellas fuera del lienzo.
- R2C · Geometría: sin alertas · Color PNG (estimación): centro 14/201, huella 51/201 sospechosos; 8 huellas fuera del lienzo.
- R2D · Geometría: sin alertas · Color PNG (estimación): centro 7/201, huella 44/201 sospechosos; 8 huellas fuera del lienzo.
- R3A · Geometría: curva 23: giro 18°, radio estimado 25px; curva 35: giro 19°, radio estimado 25px · Color PNG (estimación): centro 6/201, huella 33/201 sospechosos; 8 huellas fuera del lienzo.
- R3B · Geometría: curva 13: giro 18°, radio estimado 27px · Color PNG (estimación): centro 9/201, huella 30/201 sospechosos; 8 huellas fuera del lienzo.
- R3C · Geometría: sin alertas · Color PNG (estimación): centro 4/201, huella 26/201 sospechosos; 8 huellas fuera del lienzo.
- R3D · Geometría: sin alertas · Color PNG (estimación): centro 1/201, huella 23/201 sospechosos; 8 huellas fuera del lienzo.
- R3E · Geometría: sin alertas · Color PNG (estimación): centro 6/201, huella 26/201 sospechosos; 8 huellas fuera del lienzo.
- R3F · Geometría: curva 8: giro 18°, radio estimado 18px · Color PNG (estimación): centro 12/201, huella 47/201 sospechosos; 8 huellas fuera del lienzo.
- R3G · Geometría: sin alertas · Color PNG (estimación): centro 9/201, huella 40/201 sospechosos; 8 huellas fuera del lienzo.
- R3H · Geometría: sin alertas · Color PNG (estimación): centro 12/201, huella 48/201 sospechosos; 8 huellas fuera del lienzo.
- DELIVERY · Geometría: sin alertas · Color PNG (estimación): centro 1/201, huella 8/201 sospechosos; 5 huellas fuera del lienzo.

## Conexiones · estimación raster

- S0 → R1A: ADVERTENCIA · Δcentro 2,0px, Δancho 4,0px (umbral 3px).
- S0 → R1B: dentro de tolerancia estimada · Δcentro 1,0px, Δancho 2,0px (umbral 3px).
- R1A → R2A: ADVERTENCIA · Δcentro 1,0px, Δancho 6,0px (umbral 3px).
- R1A → R2B: dentro de tolerancia estimada · Δcentro 1,0px, Δancho 2,0px (umbral 3px).
- R1B → R2C: dentro de tolerancia estimada · Δcentro 1,5px, Δancho 3,0px (umbral 3px).
- R1B → R2D: dentro de tolerancia estimada · Δcentro 0,0px, Δancho 0,0px (umbral 3px).
- R2A → R3A: dentro de tolerancia estimada · Δcentro 1,0px, Δancho 0,0px (umbral 3px).
- R2A → R3B: ADVERTENCIA · Δcentro 4,0px, Δancho 6,0px (umbral 3px).
- R2B → R3C: dentro de tolerancia estimada · Δcentro 3,0px, Δancho 0,0px (umbral 3px).
- R2B → R3D: dentro de tolerancia estimada · Δcentro 1,5px, Δancho 1,0px (umbral 3px).
- R2C → R3E: ADVERTENCIA · Δcentro 1,0px, Δancho 8,0px (umbral 3px).
- R2C → R3F: ADVERTENCIA · Δcentro 0,5px, Δancho 5,0px (umbral 3px).
- R2D → R3G: dentro de tolerancia estimada · Δcentro 0,0px, Δancho 0,0px (umbral 3px).
- R2D → R3H: dentro de tolerancia estimada · Δcentro 1,0px, Δancho 2,0px (umbral 3px).
- R3A → DELIVERY: transición de escena; no requiere encaje de bordes.
- R3B → DELIVERY: transición de escena; no requiere encaje de bordes.
- R3C → DELIVERY: transición de escena; no requiere encaje de bordes.
- R3D → DELIVERY: transición de escena; no requiere encaje de bordes.
- R3E → DELIVERY: transición de escena; no requiere encaje de bordes.
- R3F → DELIVERY: transición de escena; no requiere encaje de bordes.
- R3G → DELIVERY: transición de escena; no requiere encaje de bordes.
- R3H → DELIVERY: transición de escena; no requiere encaje de bordes.

## Límites de la revisión

Los segmentos largos indican escasez de puntos, no necesariamente teletransporte. El radio es una aproximación de polilínea: vértices y ruido pueden producir falsos positivos. La clasificación por color no distingue asfalto de tejados o rocas grises, ni reconoce guardarraíles o árboles semánticamente. Sombras y marcas pueden producir falsos positivos. Las huellas que salen en 0/100% pueden estar parcialmente fuera de pantalla por la entrada/salida del sprite. El eje almacenado suele ser la línea central de dos carriles; seleccionar desplazamiento lateral antes de certificar un carril. No existen máscaras de superficie, sprite final, colliders ni validación física. DELIVERY usa transición de escena y el patio tiene una clasificación más permisiva. Ningún PNG o .asset se corrige automáticamente. Revisar todas las alertas visualmente; no aprobar la cabida solo con límites o colores.