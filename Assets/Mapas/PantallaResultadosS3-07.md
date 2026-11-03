# S3-07 · pantalla visual de resultados del jugador

Unity 6000.6.7f1 · rama feat/unity-map · raíz Life-Drive.

## Entrega y ejecución

Escena creada: **Assets/Scenes/Resultado.unity**. Abrirla y pulsar Play. Incluye selector de tres ejemplos, resumen de entrega, puntos e insignias, revisión de decisiones y botón Volver a Inicio.

**Todo resultado es DEMO.** Los títulos no se calculan a partir de puntos, carretera, decisiones o personaje. No se implementa una clasificación del motor y no se guarda información. Los tres ejemplos tienen entrega completada en DELIVERY y trayecto al 100%, incluso OPERACIÓN CRÍTICA.

La pantalla muestra una instantánea de llegada: el fondo DELIVERY y el camión existente colocado como capa independiente en el patio, alrededor de su punto final documentado. No reproduce una llegada nueva ni conecta la escena a la partida. La animación y la llegada funcional de DemoInterfazPartida permanecen independientes.

## Estructura visual

- Cabecera Life Drive / Resultados DEMO, título del ejemplo y aviso permanente de datos ficticios.
- Selector exclusivo de demostración con tres botones numerados y etiquetados. El título explícito diferencia el resultado además del acento verde agua, amarillo o coral; el color no es la única señal.
- Panel de entrega: DELIVERY sin modificar, camión separado con su proporción visible existente, texto ENTREGA COMPLETADA y barra de trayecto completa.
- Panel de balance: jugador ficticio Conductor DEMO, personaje Camila (ID real de personaje 2), retrato de concepto ya existente, puntuación simulada, resumen y mensaje educativo.
- Panel de insignias: nombres e iconos existentes de S3-04, cantidad obtenida y texto OBTENIDA / NO OBTENIDA EN ESTE EJEMPLO. No se conceden nuevas insignias por mostrar el panel.
- Revisar decisiones DEMO sustituye el contenido central por tres filas de decisiones/consecuencias ficticias y sus cambios ilustrativos de puntos. El botón permite volver al resumen. El título aclara que no es un registro real.
- Volver a Inicio carga la escena anterior mediante el puente de previsualización existente. No cierra una sesión real.

Se reutilizan la paleta oscura/marfil/verde agua y los botones uGUI/TMP de Inicio y Registro. CanvasScaler usa referencia 1920×1080 y escala proporcional. No se crearon ilustraciones nuevas, mapas ni PNG para esta pantalla.

## Tres conjuntos de datos ficticios

| Recurso | Título literal | Puntos prefijados | Insignias prefijadas | Entrega |
|---|---|---:|---|---|
| OperacionSeguraDemo.asset | OPERACIÓN SEGURA | +25 | Conductor preventivo, Ruta responsable y Respuesta ante emergencias | DELIVERY, completada, 100% |
| EntregaComplicacionesDemo.asset | ENTREGA CON COMPLICACIONES | −5 | Conductor preventivo | DELIVERY, completada, 100% |
| OperacionCriticaDemo.asset | OPERACIÓN CRÍTICA | −20 | Ninguna | DELIVERY, completada, 100% |

Los tres recursos están en Assets/Mapas/InterfazVisual/Resultados. Usan el mismo personaje y el mismo destino para evitar presentar el camino visual como clasificador. No incluyen un ID de partida, decisión o evento real.

Sus tres filas de historia describen confirmar/omitir un aviso, coordinar/no confirmar el corredor y atender/omitir un aviso de apoyo del ejercicio. Los deltas ilustrativos son +10/+5/+10, +10/−5/−10 y −5/−5/−10 respectivamente. **La pantalla no suma esos deltas para decidir el total ni aplica un umbral para decidir el título**: ambos son datos escritos para cada maqueta. Los textos de consecuencias moderadas o graves son también prefijados y no crean incidentes ni eventos en una partida.

Los mensajes educativos son distintos: prevención y comunicación en el ejemplo seguro; revisión para mejorar en el de complicaciones; recordar que llegar no elimina consecuencias en el crítico. No son diagnósticos de desempeño de una persona real.

## Arquitectura y reutilización

**ResultadoEjemploDemo** es un ScriptableObject de instantánea. Separa título, tono visual, jugador ficticio, personaje, puntos, textos, lista de insignias e historia de muestra. **DecisionResumenDemo** contiene una fila de presentación, no el registro Decision del motor. **TonoResultadoDemo** selecciona solo color/estilo; no evalúa nada.

