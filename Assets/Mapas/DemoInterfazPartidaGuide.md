# DemoInterfazPartida · prototipo de presentación

Unity 6000.6.7f1 · rama feat/unity-map. Escena independiente, sin integración con el motor ni las pantallas de Javier.

## Ejecutar y usar

1. Abrir `Assets/Scenes/DemoInterfazPartida.unity` y pulsar Play. Maximizar Game View para revisar textos y botones; recomendado 1280×720 o mayor. No es necesario añadir la escena a Build Settings.
2. El camión avanza automáticamente desde S0. Aproximadamente a los 3.5 segundos, con los valores iniciales, se detiene y aparece una situación ficticia.
3. Elegir una de las dos respuestas haciendo clic. La retroalimentación y el indicador de puntos son simulados; el camión sigue detenido.
4. Pulsar **Continuar recorrido**. Se conserva la posición en S0 y se prepara la continuación visual correspondiente. El vehículo atraviesa los fondos con los fundidos existentes hasta DELIVERY.
5. **Pausar / Reanudar** controla el movimiento y los fundidos durante la marcha. Durante situación y retroalimentación el camión ya está detenido; no aparece ese botón para evitar saltarse la respuesta.
6. **Reiniciar** está disponible en todos los estados válidos. Borra solo la elección y puntos en memoria de esta demo, vuelve al inicio de S0 e inicia otra vez el recorrido. No reinicia ni borra partidas reales.

No hay conducción manual, WASD ni flechas. No se solicita identidad ni se abre Sesion.Juego.

## Diseño de interfaz

Lienzo lógico de 1600×900, ajustado proporcionalmente a la ventana y centrado, con márgenes para otras relaciones de aspecto. El mapa completo ocupa un rectángulo 16:9 a la izquierda y la bitácora/situación queda a la derecha. El camión se dibuja separado, con el sprite y escala existentes. No se recortan ni deforman fondos para hacer espacio a paneles.

Cabecera: nombre, aviso permanente **PROTOTIPO · DATOS FICTICIOS · SIN GUARDADO**, barra segmentada del progreso total y puntos de seguridad simulados. Panel lateral: situación, respuestas grandes, retroalimentación o llegada. Franja bajo el mapa: ID visual actual y estado del vehículo. Pie: recordatorio de simulación y Reiniciar.

Paneles oscuros, texto marfil, acentos verde agua, bordes rectos y sombras escalonadas acompañan el pixel art sin añadir decoración sobre la carretera. Los controles se dibujan con IMGUI propio; no se usan ni alteran prefabs o pantallas de Javier. La tipografía es la de Unity, elegida por legibilidad: no es una fuente pixel art personalizada. Esta es una maqueta funcional, no la UI definitiva.

## Datos aislados y cambio ficticio de ruta

`Assets/Mapas/DemoInterfaz/SituacionRadioDemo.asset` es un ScriptableObject propio con título, enunciado, punto de demostración y dos respuestas. Cada respuesta contiene texto, retroalimentación, consecuencia ilustrativa, puntos simulados y referencia a una hoja visual existente. No contiene IDs de tramo, situación o alternativa del motor.

Ejemplo original: un aviso de acceso por radio llega incompleto. No se copiaron ni modificaron las preguntas de ruta.json.

| Respuesta de prueba | Puntos simulados | Secuencia visual ficticia |
|---|---:|---|
| Confirmar el mensaje antes de continuar | +10 | S0 → R1A → R2A → R3A → DELIVERY |
| Seguir el corredor señalizado disponible | +0 | S0 → R1B → R2D → R3H → DELIVERY |

Estas asociaciones existen **solo para comparar la presentación de esta demo**. No son equivalencias educativas autorizadas, no rellenan CatalogoMapas.asociaciones y no implican éxito/fracaso del jugador. Los valores se muestran directamente desde el recurso: no hay evaluación, reglas de puntos ni copia del sistema Strategy de Javier. Responder otra vez durante retroalimentación no cambia la elección.

Para ajustar el momento de parada, seleccionar SituacionRadioDemo.asset y cambiar **Punto Demostracion** entre 0.05 y 0.90 antes de iniciar Play. Valor inicial 0.35: es 35% de la longitud de S0, no del recorrido completo. Por eso la cabecera muestra aproximadamente 7% de progreso total al aparecer la situación. La velocidad inicial de esta escena es 48 píxeles lógicos/s; puede ajustarse en el componente DemoRecorrido del objeto Prototipo Interfaz Partida. La escala sigue siendo 18×5.3467 píxeles lógicos, carril derecho 6 px.

## Arquitectura y reutilización

- `SituacionInterfazDemo`: datos ficticios serializados, separados de la interfaz y del contenido real. No accede a archivos ni servicios.
- `DemoInterfazPartida`: máquina de estados EnMarcha, Situacion, Retroalimentacion, Finalizado y Error; presenta la maqueta, controla espera y elección en memoria. Valida referencias y continuaciones al iniciar.
- `DemoRecorrido`: conserva su modo de demo autónoma. Se añadieron ControlExterno, AreaVisualExterna y PosicionarProgreso para que un presentador propio controle el reloj, use un área de mapa específica y restaure el porcentaje al elegir una continuación ficticia. Cuando ControlExterno es false, mantiene sus controles y Update originales. PosicionarProgreso pausa y no actúa durante fundidos.
- `RecorridoDemo` y `RecorridosVisualesDemo`: se reutilizan sin cambios para geometría, longitud, carril y enumeración de caminos. No se duplican las dieciséis definiciones, las trayectorias ni el sprite.

