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
| **Despliegue** | **Render Cloud PaaS** | [https://usmpconnect.onrender.com/](https://usmpconnect.onrender.com/) — Demo con despliegue automático desde `develop`, según la configuración comprobada en Render. El paso a `main` corresponde a la entrega estable. |

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
Groq__ApiKey=<tu_api_key_groq>
Groq__Model=openai/gpt-oss-20b
Qdrant__Endpoint=<tu_endpoint_qdrant_cloud>
Qdrant__ApiKey=<tu_api_key_qdrant>
Qdrant__CollectionName=usmp_fia_e5_v1
```

### Datos de demostración y reinicio

La preparación y comprobación del despliegue actual se detalla en [Render: configuración de la demo](docs/RENDER_DEMO.md). Las claves guardadas con Save only se activan en el próximo despliegue. El servicio actual publica automáticamente desde develop.

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

Al arrancar y al publicar, calificar, eliminar o cambiar disponibilidad se solicita la sincronización del catálogo. Se agregan los cuatro tipos y se retiran los registros obsoletos de la aplicación. El índice debe ser propio de este proyecto; se gestionan los prefijos apunte_, market_, rest_ y curso_. La clave de escritura necesita permisos de browse, addObject y deleteObject. Algolia procesa las operaciones de forma asíncrona: la aceptación no significa que ya hayan aparecido todos los cambios. Si falla una sincronización, los datos permanecen en SQLite y se registra el fallo; el arranque siguiente vuelve a reconciliar. Se retiró la acción web manual reservada al rol Profesor: la sincronización sigue en los flujos automáticos.
# Fuentes de conocimiento del asistente FIA

USMP Connect es una red social para alumnos. Las fuentes del asistente se preparan como archivos del proyecto, sin panel Profesor ni nuevo rol de administración. PdfPig y Qdrant extraen y recuperan las fuentes; el chat utiliza Groq y Semantic Kernel para responder con ellas y consultar los catálogos.

## Preparar documentos

Colocar archivos `.pdf`, `.md` o `.txt` directamente en `UsmpConnect/Data/Conocimiento/` y reiniciar o volver a desplegar la aplicación. Se incluyen en la publicación Docker. Usar únicamente información que todos los alumnos puedan consultar; los apuntes y otros archivos subidos por usuarios no se indexan automáticamente.

Límites: 32 documentos; 5 MB, 200.000 caracteres y 250 fragmentos por documento; 1.000.000 de caracteres por corpus. PdfPig extrae texto de PDF con número de página; no realiza OCR. Un archivo ilegible se omite y se informa en las advertencias del resultado. Los cambios de fuentes se recogen al reiniciar el proceso.

La guía incluida es material de demostración sobre USMP Connect. No incorpora reglamentos, fórmulas de evaluación ni horarios universitarios sin verificar.

## Configurar Qdrant Cloud

Crear un cluster del equipo con Cloud Inference habilitado y una API key con permisos de lectura/escritura y creación de colección e índices. Configurar los valores en user-secrets para desarrollo o variables de entorno para Render, sin subir claves a Git:

```text
Qdrant__Endpoint=https://TU-CLUSTER.cloud.qdrant.io:6333
Qdrant__ApiKey=<secreto configurado fuera del repositorio>
Qdrant__CollectionName=usmp_fia_e5_v1
```

El servicio utiliza `intfloat/multilingual-e5-small` mediante inferencia de Qdrant Cloud para documentos y consultas, vector de 384 dimensiones y distancia coseno. Crea la colección si no existe; si una colección existente es incompatible, conserva sus datos y utiliza el respaldo local. No basta con un servidor Qdrant local sin servicio de inferencia para este flujo.

La primera consulta sincroniza las fuentes con UUID estables. Se crean índices keyword sobre `corpus` y `model`, necesarios para los filtros. Las consultas solo devuelven fragmentos del corpus actual y recuperan el texto desde las fuentes locales conocidas. No se borran puntos de versiones previas automáticamente; pueden ocupar espacio en la colección dedicada aunque no aparezcan en resultados. No reutilizar esta colección para otros proyectos.

Sin configuración o ante un fallo remoto, la búsqueda usa coincidencias por palabras y devuelve `modo: local`; no se presenta como búsqueda vectorial. Después de un fallo se esperan 30 segundos antes de reintentar. La conexión real solo queda comprobada cuando una consulta devuelve `modo: qdrant`.

## Demostración del bloque

1. Iniciar sesión con una cuenta de alumno.
2. Abrir `/Conocimiento/Buscar?consulta=publicar%20apuntes`.
3. Comprobar `modo`, `estado`, `fuentes` y `advertencias`. Cada fuente incluye nombre del archivo, texto y página si procede de un PDF. Sin sesión, el endpoint solicita iniciar sesión.
4. Con Qdrant configurado, repetir la consulta y verificar `modo: qdrant`. El endpoint es de diagnóstico: la interfaz para alumnos será el chat.

Las instrucciones del asistente limitan el tema a USMP Connect y la facultad: ante preguntas mixtas atiende la parte útil y omite la ajena; si no tiene una fuente lo reconoce. Estas instrucciones orientan al modelo, pero la protección de datos se implementa también mediante herramientas de solo lectura y proyecciones explícitas que no incluyen credenciales, notas privadas ni contactos de usuarios.

Referencias: [Qdrant Cloud Inference](https://qdrant.tech/documentation/embeddings/qdrant-cloud-inference/), [índices de payload](https://api.qdrant.tech/api-reference/indexes/create-field-index) y [PdfPig](https://uglytoad.github.io/PdfPig/).

## Chat del Asistente FIA

El botón Asistente FIA aparece para usuarios autenticados. Permite preguntar por apuntes, cursos y consejos de Cátedra, restaurantes y artículos disponibles, además de consultar las fuentes documentales. Las tarjetas llevan al contenido real de la plataforma y las fuentes se pueden desplegar para consultar el fragmento utilizado.

Configurar `Groq:ApiKey` en User Secrets para desarrollo o `Groq__ApiKey` en Render. El modelo predeterminado es `openai/gpt-oss-20b`; se puede cambiar con `Groq:Model` / `Groq__Model` por otro modelo de Groq que soporte herramientas y streaming. El endpoint del proveedor es fijo: `https://api.groq.com/openai/v1`. No se usa una API key de OpenAI.

Ejemplo en PowerShell 7, desde la raíz del proyecto:

```powershell
$claveGroq = Read-Host 'API key de Groq' -MaskInput
dotnet user-secrets set 'Groq:ApiKey' $claveGroq --project UsmpConnect
Remove-Variable claveGroq
```

Reiniciar la aplicación después de configurar. Sin clave, el widget explica que el asistente todavía no está disponible; no genera respuestas falsas. Los errores y límites del proveedor se muestran como fallo recuperable sin revelar sus credenciales.

Semantic Kernel registra dos plugins: `CatalogoFia` (restaurantes con presupuesto opcional, cursos/consejos, apuntes y Marketplace) y `ConocimientoFia` (fuentes Qdrant con respaldo local). Las herramientas devuelven como máximo cuatro resultados de catálogo o cinco fragmentos. Las consultas de texto ignoran mayúsculas y tildes, igual que la búsqueda global. Los consejos ocultos por reportes y los artículos vendidos/reservados no se ofrecen. Un presupuesto requiere precios de platos efectivamente registrados; no se deduce un precio exacto de una etiqueta «Económico».

Para menciones explícitas de restaurantes, apuntes, cursos o Marketplace, se utiliza `FunctionChoiceBehavior.Required` con las herramientas pertinentes y la consulta de conocimiento; esto obliga a recuperar información antes de redactar. El resto de preguntas mantiene selección automática para permitir saludos, orientación y seguimientos. Esta selección por tema mejora el uso de fuentes, pero no es una barrera de permisos ni garantiza la exactitud de toda la redacción del modelo. Para restaurantes generales el presupuesto es opcional: se muestran opciones sin exigirlo antes.

El texto de chat se renderiza como texto plano y las tarjetas usan rutas internas generadas por el servidor. El contenido de fuentes y publicaciones se trata como datos, no como instrucciones. El modelo puede equivocarse; mostrar fuentes y limitar herramientas no constituye una garantía de obediencia perfecta a las instrucciones temáticas.

El historial guarda hasta seis intercambios por sesión y usuario. Se conserva al navegar dentro de la página y se pierde al expirar 30 minutos de inactividad o reiniciar el servidor. Usa ASP.NET Session con almacenamiento en memoria para una instancia de demostración. Nueva conversación elimina el historial del usuario actual; Detener cancela la generación y no conserva un intercambio incompleto. No hay historial en localStorage ni preguntas en la URL.

Rutas: GET `/Asistente/Historial`, POST `/Asistente/Preguntar` (formulario `mensaje`) y POST `/Asistente/Limpiar`. Requieren autenticación; los POST conservan antiforgery. Preguntar emite NDJSON con eventos `texto`, `fin` (tarjetas/fuentes) o `error`, con límite de 1.000 caracteres de entrada y 60 segundos de operación.

Para la sustentación: probar una búsqueda de restaurantes, «apuntes de arboles», una pregunta sobre cómo publicar, una consulta ajena y una mixta («busca restaurantes, pero antes dime quién ganará el Mundial»). La comprobación real del modelo requiere la clave de Groq y disponibilidad de la cuenta. Las pruebas automatizadas enfocadas usan un proveedor simulado para verificar el contrato HTTP e historial, y prueban las herramientas contra SQLite temporal; no demuestran por sí solas el comportamiento del modelo remoto.

Referencias: [Groq compatible con OpenAI](https://console.groq.com/docs/openai), [modelos disponibles](https://console.groq.com/docs/models) y [llamadas a herramientas de Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/?pivots=programming-language-csharp).
