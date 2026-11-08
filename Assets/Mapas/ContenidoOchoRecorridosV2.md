# Contenido candidato V2 para ocho recorridos educativos

Life-Drive · Unity 6000.6.7f1 · `feat/unity-map` · 5 de noviembre de 2026.

El [paquete candidato](../StreamingAssets/Contenido/ruta-v2-candidata.json) contiene el grafo lógico, las situaciones, la relación explícita de 16 mapas con tramos y las 14 asociaciones entre respuestas y mapas. Su estado es `CANDIDATO`. Las 11 situaciones tienen `validada: false` y la fecha de validación vacía. Los puntajes, retrasos, veredictos y marcas de buena práctica de las alternativas nuevas son datos provisionales para probar el flujo. La publicación requiere revisión de la organización.

## Decisiones de navegación

Cada fila define las dos alternativas navegables de un punto. La asociación se identifica por `(contenido_version, mapa_origen, situacion_id, alternativa_id)`. El destino visual está escrito en la fila y el destino lógico en la alternativa. El [validador](../Scripts/Mapas/PaqueteRecorridos.cs) exige que coincidan mediante el vínculo explícito mapa–tramo. La consecuencia, los puntos, el retraso y las señales de seguridad se consultan en la alternativa referenciada y quedan disponibles en la decisión que confirma el evaluador existente.

| Mapa y tramo de origen | Situación V2 y tipo | Alternativa y destino | Alternativa y destino | Criterio educativo por validar |
| --- | --- | --- | --- | --- |
| S0 · 101 | 101 · `RIESGO` | 101 · Completar inspección → R1A · 102 | 102 · Salir sin completar la inspección → R1B · 103 | Diferenciar conocer el procedimiento de ejecutarlo bajo presión de despacho. |
| R1A · 102 | 102 · `DECISION_RUTA` | 103 · Corredor principal → R2A · 104 | 104 · Atajo rural → R2B · 105 | Comparar visibilidad y exposición con ambos corredores habilitados para el vehículo. |
| R1B · 103 | 103 · `DECISION_RUTA` | 105 · Zona de servicio → R2C · 106 | 106 · Corredor industrial sin verificar → R2D · 107 | Buscar una parada habilitada para la verificación pendiente y reportar hallazgos. |
| R2A · 104 | 104 · `DECISION_RUTA` | 107 · Curvas amplias → R3A · 108 | 108 · Curvas cerradas para recuperar tiempo → R3B · 109 | Evaluar visibilidad y trazado antes de priorizar tiempo. |
| R2B · 105 | 105 · `DECISION_RUTA` | 109 · Vía abierta → R3C · 110 | 110 · Tramo exigente por distancia → R3D · 111 | Anticipar usuarios vulnerables y respetar la demarcación. |
| R2C · 106 | 5 · `RIESGO` | 12 · Detenerse y hacer pausa → R3E · 112 | 13 · Bajar la ventana y continuar → R3F · 113 | Reconocer que seguir con síntomas de fatiga no sustituye una pausa segura. |
| R2D · 107 | 106 · `RIESGO` | 111 · Comprobar amarres en lugar habilitado → R3G · 114 | 112 · Aplazar la comprobación → R3H · 115 | Revisar la sujeción de la carga tras circular por firme irregular y gestionar hallazgos. |

