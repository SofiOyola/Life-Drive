# Cambios de Nicolás — Life Drive

Rama: `nicolas/bifurcaciones-visibles`.

Esta nota describe los cambios del juego para su revisión e integración. El documento final del equipo queda a cargo de Jorge.

## Qué cambió

- Se sustituyó el camión poco reconocible por un sprite de cabina roja y carga ocre.
- Se incorporaron bifurcaciones visibles: el camión recorre la carretera elegida antes de pasar al mapa asociado a la decisión.
- Se crearon siete fondos con los cruces integrados al paisaje. Se retiró el dibujo de carreteras superpuesto y se ajustaron las trayectorias al asfalto.
- Se eliminaron los letreros de vía principal/desvío, los números sobre las carreteras y los anuncios del próximo mapa para no anticipar la respuesta.
- Se añadieron siete paisajes deteriorados para R1B, R2B, R2D, R3B, R3D, R3F y R3H: asfalto agrietado, vegetación seca y construcciones descuidadas.
- Las decisiones correctas de los cruces llevan a tramos cuidados; las incorrectas llevan a los deteriorados. En un mapa con variante deteriorada, una respuesta correcta ya presentada recupera su fondo cuidado. Los retrasos acumulados no se borran.
- Sobre un fondo deteriorado, el camión circula al 75 % de su velocidad base y presenta una vibración leve. El movimiento respeta la pausa y los puntos de pregunta.
- Durante el fundido entre mapas, el camión sale y entra por los bordes. El estado del paisaje de destino se aplica antes de revelarlo, también al pausar durante la transición.

## Verificación y entrega

- Tras integrar los paisajes: 145 pruebas EditMode existentes aprobadas y una prueba adicional de correspondencia entre decisiones, paisajes y salidas de recuperación aprobada.
- Tras mejorar el movimiento: 8 pruebas específicas aprobadas, incluidas desaceleración, pausa, recuperación, transición y geometría de los siete cruces. No se repitió entonces toda la suite.
- Compilación Windows de desarrollo completada sin errores; Unity informó 5 advertencias.
- Ejecutable local actualizado: `Builds/PilotoV2-Vulkan/LifeDrivePiloto.exe`. La carpeta completa del build es necesaria para ejecutarlo y no está versionada en Git.
- La última versión del movimiento todavía requiere revisión visual jugando. Estas pruebas no sustituyen la prueba de usabilidad con participantes ni la aceptación del equipo.
- Se mantiene el comportamiento existente: el piloto V2 se activa en Editor o Development Build; una compilación normal conserva V1.

## Alcance de la rama

No se ha fusionado esta rama con `main` ni con las ramas de los compañeros. Los ajustes locales de Vulkan, conexión Pipeline, fuentes generadas y archivos de diagnóstico no forman parte de estos commits. La implementación de KPI, las pruebas formales del piloto y el documento final siguen con sus responsables.

## Commits principales

- `a9e2289`: bifurcaciones visibles y movimiento por el camino elegido.
- `9215186`: cruces integrados en los fondos y eliminación de pistas sobre las rutas.
- `918e304`: paisajes deteriorados y recuperación visual.
- `5b9c741`: movimiento en terreno deteriorado y transiciones entre mapas.