**PrevisualizacionResultado** valida referencias, lee en modo lectura el catálogo de personajes mediante Almacen.CargarContenido, presenta un ejemplo por índice y alterna la revisión. No tiene dependencias de Sesion, ServicioPartidas, Evaluacion o BaseDatos; no escribe archivos ni PlayerPrefs. Cambiar el selector reemplaza una instantánea, sin acumular puntos, conceder premios o simular una partida.

Se reutilizan **GamificacionDemo / ReconocimientoDemo** para nombres e iconos y **IconosReconocimientoDemo** para texturas Point 16×16 generadas en memoria. No se usa EstadoGamificacionDemo.Aplicar para reconstruir o evaluar resultados; las insignias son una lista literal de la instantánea. La caché de iconos se libera al salir de la escena.

También se reutilizan BotonPrincipal.prefab, Inter-Regular.ttf, shaders TMP, PersonajesAtlas.png, DELIVERY.png y CamionCarga.png. Se creó FuenteResultado.asset como copia TMP de presentación para los textos propios, sin cambiar la fuente TTF. Durante las pruebas TMP actualizó datos serializados de la fuente de interfaz compartida FuenteInterfaz.asset; se informa como modificación de recurso propio, no como cambio de escenas o reglas.

**ConstructorResultado**, solo de Editor, construye la nueva escena y los datos una vez y se niega a sobrescribir Resultado existente o trabajar durante Play/con una escena sucia. No se ejecuta durante la partida. No se editaron constructores o puentes de navegación anteriores.

## Datos disponibles del motor y límites de la conexión

Se inspeccionaron PantallaResultado.cs y ServicioDesempeno.cs en modo lectura, contrastando el diagnóstico y AnalisisIntegracion.md.

| Información de pantalla | Fuente real disponible | Qué falta o no se debe suponer |
|---|---|---|
| Nombre de jugador | Resumen.Jugador.nombre | Acceso mediante sesión/partida autorizada; no Conductor DEMO |
| Personaje | Resumen.Jugador.personaje_id y Contenido.personajes | Enlace visual por ID; el resumen no trae retrato |
| Puntos | Resumen.Partida.puntos | Mostrar el valor confirmado, sin recalcularlo en la vista |
| Retraso y métricas | Partida.retraso_min; preguntas, decisiones, riesgos y buenas prácticas de Resumen | No implican por sí solos una categoría de final |
| Decisiones y consecuencias | Resumen.Historial, con Decision, puntos aplicados, consecuencia y tramos; RutaRecorrida | No sustituirlo por las filas ficticias ni copiar el registro de eventos |
| Fin lógico | Partida.estado / fecha_fin | FINALIZADA no prueba por sí sola que la animación ya llegó al patio |
| Progreso físico | No está persistido en el resumen | Necesita contrato/checkpoint de presentación; el 100% actual es de maqueta terminada |
| Insignias obtenidas | No existe un catálogo persistido de estos tres reconocimientos reales | es_buena_practica y el contador BuenasPracticas no equivalen automáticamente a esas insignias |
| Tres categorías de resultado | No existe clasificador ni umbrales implementados en los archivos inspeccionados | Reglas y coordinación con Javier antes de conectar etiquetas reales |

PantallaResultado está adjunto a la escena, **desactivado**, con textoPuntos, textoDetalle, textoRuta y botonSalir asignados para revisión de compatibilidad. Su Start consulta Sesion.Juego.Desempeno.Resumen y, si falta PartidaId, vuelve a Inicio. No se ejecutó. Sus cuatro campos actuales no cubren todos los nuevos elementos de presentación, ni los hooks necesarios para la llegada; activarlo no completa la integración. Los campos compartidos necesitarán un contrato de vista y distribución revisado antes de mostrar un historial real de longitud variable.

La futura capa adaptadora debe recibir resultados e historial confirmados, respetar permisos y Repetida, resolver el personaje por ID y esperar la llegada visual acordada. Las tres categorías y las reglas de reconocimientos deben aprobarse con Javier. Ninguna tabla de esta demostración constituye una asociación entre IDs numéricos y carreteras S0–R3H.

## Navegación y Build Settings

Resultado no se añadió a Build Settings y ninguna configuración de compilación se modificó. Para probar, abrir la escena desde el Editor. El botón Volver aprovecha el puente de Editor existente para Inicio. En un ejecutable solo funcionaría el fallback si Inicio ya estuviese incluida; de lo contrario se presenta un aviso.

No se añadió una entrada a Inicio para abrir Resultado ni se modificaron Inicio, Registro o DemoInterfazPartida. No hay enlace automático desde una partida a esta maqueta de resultados.

## Archivos creados o modificados

Creados, con sus `.meta`:

