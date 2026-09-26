# Módulos de Andrew: Cátedra & Tips + Banco de Apuntes

Este documento explica qué hace cada módulo, qué archivos tiene y cómo funciona por dentro,
por si el profesor pregunta.

## 1. Cátedra & Tips (`/Catedra`)

Opiniones y consejos de los alumnos sobre cada curso y sus docentes.

**Lista de cursos (`/Catedra`)**

- Buscador por curso, código o docente. No distingue mayúsculas ni tildes: "ramirez" encuentra "Ramírez".
- Filtro por ciclo (I a V) con chips.
- Cada curso muestra su **dificultad comunitaria**, los docentes que lo dictan y cuántos tips tiene.

**Detalle de un curso (`/Catedra/Curso/{id}`)**

- Encabezado con código, créditos, ciclo y la barra de **Dificultad Comunitaria** (promedio de 1 a 5).
- **Docentes que dictan esta cátedra**: sección, estrellas de **claridad** (promedio) y etiqueta de
  **exigencia** (Baja / Media / Alta / Muy Alta).
- **Muro de Tips y Consejos**, ordenable por "Más útiles primero" (👍 menos 👎) o "Más recientes".
  - **Votar** 👍 / 👎 sin recargar la página (`fetch`). Votar igual otra vez quita el voto.
  - **Reportar** un tip inapropiado. Con 3 reportes de distintos alumnos, el tip se oculta.
  - El autor puede eliminar su tip. No puede votarlo ni reportarlo.
- **Aportar un Tip** (solo alumnos): curso, docente (la lista se filtra según el curso), texto,
  dificultad del curso, y claridad y exigencia del docente.

Todos los promedios se calculan a partir de los tips: cada tip es también una pequeña encuesta.

## 2. Banco de Apuntes (`/Apuntes`)

Resúmenes, exámenes resueltos, guías de laboratorio y apuntes que comparten los alumnos.

- Buscador, filtros por **ciclo** y **asignatura**, chips por **tipo** y orden por mejor calificados,
  más descargados o más recientes.
- **Subir Apunte o Guía**: título, asignatura (filtrada por ciclo), profesor (con sugerencias), tipo,
  descripción y archivo **PDF, DOC o DOCX de hasta 25 MB**.
- **Vista Previa**: abre el PDF dentro de la página. Los Word solo se descargan.
- **Descargar**: descarga con el nombre original del archivo y suma 1 al contador de descargas.
- **Calificar** de 1 a 5 estrellas: una calificación por alumno (se puede cambiar) y no puedes calificar
  tus propios apuntes. El autor recibe una notificación en "Noticias".

Los apuntes de ejemplo no tienen un archivo guardado: su PDF se genera al momento de verlo o descargarlo
(`Services/PdfSimple.cs`). Así funcionan aunque el disco de Render se borre en cada despliegue.

## Archivos del módulo

| Archivo | Qué contiene |
|---|---|
| `Models/Catedra.cs` | Entidades `Tip`, `VotoTip`, `ReporteTip` y enum `Exigencia` |
| `Models/Apuntes.cs` | Entidades `Apunte` y `CalificacionApunte`, y enum `TipoDocumento` |
| `Data/ApplicationDbContext.Catedra.cs` | `DbSet` de tips, votos y reportes, y sus índices únicos |
| `Data/ApplicationDbContext.Apuntes.cs` | `DbSet` de apuntes y calificaciones |
| `Data/Seed/CatedraSeeder.cs` | 15 tips de prueba en 7 cursos |
| `Data/Seed/ApuntesSeeder.cs` | 8 apuntes de prueba |
| `Services/PdfSimple.cs` | Genera PDFs de una página sin librerías externas |
| `ViewModels/CatedraViewModels.cs` | Datos de las vistas y del formulario "Aportar un Tip" |
| `ViewModels/ApuntesViewModels.cs` | Datos de la vista y del formulario "Subir" |
| `Controllers/CatedraController.cs` | `Index`, `Curso`, `Aportar`, `Votar`, `Reportar`, `Eliminar` |
| `Controllers/ApuntesController.cs` | `Index`, `Subir`, `Ver`, `Descargar`, `Calificar`, `Eliminar` |
| `Views/Catedra/Index.cshtml` | Lista de cursos |
| `Views/Catedra/Curso.cshtml` | Docentes y muro de tips |
| `Views/Catedra/_Barra.cshtml`, `_AportarTip.cshtml`, `_Estilos.cshtml` | Partes compartidas por las dos vistas |
| `Views/Apuntes/Index.cshtml` | Lista, filtros y modales "Subir", "Calificar" y "Vista previa" |

## 3. Food & Spots FIA (`/FoodSpots`)

Huariques, cafeterías y restaurantes recomendados por y para alumnos de la FIA USMP alrededor del campus La Fontana.

- **Mapa Interactivo (Leaflet.js + OpenStreetMap)**:
  - Centrado en el campus de la Facultad (`Av. La Fontana 1250, La Molina`, coordenadas `-12.0722, -76.9632`).
  - Marcador principal de la FIA ("CAMPUS FIA USMP") y pines dinámicos de cada restaurante con su calificación en estrellas, rango de precios y botón directo a la carta.
  - Botón de geolocalización rápida "Centrar en la FIA".
