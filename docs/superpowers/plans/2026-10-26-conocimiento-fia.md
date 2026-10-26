# Base de conocimiento FIA Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Preparar fuentes del asistente para alumnos con PdfPig y Qdrant y respaldo local.
**Architecture:** Archivos del proyecto como fuente de verdad; corpus inmutable por proceso; índice remoto filtrado por versión y contenido devuelto desde fuentes locales conocidas.
**Tech Stack:** .NET 10, PdfPig 0.1.16, HttpClient, Qdrant Cloud.
**Spec:** docs/superpowers/specs/2026-10-26-conocimiento-fia.md.

## Global Constraints
Sin panel Profesor ni nuevo rol. Sin secretos en Git. Pruebas solo primordiales. PR desde feature/conocimiento-fia a develop, sin fusionar.

## Review Focus
Archivo inválido no causa caída del sitio. Red caída produce respaldo explícito. Puntos de otra versión no se consultan. Cancelación se respeta. Texto remoto no sustituye fuentes locales verificadas.

## Tasks
- [x] Probar extracción de texto y límites. Implementar DocumentoTextoExtractor, CatalogoConocimiento y contratos de fuente/resultado. Semillas en Data/Conocimiento copiadas al publicar.
- [x] Implementar cliente Qdrant: verificar o crear colección dedicada 384/coseno, insertar UUID estables con payload de versión y consultar con filtro. Test de contrato HTTP con handler en memoria.
- [x] Implementar búsqueda compartida, respaldo por términos sin puntuación vectorial fingida, sincronización una vez y cooldown de reintentos.
- [x] Endpoint autenticado de diagnóstico, guía de configuración README y registro DI.
- [x] Build y pruebas enfocadas, revisión estática final y PR.

## Ledger
Ruling: el diseño anterior de SQLite y gestión Profesor se sustituye por corpus de archivos del proyecto, según aclaración y aprobación del usuario. No hay cargas web ni necesidad de nuevas tablas. No se modifica la UI en esta rama.

Verificación: 4 pruebas enfocadas aprobadas (extracción y archivo inválido, búsqueda local y fragmentación, contrato REST Qdrant, acceso autenticado y consulta alumno). Revisión estática independiente sin hallazgos importantes. Conexión real Qdrant pendiente de configuración; no se afirma validada. No se modificaron roles ni pantallas de alumnos.
