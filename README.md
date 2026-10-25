# USMP Connect 🎓

<div align="center">
  <img src="UsmpConnect/wwwroot/img/logo_USMP-CONNECT.png" alt="USMP Connect Logo" width="160" />
  <h3>Ecosistema Comunitario y Productividad Estudiantil</h3>
  <p><strong>Facultad de Ingeniería y Arquitectura (FIA) · Universidad de San Martín de Porres</strong></p>
  <p>
    <img src="https://img.shields.io/badge/.NET-10.0-purple?logo=dotnet" alt=".NET 10" />
    <img src="https://img.shields.io/badge/C%23-14-blue?logo=csharp" alt="C# 14" />
    <img src="https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?logo=dotnet" alt="ASP.NET Core MVC" />
    <img src="https://img.shields.io/badge/Docker-Container-2496ED?logo=docker" alt="Docker" />
    <img src="https://img.shields.io/badge/Redis_Cloud-Cache-DC382D?logo=redis" alt="Redis Cloud" />
    <img src="https://img.shields.io/badge/CloudAMQP-RabbitMQ-FF6600?logo=rabbitmq" alt="RabbitMQ" />
    <img src="https://img.shields.io/badge/Algolia-Search-003DFF?logo=algolia" alt="Algolia" />
    <img src="https://img.shields.io/badge/Status-Producción-success" alt="Status" />
  </p>
  <p>
    <a href="https://usmpconnect.onrender.com/" target="_blank">
      <img src="https://img.shields.io/badge/🌐_Sitio_Oficial_En_Vivo-usmpconnect.onrender.com-success?style=for-the-badge&logo=render" alt="Sitio Web En Vivo" />
    </a>
  </p>
</div>

---

## 📌 Visión del Proyecto

**USMP Connect** es una plataforma web integral creada **por y para la comunidad estudiantil** de la Facultad de Ingeniería y Arquitectura (FIA) de la USMP (Campus La Molina, Lima - Perú). 

A diferencia de los sistemas administrativos tradicionales, USMP Connect consolida en un único entorno moderno las herramientas cotidianas de vida universitaria: gestión académica personal, intercambio de materiales de estudio, recomendaciones de cátedra, seguridad comunitaria de objetos perdidos, guía gastronómica del campus y marketplace entre compañeros.

---

## ✨ Características y Módulos Principales

### 1. 📅 Horario del Día & Enlace a Calculadora FIA
* Horario del día interactivo con asignación de aulas, asignaturas y estado de asistencia.
* Acceso directo integrado a **Calculadora FIA** (`calculadorafiausmp.com`) para proyección de promedios ponderados según el sílabo oficial.

### 2. 👨‍🏫 Cátedra & Tips
* Directorio exhaustivo de cursos y docentes de la facultad (Ciclos I al V).
* Muro colaborativo de recomendaciones entre alumnos: nivel de exigencia, claridad pedagógica y consejos para aprobar cada asignatura.
* Sistema de votación comunitaria (útil / no útil) para destacar los mejores consejos de estudio.

### 3. 📚 Banco de Apuntes Comunitario
* Repositorio colaborativo de resúmenes, exámenes pasados y guías de laboratorio.
* Visualizador integrado de documentos PDF directamente en el navegador sin extensiones pesadas.
* Filtros por ciclo y asignatura, contador de descargas y calificación por estrellas.

### 4. 🛒 Marketplace Universitario
* Tablón seguro de compra, venta y donación de materiales exclusivos para ingeniería y arquitectura (calculadoras científicas Casio, batas de laboratorio, placas Arduino, libros y cuartos universitarios).
* Enlaces directos a WhatsApp con mensajes preformateados para coordinar entregas dentro del campus.

### 5. 🔍 Objetos Perdidos (Lost & Found Seguro)
* **Tablón de Encontrados:** Publicación de pertenencias halladas en el campus bajo custodia oficial en garitas y secretaría.
* **Privacidad de Custodia:** La ubicación física exacta se mantiene reservada y solo se revela al alumno cuyo reclamo sea verificado mediante señas particulares.
* **Confirmación de Recepción:** Ciclo cerrado donde el dueño confirma la recepción física en garita para archivar el reporte.
* **Tablón de Extraviados:** Alertas de búsqueda comunitaria con contacto directo.

### 6. 🍔 Restaurantes Cercanos
* Guía gastronómica perimetral al campus de La Molina.
* Mapa interactivo delimitado al cuadrante universitario: **Av. La Fontana, Av. Los Frutales, Av. Constructores y Av. Flora Tristán**.
* Menús diarios, rangos de precios estudiantiles y filtrado por cercanía a las puertas de acceso.

### 7. 📅 Mi Calendario Académico
* Cuadrícula de 7 columnas con el cronograma oficial del periodo 2026-II.
* Registro de feriados oficiales de Perú, fechas de matrícula, semanas de exámenes parciales y finales.

