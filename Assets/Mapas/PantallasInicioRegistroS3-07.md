# S3-07 · Inicio y Registro visuales

Unity 6000.6.7f1 · rama feat/unity-map · raíz Life-Drive.

## Alcance entregado

Se crearon **Assets/Scenes/Inicio.unity** y **Assets/Scenes/Registro.unity**, con Canvas, CanvasScaler, EventSystem, módulo de entrada Input System, controles uGUI/TextMeshPro y recursos propios. Las dos escenas se pueden recorrer en Play Mode dentro del Editor sin modificar Build Settings.

**Son vistas funcionales de presentación, no un registro real integrado.** No crean usuarios ni partidas, no inician sesión y no llaman Sesion.Juego. Los controladores originales PantallaInicio y PantallaRegistro están adjuntos, con todas sus referencias asignadas, pero desactivados en estas escenas. No se modificó ninguno de sus archivos.

## Ejecutar y controles

Abrir Inicio.unity y pulsar Play. Jugar carga Registro; Volver regresa a Inicio. La navegación de previsualización utiliza un puente exclusivamente de Editor que carga esos dos archivos de escena sin añadirlos a la compilación. También se puede abrir Registro directamente y pulsar Play.

Inicio muestra Life Drive, el fondo S0 existente sin modificar, Jugar y acceso administrativo. Este último informa que AdminLogin está pendiente, en lugar de cargar una escena inexistente o simular una sesión.

Registro muestra nombre, documento, cuatro tarjetas y **Comprobar datos**. El clic en cualquier parte de la tarjeta selecciona un solo personaje, con marco verde y etiqueta SELECCIONADO. Los botones tienen estados de foco, puntero y pulsación; los campos tienen caret y selección de texto.

Comprobar datos llama a **LifeDrive.Datos.Validacion.ErrorRegistro**, reutilizando las reglas existentes: nombre no vacío, documento de 5–15 dígitos sin puntos/letras y personaje seleccionado. El campo de documento usa IntegerNumber y límite de 15 caracteres; no convierte el documento a un entero ni guarda información. Con entradas válidas muestra: “Datos válidos para esta previsualización. No se creó un usuario. La conexión con Partida queda pendiente.” No se llama RegistrarJugador ni se comprueba identidad existente o permisos de una partida.

## Personajes y recursos visuales

Nombres, descripciones e IDs se leen en modo lectura de ruta.json mediante Almacen.CargarContenido, sin inicializar servicios. Se preservan exactamente:

| ID | Nombre | Descripción del contenido | Celda del atlas |
|---|---|---|---|
| 1 | Andrés | Conductor con 10 años en carretera | Superior izquierda |
| 2 | Camila | Conductora de tractocamión | Superior derecha |
| 3 | Miguel | Conductor de carga refrigerada | Inferior izquierda |
| 4 | Valentina | Conductora de carga seca | Inferior derecha |

No se añadieron habilidades, estadísticas o efectos sobre puntos. Las representaciones son arte de concepto para los personajes existentes; no son retratos oficiales derivados de una descripción física aprobada. Las tarjetas usan las cuatro regiones UV del atlas, no una identificación por parecido visual o texto del botón. El anfitrión verifica los cuatro IDs antes de enlazar los clics.

Atlas guardado: **Assets/Mapas/InterfazVisual/Personajes/PersonajesAtlas.png**, de 1254×1254, con alfa real (798 186 píxeles totalmente transparentes comprobados). Importación Sprite Single, Point, sin compresión, sin mipmaps y sin reescalar a potencia de dos. RawImage presenta una celda por tarjeta; el sprite importado corresponde al atlas completo. No se alteró ningún PNG anterior ni se generó una nueva carretera.

Se utilizó la herramienta integrada imagegen con S0.png como referencia de dirección artística. No se usó CLI/API externa ni se dejó una dependencia de archivos fuera del proyecto. El archivo generado se copió al proyecto manteniendo el original del generador. Prompt ejecutado:

```text
Use case: stylized-concept. Asset: ONE Unity sprite atlas of four pixel-art character selection portraits for Colombian transport educational game Life Drive. Use provided S0 image only as art direction reference: retro 16-bit, crisp visibly chunky square pixels, natural greens, navy and ivory work clothing, warm consistent daylight. Do not copy its scenery. Generate a square transparent atlas with EXACTLY four equal square cells in a 2x2 grid, no dividers, each cell containing ONE centered waist-up bust with generous transparent padding, all four equal scale. Top-left: Andrés, adult male Colombian truck driver with short dark hair, subtle stubble, navy work shirt. Top-right: Camila, adult female Colombian truck driver with dark hair tied back, teal work shirt. Bottom-left: Miguel, adult male Colombian truck driver with short wavy dark hair, muted sage work jacket. Bottom-right: Valentina, adult female Colombian truck driver with shoulder-length dark hair, ivory work shirt. Friendly professional diverse ordinary people, not caricatures. Concept character representations only, no abilities, medals, stats, vehicles or background. Same front three-quarter portrait framing and same light direction in every cell. No text, letters, names, numbers, logos, UI, borders, watermarks, floor shadows or checkerboard. Real transparent alpha. Pixel art should resemble low-resolution handcrafted game portrait sprites, no soft photographic rendering. Each bust contained entirely in its own quadrant.
```

