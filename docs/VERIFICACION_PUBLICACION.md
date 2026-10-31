# Verificación de la publicación

Comprobación realizada el **26 de octubre de 2026**, hora de Lima, sobre https://usmpconnect.onrender.com/.

## Versión

- Rama de la demo: develop.
- Commit: `ba2649c18696335926da778a495b84a3e3b1f44e` (merge del PR #24).
- Render confirmó ese commit como Live.
- Se configuró Health Check Path como `/health`. Render inició automáticamente un nuevo despliegue por ese ajuste y lo completó correctamente con el mismo commit. La sesión y las últimas observaciones del panel pueden reiniciarse con el proceso.
- Las variables Groq/Qdrant guardadas en la preparación ya estaban activas en las comprobaciones del chat.

## Resultados observados

| Recorrido | Resultado |
|---|---|
| `/health` | HTTP 200 y `estado: ok` |
| Login de alumno demo | Sesión creada y acceso al historial del asistente |
| `/api/restaurantes` | JSON con 52 restaurantes de la demo |
| `/swagger/v1/swagger.json` | OpenAPI con rutas documentadas |
| Horario, Cátedra, Apuntes, Marketplace, Restaurantes, Objetos Perdidos y Calendario | HTTP 200 con sesión de alumno |
| Consulta mixta de Mundial y restaurantes | Flujo completo, cuatro tarjetas y ningún evento de error |
| Cómo publicar apuntes con fuentes | Flujo completo, tres fragmentos de fuente y ningún evento de error |
| Panel Groq y Qdrant | Mostró respuesta remota y consulta semántica completadas tras usar el chat |
| Redis | El panel mostró conexión remota abierta |
| RabbitMQ / CloudAMQP | El panel mostró conexión remota abierta |
| Modelos ML | El panel mostró sentimiento y recomendación cargados con métricas de la partición de demostración |

Las comprobaciones remotas se hicieron con la cuenta demo de alumno. No se publicaron reseñas ni archivos nuevos durante esta verificación.

## Qué no demuestra esta comprobación

- Un canal RabbitMQ abierto no demuestra por sí solo que un mensaje se haya consumido. La sustentación debe incluir una reseña de demostración si se desea evidenciar productor, consumidor y notificación.
- Algolia figuró configurado, pero no se registró en esta comprobación evidencia diferenciada de respuesta remota frente al respaldo SQLite.
- PieSocket figuró configurado, pero no se verificó recepción de un evento WebSocket en el navegador.
- No se realizó un login real con Google institucional; existen comprobaciones automatizadas de validación y permisos, pero son evidencias distintas.
- Las fuentes del chat procedieron del corpus de referencia. Esta prueba no incluyó cargar un PDF nuevo ni comprobar OCR, que el extractor no implementa.
- No es una prueba de carga, disponibilidad continua ni persistencia. El alojamiento Free conserva las limitaciones descritas en RENDER_DEMO.md.

La compilación y las pruebas enfocadas del código se documentan en el PR #24. Esta guía añade evidencia del sitio publicado y no cambia el comportamiento de la aplicación.


## Preparación del 30/10/2026 — no sustituye la evidencia anterior

Los PR #33–#35 y el bloque de diagnósticos se prepararon en ramas independientes desde develop. Las verificaciones locales no acreditan Redis, Algolia, CloudAMQP ni recepción PieSocket en Render. Sus recorridos completos quedan pendientes después de los merges y el despliegue. Seguir la lista de evidencia remota de GUIA_SUSTENTACION.md y anotar commit desplegado, hora y resultado antes de declarar éxito. Microsoft/Entra queda cancelado; Google personal es acceso a la maqueta.
