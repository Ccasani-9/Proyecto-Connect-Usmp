# Contexto del Proyecto: USMP Connect (Guía para Asistente de IA)

> **Instrucciones para la IA receptora:** Lee este documento atentamente antes de generar código o proponer cambios. Contiene toda la arquitectura, decisiones de diseño, estado actual del repositorio y tareas pendientes del proyecto **USMP Connect**.

---

## 1. Visión y Filosofía del Proyecto

* **Nombre del proyecto:** USMP Connect (USMP Hub).
* **Propósito:** Portal comunitario y panel personal autónomo creado **por y para estudiantes** de la Facultad de Ingeniería y Arquitectura (FIA) de la Universidad de San Martín de Porres (USMP).
* **Concepto clave (MUY IMPORTANTE):**
  * **La plataforma es 100% para ESTUDIANTES.** Funciona como una red social universitaria combinada con herramientas de productividad estudiantil.
  * **NO es un sistema administrativo para docentes:** La universidad ya tiene SAP y Aula Virtual. Los profesores **NO** se loguean, no tienen cuentas ni entran a subir notas oficiales.
  * Los profesores existen en el sistema únicamente como **sujetos de evaluación comunitaria** en el módulo *Cátedra & Tips* (para calificar su claridad, nivel de exigencia y dejar consejos entre alumnos).
  * La sección *"Mis Notas"* es un **panel personal y simulador del estudiante (Calculadora FIA)** para proyectar notas y calcular el promedio ponderado.

---

## 2. Stack Tecnológico