Los paneles mantienen azul oscuro, marfil y verde agua de la demo. Los retratos aportan pixel art sin usar texto rasterizado para formularios. La tipografía Inter conserva legibilidad y tildes; no se presenta como una fuente pixel art nativa.

## Compatibilidad con los controladores del equipo

PantallaInicio solo expone los campos serializados botonJugar, botonAdministrador y textoMensaje; sus acciones se enlazan desde Start. PantallaRegistro recibe campoNombre, campoCedula, contenedorPersonajes, prefabBoton, botonIniciar, botonVolver y textoMensaje. Start y Registrar son privados; no hay API pública de presentación que permita sustituir solo navegación o envío desde otra clase sin cambiar el contrato.

Las tres referencias de Inicio y las siete de Registro están configuradas y se verificaron no nulas. El prefab de tarjeta contiene Button y un TMP_Text de nombre como primer texto descendiente, compatible con la operación básica BotonesLista.Agregar.

No se activaron los originales por estas dependencias pendientes:

- PantallaInicio.Start llama Sesion.Cerrar y Sesion.Juego, y enlaza LoadScene a Registro/AdminLogin. Abrirlo ya no sería una previsualización aislada.
- PantallaRegistro.Start accede a Sesion.Juego; su botón registra realmente un jugador y carga Partida. Partida y AdminLogin todavía no están montadas, y las nuevas escenas no están en Build Settings.
- BotonesLista solo asigna el nombre. El controlador no entrega a la vista el ID y la descripción para enlazar retrato y selección personalizada; al activarlo destruye las tarjetas estáticas y crea otras desde el prefab. Ese binding necesita una solución acordada, no basta con activar el checkbox.
- No existe un método público para validar sin registrar o para cambiar el destino posterior al alta. Se reutilizó la función pura de validación en lugar de reflejar/invocar métodos privados o duplicar la lógica de servicio.

El anfitrión PrevisualizacionPantallas no implementa una alternativa a ServicioUsuarios: solo enlaza clics de prueba, muestra datos de contenido, valida formato y presenta mensajes. No debe convivir activo con los controladores originales. **No activar el registro original sin desactivar el anfitrión, revisar retratos/datos y coordinar navegación e integración.** No se afirma que esa activación por sí sola complete el producto.

## Build Settings

No se modificó EditorBuildSettings.asset ni se solicitó una modificación: no fue necesaria para la previsualización autorizada. NavegacionPreviewEditor usa EditorSceneManager.LoadSceneInPlayMode exclusivamente para Inicio/Registro. El runtime tiene un fallback a SceneManager solo si la escena ya está incluida; en otro caso muestra un aviso.

Esto **no habilita navegación en un ejecutable distribuido** con la configuración actual. Añadir Inicio/Registro y decidir la escena inicial de un build requiere aprobación previa del usuario y coordinación con el equipo. No se sustituyó la referencia antigua a SampleScene ni se añadieron demos al build.

## Archivos creados

Escenas:

- Assets/Scenes/Inicio.unity
- Assets/Scenes/Registro.unity

Presentación runtime propia:

- Assets/Scripts/Mapas/InterfazVisual/PrevisualizacionPantallas.cs
- Assets/Scripts/Mapas/InterfazVisual/TarjetaPersonajeVisual.cs

Herramientas de Editor propias:

- Assets/Editor/InterfazVisual/ConstructorInicioRegistro.cs
- Assets/Editor/InterfazVisual/NavegacionPreviewEditor.cs

Prefabs:

- Assets/Mapas/InterfazVisual/Prefabs/BotonPrincipal.prefab
- Assets/Mapas/InterfazVisual/Prefabs/CampoTexto.prefab
- Assets/Mapas/InterfazVisual/Prefabs/TarjetaPersonaje.prefab

Recursos:

- Assets/Mapas/InterfazVisual/Personajes/PersonajesAtlas.png
- Assets/Mapas/InterfazVisual/Fuentes/Inter-Regular.ttf
- Assets/Mapas/InterfazVisual/Fuentes/Inter-LICENSE.txt
- Assets/Mapas/InterfazVisual/FuenteInterfaz.asset, con material/atlas TMP como subrecursos
- Assets/Mapas/InterfazVisual/Resources/TMP Settings.asset
- Assets/Mapas/InterfazVisual/Shaders/TMP_SDF.shader
- Assets/Mapas/InterfazVisual/Shaders/TMP_SDF-Mobile.shader
- Assets/Mapas/InterfazVisual/Shaders/TMPro.cginc
- Assets/Mapas/InterfazVisual/Shaders/TMPro_Mobile.cginc
- Assets/Mapas/InterfazVisual/Shaders/TMPro_Properties.cginc
- Assets/Mapas/InterfazVisual/Shaders/Unity-UGUI-LICENSE.md

