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

No se modificó ningún archivo base del proyecto. Todo se conecta solo, siguiendo `docs/GUIA-MODULOS.md`.

## Conceptos que se aplican (para la exposición)

- **MVC**: el controlador consulta la BD con EF Core, arma un ViewModel y la vista Razor lo muestra.
  Las vistas parciales (`_Barra`, `_AportarTip`) evitan repetir código.
- **Entity Framework Core**: `GroupBy` + `Average` para la dificultad comunitaria; índices **únicos**
  (`TipId + UsuarioId`) para que nadie vote, reporte o califique dos veces.
- **Contadores desnormalizados**: `Utiles`, `NoUtiles`, `SumaEstrellas` y `NumCalificaciones` se guardan
  en la tabla para no recalcularlos en cada visita. El detalle de quién votó está en `VotoTip` y `CalificacionApunte`.
- **AJAX**: votar y reportar usan `fetch` (`usmpPost` de `site.js`) y el controlador responde JSON.
- **Seguridad**: todo exige login; los POST validan el token antiforgery; solo los alumnos aportan tips
  (`[Authorize(Roles = Roles.Alumno)]`); solo el autor puede eliminar lo suyo; los archivos se validan por
  extensión y tamaño.
- **Archivos**: `IAlmacenArchivos` guarda lo subido; el controlador devuelve el archivo con
  `PhysicalFile` o `File`: sin nombre se abre en el navegador (vista previa) y con nombre se descarga.

## Cómo probarlo

```bash
cd UsmpConnect
dotnet run
```

Abrir http://localhost:5175 (clave de todas las cuentas: `Usmp2026!`):

- `alessandro.morales@usmp.pe` (alumno): aportar tips, votar, reportar, subir y calificar apuntes.
- `jramirez@usmp.pe` (docente): puede ver los tips sobre sus cursos, pero no aportar.
- Curso con más datos: **Algoritmos y Estructuras de Datos** (III Ciclo), con 3 docentes y 7 tips.
