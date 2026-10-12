# USMP Connect · Documento de Traspaso de Contexto (Handoff) 🚀

> **Para el Asistente AI (Antigravity) o Desarrollador en la Laptop:**  
> Lee este documento antes de realizar cualquier cambio. Resume la arquitectura, decisiones de diseño, credenciales configuradas y estado exacto del repositorio.

---

## 1. Identidad y Propósito del Proyecto
- **Proyecto:** USMP Connect — Red comunitaria y académica de estudiantes de la Facultad de Ingeniería y Arquitectura (FIA) de la USMP (Campus La Molina, Lima - Perú).
- **Autor / Lead:** Andrew (@aspm1901) & equipo (Ccasani, Máximo).
- **Stack Técnico:**
  - **Framework:** .NET 10 (C# 14), ASP.NET Core MVC.
  - **Base de Datos:** SQLite con Entity Framework Core (sembradores automáticos de prueba en `UsmpConnect/Data/Seed/`).
  - **Autenticación Externa:** Firebase Authentication (Google Sign-In mediante popup en cliente).
  - **Servicios Cloud Integrados:** Algolia Search, Redis Cloud (cache), CloudAMQP RabbitMQ (cola de notificaciones).
  - **Despliegue Producción:** Render Web Service (`https://usmpconnect.onrender.com/`).

---

## 2. Flujo de Trabajo en Git (Git Flow Estricto)
1. **`main`:** Producción pura. Conectada al despliegue automático de Render. **Nunca se hace commit directo a `main`.**
2. **`develop`:** Rama troncal de desarrollo activa y actualizada. Todo el código estable se integra aquí vía Pull Requests.
3. **`feature/<nombre>`:** Ramas de trabajo cortas que se crean a partir de `develop`:
   - *Ejemplo:* `git checkout -b feature/restaurantes`
   - Se trabaja, se suben commits y se abre Pull Request en GitHub hacia `develop`.
   - Una vez aprobado el PR en GitHub, se hace Merge a `develop` y se borra la rama temporal.

---

## 3. Decisiones de Negocio y Restricciones Importantes
1. **Límite de Firebase:** Firebase se utiliza **exclusivamente** para el inicio de sesión con Google (`signInWithPopup`). La información de usuarios y perfiles se sincroniza localmente en SQLite mediante Identity.
2. **Sin Módulo de Notas Interno:** No existe módulo inventado de registro de notas dentro de la app (los cursos tienen fórmulas complejas y los profesores usan exclusivamente SAP oficial). Se enlaza directamente a la herramienta externa **Calculadora FIA** (`https://calculadorafiausmp.com/`).
3. **Sin Asesores ni Soporte de Matrícula:** La web es una red social y comunitaria de alumnos, no un canal administrativo institucional. No promocionar asesorías de matrícula. En su lugar se destaca **Restaurantes cercanos** (`/Restaurantes`).
4. **Navegación Pública (Modo Invitado):** Cualquier visitante sin iniciar sesión puede explorar libremente los catálogos (Restaurantes, Cátedra, Marketplace, Objetos Perdidos). **No colocar banners intrusivos o flotantes de "estás en modo invitado".**

---

## 4. Estado Actual de las Funcionalidades (Sprint Status)
- ✅ **Login (`feature/login`):** Fusionado en `develop`.
  - Google Sign-in oficial de Firebase (proyecto `usmp-connect`).
  - Tarjetas de 1 clic para cuentas demo (`alessandro.morales@usmp.pe`, `maria.gonzales@usmp.pe`, `carlos.rojas@usmp.pe` con clave `Usmp2026!`).
  - Logo oficial USMP Connect ampliado a 70px.
  - Redacción oficial en banner de login: *Restaurantes cercanos*, *Cátedra & Tips*, *Marketplace*, *Objetos perdidos*.
- ✅ **Home & Navegación:**
  - Banner principal promociona Restaurantes Cercanos.
  - Tarjeta y accesos directos a Calculadora FIA.
- ⏳ **Próximo Módulo a Desarrollar:** Según prioridad del usuario (Restaurantes Cercanos, Cátedra & Tips o Marketplace).

---

## 5. Configuración de Firebase Auth (Frontend)
- **Proyecto:** `usmp-connect`
- **Credenciales en cliente ([Login.cshtml](file:///c:/Proyectos/PROGRAMACI%C3%93N-1%20PROYECTO%20MI%20USMP/UsmpConnect/Views/Cuenta/Login.cshtml)):**
  - `apiKey`: `AIzaSyAOmZ_zHT2LQx3j6yyHa93qo1-uxGM5z5s`
  - `authDomain`: `usmp-connect.firebaseapp.com`
  - `projectId`: `usmp-connect`
  - `appId`: `1:160957209032:web:87499388926533345b88fd`

---

## 6. Comandos para Trabajar en la Laptop
1. **Descargar / Actualizar en la Laptop:**
   ```powershell
   git checkout develop
   git pull origin develop
   ```
2. **Ejecutar en Local:**
   ```powershell
   dotnet run --project UsmpConnect
   ```
   *Acceso local:* `http://localhost:5175`
3. **Crear una nueva rama para el siguiente feature:**
   ```powershell
   git checkout -b feature/nombre-del-modulo
   ```
