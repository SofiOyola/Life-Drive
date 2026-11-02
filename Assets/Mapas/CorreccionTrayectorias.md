# Life Drive · corrección de trayectorias visuales

Se consultaron ContinuidadVisual.md, R2Continuidad.md, R3ABCDContinuidad.md, R3EFGHContinuidad.md, DELIVERYContinuidad.md, RoutePreviewReview.md y RoutePreviewGuide.md. Esta revisión sustituye los datos históricos de puntos de los cinco mapas corregidos, sin cambiar los conceptos visuales.

## Diagnóstico original y cambios

Se mostraron en Route Preview los cinco trazados originales sobre sus PNG. R3G tenía zigzags generalizados; R3E tenía ruido después del descenso; R3H tenía dos grupos de quiebres diminutos. R1A y R1B usaban seis puntos y segmentos de hasta 115 px para curvas reales.

Las correcciones son redibujados de polilínea con puntos de control inspeccionados contra el eje visible del asfalto; no se aplicó un spline ni un filtro de suavizado automático sobre los datos. Se mantuvieron entrada (0,.5), salida (1,.5), tangentes horizontales, IDs, imágenes, ancho nominal, conexiones y resto de campos. Los segmentos interpolan linealmente entre puntos, tal como Route Preview los muestra. No se modificaron los otros once mapas.

| Mapa | Puntos antes → después | Giro local máximo después | Resultado geométrico |
|---|---:|---:|---|
| R3G | 60 → 23 | 11.1° | Sin alertas |
| R3E | 29 → 25 | 13.4° | Sin alertas |
| R3H | 39 → 30 | 14.5° | Sin alertas |
| R1A | 6 → 23 | 10.5° | Sin alertas |
| R1B | 6 → 23 | 18.8° | Sin alertas |

R3G: se sustituyeron las inversiones de pendiente de un píxel por una curva amplia continua siguiendo la demarcación. R3E: se conservaron los primeros 18 puntos estables y se reemplazó el tramo final ruidoso por controles sobre la ondulación real. R3H: se eliminaron inversiones alrededor de las dos curvas y se trazó la S mediante controles espaciados según el relieve del PNG. R1A/R1B: se añadieron controles en entrada, desarrollo y salida de las curvas; las mesetas se ajustaron a las alturas reales del asfalto (.634 y .328), sin alterar extremos.

### Registro de puntos originales señalados (índices desde cero)

Cada punto de esta tabla afecta a los segmentos i−1→i e i→i+1 de la polilínea anterior. Se conserva este registro para que sea posible comparar con la revisión original, que se regeneró después de corregir.

