# DemoRecorrido · demostración independiente

Validación: 2 de noviembre de 2026, Unity 6000.6.7f1, rama feat/unity-map.

## Ejecutar

Abrir `Assets/Scenes/DemoRecorrido.unity` y pulsar Play. Usar Game View suficientemente amplia (recomendado 960×540 o mayor; maximizarla si los controles quedan pequeños). El fondo se ajusta completo a 16:9 sin deformarlo, con bandas negras según el espacio disponible. S0 empieza automáticamente. Seleccionar otro mapa reinicia su progreso y lo deja pausado; pulsar Reproducir / Reanudar. Reiniciar vuelve al comienzo y pausa. Al terminar se detiene; para repetir, Reiniciar y Reproducir.

Los controles inferiores permiten elegir cualquiera de los 16 IDs del catálogo, pausar/reanudar, reiniciar, cambiar velocidad (1–160 píxeles lógicos/s), ajustar carril derecho (0–12 píxeles) y ver el progreso. No hay controles de teclado de conducción. Los cambios de Play Mode son temporales. Para valores iniciales persistentes, seleccionar el objeto Demo Recorrido en modo edición y ajustar su Inspector.

No se agregó la escena a EditorBuildSettings ni se modificaron escenas existentes. Se reemplazó únicamente la escena temporal sin guardar y sin cambios pendientes, que contenía una cámara básica, para abrir esta nueva escena.

## Componentes y responsabilidades

- `Assets/Scripts/Mapas/Demo/RecorridoDemo.cs`: construye en memoria un recorrido a partir de `MapaVisual.trayectoria`, sin escribir al ScriptableObject. Convierte coordenadas normalizadas a 480×270, elimina duplicados consecutivos y redondea localmente los vértices con curvas cuadráticas (recorte máximo de 4 píxeles, limitado además por longitud de segmentos). Remuestrea aproximadamente cada píxel y calcula la normal derecha `(t.y, -t.x)` en coordenadas con Y hacia arriba. Reconstruye la longitud acumulada del recorrido lateral para avanzar por distancia, no por índice de punto.
- `Assets/Scripts/Mapas/Demo/DemoRecorrido.cs`: reproduce, pausa, reinicia, cambia mapa y velocidad, orienta progresivamente con SmoothDampAngle y presenta los controles temporales mediante IMGUI. Consume directamente CatalogoMapas y el sprite existente. El fondo y el camión se dibujan como capas independientes en Game View; este prototipo no usa SpriteRenderer, física ni colliders. La escena incluye cámara y luz principal.
- `Assets/Scenes/DemoRecorrido.unity`: referencias serializadas al catálogo y CamionCarga.png, cámara, luz y controlador. No duplica las 16 definiciones.
- Este documento y los archivos `.meta` correspondientes.

No hay referencias a ServicioJuego, ServicioPartidas, Evaluacion, Datos, UI de Javier ni ruta.json. No existen decisiones, preguntas, puntuación, finales ni cambios automáticos de mapa.

## Escala, posición y orientación

Valores iniciales: velocidad 24 px/s, desplazamiento derecho 6 px, largo visible 18 px y ancho aproximado 5.3467 px, tiempo de giro 0.12 s. La base del camión apunta a la derecha. Se usa el rectángulo alfa visible del recurso para centrar y dibujar su silueta, conservando su proporción; no se cambia el PNG, su pivot ni su importación. Las texturas existentes mantienen Point, sin mipmaps ni compresión.

El punto inicial es la entrada de la trayectoria desplazada al carril; los puntos centrales originales permanecen intactos. La normal depende de la tangente local, también en curvas. Cambiar el desplazamiento conserva el porcentaje recorrido y recalcula su posición. Puede mover lateralmente el vehículo de inmediato: es un control diagnóstico, no una maniobra animada.

El redondeo solo afecta a esta demostración en memoria y pasa cerca de los vértices aprobados; no constituye una nueva trayectoria aprobada. La longitud se mide sobre la curva lateral resultante. La rotación se interpola por el ángulo más corto; el frente no se invierte deliberadamente al cambiar de segmento. El centro se limita al lienzo si un ajuste extremo lo rebasa y aparece un aviso: no es corrección de pavimento. Con el ajuste inicial no se activó ese límite en ningún mapa.

## Pruebas realizadas en Play Mode

Se abrió y ejecutó la escena real. Se observó avance mediante Update entre llamadas: S0 pasó de 0 a 151.91 px de distancia. También se probaron los métodos públicos sobre el componente vivo usando el mismo Avanzar que llama Update, a pasos de 1/60 s: 20 051 pasos en los 16 recorridos a 24 px/s y carril derecho 6 px. No es una reproducción en tiempo real de cada uno de esos pasos.

En los 16 mapas se comprobaron inicio, 25%, 50%, 75% y final durante los barridos, además de todos los pasos intermedios. Los centros permanecieron dentro de 480×270, los ángulos fueron finitos, el recorrido llegó al final y quedó detenido. No hubo recortes numéricos de centro por límites. Máximo cambio angular por paso: 1.49° en R2C a la velocidad inicial. Esto no certifica radio de giro físico ni ausencia de obstáculos.

| Mapa | Longitud lateral aproximada, px | Barrido funcional |
|---|---:|---|
| S0 | 480.00 | Pasa |
| R1A | 494.82 | Pasa |
| R1B | 504.25 | Pasa |
| R2A | 506.09 | Pasa |
| R2B | 523.98 | Pasa |
| R2C | 523.84 | Pasa |
| R2D | 501.66 | Pasa |
| R3A | 499.19 | Pasa |
| R3B | 516.77 | Pasa |
| R3C | 501.02 | Pasa |
| R3D | 531.99 | Pasa |
| R3E | 499.46 | Pasa |
| R3F | 532.67 | Pasa |
| R3G | 492.58 | Pasa |
| R3H | 523.98 | Pasa |
| DELIVERY | 385.26 | Pasa |