* **Framework:** ASP.NET Core MVC en **.NET 10** (`net10.0`).
* **Base de Datos:** Entity Framework Core con **SQLite** (en desarrollo se recrea en cada arranque con datos de prueba realistas mediante `DbInitializer`).
* **Autenticación:** ASP.NET Core Identity por cookies (rol único: `Roles.Alumno`). Registro exclusivo con código universitario de 10 dígitos y correo `@usmp.pe`.
* **Frontend:** Razor Views (`.cshtml`), Bootstrap 5, Bootstrap Icons, estilos personalizados en `wwwroot/css/site.css` basados en la identidad visual de la USMP (color guinda `#8B1D2C`, tarjetas limpias, badges, modales).
* **Infraestructura Cloud:**
  * **Hosting:** Desplegado en **Render.com** mediante contenedor Docker (`Dockerfile` y `render.yaml`).
  * **URL en vivo:** [https://usmp-connect.onrender.com/](https://usmp-connect.onrender.com/)

---

## 3. Estado Actual de Git y Repositorio

* **Repositorio GitHub:** `https://github.com/Ccasani-9/Proyecto-Connect-Usmp.git`
* **Integrante actual:** Andrew (`aspm1901` / `andrewpella2004@gmail.com`).
* **Rama de trabajo:** **`Andrew`** (siempre hacer commits aquí y abrir Pull Request hacia `develop`).
* **Flujo Git (GitFlow):**
  1. Ramas de desarrollador: `Andrew`, `Ccasani`.
  2. Rama integradora: `develop`.
  3. Rama de producción: `main` (Render escucha `main` para despliegue continuo).
* **Últimos commits en `Andrew`:**
  * `b90e4b0`: `refactor: enfocar la plataforma 100% en estudiantes y retirar modulos docentes`
  * `ea84dcd`: `feat: Marketplace Comunitario (compra, venta y donaciones con WhatsApp)`
  * `b87a3ee`: `feat: Banco de Apuntes (subir, vista previa, descargar y calificar)`
  * `0603b2a`: `feat: Cátedra & Tips (docentes, dificultad comunitaria y muro de tips con votos)`
  * `61becc6`: `feat: modelos y datos de prueba de Cátedra & Tips y Banco de Apuntes`

---

## 4. ⚠️ Reglas de Oro de Arquitectura (Para CERO Conflictos en Git)

Para que ningún integrante rompa el código de los demás ni se generen conflictos de fusión:

1. **NUNCA modificar los archivos base compartidos:**
   * ❌ No tocar `Program.cs` (salvo que sea estrictamente necesario para registrar un servicio global).
   * ❌ No tocar `Data/ApplicationDbContext.cs`.
   * ❌ No tocar `Views/Shared/_Layout.cshtml`.
   * ❌ No tocar `Data/Seed/BaseSeeder.cs`.
2. **Usar Clases Parciales para la Base de Datos:**
   * Cada módulo agrega sus tablas en un archivo propio:
     `Data/ApplicationDbContext.<Modulo>.cs`
   * Ejemplo:
     ```csharp
     namespace UsmpConnect.Data;
     public partial class ApplicationDbContext
     {
         public DbSet<MiEntidad> MisEntidades => Set<MiEntidad>();
     }
     ```
3. **Seeders Automáticos:**
   * Cada módulo crea una clase que implemente `IModuleSeeder` con `Orden >= 10`.
   * `DbInitializer` la detectará y ejecutará automáticamente al iniciar.
4. **Enrutamiento Autónomo en Menú Lateral:**
   * El menú ya tiene reservadas las rutas: `/FoodSpots`, `/Catedra`, `/Marketplace`, `/Apuntes`, `/LostFound`, `/Calendario`.
   * Si el controlador no existe, muestra "En construcción". Apenas se crea `Controllers/<Modulo>Controller.cs`, la página cobra vida sin tocar el layout.

---

## 5. Módulos Ya Implementados y Funcionando

1. **Dashboard de Inicio (`/`):**
   * Saludo dinámico según la hora del día, resumen de notas con promedios, horarios del día (`HOY JUE`, `MAÑANA VIE`), calendario académico del ciclo 2026-II y banners informativos.
2. **Cátedra & Tips (`/Catedra`):**
   * Catálogo de cursos con filtro por ciclos (I al V) y buscador insensible a mayúsculas/tildes.
   * Barra de **Dificultad Comunitaria** calculada dinámicamente.
   * Fichas de profesores que dictan el curso (claridad con estrellas y nivel de exigencia: Baja, Media, Alta, Muy Alta).
   * Muro de tips con votos útiles (👍 / 👎) por AJAX y sistema de reporte de comentarios inapropiados.
   * Modal para que los alumnos aporten tips.
3. **Banco de Apuntes (`/Apuntes`):**
   * Búsqueda y filtros por ciclo, asignatura y tipo (*Resúmenes, Exámenes, Guías de Lab*).
   * Visualizador integrado de PDF sin librerías pesadas (`Services/PdfSimple.cs`).
   * Contador de descargas y calificación de 1 a 5 estrellas con notificaciones al autor.
   * Modal de subida de archivos (PDF, DOC, DOCX de hasta 25 MB).
4. **Marketplace Comunitario (`/Marketplace`):**
   * Tablón de compra, venta y donación (*Calculadora Casio fx-991LA X, Bata de laboratorio, Arduino Uno, Cuarto en Santa Anita, etc.*).
   * Filtros por categorías, badges de estado (`USADO`, `NUEVO`, `DONACIÓN`), estado (`Disponible`, `Reservado`, `Vendido`).
   * Botón directo de **WhatsApp** con enlace `wa.me` y mensaje prellenado.
   * Modal completo con dropzone para fotos.
5. **Autenticación Estudiantil (`/Cuenta`):**
   * Login y Registro 100% exclusivo para alumnos.

---

## 6. Tareas Pendientes para el Domingo al Mediodía

El profesor solicitó que el proyecto incluya y aplique las siguientes tecnologías y pantallas:

### A. Módulos de Pantallas Pendientes (Según Prototipo Figma):
1. **Food & Spots (`/FoodSpots`):**
   * Mapa interactivo centrado en el campus FIA USMP (Av. La Fontana, Santa Anita). Se recomienda usar **Leaflet.js con OpenStreetMap** (o Google Maps API).
   * Restaurantes cercanos (*El Carbón Criollo, Chifa Wong Fu, La Esquina Verde, Pollería Don Pepe*).
   * Filtros por chips (*Menú < S/ 12, Criollo, Chifa, Con Wi-Fi / Enchufes, Acepta Yape/Plin*).
   * Calificaciones por Rapidez, Precio/calidad y Sabor, con modal para recomendar un local.
2. **Lost & Found - Objetos Perdidos (`/LostFound`):**
   * Pestañas: *Objetos Encontrados (Esperando dueño)* y *Objetos que Busco (Extraviados)*.
   * Badges de estado (*En custodia, Reportado hoy, Reclamado*).
   * Modales para reportar hallazgos con foto y formulario de reclamo mediante código universitario.
3. **Mi Calendario (`/Calendario`):**
   * Vista mensual completa de clases, exámenes y eventos personales, con modal para añadir recordatorios.

### B. Integraciones en la Nube (Requisitos del Profesor):
1. **Redis (`cloud.redis.io`):**
   * Implementar caché en RAM para datos de lectura frecuente (catálogo de cursos, horarios de clase, feed de Marketplace) y sesiones distribuidas con `Microsoft.Extensions.Caching.StackExchangeRedis`.
2. **Algolia (`dashboard.algolia.com`):**
   * Motor de búsqueda inteligente para Banco de Apuntes, Cátedra y Marketplace (búsqueda instantánea as-you-type con tolerancia a errores ortográficos).
3. **PieHost / WebSockets (`piehost.com`):**
   * Comunicación bidireccional en tiempo real: campanita de notificaciones en vivo y actualización de votos 👍/👎 en Cátedra sin recargar.
4. **CloudAMQP / RabbitMQ (`customer.cloudamqp.com`):**
   * Cola de mensajería en segundo plano para procesar tareas lentas (indexación asíncrona de archivos subidos y despacho de notificaciones).

---

## 7. Cómo Levantar y Probar el Proyecto Localmente

```bash
# 1. Clonar el repositorio (si estás en una máquina nueva)
git clone https://github.com/Ccasani-9/Proyecto-Connect-Usmp.git
cd Proyecto-Connect-Usmp

# 2. Posicionarse en la rama de Andrew
git checkout Andrew
git pull origin Andrew

# 3. Compilar la solución
dotnet build UsmpConnect/UsmpConnect.csproj

# 4. Ejecutar el servidor de desarrollo
dotnet run --project UsmpConnect/UsmpConnect.csproj
```

* **URL local:** `http://localhost:5175`
* **Credenciales de prueba de estudiante (precargadas):**
  * **Usuario:** `alessandro.morales@usmp.pe`
  * **Contraseña:** `Usmp2026!`
  * *(Otras cuentas de prueba: `maria.gonzales@usmp.pe`, `carlos.rojas@usmp.pe` con la misma clave)*.

---

## 8. Mensaje de Inicialización Sugerido para la Nueva IA

Copia y pega este prompt directamente al abrir la conversación en tu otra laptop:

```text
Hola. Estoy trabajando en el proyecto USMP Connect (ASP.NET Core MVC con .NET 10 y EF Core SQLite). He clonado el repositorio y estoy en mi rama 'Andrew'.

Por favor, lee el archivo CONTEXTO_PROYECTO_IA.md que está en la raíz del proyecto para entender la arquitectura, las reglas de no modificar archivos base y el enfoque 100% para estudiantes (sin módulos de docentes).

Nuestra meta para el domingo al mediodía es:
1. Implementar el módulo Food & Spots (/FoodSpots) con el mapa interactivo de la FIA USMP (Av. La Fontana) y los restaurantes del Figma.
2. Integrar los servicios en la nube solicitados por el profesor: Redis, Algolia, CloudAMQP y PieHost (WebSockets).

Dime que has entendido el contexto y sugiéreme por cuál de estos componentes comenzamos a programar.
```
