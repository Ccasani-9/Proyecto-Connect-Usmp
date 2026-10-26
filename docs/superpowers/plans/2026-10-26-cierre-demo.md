# Cierre de demo: alumnos, diagnóstico y Render

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Completar los puntos 1, 2 y 3 aprobados por el usuario: experiencia de alumnos, panel técnico fiel y configuración de Render.
**Architecture:** Conservar docentes como datos de Cátedra; restringir acceso de usuarios a Alumno; observar últimas operaciones de Qdrant y Groq sin hacer llamadas al abrir el panel; preparar variables de Render sin desplegar main ni contratar recursos.
**Tech Stack:** ASP.NET Identity/MVC, Razor/Bootstrap, estados en memoria y Render Docker.

## Constraints
No borrar docentes ni datos del usuario. No cambiar main ni realizar merges automáticamente. No añadir roles. Secretos únicamente en User Secrets/Render. Mantener plan gratuito; no contratar discos. Pruebas solo primordiales. Render requiere sesión del usuario para configurar su cuenta.

## Tasks
- [x] Acceso de alumnos: impedir login docente por contraseña o Google; política de alumnos para acciones autenticadas; retirar sincronización web reservada a Profesor; horario de alumno.
- [x] Diagnóstico: conteos reales de SQLite; estados honestos para Redis/colas, Algolia, WebSockets, ML, Qdrant, Groq y entorno; última consulta registrada con fecha, sin llamada externa desde GET; Swagger y consulta de conocimiento enlazados.
- [x] Render: revisar plan/ramas/variables, configurar Groq y Qdrant con credenciales existentes si hay sesión; revisar almacenamiento y documentar límites gratuitos y pasos de publicación pendiente.
- [x] Compilar, comprobar login/panel y navegación básica; abrir PR a develop con resultados y pendientes explícitos.

## Review Focus
Google docente no se convierte en alumno ni se vincula antes de denegar acceso. GET diagnóstico no consume cuota de IA. Estado configurado no significa conexión comprobada. No exponer secretos ni simular WebSocket local. Free Render no ofrece almacenamiento persistente: no prometer permanencia de aportes entre despliegues.


## Resultado
- Contraseña y Google deniegan acceso docente sin borrar las referencias de Cátedra; política autenticada exige Alumno. Se retira sincronización web Profesor y el horario utiliza la matrícula del alumno.
- Panel autenticado con diez tecnologías, SQLite real, Qdrant/Groq observados y hora de Lima, enlaces a Swagger/fuentes y descripción precisa de respaldos. Abrirlo no consume cuota de IA.
- Render revisado: Docker/Free, auto-deploy develop, commit publicado e7ce3f8. Cinco variables nuevas guardadas con Save only y ResetOnStartup corregido de true a false. No se lanzó deploy ni se cambió la rama o plan. Se activarán al fusionar el PR y completar el despliegue automático.
- Build sin advertencias/errores, publicación Release correcta, tres casos enfocados de acceso/panel/roles aprobados. /health respondió ok desde la publicación local.
- Consulta real del chat verificó transición del panel desde sin operación comprobada a respuesta Groq y consulta Qdrant completadas. Navegador estrecho: ancho del documento y contenido 300 px después de corregir etiquetas largas. Auditoría estática UI sin hallazgos; revisión independiente sin fallos importantes.
- Verificación final en Render pendiente del merge/despliegue. La limitación de disco efímero no se resolvió contratando recursos; queda documentada para la demo.