El anfitrión fija ControlExterno en Awake; se inicializa después del reproductor y es el único que llama a Avanzar durante esta escena. Así se evita doble velocidad por dos Update. Al alcanzar el punto de prueba recorta el avance del fotograma exactamente al umbral, pausa y abre la situación. Continuar cambia la secuencia visual y restaura el porcentaje de S0 antes de volver a reproducir; no teletransporta al inicio del mapa. La llegada final se consulta del reproductor existente.

Puntos que podrán conectarse en otra etapa: suministro de texto/alternativas, notificación de llegada a un hito, envío del clic a un adaptador, retroalimentación confirmada, comandos incrementales de mapa y autorización para navegar a resultados. El modelo de ejemplo no es un contrato definitivo con Motor. El análisis de grafos y los acuerdos pendientes con Javier en AnalisisIntegracion.md siguen vigentes.

## Archivos de esta etapa

Modificado:

- `Assets/Scripts/Mapas/Demo/DemoRecorrido.cs`

Creados:

- `Assets/Scenes/DemoInterfazPartida.unity` y `.meta`
- `Assets/Scripts/Mapas/Demo/DemoInterfazPartida.cs` y `.meta`
- `Assets/Scripts/Mapas/Demo/SituacionInterfazDemo.cs` y `.meta`
- `Assets/Mapas/DemoInterfaz.meta`
- `Assets/Mapas/DemoInterfaz/SituacionRadioDemo.asset` y `.meta`
- `Assets/Mapas/DemoInterfazPartidaGuide.md` y `.meta`

DemoRecorrido.unity, EditorBuildSettings, las dieciséis definiciones, el catálogo y los scripts RecorridoDemo/RecorridosVisualesDemo no se modificaron.

## Pruebas realizadas

Unity compiló los componentes nuevos y modificados. Se abrió la escena real en Play Mode y se inspeccionó Game View: mapa, vehículo, contraste, márgenes, situación, botones, retroalimentación y estado final. Se pulsaron con el mouse ambas respuestas y Continuar, comprobando que el estado del componente y el movimiento respondían a esos clics. La elección A mostró +10 simulados y B +0 simulados. Se observó continuación y llegada real mediante Update en la rama B, acelerada temporalmente a 160 px/s para revisar el final; el valor guardado en la escena permanece en 48.

Además, sobre el componente vivo en Play Mode se ejecutó el mismo Avanzar usado por Update a pasos de 1/60 s:

| Comprobación | Resultado |
|---|---|
| Detención en S0 al 35%, incluso recibiendo un paso grande | Pasa, ambas respuestas |
| Espera sin avanzar durante situación y retroalimentación | Pasa |
| Segunda respuesta ignorada durante retroalimentación | Pasa |
| Continuar conserva distancia en S0 | Pasa |
| Pausar detiene distancia; reanudar continúa | Pasa |
| Rama A: orden S0/R1A/R2A/R3A/DELIVERY | Pasa; 2911 pasos después de la continuación y prueba de pausa |
| Rama B: orden S0/R1B/R2D/R3H/DELIVERY | Pasa; 2949 pasos después de la continuación y prueba de pausa |
| Final de ambas ramas dentro de zonaEntrega, detenido | Pasa |
| Reiniciar devuelve progreso y puntos simulados a cero | Pasa |
| Una instancia de reproductor y una de presentador | Pasa |

Los barridos de métodos en Play Mode no se presentan como inspección visual de todos sus fotogramas. Se revisaron visualmente estados concretos y el flujo de clics, no la envolvente completa del camión en todas las curvas.

Regresión de la escena anterior DemoRecorrido en Play Mode: ControlExterno=false, mapa único avanzando y ocho recorridos completos detenidos en DELIVERY. Los ocho pasaron tras la ampliación del reproductor.

Integridad: 117 archivos protegidos de Motor, Datos, UI, StreamingAssets y Backgrounds conservaron sus hashes SHA-256. También los conservaron los 19 archivos comprobados de definiciones/catálogo, DemoRecorrido.unity y EditorBuildSettings. En Application.persistentDataPath no existían archivos lifedrive-db.json* antes de la prueba ni aparecieron después. El código nuevo no tiene referencias a Sesion, servicios, tablas, File ni PlayerPrefs. No se generó ninguna decisión o puntuación real.

## Limitaciones

- Una sola situación ficticia y dos continuaciones, no todas las combinaciones educativas ni las ocho rutas seleccionables desde esta interfaz. La demo anterior conserva su selector de ocho recorridos.
- El contenido y los puntos son ilustrativos y no validados pedagógicamente. No se implementan tres categorías de finales, consecuencias reales, persistencia o reanudación de partida.
- No es una integración con PantallaPartida ni un reemplazo de esa pantalla. No existen asociaciones aprobadas con los cuatro tramos lógicos.
- Fuente estándar y UI escalada proporcionalmente. En ventanas pequeñas el texto pierde tamaño; no es una adaptación definitiva para móviles o accesibilidad. Rotaciones y escalas no enteras conservan las limitaciones de pixelado existentes.
- Se mantienen las cinco advertencias artísticas entre fondos y las revisiones de carril/curvas documentadas. Los fundidos no validan cabida física, colisiones ni continuidad métrica.
- El sprite conserva las limitaciones de perspectiva y escala provisional de ValidacionCamion.md. No se han cambiado fondos ni geometría para disimularlas.
- La elección ficticia selecciona una continuación completa a partir de S0. La integración futura requerirá comandos incrementales y contratos de espera acordados, sin reutilizar esta tabla ficticia como lógica real.

La revisión final de Console devolvió cero errores y cero advertencias. Se preservaron los cambios locales previos y no se hizo commit ni push. La escena nueva queda abierta en modo edición al terminar las pruebas.