El contexto colombiano se plantea con reportes viales ficticios, corredores habilitados, inspección del vehículo, visibilidad, fatiga y sujeción de carga. Las referencias orientan el diseño pedagógico. [Sistema de Información Vial de Invías](https://mintransporte.gov.co/publicaciones/11542/invias-relanza-el-sistema-de-informacion-vial/) · [Código Nacional de Tránsito, arts. 73 y 74](https://www.funcionpublica.gov.co/eva/gestornormativo/norma.php?769=&i=5557).

### Reutilización y diferencias frente al contrato de diseño

- `s1/a1–a3` conserva la pregunta y los efectos en S0. La acción operativa usa `s101/a101–a102` para que una respuesta de conocimiento no seleccione una ruta por sí sola.
- `s5/a12–a13` conserva los textos, puntos, retrasos y veredictos. V2 añade sus destinos explícitos. `a14` queda en V1 para que en la bifurcación candidata haya exactamente dos alternativas.
- `s2/a4–a5` queda en V1 porque la vía alterna descrita como más larga no corresponde al atajo propuesto. `s4/a9–a11` también queda en V1 porque el retorno de esa situación carece de una equivalencia con la montaña de R3B.
- `s3/a6–a8` se reutiliza en R2A antes de su decisión de ruta. `s6/a15–a17` y `s7/a18–a20` se reutilizan en el tramo terminal lógico 116, con la emergencia en la aproximación y la pregunta de descargue al llegar. En las pruebas, el cursor cruza de la hoja R3 a `DELIVERY` después de responder s6. La UI futura debe sincronizar ese paso con la confirmación de la consecuencia y el hito de llegada.
- Las seis situaciones nuevas usan IDs 101–106 y sus 12 alternativas usan IDs 101–112. Las situaciones y alternativas conservan espacios de IDs distintos. `ruta.json` mantiene las 7 situaciones originales y las 20 alternativas, comprobado también mediante su SHA-256 idéntico antes y después (`E7AD6F40DDC679348A934CAECC052376AE0607A2BAB07487E07A7F156FE28A7A`).

## Recorridos verificados

| Camino completo | Alternativas navegables, en orden |
| --- | --- |
| S0 → R1A → R2A → R3A → DELIVERY | 101, 103, 107 |
| S0 → R1A → R2A → R3B → DELIVERY | 101, 103, 108 |
| S0 → R1A → R2B → R3C → DELIVERY | 101, 104, 109 |
| S0 → R1A → R2B → R3D → DELIVERY | 101, 104, 110 |
| S0 → R1B → R2C → R3E → DELIVERY | 102, 105, 12 |
| S0 → R1B → R2C → R3F → DELIVERY | 102, 105, 13 |
| S0 → R1B → R2D → R3G → DELIVERY | 102, 106, 111 |
| S0 → R1B → R2D → R3H → DELIVERY | 102, 106, 112 |

Las ocho pruebas jugaron con `ServicioPartidas`, `EvaluadorSituaciones` a través del servicio y el cursor existente. Cada caso volvió a abrir su base temporal y comprobó la finalización y las decisiones navegables guardadas. El validador comprueba que la bifurcación sea la última situación del tramo, que cubra sus dos hijos y que los avances por defecto respeten el árbol. Hay pruebas de rechazo para situación, alternativa y mapa inexistentes, arista inválida, alternativa duplicada, versión incorrecta, destino lógico discrepante y aprobación indebida.

## Datos que puede consumir S3-09

Estas son entradas disponibles o pendientes para la definición de Jorge. Las pruebas verifican la estructura y los valores persistidos en casos de prueba. La fórmula, la población y la agregación de cada indicador quedan a cargo de S3-09.

| Indicador | Datos disponibles | Decisión pendiente de Jorge y del equipo |
| --- | --- | --- |
| TDS · Tasa de Decisiones Seguras | `Decision.tipo`, `es_acertada`, `es_buena_practica`, IDs de partida/situación/alternativa y estado de partida. | Qué tipos entran en el denominador, qué significa una acción segura y cómo se tratan partidas incompletas. |
| IEE · Índice de Efectividad Educativa | Preguntas identificables por `PREGUNTA`, acierto, jugador por partida, tiempos y versión candidata disponible en el paquete. | Instrumento y población de comparación. Si exige medición antes/después, hacen falta hitos o evaluaciones comparables aprobadas. El prototipo carece de esa medición. |
| PPS · Promedio de Puntos de Seguridad | `Decision.puntos_aplicados`, `Partida.puntos`, estado, fechas y jugador. | Unidad de promedio y población elegible. Revisar comparabilidad entre versiones con distinto número de situaciones y efectos provisionales. |
| PCRS · Porcentaje de Cumplimiento de Rutas Seguras | Respuestas confirmadas, tipos, tramos origen/destino, estado `FINALIZADA` y asociaciones versionadas del candidato. | Acordar si el cumplimiento se evalúa por alternativa, por conducta en contexto o por un conjunto de hitos. Aprobar elegibilidad y tratamiento de riesgo, emergencia y preguntas. |

El modelo separa `PREGUNTA`, `DECISION_RUTA`, `RIESGO` y `EMERGENCIA`, acciones acertadas, buenas prácticas y partidas finalizadas. El ID de mapa describe ubicación. Para PCRS se deben revisar de forma expresa la salida sin inspección (101/102), la elección de corredor por visibilidad o tiempo (102/103–104, 104/107–108, 105/109–110), la parada de verificación (103/105–106), la respuesta a la fatiga (5/12–13) y la comprobación de amarres (106/111–112). También falta acordar si la conducta ante la avería (6/15–17) y la pregunta de descargue (7/18–20) cuentan para el indicador. La evaluación provisional de una alternativa describe su escenario. Una hoja R3 o la llegada a DELIVERY no fijan la seguridad de una partida.

## Aislamiento y activación posterior

La carga candidata distingue versión ausente representada por 0, versión 1 y versión 2. 0 y 1 cargan mediante `Almacen` la misma ruta clásica. 2 requiere selección explícita del archivo candidato y las versiones desconocidas se rechazan. La prueba compara los modelos V1 y comprueba que el archivo activo conserva sus bytes. `ServicioJuego` sigue leyendo la ruta de producción mediante su entrada normal. El prototipo no persiste una versión de contenido en `Partida`; la reapertura comprobada corresponde a bases temporales y no certifica reanudación V2, migración ni convivencia real de versiones en una base de producción.

1. Aprobar situaciones, efectos, asociaciones y criterios de seguridad con la organización. Coordinar las definiciones de S3-09 con Jorge.
2. Publicar un paquete aprobado e inmutable. Fijar versión por partida y resolverla también al reanudar y generar reportes. Probar datos previos, backups y versiones desconocidas antes de habilitar nuevas partidas.
3. Añadir el delta confirmado, checkpoint visual y hooks de presentación descritos en el [contrato](ContratoIntegracionRecorridos.md). Comprobar recuperación tras guardado, fundido y finalización, junto con la llegada y el resumen en la escena real.
4. Habilitar V2 para nuevas partidas únicamente después de la aprobación y las pruebas de persistencia y presentación.

## Resultados del Unity Test Runner

Compilación y recarga de dominio confirmadas en el editor abierto mediante Unity MCP por HTTP local.

| Ejecución | Aprobadas | Fallidas | Omitidas | ID |
| --- | ---: | ---: | ---: | --- |
| `PaqueteRecorridosTests` | 18 | 0 | 0 | `84aaf7732403457586815a2e97125a30` |
| EditMode completo | 45 | 0 | 0 | `a3c3ce2f670f476bafb581c903994a58` |

EditMode incluye las 27 pruebas previas y las 18 nuevas. Console devolvió cero errores y la advertencia de Unity MCP ya presente antes de las pruebas, `[WebSocket] Unexpected receive error: WebSocket is not initialised`. Ambas ejecuciones finalizaron y devolvieron sus resultados.