- **Tarjetas de Restaurantes**:
  - Foto, categoría (Menú Criollo, Comida Rápida, Chifa, Cafetería & Huarique, Pastelería, Saludable), distancia a pie desde la puerta principal (ej. "A 40m de puerta 1", "Dentro del campus") y rango de precios.
  - Calificación en estrellas con promedio y total de opiniones.
  - Plato estrella recomendado con precio.
  - Botón **"Ver en mapa"** (centra y hace zoom al pin correspondiente) y botón **"Ver carta"**.
- **Modal de Detalle & Reseñas**:
  - Lista completa de platos de la carta con precios y badge de "Popular".
  - Botón directo para pedir o consultar por WhatsApp (`wa.me/51...`).
  - Muro de reseñas de alumnos con calificación (1 a 5 estrellas) y comentarios.
  - Formulario interactivo para que cualquier alumno aporte su reseña.

---

## 4. Stack Tecnológico Cloud (Exigido por el Docente)

Andrew implementó la capa de infraestructura cloud del proyecto con un patrón de **Graceful Fallback**: si las variables de entorno están activas, utiliza los servicios en la nube; si no, conmuta automáticamente a memoria local sin caerse jamás.

| Tecnología | Rol en USMP Connect | Dónde se aplica en el código |
|---|---|---|
| **SQLite + EF Core** | Base de datos relacional con 11 tablas | `ApplicationDbContext`, migraciones/seeders automáticos |
| **Redis Cloud** (`cloud.redis.io`) | Caché ultrarrápida (sub-milisegundo) | `IRedisCacheService` en `FoodSpotsController` para catálogos y restaurantes |
| **CloudAMQP** (`customer.cloudamqp.com`) | Cola de mensajería asíncrona RabbitMQ | `IColaMensajes` para encolar auditorías y alertas de nuevas reseñas |
| **Algolia** (`dashboard.algolia.com`) | Motor de búsqueda inteligente instantáneo | `IBuscadorAlgolia` con indexación REST y búsqueda typo-tolerant |
| **PieHost** (`piehost.com`) | WebSockets para tiempo real | `IPieHostService` y `site.js` para emitir alertas en vivo al publicar reseñas |
| **Render** (`onrender.com`) | Despliegue en contenedor Docker | `Dockerfile`, `render.yaml` en producción continua |

### Panel de Diagnóstico (`/Tecnologia`)
Disponible en el menú superior (**STACK CLOUD**) o navegando a `/Tecnologia`. Muestra el estado en vivo de los 6 servicios con tarjetas explicativas y métricas de la base de datos.

### Seguridad de Credenciales
Ninguna API Key ni cadena de conexión sensible está expuesta en GitHub.
- **En local**: se gestionan mediante `dotnet user-secrets` y `appsettings.Local.json` (incluido en `.gitignore`).
- **En Render**: se inyectan como variables de entorno (`Redis__ConnectionString`, `CloudAMQP__Url`, etc.).

---

## Archivos del módulo

| Archivo | Qué contiene |
|---|---|
| `Models/FoodSpots.cs` | Entidades `Restaurante`, `PlatoRestaurante`, `ResenaRestaurante` y enums |
| `Data/ApplicationDbContext.FoodSpots.cs` | `DbSet` de restaurantes, platos y reseñas |
| `Data/Seed/FoodSpotsSeeder.cs` | 6 restaurantes reales de la FIA, platos y reseñas iniciales |
| `ViewModels/FoodSpotsViewModels.cs` | ViewModels de lista, detalle, platos y formulario de reseñas |
| `Controllers/FoodSpotsController.cs` | `Index` (con Redis), `Detalle`, `Resenar` (con CloudAMQP y PieHost), `Buscar` (Algolia) |
| `Views/FoodSpots/Index.cshtml` | Vista con mapa interactivo Leaflet y modal de carta y reseñas |
| `Services/CacheService.cs` | Cliente de Redis Cache con fallback en memoria |
| `Services/ColaMensajes.cs` | Productor CloudAMQP (RabbitMQ) con fallback local |
| `Services/BuscadorAlgolia.cs` | Motor Algolia Search con indexador y búsqueda instantánea |
| `Services/PieHostService.cs` | Emisor WebSockets PieHost para tiempo real |
| `Controllers/TecnologiaController.cs` | Dashboard de verificación de infraestructura para el profesor |
| `Views/Tecnologia/Index.cshtml` | Vista con semáforos y métricas de los 6 servicios |

---

## Cómo probarlo en la sustentación

```bash
cd UsmpConnect
dotnet run
```

Abrir http://localhost:5175 (clave: `Usmp2026!`):
1. **Verificar el Stack Tecnológico**: Haz clic en **STACK CLOUD** en la barra superior o entra a `/Tecnologia` para mostrarle al profesor los 6 servicios operativos.
2. **Food & Spots**: Entra a `/FoodSpots`, navega por el mapa interactivo alrededor de la FIA, haz clic en "Ver en mapa", abre la carta de "Doña Rossi", consulta por WhatsApp y publica una reseña.
3. **Comprobar Redis**: Al cargar `/FoodSpots`, verás la etiqueta `Redis Cache (Hit ⚡)`.
4. **Comprobar WebSockets**: Abre dos pestañas del navegador; al publicar una reseña en una, saldrá el aviso en vivo por WebSocket en la otra.

