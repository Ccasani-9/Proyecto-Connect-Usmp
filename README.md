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

### 1. 📊 Panel Académico & Simulador de Notas (Calculadora FIA)
* Proyección de notas y cálculo automático de promedios según la fórmula ponderada oficial de la facultad (Evaluaciones Continuas, Parciales, Finales y Trabajos).
* Resumen en tiempo real del promedio semestral y alertas de rendimiento académico.
* Horario del día interactivo con asignación de aulas y estado de asistencia.

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

### 6. 🍔 Restaurantes & Food Spots FIA
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
Database__ResetOnStartup=true
Redis__ConnectionString=<tu_conexion_redis_cloud>
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

---

<div align="center">
  <sub>Desarrollado con dedicación para los estudiantes de la Facultad de Ingeniería y Arquitectura · USMP 2026</sub>
</div>
