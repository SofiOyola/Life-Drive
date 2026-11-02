# Life Drive · composición y continuidad de 16 fondos

Especificación visual, sin gameplay ni asociaciones con ruta.json.

## Contrato artístico y de enlace

Pixel art 2D indie de 16 bits; lienzo lógico 640×360 (16:9), cenital ortográfico fijo, sin horizonte ni perspectiva oblicua. Escala uniforme: calzada asfaltada de 32 px, dos carriles, bermas de 4 px; sin estrecharla en puentes. Paleta común verde oliva, café tierra, gris cálido y azul desaturado; luz diurna desde noroeste y sombras cortas hacia sureste. Importación futura Point, sin suavizado; exportaciones a múltiplos enteros.

Fondos limpios: sin vehículos, animales, personas, lluvia, niebla, obstáculos móviles, HUD, textos ni botones. Señalización horizontal estática sin palabras. No generar PNG en esta etapa.

Coordenadas normalizadas (0,0 abajo izquierda). Todos entran en (0,.5), rumbo este, y salen en (1,.5), rumbo este. Primer y último 18% del lienzo rectos. Franja de enlace: asfalto y bermas iguales, pasto bajo uniforme sin hitos ni edificios; cambios de bioma solo en el interior. Los enlaces son cambios de panel/escenario, no un mosaico geográfico ni carreteras que se fusionan. La selección de rama ocurre en UI: cada panel presenta una carretera única plausible. Las ocho llegadas a DELIVERY son alternativas de acceso a la misma escena común; nunca coexisten como ocho carreteras en pantalla.

## Trayectorias

Los seis puntos de cada fila definen un eje visual: (0,.5), (.18,.5), (.38,Y1), (.62,Y2), (.82,.5), (1,.5). Curvas futuras: suavizar dentro de la calzada preservando tangentes horizontales de entrada/salida; no usar los vértices como giros instantáneos. Son guías de composición, no movimiento ejecutable ni colliders. Parada de decisión sugerida antes de x=.82. DELIVERY termina su animación futura en (.82,.5); su salida al borde es solo continuidad gráfica, sin conexión posterior.

### S0 · Patio de salida

- Paisaje: Zona logística rural; portería fuera de la vía.
- Vegetación y relieve: Pastos cortos y ladera distante.
- Geometría: Y1=0.5, Y2=0.5; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R1A · Valle agrícola

- Paisaje: Valle abierto con fincas dispersas.
- Vegetación y relieve: Potreros y árboles aislados.
- Geometría: Y1=0.65, Y2=0.65; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R1B · Piedemonte

- Paisaje: Ascenso suave junto a talud.
- Vegetación y relieve: Helechos y bosque de ladera.
- Geometría: Y1=0.35, Y2=0.35; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R2A · Ribera del valle

- Paisaje: Río al norte, paralelo a carretera.
- Vegetación y relieve: Guadua y cultivos bajos.
- Geometría: Y1=0.7, Y2=0.55; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R2B · Fincas cafeteras

- Paisaje: Mosaico de cafetales y acceso lateral cerrado.
- Vegetación y relieve: Cafetales y colinas.
- Geometría: Y1=0.35, Y2=0.65; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R2C · Ladera rocosa

- Paisaje: Talud contenido al sur.
- Vegetación y relieve: Arbustos y roca expuesta estable.
- Geometría: Y1=0.65, Y2=0.35; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R2D · Meseta rural

- Paisaje: Meseta con parcelas.
- Vegetación y relieve: Pastos altos y cercas.
- Geometría: Y1=0.3, Y2=0.4; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R3A · Puente de ribera

- Paisaje: Puente recto sobre cauce norte-sur en x=.5.
- Vegetación y relieve: Vegetación riparia sin cubrir calzada.
- Geometría: Y1=0.5, Y2=0.5; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Tramo central recto sobre puente, parapetos fuera de los 32 px de calzada.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R3B · Cultivos del valle

- Paisaje: Campos de arroz y canales laterales.
- Vegetación y relieve: Vegetación baja y terreno llano.
- Geometría: Y1=0.65, Y2=0.7; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R3C · Loma cafetera

- Paisaje: Curva amplia alrededor de loma.
- Vegetación y relieve: Café y árboles de sombra.
- Geometría: Y1=0.3, Y2=0.35; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R3D · Quebrada rural

- Paisaje: Quebrada al sur; vía sobre terraza.
- Vegetación y relieve: Guadua y pequeños taludes.
- Geometría: Y1=0.65, Y2=0.4; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R3E · Corte de montaña

- Paisaje: Corte estable en roca.
- Vegetación y relieve: Matorral de montaña.
- Geometría: Y1=0.7, Y2=0.6; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R3F · Terraza de montaña

- Paisaje: Terraza amplia con vista cenital de pendientes.
- Vegetación y relieve: Bosque fragmentado y pastos.
- Geometría: Y1=0.35, Y2=0.3; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R3G · Fincas ganaderas

- Paisaje: Potreros divididos y depósitos agrícolas.
- Vegetación y relieve: Pastos y árboles dispersos.
- Geometría: Y1=0.3, Y2=0.6; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### R3H · Corredor industrial rural

- Paisaje: Bodegas pequeñas retiradas de la carretera.
- Vegetación y relieve: Pastos y terreno nivelado.
- Geometría: Y1=0.6, Y2=0.7; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Recorrido de oeste a este sin reversa.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

### DELIVERY · Centro de distribución

- Paisaje: Acceso único a patio logístico; muelles fuera de trayectoria.
- Vegetación y relieve: Jardines bajos y explanada.
- Geometría: Y1=0.5, Y2=0.5; entrada oeste (0,.5), salida este (1,.5); rumbo inicial este. Curvas amplias, asfalto seco continuo y bermas despejadas.
- Trayectoria: aplicar los seis puntos comunes con estos valores. Detención futura en patio; no ramifica.
- Continuidad: conservar el contrato de enlace en todas sus conexiones; el bioma interior aparece gradualmente después del primer 18%.

## Conexiones permitidas

S0 → R1A/R1B

R1A → R2A/R2B; R1B → R2C/R2D

R2A → R3A/R3B; R2B → R3C/R3D; R2C → R3E/R3F; R2D → R3G/R3H

R3A, R3B, R3C, R3D, R3E, R3F, R3G, R3H → DELIVERY

Cada hijo recibe el paisaje de su padre mediante la franja neutra común; no hay uniones antes de DELIVERY. DELIVERY no tiene siguientes. Sus ocho predecesores se guardan en origenes; origen único queda vacío. Las categorías de final dependerán de consecuencias acumuladas, nunca del ID visual. Las asociaciones numéricas permanecen sin asignar.

## Validación

El visor valida automáticamente cardinalidad, IDs, árbol exacto, referencias externas, predecesores, extremos, límites y tangentes de trayectoria, altura de enlace y ancho de vía. El árbol exacto garantiza ocho rutas completas y ausencia de ciclos o reconvergencias intermedias. No valida imágenes: siguen PLACEHOLDER. Revisar curvas suavizadas y continuidad artística cuando existan imágenes; todavía no hay animación.