### 8. 🔗 Accesos Directos Institucionales
* Barra de navegación y menú móvil con enlaces integrados a:
  * **Correo USMP:** Outlook 365 institucional.
  * **Aula Virtual FIA:** Plataforma Moodle de cursos.
  * **Portal SAP:** Autoservicio universitario de matrícula y pagos.
  * **Calculadora FIA:** Simulador web externo de promedios.

---

## 🛠️ Stack Tecnológico

| Capa | Tecnología / Servicio | Descripción |
| :--- | :--- | :--- |
| **Backend** | **ASP.NET Core MVC 10** (`net10.0`) | Framework moderno de alto rendimiento en C# 14. |
| **Autenticación** | **ASP.NET Core Identity** | Autenticación basada en cookies con validación de correo `@usmp.pe` y código de 10 dígitos. |
| **Base de Datos** | **Entity Framework Core 10 + SQLite** | ORM con migraciones automáticas y sembrado de datos en memoria/disco. |
| **Caché en la Nube** | **Redis Cloud** | Optimización de tiempos de respuesta y almacenamiento de caché distribuida. |
| **Mensajería Asíncrona** | **CloudAMQP (RabbitMQ)** | Cola de eventos y procesamiento en segundo plano de notificaciones. |
| **Motor de Búsqueda** | **Algolia Search API** | Búsqueda indexada en tiempo real de apuntes, docentes y restaurantes. |
| **Tiempo Real** | **PieHost (WebSockets)** | Canal bidireccional para alertas instantáneas y notificaciones Push. |
| **Frontend** | **Razor Views + Bootstrap 5 + Vanilla JS** | Interfaz responsive, accesible y ligera sin sobrecarga de frameworks SPA. |
| **Identidad Visual** | **CSS Institucional USMP** | Paleta cromática oficial (Guinda `#8B1D2C`, blanco y gris perla). |
| **Contenedores** | **Docker Multi-Stage Build** | Imagen optimizada sobre `mcr.microsoft.com/dotnet/aspnet:10.0` (usuario no-root `$APP_UID`). |
| **Despliegue** | **Render Cloud PaaS** | [https://usmpconnect.onrender.com/](https://usmpconnect.onrender.com/) — Integración continua (CI/CD) conectada a `main`. |

---

## 🚀 Instalación y Ejecución Local

### Prerrequisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) instalado en tu equipo.
* Git.

### Pasos
1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/Ccasani-9/Proyecto-Connect-Usmp.git
   cd Proyecto-Connect-Usmp
   ```

2. **Compilar y ejecutar la aplicación:**
   ```bash
   dotnet run --project UsmpConnect
   ```

3. **Acceder en el navegador:**
   Ingresa a: `http://localhost:5175`  
   *(La base de datos SQLite y los datos de prueba se inicializan automáticamente al primer arranque).*

---

## 👥 Cuentas de Prueba (Entorno de Demostración)

Todas las cuentas demo utilizan la contraseña predeterminada: `Usmp2026!`

| Alumno | Correo Institucional | Especialidad |
| :--- | :--- | :--- |
| **Alessandro Morales** | `alessandro.morales@usmp.pe` | Ingeniería de Computación y Sistemas |
| **María Gonzales** | `maria.gonzales@usmp.pe` | Ingeniería Industrial |
| **Carlos Rodríguez** | `carlos.rodriguez@usmp.pe` | Ingeniería Civil |

---

## 🔒 Variables de Entorno en Producción

Para el despliegue en contenedores Docker o plataformas en la nube (como Render), la aplicación consume las siguientes variables de configuración:

```ini
ASPNETCORE_ENVIRONMENT=Production
Database__ResetOnStartup=false
Firebase__ProjectId=usmp-connect
Redis__ConnectionString=<tu_conexion_redis_cloud>
Redis__WarmupOnStartup=true
CloudAMQP__Url=<tu_url_amqp_rabbitmq>
Algolia__ApplicationId=<tu_app_id_algolia>
Algolia__SearchApiKey=<tu_search_key_algolia>
Algolia__WriteApiKey=<tu_write_key_algolia>
Algolia__IndexName=usmp_connect
PieHost__ClusterId=<tu_cluster_piehost>
PieHost__ApiKey=<tu_api_key_piehost>
PieHost__ApiSecret=<tu_api_secret_piehost>
PieHost__WebSocketUrl=<tu_websocket_url_piehost>
```

### Datos de demostración y reinicio

La aplicación crea SQLite y siembra las cuentas y contenidos de ejemplo cuando la base de datos no existe. Por defecto conserva los datos entre arranques. `Database__ResetOnStartup=true` activa un reinicio destructivo de la base de datos **y de los archivos subidos**: úsalo únicamente para una demostración desechable y vuelve a desactivarlo después. Conservar datos en Render también requiere almacenamiento persistente; esta opción no convierte el disco efímero del alojamiento en persistente.

### Acceso con Google

El cliente obtiene un ID token de Firebase y el servidor verifica su firma con los certificados públicos de Google, el proyecto, la vigencia, el proveedor Google y el correo verificado. `Firebase__ProjectId` debe coincidir con el proyecto del cliente. Se aplica el dominio configurado en `Registro__DominioCorreo` (por defecto `usmp.pe`). No se necesita una clave privada de servicio para esta verificación; sí acceso HTTPS al endpoint público de certificados. La verificación no consulta revocación de tokens en Firebase.

Los nuevos accesos Google se vinculan por UID con Identity, reciben el rol Alumno y no obtienen una contraseña compartida ni un código universitario inventado. Las cuentas demo conservan su acceso por correo y contraseña. Los roles ya asignados se mantienen.

### Comprobación local

```bash
dotnet build UsmpConnect
dotnet test UsmpConnect.Tests
```

Las pruebas usan bases SQLite temporales independientes y certificados RSA de prueba; no necesitan credenciales ni escriben en la base de datos local de la aplicación. Cubren validación de tokens, acceso y roles, reseñas sin apellido, permisos de recordatorios y conservación de datos sin reset.

### Caché de restaurantes

El catálogo completo y sus platos se guardan durante cinco minutos en `usmp:foodspots:catalogo:v1`. Las categorías y búsquedas filtran esa copia; no se crea una clave por cada texto ingresado. Al publicar una reseña se invalida la copia completa, por lo que todos los filtros reciben la calificación y el contador actualizados en la siguiente consulta.

`Redis__WarmupOnStartup=true` precarga el catálogo después de inicializar SQLite; `false` lo carga en la primera consulta. Si Redis no está configurado o no responde, se utiliza memoria local con el mismo vencimiento. Una eliminación realizada durante una desconexión queda pendiente y se aplica al recuperar la conexión. Mientras Redis responde, una clave ausente o corrupta obliga a recargar el catálogo y no reutiliza una copia local antigua. La coordinación de cargas e invalidación es por proceso; no constituye una transacción distribuida entre varios servidores.

La precarga y las invalidaciones se registran en logs; los aciertos de caché se registran con nivel Debug en `UsmpConnect.Services.CatalogoRestaurantesService`. No se muestran indicadores técnicos adicionales a los alumnos.

Las pruebas normales funcionan sin Redis. Para ejecutar también las pruebas de Redis real, usa una instancia **desechable de pruebas** y configura `USMP_TEST_REDIS`; no uses la instancia del despliegue. Por ejemplo, en PowerShell:

```powershell
docker run --rm -d --name usmp-redis-tests -p 127.0.0.1:6379:6379 redis:7-alpine
$env:USMP_TEST_REDIS = '127.0.0.1:6379,abortConnect=false'
dotnet test UsmpConnect.Tests
docker stop usmp-redis-tests
Remove-Item Env:USMP_TEST_REDIS
```

Las pruebas del servicio usan claves `usmp:tests:<identificador>`; la prueba del catálogo usa y elimina `usmp:foodspots:catalogo:v1`, por eso requieren una instancia desechable. Comprueban invalidación entre instancias, expiración, JSON corrupto y recuperación de una invalidación tras una desconexión. Si `USMP_TEST_REDIS` no está configurado, se marcan explícitamente como omitidas.

---

<div align="center">
  <sub>Desarrollado con dedicación para los estudiantes de la Facultad de Ingeniería y Arquitectura · USMP 2026</sub>
</div>

### Búsqueda global

El buscador común ofrece sugerencias de restaurantes, cursos/docentes, apuntes y artículos disponibles del marketplace. `/Buscar?q=...` ofrece una página de resultados y `/Buscar/Sugerencias?q=...` el autocompletado. Las sugerencias esperan 250 ms y cancelan solicitudes anteriores; Escape cierra el panel, las flechas recorren resultados y el botón de limpiar vuelve al campo.

Algolia se consulta desde el servidor: las claves no se envían al navegador. Si no está configurado o falla la consulta se utiliza SQLite, con búsqueda sin distinguir tildes ni mayúsculas. Las búsquedas sin resultados en Algolia se respetan. Los resultados se contrastan con SQLite para no enlazar apuntes borrados ni artículos vendidos o reservados.

Al arrancar y al publicar, calificar, eliminar o cambiar disponibilidad se solicita la sincronización del catálogo. Se agregan los cuatro tipos y se retiran los registros obsoletos de la aplicación. El índice debe ser propio de este proyecto; se gestionan los prefijos apunte_, market_, rest_ y curso_. La clave de escritura necesita permisos de browse, addObject y deleteObject. Algolia procesa las operaciones de forma asíncrona: la aceptación no significa que ya hayan aparecido todos los cambios. Si falla una sincronización, los datos permanecen en SQLite y se registra el fallo; el arranque siguiente vuelve a reconciliar. La acción manual de sincronización está restringida al rol Profesor.
