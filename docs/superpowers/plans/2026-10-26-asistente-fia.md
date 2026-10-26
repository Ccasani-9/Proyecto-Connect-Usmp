# Asistente FIA Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Chat de alumnos que busca contenido real y fuentes de USMP Connect.
**Architecture:** Servicio scoped con kernel por consulta, plugins de solo lectura, sesión por usuario y salida NDJSON. Widget Razor/JS sin frameworks nuevos.
**Tech Stack:** .NET 10, Semantic Kernel 1.81.0, Groq compatible con OpenAI, Qdrant existente, Bootstrap y JavaScript.
**Spec:** docs/superpowers/specs/2026-10-26-asistente-fia.md.

## Global Constraints
Solo alumnos como audiencia. No implementar rol Profesor ni admin. Pruebas primordiales; sin merge a main; claves fuera de Git.

## Review Focus
Pregunta mixta no bloquea herramientas válidas. Herramientas no exponen perfiles privados. Error/aborto no guarda conversación parcial. Cambio de cuenta no hereda historial. Modelo no introduce HTML ni URLs arbitrarias.

## Tasks
- [x] Contratos de mensaje, tarjeta y fuente; plugins Conocimiento y Catálogo con resultados acotados; pruebas mínimas de filtrado y permisos efectivos.
- [x] Servicio Groq con kernel y herramientas, prompt acordado y streaming; sesión por usuario con historial acotado y protección contra envío simultáneo.
- [x] Controller autenticado con antiforgery; DI, Session y opciones Groq; comprobar contrato con proveedor simulado.
- [x] Widget flotante, JS con parser NDJSON parcial y CSS de tokens existentes; DESIGN.md y guía de configuración.
- [x] Build, pruebas enfocadas, recorrido en navegador y revisión final; PR a develop.

## Ledger
Se continúa en una rama dentro del checkout actual como en los bloques anteriores, siguiendo el flujo autorizado por el usuario. No se crean worktrees adicionales ni baterías extensas de pruebas. API key Groq pendiente; el usuario está creando su cuenta.

## Verificación de la rama
Compilación: 0 advertencias, 0 errores. Dos pruebas enfocadas aprobadas: contrato NDJSON, autenticación, antiforgery y aislamiento de historial; herramientas de catálogo con presupuesto, disponibilidad y búsquedas sin tildes. Sintaxis JavaScript correcta. Auditoría UI estática: 0 hallazgos. Navegador real en ventana estrecha: bienvenida, carga sin clave, validación de mensaje vacío y error recuperable; se corrigió el desbordamiento horizontal del panel. Revisión estática encontró y motivó dos correcciones (bloqueo durante limpieza y búsqueda sin tildes). API key de Groq pendiente del equipo; no se afirma haber probado respuestas remotas, llamadas a herramientas del modelo ni su obediencia temática.

## Comprobación remota posterior al merge del PR #22
Groq configurado localmente en User Secrets y validado con el servicio real. Se detectó que la selección automática podía redactar recomendaciones sin llamar al catálogo; se corrigió en fix/asistente-consultas-mixtas usando FunctionChoiceBehavior.Required para temas explícitos y manteniendo selección automática para el resto. Consulta mixta Mundial/restaurantes: cuatro tarjetas de restaurantes reales de SQLite, final completo y sin error. Consulta de publicación de apuntes: tres fragmentos de la guía, final completo. Consulta solo Mundial: declinada sin tarjetas. Historial de las tres consultas: seis mensajes guardados. Build de la corrección: cero errores y advertencias. Se precisó la guía de apuntes conforme a los controles reales (ciclo de malla, tipo de documento y ausencia de etiquetas); la generación del modelo sigue siendo probabilística. Credenciales fuera de Git; Render todavía requiere sus variables de entorno.