Pausa impide que Avanzar modifique la distancia; reanudar conserva y continúa el progreso. Reiniciar y seleccionar mapa devuelven distancia cero y estado pausado. A 24 px/s, avanzar 0.25 s produce 6 px; a 48 px/s produce 12 px. El cambio lateral 6→8 conserva el porcentaje. Se verificaron además los botones visibles Reanudar, Reiniciar y selector (S0→R2C), con lectura del estado real del componente. El avance normal y la parada final también se vieron en pantalla.

Se inspeccionó visualmente Game View con los 16 fondos: aproximadamente 50% en los 15 mapas de carretera y 100% en DELIVERY. No se presenta esto como aprobación visual de todos los porcentajes del barrido. En DELIVERY la silueta termina dentro del patio libre, sin superponerse a las bodegas en esa muestra.

## Advertencias visuales y limitaciones

- R2A, R2B y R2C alrededor de 50%: el camión queda próximo a la demarcación lateral en curvas con carril 6. La muestra de R2A alcanza visualmente esa línea; requiere inspección ampliada de la envolvente antes de aprobar el carril. El giro suavizado y el desplazamiento pueden aumentar el barrido de las esquinas. No se fuerza una trayectoria distinta para disimularlo.
- R2D y R3F conservan prioridad de revisión según ValidacionCamion.md; su muestra central no basta para descartar problemas en 25/75% o sombras. Los demás mapas cargan y muestran orientación coherente en la muestra, sin certificación continua de pavimento.
- Los puntos extremos están en el borde del lienzo: parte de la silueta queda recortada al entrar/salir de los mapas de carretera. Solo se garantiza el centro dentro de la imagen, no la carrocería completa. DELIVERY tiene final interior.
- Escala y desplazamiento son hipótesis visuales. No existe referencia métrica validada, máscara de vía, detección semántica de árboles/guardarraíles, articulación de remolque ni física. No se afirma cabida física ni ausencia de solapes en todo el recorrido. Desplazamientos grandes pueden sacar la huella del asfalto aunque el centro siga dentro del PNG.
- El redondeo evita esquinas instantáneas pero puede cortar ligeramente una curva; la rotación tiene retraso intencional. Se requieren revisión ampliada y envolvente continua antes de gameplay. No se validaron visualmente todas las velocidades hasta 160 px/s ni todos los desplazamientos hasta 12 px.
- Se conserva Point y la proporción, pero una escala de ventana no entera y rotaciones arbitrarias producen aliasing. El sprite pierde detalle a tamaño pequeño, y sus luces pintadas giran con él. Las fachadas parcialmente oblicuas no tienen la misma perspectiva cenital que el camión.
- Los controles son provisionales y requieren una Game View con tamaño suficiente; no constituyen una interfaz adaptable final.

## Conexiones pendientes

Se mantienen las cinco advertencias artísticas de RoutePreviewReview.md. Estimaciones de píxeles lógicos de borde; no son errores geométricos del movimiento:

| Conexión | Δcentro / Δancho | Pendiente |
|---|---|---|
| S0 → R1A | 2 / 4 | Discontinuidad visual leve de anchura |
| R1A → R2A | 1 / 6 | Revisar diferencia de anchura |
| R2A → R3B | 4 / 6 | Mayor prioridad por posición y anchura |
| R2C → R3E | 1 / 8 | Confirmar anchura frente a sombras/guardarraíles |
| R2C → R3F | 0.5 / 5 | Revisar demarcación y anchura |

La demo no concatena mapas ni oculta esas diferencias mediante transiciones. R3→DELIVERY sigue siendo una transición conceptual de escena compartida, no una unión física de ocho vías.

## Integridad y cierre

Solo se crearon los dos componentes, su carpeta Demo, la escena y esta guía con sus metadatos Unity. No se modificaron PNG, miniaturas, vehículo, catálogo, definiciones ni trayectorias. Los archivos protegidos se compararon por SHA-256 antes/después. No se modificaron EditorBuildSettings, motor, Datos, UI ni ruta.json. No hubo commit ni push.

Resultado final: Unity compiló los dos componentes sin errores; Console devolvió cero errores y cero advertencias en la revisión final. ValidadorContinuidad.Revisar devolvió una lista vacía. Los 51 archivos del conjunto de integridad (32 PNG de fondos/miniaturas, 17 assets de catálogo/definiciones, camión y EditorBuildSettings) conservan sus hashes. Se salió de Play Mode y quedó abierta DemoRecorrido en edición. Se conservaron los cambios locales previos del repositorio.

Lista exacta de archivos creados en esta etapa:

- Assets/Scenes/DemoRecorrido.unity
- Assets/Scenes/DemoRecorrido.unity.meta
- Assets/Scripts/Mapas/Demo.meta
- Assets/Scripts/Mapas/Demo/DemoRecorrido.cs
- Assets/Scripts/Mapas/Demo/DemoRecorrido.cs.meta
- Assets/Scripts/Mapas/Demo/RecorridoDemo.cs
- Assets/Scripts/Mapas/Demo/RecorridoDemo.cs.meta
- Assets/Mapas/DemoRecorridoGuide.md
- Assets/Mapas/DemoRecorridoGuide.md.meta
