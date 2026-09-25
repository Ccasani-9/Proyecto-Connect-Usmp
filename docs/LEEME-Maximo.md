# Módulo de Maximo: Lost & Found

Este documento explica qué hace el módulo, qué archivos tiene y cómo funciona por dentro,
por si el profesor pregunta.

## Qué es (`/LostFound`)

Objetos perdidos y encontrados en el campus. Tiene dos pestañas:

- **Objetos Encontrados (Esperando dueño)**: alguien encontró algo y lo reporta, indicando pabellón,
  lugar exacto, fecha y hora, foto y dónde lo dejó (Seguridad, con el reportante o Secretaría).
- **Objetos que Busco (Extraviados)**: el dueño publica lo que perdió con su WhatsApp. Quien lo encuentre
  pulsa **"Lo encontré"** y le escribe directo (`wa.me/51<número>` con el mensaje ya escrito).

Las tarjetas se filtran por categoría con chips: Carnés, Electrónicos / Audífonos, Ropa / Casacas y
Cuadernos / Otros. Cada una lleva una etiqueta: **EN CUSTODIA** (verde), **REPORTADO HOY** (amarillo)
o **RECLAMADO** (azul).

## El flujo de reclamo (lo más importante del módulo)

1. El dueño pulsa **"Reclamar pertenencia"** y llena su código de alumno y nombre (se autocompletan),
   una descripción que solo el dueño conocería (color, marca, contenido...) y su WhatsApp.
2. Quien reportó el objeto recibe una **notificación** en la campanita "Noticias".
3. En **"Ver reclamos"** compara la descripción con el objeto y **aprueba** o **rechaza**.
4. Al aprobar, el objeto pasa a "Reclamado / Entregado a su dueño", las demás solicitudes se rechazan solas
   y cada alumno recibe una notificación con el resultado (y dónde recoger el objeto).

Reglas: no puedes reclamar un objeto que tú reportaste, no puedes enviar dos reclamos pendientes por el
mismo objeto y un reclamo ya resuelto no se puede volver a resolver.

Quien reportó también puede marcar el objeto como **entregado / recuperado** o **eliminar** su reporte.

## Archivos del módulo

| Archivo | Qué contiene |
|---|---|
| `Models/LostFound.cs` | Entidades `ObjetoPerdido` y `ReclamoObjeto`, y enums de tipo, categoría, custodia y estado |
| `Data/ApplicationDbContext.LostFound.cs` | `DbSet` de objetos y reclamos (clase parcial) y sus relaciones |
| `Data/Seed/LostFoundSeeder.cs` | 9 objetos y 2 reclamos de prueba |
| `Services/WhatsApp.cs` | Valida el número (9 dígitos, empieza con 9) y arma el enlace `wa.me` |
| `ViewModels/LostFoundViewModels.cs` | Datos de la vista y de los formularios "Reportar" y "Reclamar" con validaciones |
| `Controllers/LostFoundController.cs` | `Index`, `Reportar`, `Reclamar`, `Reclamos`, `ResolverReclamo`, `MarcarDevuelto`, `Eliminar` |
| `Views/LostFound/Index.cshtml` | Pestañas, tarjetas y modales "Reportar" y "Reclamar Pertenencia" |
| `Views/LostFound/Reclamos.cshtml` | Revisión de las solicitudes de reclamo |

No se modificó ningún archivo base del proyecto. Todo se conecta solo, siguiendo `docs/GUIA-MODULOS.md`:
el menú lateral ya tenía el enlace `/LostFound`.

## Conceptos que se aplican (para la exposición)

- **MVC**: el controlador recibe la petición, consulta la BD con EF Core y le pasa un ViewModel a la vista Razor.
- **Entity Framework Core**: las clases de `Models/` son tablas. La relación "un objeto tiene muchos reclamos"
  se configura con `IEntityTypeConfiguration`; `Include(...)` trae los reclamos junto con el objeto.
- **Validación**: atributos `[Required]`, `[StringLength]` y `[RegularExpression]` en los ViewModels, más reglas
  en el controlador (fecha no futura, pabellón de la lista, custodia obligatoria si fue encontrado, WhatsApp
  obligatorio si está perdido, no reclamar lo propio).
- **Seguridad**: todo exige login; los POST validan el token antiforgery; solo quien reportó el objeto puede
  ver y resolver sus reclamos, marcarlo como entregado o eliminarlo (si no, responde 404).
- **Subida de archivos**: las fotos pasan por `IAlmacenArchivos` (solo JPG/PNG/WEBP, máx. 5 MB).
- **Notificaciones**: `INotificacionService` avisa en "Noticias" cuando llega un reclamo y cuando se resuelve.

## Cómo probarlo

```bash
cd UsmpConnect
dotnet run
```

Abrir http://localhost:5175 (clave de todas las cuentas: `Usmp2026!`):

- `alessandro.morales@usmp.pe`: reportó el "Laptop Charger USB-C" y Lucía lo reclamó, así que puede
  aprobar o rechazar ese reclamo en **Ver reclamos**. También reclamó los "Audífonos Sony".
- `maria.gonzales@usmp.pe`: reportó los "Audífonos Sony", que Alessandro reclamó.
