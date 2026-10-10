# Life Drive

Juego de tablero educativo sobre seguridad en el transporte de carga masiva ("Ruta 360: El viaje seguro"). Cada jugador es un trabajador que lleva su vehículo de **Inicio de operación** a **Destino final**. Mensaje central: *llegar primero no significa ganar; la meta es completar la ruta de forma segura.*

Producto comercial: los usuarios inician sesión y su progreso se guarda. Proyecto de aula de Ingeniería de Software 2, con Scrum adaptado (sprints semanales, backlog en [BACKLOG.md](BACKLOG.md)).

## Estructura

| Ruta | Para qué sirve |
|---|---|
| `game/` | Proyecto de **Unity** (el cliente que ve el jugador). Dentro de `Assets/`: `Scenes`, `Scripts` (`Motor` reglas y puntaje, `Datos`, `Red` llamadas a la API, `UI`, `Editor`), `Prefabs`, `Art`, `Audio`, `Materials`, `Animations`, `Fonts`, `Tests` (`EditMode`, `PlayMode`) |
| `game/Assets/StreamingAssets/Contenido/` | Fuente única del contenido: `tablero.json` y `cartas/` con los 3 mazos. El servidor lee los mismos archivos |
| `server/src/` | API de inicio de sesión y progreso: `rutas`, `controladores`, `servicios` (autenticación, validación de partidas), `modelos` (usuario, partida, progreso), `middlewares`, `config` |
| `database/` | `migraciones/` del esquema y `seeds/` para datos iniciales |
| `tests/` | Pruebas del servidor: `unitarias/`, `integracion/` |
| `deploy/` | Configuración de despliegue y publicación |
| `.github/workflows/` | Integración continua |

## Reglas

Turno: lanza el dado, avanza, ejecuta la casilla. Tipos de casilla (ver `game/Assets/StreamingAssets/Contenido/tablero.json`):

| Casilla | Acción |
|---|---|
| 🟦 Trivia | Pregunta. Correcta: avanzas 2. Incorrecta: te quedas. |
| 🟨 Decisión | Eliges A/B/C; puede avanzar, retroceder, perder turno o dar puntos. |
| 🚨 Emergencia | Resuelves una situación. Acierto: continúas. Error: penalización. |
| 🟩 Seguridad | +10 puntos. |
| 🚧 Riesgo | Carta de situación y decisión. |
| ⭐ Buena práctica | Avanzas 3 y sumas 20. |

Puntaje: destino +50 · trivia correcta +10 · buena decisión +15 · buena práctica +20 · incidente por mala decisión −20. Gana quien más puntos tenga al terminar.

## Flujo de trabajo

Una rama por tarea (`feat/...`, `fix/...`), integración a `main` por pull request. Las cartas se editan solo en `game/Assets/StreamingAssets/Contenido/cartas/*.json` (un archivo por mazo para evitar conflictos).
