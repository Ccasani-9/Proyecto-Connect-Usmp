# USMP Connect · Guía de Estado, Metodología GitFlow y Directivas para el Equipo (e IAs)

> **Documento oficial de coordinación para los integrantes (Andrew, Ccasani, Maximo) y sus asistentes de Inteligencia Artificial.**  
> *Léelo atentamente antes de escribir cualquier línea de código o abrir un nuevo chat con IA.*

---

## 1. 📌 Visión y Filosofía del Proyecto

* **Nombre:** **USMP Connect** (Ecosistema Estudiantil FIA).
* **Propósito:** Plataforma comunitaria y de productividad académica creada **exclusivamente por y para estudiantes** de la Facultad de Ingeniería y Arquitectura (FIA) de la USMP (Campus La Molina).
* **Concepto Clave:** **NO es un sistema administrativo para profesores.** Los docentes no tienen panel de notas oficial (para eso la universidad ya tiene SAP y Moodle). Los profesores solo figuran como sujetos de recomendación comunitaria en *Cátedra & Tips* (para calificar su claridad y nivel de exigencia entre alumnos).
* **Stack Tecnológico:**
  * **Backend:** ASP.NET Core MVC en **.NET 10** (`net10.0`) y C# 14.
  * **Base de Datos:** Entity Framework Core 10 con SQLite (`usmpconnect.db`), con recreación y sembrado automático de datos realistas al arranque (`DbInitializer`).
  * **Autenticación:** ASP.NET Core Identity por cookies (exclusivo alumnos con correo `@usmp.pe` y código de 10 dígitos).
  * **Servicios Cloud Integrados:** Redis Cloud (Caché), CloudAMQP RabbitMQ (Mensajería Asíncrona), Algolia Search API (Búsqueda Full-Text instantánea) y PieHost (WebSockets para tiempo real).
  * **Frontend:** Razor Views (`.cshtml`), Bootstrap 5.3, Bootstrap Icons y CSS institucional USMP (Guinda `#8B1D2C`). Diseño 100% responsive en Desktop, Laptops y Móviles (menú *drawer* con backdrop).
  * **Despliegue:** Docker Multi-stage non-root en **Render.com** (`https://usmpconnect.onrender.com`).

---

## 2. 🚀 Lo que YA ESTÁ IMPLEMENTADO y Funcionando al 100%

*(Todo esto ya está probado con Playwright en el navegador, sin errores de compilación y sincronizado en `Andrew`, `develop` y `main`)*:

1. **Dashboard de Inicio (`/`):** Saludo dinámico según la hora, notas del ciclo, asignaturas del día (`HOY`, `MAÑANA`) y accesos directos institucionales.
2. **Mis Notas / Calculadora FIA (`/Notas`):** Proyección de notas y promedio ponderado semestral con la fórmula de evaluación continua y exámenes de la FIA USMP.
3. **Cátedra & Tips (`/Catedra`):** Catálogo de asignaturas (Ciclos I al V), dificultad comunitaria, muro de recomendaciones por docente y votos útiles.
4. **Banco de Apuntes (`/Apuntes`):** Repositorio colaborativo de PDFs con visor integrado sin extensiones pesadas, conteo de descargas y calificación por estrellas.
5. **Marketplace Universitario (`/Marketplace`):** Tablón de compra, venta y donación de insumos de ingeniería (calculadoras, batas, Arduinos) con botón directo de WhatsApp.
6. **Objetos Perdidos (`/ObjetosPerdidos`):**
   * Pestaña `📦 Objetos Encontrados`: Tablón público con **custodia confidencial** (no se revela la garita a curiosos para evitar oportunismo).
   * Pestaña `🔍 Objetos Extraviados`: Reportes de búsqueda con contacto directo.
   * Pestaña `👥 Mis Publicaciones`: Gestión privada de reportes propios con botón `Ver solicitudes recibidas` y selector de garita oficial al aprobar.
   * Pestaña `👆 Mis Solicitudes Enviadas`: Seguimiento de reclamos con ubicación de entrega revelada al ser aprobado y botón de confirmación física `✓ Ya lo recogí en Garita`.
7. **Restaurantes Cercanos (`/Restaurantes`):** Guía gastronómica perimetral con mapa Leaflet acotado estrictamente al cuadrante del campus FIA (Av. La Fontana, Flora Tristán, Constructores y Av. La Molina).
8. **Mi Calendario (`/Calendario`):** Cuadrícula completa de 7 columnas alineada por días, cronograma 2026-II, feriados peruanos oficiales y horarios de clase.
9. **Barra Superior y Logos Oficiales:** Botones tipo pastilla con logos de alta resolución en primer plano con fondo transparente (Correo USMP, Aula Virtual FIA, Portal SAP y Calculadora FIA).

---

## 3. 🎯 Estado de Entregas: Parcial vs. Final

### ✅ Entrega Parcial (ESTADO ACTUAL — 100% CUMPLIDA)
* Toda la plataforma funcional y navegable sin pantallas vacías ni enlaces rotos.
* Todos los servicios Cloud conectados (Redis, RabbitMQ, Algolia, SQLite).
* UI adaptada a celulares y computadoras.
* Despliegue en vivo en Render y gráfico de ramas GitFlow impecable.

### 🔮 Entrega Final (Siguiente Etapa para Fin de Ciclo)
1. Migración de base de datos a PostgreSQL administrado en la nube (Supabase o Neon).
2. Almacenamiento de archivos adjuntos (PDFs pesados y fotos) en AWS S3 o Cloudinary.
3. Inicio de sesión Single Sign-On (SSO) con Microsoft Azure AD / Office 365 USMP.
4. Filtro de moderación automática de contenido con IA (Gemini API).

