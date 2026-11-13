# Cambios de Jorge: KPI (S3-09) y cobertura (S4-07)

Rama `jorge/kpi-s3-09-cobertura-s4-07`, creada desde `nicolas/bifurcaciones-visibles` (incluye el trabajo de Javier hasta `69d7513`). Unity 6000.6.7f1.

## 1. Los cuatro KPI (S3-09)

Se calculan en el motor y la interfaz solo los muestra. Las fórmulas son las del documento de S3-09, recogidas en la sección 17.3 del informe de Sofía:

| KPI | Fórmula | Qué cuenta |
|---|---|---|
| **TDS** · Tasa de Decisiones Seguras | seguras / decisiones × 100 | Situaciones `RIESGO`, `EMERGENCIA` y `DECISION_RUTA` |
| **IEE** · Índice de Efectividad Educativa | acertadas / respondidas × 100 | Solo situaciones `PREGUNTA` |
| **PPS** · Promedio de Puntos de Seguridad | suma de puntos / número de partidas | Solo partidas `FINALIZADA` |
| **PCRS** · Porcentaje de Cumplimiento de Rutas Seguras | rutas seguras / decisiones de ruta × 100 | Solo `DECISION_RUTA`, según `es_acertada` |

**Decisiones tomadas:**
- **Por partida y acumulados.** Cada partida tiene sus KPI. El acumulado del jugador se calcula sobre sus partidas finalizadas; las que siguen en curso no cuentan.
- **Se muestran en el panel de administración y en el PDF**, cada uno con dos columnas: esta partida y el acumulado.
- **Sin casos, el valor es 0**, como dice el documento. Además se muestra el conteo (`75,0 % (3 de 4)`, `0 % (sin casos)`) para que un 0 real no se confunda con la falta de datos.
- **Valores provisionales.** Si alguna decisión sale de una situación con `validada = false`, el panel y el PDF lo advierten. Hoy pasa siempre: ninguna de las 18 situaciones está validada.

**Archivos:**
- [Indicadores.cs](Assets/Scripts/Motor/Indicadores.cs) (nuevo): `CalculadoraIndicadores.Calcular(partidas, decisiones)`, la función que suma varias partidas.
- [ServicioDesempeno.cs](Assets/Scripts/Motor/ServicioDesempeno.cs): `Resumen.Indicadores` (por partida) e `IndicadoresJugador(jugadorId)` (acumulado; solo administrador, permiso `VerPartidasDeJugador`).
- [GeneradorReportes.cs](Assets/Scripts/Motor/GeneradorReportes.cs): sección «Indicadores de desempeño (S3-09)» en el PDF.
- [AdministracionPilotoV2.cs](Assets/Scripts/IntegracionV2/AdministracionPilotoV2.cs): reemplaza el texto «pendientes del servicio de indicadores».
- [ServicioPartidas.cs](Assets/Scripts/Motor/ServicioPartidas.cs) y [ServicioJuego.cs](Assets/Scripts/Motor/ServicioJuego.cs): permiten consultar si una situación V2 está validada.

**Pendiente con el equipo (afecta a PCRS):**
- V1, situación 4: hay dos alternativas seguras (ids 10 y 11), así que elegir cualquiera cuenta como ruta segura.
- V2: en las 4 decisiones de ruta, la alternativa segura está siempre en la primera posición.
- Validar el contenido (`validada`) para que los KPI dejen de ser provisionales.

## 2. Cobertura de código (S4-07): medición repetida

Se repitió con el mismo método: paquete Code Coverage 1.3.0 (ahora en `manifest.json`), modo de código Debug, filtro de ensamblado `LifeDrive`, EditMode, cobertura de líneas.

| Ámbito | Medición de Javier | Esta medición |
|---|---|---|
| Lógica del juego (sin pantallas MonoBehaviour ni `Sesion`) | 98,2 % | **98,0 %** (913/932) |
| `Motor` (sin `Sesion`) | 99,6 % | **99,7 %** (587/589) |
| `Datos` | 100 % | **100 %** (134/134) |
| Ensamblado `LifeDrive` completo | — | 56,6 % (1204/2124) |

- **La medición se confirma.** La diferencia de 0,2 puntos viene del código que Nicolás añadió después en `Mapas`.
- **El 56,6 % del ensamblado completo** incluye `UI`, `Mapas.Demo` y `Mapas.InterfazVisual`, pantallas que EditMode no ejecuta. Conviene que el informe diga qué ámbito se reporta.
- **El código nuevo de KPI tiene 100 %** de cobertura (`CalculadoraIndicadores`, `Indicador`, `GeneradorReportes`).
- **Quedan 2 líneas sin cubrir en `Motor`:** el constructor público de 4 parámetros de `ServicioDesempeno`, que nadie usa, y `ServicioPartidas.cs:164`, la guarda «la ruta no tiene salida» a la que el validador de contenido no deja llegar.

**Cómo repetirla:** Window › Analysis › Code Coverage, activar *Enable Code Coverage* y ejecutar EditMode desde el Test Runner. La configuración ya trae el filtro `LifeDrive`. Los informes van a `CodeCoverage/`, que está ignorada en git. Para que no cuenten como no cubiertas las líneas de inicialización estática, hay que medir justo después de una recarga del dominio.

## 3. Pruebas

- **EditMode: 156/156 aprobadas.** Incluye 9 nuevas en [IndicadoresTests.cs](Assets/Tests/EditMode/IndicadoresTests.cs): fórmulas, casos sin datos, PPS con puntos negativos, acumulado, permisos, contenido sin validar, V2 y PDF.
- **Play Mode: 56/66 aprobadas.** Pasan todas las de administración, incluida la comprobación nueva de los KPI en el panel real ([AdministracionPilotoV2Tests.cs](Assets/Tests/PlayMode/AdministracionPilotoV2Tests.cs)). Las **10 que fallan no las causan estos cambios**: sin ellos fallan las mismas 10.
  - `PruebaIntegracionV2Tests.OchoRutasConBotonesReales` (0–7): esperan «Situaciones resueltas: 1», pero el commit `9215186` cambió ese texto por «Tiempo simulado · Tus decisiones cuentan».
  - `FlujoJugadorPilotoV2Tests.InicioNormalDeDesarrolloOchoRutasSinSelector` (0 y 7): esperan el mapa `S0` y encuentran `R1A` o `R1B`, probablemente por el cambio de entrada entre mapas de `5b9c741`.
  - Las debe revisar Nicolás: o se actualizan las pruebas o se corrige el comportamiento.