Documentación: este archivo. Cada archivo y carpeta nueva tiene su `.meta` de Unity. No se modificó un archivo de presentación anterior para crear estas vistas.

El proyecto carecía de fuente TMP, shader SDF y TMP Settings. Se incorporó Inter desde los recursos locales de Unity con su licencia SIL OFL, y los dos shaders/CGINC necesarios desde TMP Essential Resources del paquete uGUI instalado, conservando la licencia suministrada. No se modificaron los paquetes. TMP Settings es el primer recurso compartido de ese nombre en el proyecto y fija la fuente predeterminada; está dentro de nuestro espacio de interfaz. No se importaron ejemplos ni se creó la carpeta global Assets/TextMesh Pro.

ConstructorInicioRegistro crea recursos y configura referencias desde Editor; se niega a sobrescribir escenas existentes o a trabajar con una escena sucia/Play Mode activo. No se ejecuta durante una partida. Las escenas entregadas ya están montadas y no necesitan reconstrucción para probarlas.

## Pruebas realizadas

Se compilaron las cuatro clases nuevas. Se ejecutó Inicio en Play Mode y se navegó mediante clic real a Registro y de regreso. También se abrió el acceso administrativo: mostró el pendiente sin error de escena inexistente. Se comprobaron los cuatro personajes por clic: IDs 1, 2, 3 y 4, con exactamente una tarjeta marcada cada vez. Se comprobó foco del input y escritura por teclado de un texto genérico de prueba, sin guardarlo.

| Comprobación | Resultado |
|---|---|
| Inicio a 1920×1080 | Render correcto; título completo y botones legibles |
| Registro a 1920×1080 | Cuatro retratos, nombres y descripciones visibles |
| Inicio a 1280×720 | Render correcto y retorno desde Registro |
| Registro a 1280×720 | Selección de los cuatro IDs y formulario funcional |
| Nombre vacío | Mensaje existente: Escribe tu nombre |
| Documento de tres dígitos | Mensaje existente de 5–15 dígitos, sin puntos/letras |
| Datos de formato válido sin personaje | Mensaje existente: Elige un personaje |
| Formato válido y personaje seleccionado | Confirmación explícita de previsualización sin creación de usuario |
| Referencias de controlador | Tres de Inicio y siete de Registro asignadas; ambos componentes desactivados |
| Textos | Inspección visual y comprobación de alturas preferidas sin desbordamiento en los estados muestreados |
| Console final | Sin errores ni advertencias |

Las resoluciones fueron tamaños de render reales de Game View, comprobados mediante Screen.width/height, no solo un escalado del Canvas. La captura nativa del Editor puede mostrar un zoom mayor o menor que el tamaño físico de la ventana; se ajustó el zoom para la revisión y se distinguió ese zoom de la resolución de render. No se validaron relaciones de aspecto distintas de 16:9, todos los nombres posibles ni todas las condiciones de accesibilidad.

La comprobación válida se hizo con texto y documento genéricos solo en memoria de UI. No se pulsó ni llamó al registro real. No hay llamadas a Sesion, RegistrarJugador, BaseDatos, PlayerPrefs o escrituras de datos en los componentes runtime nuevos. No aparecieron archivos lifedrive-db.json* en el persistentDataPath del proyecto.

Integridad verificada por SHA-256: los 119 archivos protegidos de Motor, Datos, UI, StreamingAssets, fondos, miniaturas y camión conservaron sus hashes. Los diez archivos capturados de escenas de demostración, scripts de las demos y EditorBuildSettings también permanecieron idénticos. Los cambios locales previos se conservaron.

## Limitaciones y pendientes

- Registro es una vista aislada con validación de formato, no un alta real. No se probaron identidad existente, permisos o reanudación de partidas; eso requiere una base de prueba autorizada y el contrato correspondiente.
- AdminLogin, Partida y Resultado siguen pendientes. No se añaden finales, puntuaciones, gamificación o movimiento en estas pantallas.
- El atlas es arte generado de concepto, pendiente de aprobación y posible acabado de bordes/consistencia de píxeles. Las apariencias no añaden atributos al contenido real. No son cuatro sprites individuales; se presenta el atlas con UV por tarjeta.
- La selección real futura debe enlazar ID, descripción, retrato y estado sin acceder a campos privados de Javier ni depender del texto del botón. Reingresar con un jugador existente conserva el personaje del servicio actual; no prometer un cambio sin acordarlo.
- La tipografía Inter es legible pero no una fuente pixelada nativa; los marcos y retratos aportan el lenguaje pixel art. Escalas no enteras de ventana pueden producir aliasing del arte Point.
- Cambiar Build Settings requiere solicitud y aprobación previas. El puente de previsualización es de Editor y no se incluye en un ejecutable.
- El soporte de lectura actual usa Almacen/File en el entorno de escritorio del proyecto; no se certifica un build Android/WebGL.

Se salió de Play Mode y quedó abierta Inicio en edición. No se hace commit ni push.