- Assets/Scenes/Resultado.unity
- Assets/Scripts/Mapas/InterfazVisual/ResultadoEjemploDemo.cs
- Assets/Scripts/Mapas/InterfazVisual/PrevisualizacionResultado.cs
- Assets/Editor/InterfazVisual/ConstructorResultado.cs
- Assets/Mapas/InterfazVisual/Resultados/OperacionSeguraDemo.asset
- Assets/Mapas/InterfazVisual/Resultados/EntregaComplicacionesDemo.asset
- Assets/Mapas/InterfazVisual/Resultados/OperacionCriticaDemo.asset
- Assets/Mapas/InterfazVisual/Resultados/FuenteResultado.asset
- Assets/Mapas/PantallaResultadosS3-07.md

Se creó también el `.meta` de la carpeta Resultados. Modificado por actualización de datos TMP durante la validación: Assets/Mapas/InterfazVisual/FuenteInterfaz.asset. No se modificaron escenas existentes, prefabs anteriores, controladores de Javier, contenido, mapas, camión o reglas de S3-04.

## Pruebas realizadas

Se compiló y abrió Resultado en Play Mode. Se inspeccionaron capturas nativas de Game View de los tres estados y del panel de decisiones. Se cambiaron los ejemplos por clic real, se abrió la revisión y se pulsó Volver a Inicio; el retorno llegó a Inicio. Los títulos y textos explícitos diferencian estados además de los colores.

Se fijaron resoluciones de render reales mediante Game View y se comprobaron Screen.width/height. Para cada ejemplo se revisaron resumen e historial tanto a **1920×1080** como a **1280×720**: 12 combinaciones. Se forzó la actualización de Canvas/TMP y se comparó la altura preferida con el rectángulo de texto: no hubo desbordamientos en esos datos y estados. Se inspeccionó también el aspecto visible. El zoom del visor del Editor se ajustó para ver el lienzo completo; no se confundió ese zoom con la resolución de render.

| Prueba | Resultado |
|---|---|
| Selector de tres estados | Título, textos, puntuación e insignias corresponden a cada instantánea |
| Seguro | +25 pts DEMO, 3/3 insignias |
| Complicaciones | −5 pts DEMO, 1/3 insignias |
| Crítico | −20 pts DEMO, 0/3 insignias |
| Entrega y trayecto | Completada en DELIVERY y 100% en los tres casos |
| Revisar/ocultar decisiones | Muestra tres filas del ejemplo; no consulta eventos ni base local |
| Controlador real | Desactivado; cuatro referencias asignadas no nulas |
| Navegación a Inicio | Pasa mediante clic en Play Mode, sin cambiar Build Settings |
| Regresión de Inicio/Registro | Inicio abre Registro; siguen disponibles IDs 1–4 y selección de Valentina |
| Regresión de DemoInterfazPartida | Tres situaciones, +25 simulados, tres insignias y llegada a DELIVERY, estado Finalizado |

La regresión de la demo se ejecutó sobre el componente vivo usando Avanzar/Responder/Continuar, los mismos métodos de su flujo normal; no se presenta como una inspección visual de todos los fotogramas. No se ejecutó un usuario o una partida real para validar resultados.

Cierre: Unity compiló las tres clases nuevas y Console final devolvió cero errores y cero advertencias. Se compararon por SHA-256 los 311 archivos existentes capturados antes de trabajar: 310 permanecieron idénticos y la única modificación fue FuenteInterfaz.asset por actualización de datos TMP, ya declarada arriba. En particular, todas las escenas anteriores, scripts protegidos, ruta.json, mapas, camión y Build Settings permanecieron idénticos. No aparecieron archivos lifedrive-db.json* en el persistentDataPath del proyecto. Se salió de Play Mode sin cambios pendientes en la escena Resultado.

## Limitaciones

- Son tres instantáneas configuradas, no finales reales ni reglas de clasificación. El selector es exclusivamente de demostración.
- La imagen de entrega muestra un estado final estático, no una transición funcional desde la partida. No se asegura cabida física por dibujar el camión en el patio.
- No hay guardado, carga de resultados reales, consulta de sesiones, exportación o nuevos eventos. No se escribió en lifedrive-db.json.
- Historia de tres filas y tamaño de texto validado para estos ejemplos. Un historial real requiere desplazamiento/paginación y estados vacíos/de error; no se garantiza ajuste de textos arbitrarios.
- Los iconos y retratos conservan su condición de recursos de prototipo. La fuente Inter es legible, no pixelada nativa; zoom/escalado no entero puede producir aliasing del arte Point.
- Solo se validó 16:9 en las dos resoluciones indicadas. No se certifica un build distribuido ni nuevas plataformas.

Se preservaron los cambios locales previos. No se hace commit ni push. La escena Resultado queda abierta en modo edición al terminar.