| Mapa | Índice original | X | Y |
|---|---:|---:|---:|
| R3E | 18 | 0.686848 | 0.500000 |
| R3E | 19 | 0.688935 | 0.503346 |
| R3E | 20 | 0.697286 | 0.505204 |
| R3E | 21 | 0.699374 | 0.502602 |
| R3E | 22 | 0.755741 | 0.509294 |
| R3E | 23 | 0.759916 | 0.515242 |
| R3E | 24 | 0.766180 | 0.513755 |
| R3E | 25 | 0.772443 | 0.505576 |
| R3G | 4 | 0.175365 | 0.496654 |
| R3G | 5 | 0.185804 | 0.491522 |
| R3G | 6 | 0.189979 | 0.492193 |
| R3G | 7 | 0.192067 | 0.495911 |
| R3G | 8 | 0.244259 | 0.459480 |
| R3G | 9 | 0.246347 | 0.461338 |
| R3G | 10 | 0.254697 | 0.455390 |
| R3G | 11 | 0.256785 | 0.450186 |
| R3G | 13 | 0.331942 | 0.394052 |
| R3G | 14 | 0.334029 | 0.397026 |
| R3G | 15 | 0.340292 | 0.393309 |
| R3G | 16 | 0.344468 | 0.386245 |
| R3G | 17 | 0.348643 | 0.384015 |
| R3G | 18 | 0.350731 | 0.386989 |
| R3G | 19 | 0.359081 | 0.382528 |
| R3G | 20 | 0.361169 | 0.377323 |
| R3G | 21 | 0.363257 | 0.380297 |
| R3G | 22 | 0.371607 | 0.375836 |
| R3G | 23 | 0.373695 | 0.371004 |
| R3G | 24 | 0.411273 | 0.356506 |
| R3G | 25 | 0.415449 | 0.362082 |
| R3G | 26 | 0.421712 | 0.360223 |
| R3G | 27 | 0.425887 | 0.353160 |
| R3G | 30 | 0.532359 | 0.347584 |
| R3G | 31 | 0.542798 | 0.354275 |
| R3G | 32 | 0.549060 | 0.349442 |
| R3G | 34 | 0.565762 | 0.353160 |
| R3G | 35 | 0.569937 | 0.362082 |
| R3G | 36 | 0.576200 | 0.364312 |
| R3G | 37 | 0.580376 | 0.357993 |
| R3G | 38 | 0.588727 | 0.364312 |
| R3G | 39 | 0.601253 | 0.365761 |
| R3G | 40 | 0.603340 | 0.369517 |
| R3G | 41 | 0.611691 | 0.373234 |
| R3G | 42 | 0.613779 | 0.371375 |
| R3G | 43 | 0.617954 | 0.377695 |
| R3G | 44 | 0.626305 | 0.381784 |
| R3G | 45 | 0.628392 | 0.378810 |
| R3G | 47 | 0.674322 | 0.408550 |
| R3G | 48 | 0.676409 | 0.414498 |
| R3G | 49 | 0.693111 | 0.421933 |
| R3G | 50 | 0.697286 | 0.430112 |
| R3G | 51 | 0.703549 | 0.434944 |
| R3G | 52 | 0.705637 | 0.431970 |
| R3G | 54 | 0.749478 | 0.469145 |
| R3G | 55 | 0.759916 | 0.471375 |
| R3H | 10 | 0.367432 | 0.658736 |
| R3H | 11 | 0.369520 | 0.662082 |
| R3H | 12 | 0.377871 | 0.658736 |
| R3H | 13 | 0.379958 | 0.652689 |
| R3H | 25 | 0.749478 | 0.393680 |
| R3H | 26 | 0.757829 | 0.404461 |
| R3H | 27 | 0.759916 | 0.411917 |
| R3H | 28 | 0.768267 | 0.420839 |
| R3H | 29 | 0.770355 | 0.418216 |
| R3H | 30 | 0.801670 | 0.454647 |
| R3H | 31 | 0.803758 | 0.461319 |
| R3H | 32 | 0.820459 | 0.476652 |
| R3H | 33 | 0.822547 | 0.481109 |

R1A y R1B: se revisaron los cinco segmentos originales 0→1, 1→2, 2→3, 3→4 y 4→5. Los cambios se concentran en las aproximaciones 1→2 y 3→4 y en la altura de la sección central 2→3. El registro original de sus seis puntos era R1A: (0,.5),(.18,.5),(.38,.65),(.62,.65),(.82,.5),(1,.5); R1B: (0,.5),(.18,.5),(.38,.35),(.62,.35),(.82,.5),(1,.5).

## Cinco conexiones revisadas

| Conexión | Δcentro raster | Δancho raster | Datos geométricos | Estado |
|---|---:|---:|---|---|
| S0 → R1A | 2 px | 4 px | Altura igual, tangentes 0°, nominal 32→32 | Pendiente de arte |
| R1A → R2A | 1 px | 6 px | Altura igual, tangentes 0°, nominal 32→32 | Pendiente de arte |
| R2A → R3B | 4 px | 6 px | Altura igual, tangentes 0°, nominal 32→32 | Pendiente de arte |
| R2C → R3E | 1 px | 8 px | Altura igual, tangentes 0°, nominal 32→32 | Pendiente de arte |
| R2C → R3F | .5 px | 5 px | Altura igual, tangentes 0°, nominal 32→32 | Pendiente de arte |

