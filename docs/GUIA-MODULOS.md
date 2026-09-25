# Guía para crear un módulo en USMP Connect

Esta guía es para cualquier integrante (o su asistente de IA) que agregue un módulo nuevo.
Si la sigues, tu módulo no genera conflictos con las ramas de los demás.

## Estructura del proyecto

```
UsmpConnect/
├─ Controllers/        un controlador por módulo (MarketplaceController, CatedraController...)
├─ Models/             entidades de EF Core (un archivo por módulo: Marketplace.cs, Catedra.cs...)
├─ ViewModels/         modelos para las vistas
├─ Data/
│  ├─ ApplicationDbContext.cs            contexto base (NO agregar DbSet de módulos aquí)
│  ├─ ApplicationDbContext.<Modulo>.cs   DbSet del módulo (clase parcial)
│  ├─ DbInitializer.cs                   crea la BD y ejecuta los seeders
│  └─ Seed/<Modulo>Seeder.cs             datos de prueba del módulo
├─ Services/           servicios compartidos (notificaciones, archivos, hora de Perú)
├─ Views/<Modulo>/     vistas Razor del módulo
└─ wwwroot/css/site.css  estilos globales (clases reutilizables)
```

## Reglas para evitar conflictos

1. **No edites archivos base** salvo que sea necesario: `Program.cs`, `ApplicationDbContext.cs`,
   `_Layout.cshtml`, `site.css`, `BaseSeeder.cs`.
2. **DbSet del módulo** → en un archivo nuevo con clase parcial:

   ```csharp
   namespace UsmpConnect.Data;
   public partial class ApplicationDbContext
   {
       public DbSet<Articulo> Articulos => Set<Articulo>();
   }
   ```

3. **Configuración de entidades** → una clase `IEntityTypeConfiguration<T>` (se registra sola).
4. **Datos de prueba** → una clase que implemente `IModuleSeeder` con `Orden` ≥ 10
   (se ejecuta sola). Usa los usuarios de `DemoUsers` (ej. `DemoUsers.Maria`).
5. **Menú lateral**: ya tiene todos los módulos. La URL de cada uno es el nombre del controlador:
   `/FoodSpots`, `/Catedra`, `/Marketplace`, `/Apuntes`, `/LostFound`, `/Calendario`.
   Si el controlador no existe, se muestra "En construcción".
6. **Estilos**: reutiliza las clases de `site.css`. Si necesitas CSS propio, ponlo en la vista con
   `@section Styles { <style>...</style> }`.

## Base de datos

- SQLite con `EnsureCreated()`. **La BD se borra y se vuelve a crear en cada arranque**
  (`Database:ResetOnStartup = true`), así siempre coincide con los modelos de todas las ramas.
- Cuando el proyecto se estabilice, pasaremos a migraciones y Postgres.
- En SQLite, los `decimal` se guardan como `REAL`, así que se pueden ordenar y comparar en consultas.

## Servicios compartidos

| Servicio | Para qué |
|---|---|
| `User.GetUserId()`, `User.GetNombre()`, `User.EsProfesor()` | Datos del usuario logueado |
| `INotificacionService.NotificarAsync(userId, mensaje, url, icono)` | Notificación en la campanita "Noticias" |
| `IAlmacenArchivos.GuardarAsync(archivo, "carpeta", extensiones, maxBytes)` | Subir imágenes o documentos a `wwwroot/uploads` |
| `HoraPeru.Ahora`, `HoraPeru.Hoy`, `HoraPeru.Hace(fechaUtc)` | Fechas en hora de Lima ("hace 2 días") |

## Seguridad

- Todo requiere login salvo lo marcado con `[AllowAnonymous]`.
- Solo docentes: `[Authorize(Roles = Roles.Profesor)]`. Solo alumnos: `[Authorize(Roles = Roles.Alumno)]`.
- Todos los POST validan el token antiforgery automáticamente. En formularios Razor con
  `method="post"` el token se agrega solo; en `fetch` usa `usmpPost(url, datos)` de `site.js`.
- Antes de modificar o eliminar algo, verifica que pertenezca al usuario logueado.

## Clases CSS más usadas

| Clase | Uso |
|---|---|
| `card-u p` | Tarjeta blanca con padding |
| `card-u page-head` | Encabezado de página (título + botón) |
| `btn-guinda`, `btn-outline-guinda`, `btn-soft`, `btn-wa` | Botones |
| `chips` + `chip` / `chip active` | Filtros tipo píldora |
| `tag tag-ok / tag-warn / tag-bad / tag-info / tag-guinda / tag-muted` | Etiquetas de estado |
| `search-box` | Buscador con ícono |
| `dropzone` + `<input type="file" hidden data-dropzone>` | Zona para subir archivos (JS incluido) |
| `vacio` | Estado vacío ("No hay resultados") |
| `avatar avatar-soft avatar-sm` | Círculo con iniciales |

Íconos: [Bootstrap Icons](https://icons.getbootstrap.com/) → `<i class="bi bi-bag"></i>`.

## Flujo de Git

```bash
git checkout develop && git pull
git checkout -b <TuRama>          # o git checkout <TuRama> && git merge develop
# ... trabajar, commits ...
git push -u origin <TuRama>
# Abrir Pull Request <TuRama> → develop en GitHub
```
