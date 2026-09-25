# USMP Connect

Plataforma comunitaria para alumnos y docentes de la USMP (Programación I).

**Stack:** ASP.NET Core MVC (.NET 10) · Identity (roles Alumno/Profesor) · EF Core + SQLite · Razor Views · Bootstrap 5 · Docker en Render.

## Módulos

| Módulo | Estado | Responsable |
|---|---|---|
| Login y registro (Alumno / Docente) | ✅ | Ccasani |
| Inicio: notas, horarios, calendario, reportar caso | ✅ | Ccasani |
| Registro de notas por el docente + notificaciones | ✅ | Ccasani |
| CalculadoraFIA | ✅ | Ccasani |
| Marketplace comunitario | 🚧 | Maximo |
| Lost & Found | 🚧 | Maximo |
| Cátedra & Tips | 🚧 | Andrew |
| Banco de Apuntes | 🚧 | Andrew |
| Mi Calendario, Food & Spots | Próxima entrega | — |

## Ejecutar en local

Requisito: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
cd UsmpConnect
dotnet run
```

Abrir http://localhost:5175. La base de datos SQLite se crea sola con datos de prueba.

### Cuentas de demostración (clave `Usmp2026!`)

| Rol | Correo |
|---|---|
| Alumno | `alessandro.morales@usmp.pe` |
| Alumna | `maria.gonzales@usmp.pe` |
| Docente | `jramirez@usmp.pe` |
| Docente | `ltorres@usmp.pe` |

Para registrarse como docente se pide el código de invitación (`Registro:CodigoDocente` en `appsettings.json`).

## Flujo de ramas

| Rama | Uso |
|---|---|
| `main` | Producción. Lo que está aquí se despliega en Render. |
| `develop` | Integración. Aquí se juntan las ramas de cada integrante y se prueba. |
| `Ccasani`, `Maximo`, `Andrew` | Rama de trabajo de cada integrante. |

1. Cada integrante trabaja en su rama y abre un **Pull Request hacia `develop`**.
2. Cuando `develop` está probado, se abre un **Pull Request `develop` → `main`**.
3. Render despliega automáticamente desde `main`.

Convenciones para crear módulos sin conflictos: [docs/GUIA-MODULOS.md](docs/GUIA-MODULOS.md).

## Despliegue en Render

1. En Render: **New → Blueprint** y seleccionar este repositorio (usa `render.yaml`), o
   **New → Web Service → Docker**, rama `main`.
2. Variable de entorno opcional: `Registro__CodigoDocente`.
3. Plan gratuito: el disco no es persistente, la BD se recrea con los datos de prueba en cada despliegue o reinicio.
