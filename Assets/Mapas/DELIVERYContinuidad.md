# DELIVERY · centro logístico compartido

Prototipo final del catálogo, generado con la herramienta integrada de imágenes usando S0, R3G y R3E como referencias PNG. Se revisaron las cuatro notas de continuidad previas, DELIVERY.asset y los ocho fondos R3 con sus salidas (1,0.5). Los conceptos de esta entrega sustituyen la trayectoria preliminar de DELIVERY en ContinuidadVisual.md; ningún R3 se modifica.

## Composición y dirección artística

Acceso único desde la izquierda, portería y postes a ambos lados del acceso, dos bodegas sobre el borde superior de una explanada grande de concreto, zona de descarga completamente vacía y áreas verdes. La perspectiva elevada con fachadas visibles, verdes oliva/tropicales, asfalto oscuro con demarcación amarilla/blanca y luz natural mantienen el lenguaje de S0 y los R3. Las montañas se sugieren por laderas verdes periféricas sin horizonte, coherentes con la cámara aprobada. No contiene vehículos, personas, mercancía, cajas, resultados, textos, interfaz ni iconos.

Prompt aplicado: crear únicamente el centro colombiano de entrega compartido, acceso izquierdo abierto a un patio libre, bodegas por encima del recorrido, vegetación y escala de las referencias, pixel art 16-bit, luz noroeste, sin ocho vías convergentes ni objetos dinámicos.

Generación normalizada a cuadrícula lógica 480×270 y exportación nearest-neighbor 1920×1080; miniatura 480×270. Ambas texturas: Sprite individual, Point, Uncompressed, mipmaps desactivados, NPOT None, máximo 2048.

## Entrada, aproximación y descarga

Coordenadas normalizadas con origen inferior izquierdo y Y hacia arriba:

- Entrada: (0,0.45), rumbo este, alineada con el acceso real del PNG.
- Trayectoria: (0,.45), (.1,.45), (.2,.45), (.28,.45), (.36,.45), (.44,.455), (.52,.47), (.60,.49), (.68,.505), (.74,.515), (.8,.52).
- Punto final: (.8,.52), equivalente a (1536,561.6) sobre 1920×1080 con origen inferior izquierdo; está sobre concreto vacío, delante de las bodegas, sin cubrir paredes, techo ni portería.
- Zona final: Rect(x=.70,y=.40,ancho=.18,alto=.18). Es un área vacía del patio; el punto final está dentro. La trayectoria atraviesa el acceso y luego describe una aproximación suave sobre el concreto. No presupone maniobra de reversa ni movimiento ya programado.

En el ScriptableObject, salida representa el punto final interior de DELIVERY. Su campo zonaEntrega define el área de parada; siguientes sigue vacío y origenes mantiene los ocho R3. En mapas no terminales, salida conserva su significado de enlace.

## Diseño de transición de escena (solo especificación)

Tras finalizar cualquier R3 se podrá ocultar brevemente ese panel mediante fundido, presentar DELIVERY con su entrada común y continuar una futura animación desde (0,.45). No se relacionan físicamente los ocho extremos ni se arrastra la posición de pantalla del R3 al nuevo patio. No se han creado escenas, fundidos ni scripts de gameplay en esta entrega.

ValidadorContinuidad permite exclusivamente los enlaces hacia DELIVERY sin exigir igualdad de altura de raster; sigue verificando árbol, referencias, predecesores y ancho nominal. Para DELIVERY exige categoría Entrega, zona válida dentro de [0,1] y extremo final contenido en ella. Mantiene la comprobación de entrada rumbo este y no exige una tangente de salida al exterior en el destino terminal.

## Verificación y límites

Se inspeccionó visualmente el fondo completo: acceso y recorrido sobre superficies libres; zona final sobre explanada, separada de edificios. El catálogo contiene 16 fondos y 16 miniaturas vinculados; el validador pasa y una prueba temporal con zona que excluye el extremo final detecta el error. La zona original fue restaurada sin guardar la prueba inválida.

Prototipo generado con IA: perspectiva parcialmente oblicua y escala visual, no topografía métrica. El acceso quedó algo por debajo del centro; es intencional que los datos sigan el PNG, aprovechando la transición permitida. La entrada y la explanada no forman un mosaico preciso con los ocho R3. Queda verificar dimensiones, orientación, carril, radio de giro y punto de anclaje del sprite definitivo antes de animarlo. La demarcación termina al entrar al patio, cuya superficie no tiene carriles pintados. Las puertas de bodega son parte estática del fondo, no animaciones de descarga. El gálibo del camión y la transitabilidad no están certificados por física o colliders.
