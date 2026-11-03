# Life Drive · Rediseño de la base visual de interfaz

Fecha: 3 de noviembre de 2026. Unity 6000.6.7f1, rama feat/unity-map.

## Alcance y estado

Se rediseñaron **Inicio.unity** y **Registro.unity**, con una base propia de presentación reutilizable. Son previsualizaciones: no registran usuarios, no abren una sesión del motor y no guardan nombre, documento ni apariencia. Los datos se pierden al cambiar de escena o salir de Play Mode.

La escena Resultado ya existía al comenzar esta solicitud. Se conservó su escena, sus datos y sus controladores sin continuar su diseño. No se crearon otras pantallas ni se alteró Build Settings. Esta guía sustituye la descripción visual y de selección de personajes del documento histórico PantallasInicioRegistroS3-07.md; no sustituye sus hallazgos sobre el motor.

## Identidad visual

Se eliminó el acento menta dominante de estas dos pantallas. La carretera y vegetación de S0 permanecen como fondo existente, sin modificar su PNG. La interfaz incorpora un velo verde y paneles propios, sin alterar las demostraciones.

| Uso | Color |
|---|---|
| Bosque / superficie oscura | #18291C |
| Panel verde | #2B402C |
| Musgo / botón secundario | #52653B |
| Hover secundario | #77864C |
| Dorado / acción principal y foco | #DFC169 |
| Hover principal | #F2D98B |
| Texto principal crema | #F5EBCF |
| Texto secundario | #C2C9A8 |

Las selecciones se distinguen por borde, marcador y etiqueta, no solo por color. Los errores se presentan como texto concreto. Se usan radios moderados de 14–32 unidades UI y una forma circular para el menú. CanvasScaler utiliza referencia 1920×1080 y escala proporcional, sin estirar el fondo.

### Tipografía