---

## 4. 🛠️ Cómo Debe Trabajar el Equipo: Metodología GitFlow y PRs

Para que el profesor vea un flujo de trabajo profesional en **Insights $\rightarrow$ Network** y en la pestaña **Pull Requests**, **DEBEN SEGUIR ESTOS PASOS ESTRICTAMENTE**:

```text
main:    ●──────────────────────────────────────────────● (Release Producción)
                                                       /
develop: ●────────────●───────────────────────────────●  (Integración de equipo)
                     /                               /
Tu rama: ●──●──●────● (Tus commits y Pull Request)──/
```

### Paso 1: Actualizarse antes de empezar
Antes de escribir una sola línea de código, descarga lo último que se integró en `develop`:
```bash
git checkout <tu-rama>      # Ej: git checkout Ccasani  o  git checkout Maximo
git pull origin develop     # Trae lo último integrado sin pisar tu código
```

### Paso 2: Commits atómicos con "Conventional Commits"
❌ **NO HAGAS:** Un commit gigante al final del día llamado *"avances"*, *"cambios"* o *"listo"*.  
✅ **HAZ:** Commits pequeños, frecuentes y descriptivos usando estos prefijos estándar:
* `feat(modulo):` Nueva funcionalidad (Ej: `feat(marketplace): agregar filtro por rango de precios`).
* `fix(modulo):` Corrección de error (Ej: `fix(calendario): corregir alineación de días feriados`).
* `style(modulo):` Mejoras visuales o CSS (Ej: `style(topbar): ajustar padding en pantallas móviles`).
* `refactor(modulo):` Limpieza o mejora de código sin cambiar funcionalidad.

```bash
git add .
git commit -m "feat(modulo): descripción clara y en minúsculas"
git push origin <tu-rama>
```

### Paso 3: Crear el Pull Request (PR) en GitHub
1. Entra a GitHub $\rightarrow$ pestaña **Pull requests** $\rightarrow$ **New pull request**.
2. **Configuración obligatoria:**
   * **Base:** `develop` $\leftarrow$ **Compare:** `<tu-rama>` *(NUNCA directo a `main`)*.
3. **Título:** Claro y profesional (Ej: `feat: Módulo de Estadísticas Académicas`).
4. **Descripción:** Redacta 3 o 4 viñetas resumiendo qué hiciste y qué pruebas realizaste.
5. Asigna a uno de tus compañeros en **Reviewers**.

### Paso 4: El botón verde de Merge (¡REGLA DE ORO PARA EL GRÁFICO!)
Al momento de aprobar y fusionar el Pull Request en GitHub:
*  **Elige SIEMPRE:** **`Create a merge commit`** (en la flechita del botón verde).
* ❌ **NUNCA elijas:** *Squash and merge* ni *Rebase and merge*.  
  *(Squash y Rebase aplastan todo en una línea recta y borran el rastro de la rama en el gráfico de Network)*.
* Al usar *Create a merge commit*, GitHub dibuja la curva de la rama uniéndose a `develop`.

### Paso 5: El pase a Producción (`main`)
Cuando el equipo decida congelar la versión para entregar:
1. Se abre un PR en GitHub: **Base:** `main` $\leftarrow$ **Compare:** `develop`.
2. Título: `release: Entrega Parcial USMP Connect v1.0`.
3. Se mergea con **`Create a merge commit`**.
4. Render detecta el cambio y actualiza el servidor en vivo automáticamente.

---

## 5. ⚠️ Reglas Técnicas para IAs Receptoras (Para CERO Conflictos)

Si le pasas este archivo a una IA para que programe contigo, dile que cumpla estas 4 reglas sagradas:

1. **NO modificar archivos base compartidos:**
   * ❌ No tocar `UsmpConnect/Views/Shared/_Layout.cshtml`.
   * ❌ No tocar `UsmpConnect/Program.cs` (salvo que sea para registrar una interfaz global estricta).
   * ❌ No tocar `UsmpConnect/Data/ApplicationDbContext.cs`.
2. **Usar Clases Parciales para la Base de Datos:**
   * Cada módulo agrega sus tablas en un archivo propio:
     `UsmpConnect/Data/ApplicationDbContext.<TuModulo>.cs`
3. **Seeders Automáticos:**
   * Todo módulo con datos de prueba implementa `IModuleSeeder` con `Orden >= 10`. `DbInitializer` lo descubrirá y ejecutará solo.
4. **No quemar credenciales en código:**
   * Nunca pongas contraseñas, URLs de bases de datos o API Keys dentro del código C# o en `appsettings.json`. Las credenciales van en `dotnet user-secrets` o variables de entorno de Render.

---

## 6. 💻 Prompt Sugerido para Iniciar la IA de tus Compañeros

Copia y pega este texto cuando abras chat con tu IA:

> *"Hola. Estoy trabajando en el repositorio del proyecto USMP Connect (ASP.NET Core MVC .NET 10 y EF Core SQLite). Acabo de hacer git checkout a mi rama y git pull origin develop. Por favor, lee atentamente el archivo `docs/ESTADO_PROYECTO_EQUIPO.md` en la raíz del proyecto para entender los módulos que ya están terminados, las reglas de arquitectura (no tocar archivos base compartidos) y la metodología de commits con Conventional Commits. Confírmame que lo has leído y dime en qué tarea empezamos."*