Comparación de salida e inicio mediante los recortes de Route Preview y medidas por color del PNG. Las cinco alertas persisten porque el raster no cambió. Escala: se conserva el lienzo lógico común 480×270, exportación 1920×1080 y ancho nominal 32/640×360; el ancho pintado varía. No existe escala métrica certificada. No se desvió ningún extremo para disimular una diferencia de arte: la corrección de estos bordes queda pendiente de autorización para editar imágenes. El muestreo por color es una estimación y debe confirmarse visualmente antes de retocar el arte.

## Validación de recorridos

Se probaron 0%, 25%, 50%, 75% y 100% en Route Preview para los cinco mapas. Los extremos permanecen iguales, las tangentes inicial/final apuntan al este y no hay puntos fuera de [0,1]. Los giros locales de las cinco polilíneas ya no disparan advertencias del analizador. R3G bajó de aproximadamente 93° a 11.1° y se inspeccionó de nuevo su superposición corregida en Unity.

Comprobación independiente de líneas blancas sobre 480 columnas de cada miniatura:

| Mapa | Ambas líneas detectadas | Distancia mínima eje-borde detectado |
|---|---:|---:|
| R3G | 476/480 | 10 px |
| R3E | 480/480 | 10 px |
| R3H | 473/480 | 12 px |
| R1A | 480/480 | 9 px |
| R1B | 475/480 | 11 px |

No se detectaron salidas geométricas del lienzo. Las columnas no clasificadas por textura no certifican un error ni un acierto; requieren revisión visual. Las polilíneas trazadas permanecen coherentes con el asfalto visible y se han mostrado superpuestas en Route Preview.

### Resultado de color y huella después

- R3G · Geometría: sin alertas · Color PNG (estimación): centro 9/201, huella 40/201 sospechosos; 8 huellas fuera del lienzo.
- R3E · Geometría: sin alertas · Color PNG (estimación): centro 6/201, huella 26/201 sospechosos; 8 huellas fuera del lienzo.
- R3H · Geometría: sin alertas · Color PNG (estimación): centro 12/201, huella 48/201 sospechosos; 8 huellas fuera del lienzo.
- R1A · Geometría: sin alertas · Color PNG (estimación): centro 9/201, huella 31/201 sospechosos; 8 huellas fuera del lienzo.
- R1B · Geometría: sin alertas · Color PNG (estimación): centro 11/201, huella 36/201 sospechosos; 8 huellas fuera del lienzo.

Estas cifras no tienen por qué disminuir al mejorar la geometría: el color de la línea amarilla, sombras, bordes y muestreo puede producir falsas alertas. El test usa camión provisional 18×7 px, desplazamiento lateral cero, 201 posiciones. Ocho huellas parcialmente fuera del lienzo corresponden a aproximación/salida del sprite en los extremos. No equivalen a que sus puntos estén fuera del mapa.

## Advertencias restantes y limitaciones

Persisten las cinco advertencias raster, alertas históricas de S0 por segmentos rectos largos y alertas locales de radio en R3A/R3B/R3F, fuera del alcance de las cinco correcciones solicitadas. El informe completo RoutePreviewReview.md se regeneró sin alterar el analizador ni sus umbrales para ocultar avisos. RoutePreviewGuide.md queda como diagnóstico histórico anterior; para el estado corregido consultar este informe y el Review regenerado.

No se certifica la cabida física: no existe máscara semántica de carriles, sprite definitivo o modelo de giro del remolque. El eje central no es un carril. Un futuro spline o desplazamiento lateral deberá validarse aparte para evitar que salga del pavimento. La eliminación de zigzags es una corrección de datos de Editor; no se programó movimiento, conducción ni gameplay. Se conservaron PNG y miniaturas; los hashes de los 32 archivos se compararon antes/después. ValidadorContinuidad volvió a pasar sin problemas. Se solicitó compilación y se revisó Console.