**Pixelify Sans** para marca, títulos, botones y encabezados; **Inter** para campos, ayudas y párrafos. Pixelify Sans se obtuvo del [repositorio oficial de Google Fonts](https://github.com/google/fonts/tree/main/ofl/pixelifysans), con su licencia SIL OFL 1.1 incluida en Fuentes/OFL.txt. Es la fuente original de The Pixelify Sans Project Authors; no se redistribuye sin licencia ni se modifica su archivo.

Los recursos TMP nuevos son FuentePixel.asset y FuenteLectura.asset. La fuente de lectura es una copia independiente de la fuente ya disponible. No se cambió el recurso global TMP Settings. Se añadieron letras españolas y signos a los atlas nuevos. El SDF mantiene la legibilidad en ambas resoluciones; no garantiza una rejilla de píxeles enteros en todos los zooms del Editor.

## Inicio

- Menú a la derecha, dentro de un aro circular con tres radios discretos, inspirado en un volante.
- Marca LIFE DRIVE protagonista; lema y opciones Jugar y Acceso para administradores.
- La parte izquierda conserva la vista del entorno logístico y rural de S0.
- Jugar abre Registro en Play Mode mediante el puente de Editor existente.
- Administradores muestra el pendiente de integración; no intenta autenticar ni cargar una pantalla inexistente.

## Registro y personalización

La composición presenta tres bloques, visibles juntos:

1. **Tus datos:** nombre y documento, con placeholder, padding, texto de 32 unidades UI y foco dorado.
2. **Tu personaje:** selección Hombre/Mujer y retrato pixelado actualizado en pantalla.
3. **A tu estilo:** cinco muestras por cada categoría: piel, ojos, cabello y traje. El nombre del color elegido acompaña las muestras.

Los botones Hombre/Mujer cambian la base visual; no son IDs del contenido ni reglas de juego. Cambiar de base conserva los colores. Las dos bases usan el mismo uniforme y distribución de prendas, con variaciones de silueta y cabello. Los cinco colores de traje no añaden habilidades.

No se muestran nombres, ocupaciones o biografías predefinidas. Los cuatro personajes reales de ruta.json siguen intactos, pero esta previsualización no los usa ni simula una asociación con ellos. El contrato futuro de personalización requiere coordinación antes de conectar el registro real.

**Ver mi personaje** comprueba solo los formatos mediante las funciones puras existentes Validacion.NombreValido y Validacion.CedulaValida. No se llama ErrorRegistro con un ID inventado, RegistrarJugador, Sesion, servicios, base de datos o PlayerPrefs. El mensaje válido declara expresamente que no se guardaron datos ni se creó un usuario.

### Retrato

AvatarPersonalizableUI dibuja un retrato conceptual por capas sobre una cuadrícula lógica de 48×64. Es un recurso nativo de UI con pequeños polígonos rectangulares de color, sin PNG generado ni dependencia de IA. Permite cambiar las cuatro regiones sin recolorear imágenes aprobadas. Es un **prototipo gráfico simple**, pendiente de acabado artístico y aprobación; no se presenta como arte de personaje definitivo. El atlas anterior se conserva intacto, pero ya no aparece en estas dos escenas.

### Hover y campos

RespuestaBotonUI anima color, escala 1 → 1.025 y sombra de -3 → -7 unidades; al pulsar reduce a 0.98. Usa tiempo no escalado y eventos de puntero/foco, por lo que no depende del reloj de gameplay. Las muestras usan borde dorado y marcador persistente. No hay conducción mediante teclado.

FocoCampoUI mantiene un caret vacío de 3×32 unidades UI y parpadeo, evitando depender del alto de la geometría de texto vacía de TMP. Cuando hay texto, TMP maneja el cursor, edición y selección normales con ancho 3. El viewport de 430×52 queda centrado dentro del campo de 474×82, con padding lateral 22. El foco se distingue con borde dorado; el placeholder se oculta al escribir.

## Componentes y reutilización

Todos los componentes nuevos están en Assets/Scripts/Mapas/InterfazVisual:

- **FormaRedondeadaUI.cs:** panel/círculo vectorial con radio configurable y hit test redondeado. Incluye CanvasRenderer requerido. Sin PNG ni shader nuevo.
- **RespuestaBotonUI.cs:** feedback reusable para botones y muestras seleccionables.
- **FocoCampoUI.cs:** estado visual del input y caret vacío, sin reglas de registro.
- **AvatarPersonalizableUI.cs:** retrato pixelado configurable mediante colores y base visual.
- **PersonalizacionRegistroUI.cs:** enlaces de la previsualización, colores elegidos y mensajes en memoria; navegación con el puente existente. No representa una entidad del motor.

Prefabs nuevos en Assets/Mapas/InterfazVisual/BaseNaturaleza/Prefabs:

- BotonPrimario.prefab
- BotonSecundario.prefab
- CampoEntrada.prefab
- PanelFormulario.prefab
- TarjetaPersonalizacion.prefab
- ContenedorSeleccion.prefab
- AvatarPersonalizable.prefab
- TituloPixel.prefab

Los prefabs son plantillas de presentación: hay que asignar sus referencias/eventos a un anfitrión al reutilizarlos. Los paneles de personalización no registran clics por sí mismos. Las escenas se construyeron con los mismos componentes y valores, pero sus objetos no son instancias vinculadas de todos los prefabs; cambiar una plantilla no actualiza automáticamente cada escena. El constructor concentra el estilo de esta versión; una configuración de tema editable como ScriptableObject puede incorporarse después si se necesitan varios temas.

## Archivos creados o modificados

**Escenas modificadas:**

- Assets/Scenes/Inicio.unity
- Assets/Scenes/Registro.unity

Sus GUID se conservaron. Sus jerarquías de presentación fueron reconstruidas; ya no llevan los controladores reales desactivados con referencias al antiguo selector de cuatro personajes. Usan exclusivamente PersonalizacionRegistroUI. Los archivos originales del equipo siguen intactos. No se debe activar la lógica de registro antigua sobre este nuevo selector: el contrato ya no es equivalente.

**Scripts nuevos:** los cinco runtime enumerados arriba y Assets/Editor/InterfazVisual/RedisenoBaseUI.cs.

**Recursos nuevos:** los ocho prefabs anteriores; BaseNaturaleza/FuentePixel.asset, BaseNaturaleza/FuenteLectura.asset, BaseNaturaleza/Fuentes/PixelifySans.ttf y BaseNaturaleza/Fuentes/OFL.txt. Todos dentro de Assets/Mapas/InterfazVisual/. Cada archivo y carpeta tiene su .meta.

**Documentación nueva:** Assets/Mapas/RedisenoBaseUI.md, con su .meta.

**Cambios automáticos adicionales detectados:** Unity/TMP reserializó Assets/Mapas/InterfazVisual/FuenteInterfaz.asset y Assets/Mapas/InterfazVisual/Resultados/FuenteResultado.asset durante la apertura, reconstrucción y pruebas. No se diseñaron cambios de estilo en esos recursos ni se sustituyeron sus fuentes. Sus hashes cambiaron, por lo que no se declara integridad binaria de esas dos fuentes anteriores. No se restauraron versiones desde Git porque eran trabajo local sin seguimiento y no había copia inicial de sus bytes.

RedisenoBaseUI es una herramienta de Editor que reconstruye únicamente Inicio y Registro y genera recursos de esta carpeta. Rechaza Play Mode y una escena activa con cambios pendientes. **No es necesario volver a ejecutarla para usar las escenas; reconstruir descarta ediciones posteriores de presentación dentro de esas dos escenas.** No modifica Build Settings ni se ejecuta durante una partida.

## Pruebas realizadas

| Comprobación | Resultado |
|---|---|
| Compilación | Seis clases nuevas compiladas y utilizadas en Unity |
| Inicio 1920×1080 y 1280×720 | Inspección visual en Game View, menú a la derecha, marca y acciones legibles |
| Registro 1920×1080 y 1280×720 | Inspección visual, tres bloques completos y textos visibles |
| Tamaño real del render | Screen.width/height verificados; el zoom nativo del Editor se trató por separado |
| Navegación | Clic real Jugar → Registro y Volver → Inicio |
| Administradores | Clic real muestra pendiente de integración |
| Selección | Clic real en Mujer y cambio de cabello; ambos estilos verificados por estado y render |
| Cuatro categorías | Los veinte botones invocados en Play Mode, verificando el índice y el color exacto del avatar; 0 fallos |
| Hover | Evento de entrada/salida probado: escala 1.025, color principal #F2D98B y sombra -7; retorno a escala 1; feedback visto en los clics |
| Inputs | Clic/foco, placeholder, escritura real de texto genérico, caret y padding revisados |
| Formato | Nombre vacío, documento corto y formato válido; mensajes correctos, sin envío |
| Textos | Revisión visual y preferredHeight; se amplió el área de la marca a 260 para acomodar sus 239.57 unidades preferidas |
| DemoInterfazPartida | Prueba de regresión en Play Mode: 3 situaciones, 3 retroalimentaciones, +25 puntos simulados, 3 insignias y DELIVERY |
| DemoRecorrido | Prueba de regresión en Play Mode: avanza, pausa, reanuda y reinicia |
| Console final | Sin errores ni advertencias en las pruebas finales |

Durante la primera revisión los gráficos propios carecían de CanvasRenderer y no se dibujaban. Se corrigió con RequireComponent y se reconstruyeron ambas escenas antes de repetir la inspección. También apareció una advertencia transitoria de reconexión de Unity MCP tras compilar; no persistió en las pruebas finales. No se borraron mensajes para ocultar fallos.

La comparación SHA-256 con los 366 archivos iniciales detectó únicamente las dos escenas autorizadas y las dos fuentes TMP adicionales indicadas. Los otros 362 archivos conservaron sus hashes en la revisión final: motor, Datos, UI original, ruta.json, mapas, camión, escenas y scripts de demostración, escena Resultado y Build Settings. No aparecieron archivos lifedrive-db.json* en el directorio persistente inspeccionado. No se hace commit ni push.

## Límites y siguientes decisiones

- La apariencia es volátil y no viaja a la partida; no se promete registro, persistencia ni selección de un personaje real.
- La navegación funciona mediante el puente de Editor ya existente. No se añadió Inicio/Registro al ejecutable ni se validó un build distribuido.
- AdminLogin y demás pantallas no se desarrollaron. Resultado quedó fuera del rediseño.
- El retrato procedural es una base funcional para explorar colores, no un retrato final de producción. Ojos y detalles pequeños requieren revisión artística al tamaño final.
- Las escalas fraccionarias suavizan o alternan algunos bordes. Solo se verificó 16:9 en 1920×1080 y 1280×720, no móvil, ultrawide ni accesibilidad completa.
- Hay cinco colores por categoría, dos siluetas y un uniforme común. No hay accesorios, cambio de peinado, animación ni estadísticas.
- La nueva selección no debe traducirse arbitrariamente a los cuatro IDs actuales. Persistir apariencia y enlazar el registro requiere un contrato acordado con Javier, sin reescribir sus validaciones desde presentación.

Para probar: abrir Inicio.unity o Registro.unity y pulsar Play. No se necesita ejecutar el constructor. Ambas pantallas comparten ahora la misma base visual bosque/oliva, dorado, crema, Pixelify Sans, controles redondeados y feedback animado.
