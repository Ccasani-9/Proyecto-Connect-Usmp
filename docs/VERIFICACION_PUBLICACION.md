# Verificación de la publicación

## Cierre local del 7 de noviembre de 2026

Rama `codex/cierre-presentacion`, basada en `develop` después del PR #53. Las mejoras pendientes del PR #35 se adaptaron al asistente actual. Ese PR se sustituye por el cierre, y el PR #31 `develop → main` queda para después de integrar y revisar el despliegue.

`dotnet test UsmpConnect.slnx --configuration Release` completó **81 pruebas aprobadas, 0 fallidas y 5 omitidas**. Las omitidas requieren una instancia Redis de prueba mediante `USMP_TEST_REDIS`.

- Acceso demo, renovación de formularios con antiforgery vencido y cierre de sesión, conservando la validación del token.
- Marketplace permite publicar, cambiar estado y eliminar con verificación de autoría. Se validan estados y categorías. La búsqueda conserva filtros, codifica los enlaces y permite limpiarlos. Las publicaciones sin teléfono muestran contacto sin registrar.
- Objetos Perdidos permite aprobar el reclamo demo y confirmar la entrega, con notificación persistida. Borrar un reporte conserva las fotografías compartidas y los ejemplos eliminados no se recrean al reiniciar.
- Cátedra permite aportar y votar un consejo. Apuntes permite modificar una valoración sin duplicarla y descargar un PDF válido.
- El asistente conserva presupuesto al cambiar de categoría en una continuación y filtra horarios publicados incompatibles con almuerzo. La recuperación del PR #35 mantiene el renderizado seguro y las mejoras posteriores del chat.
- La página de errores lee el código HTTP desde la ruta. Antes, un campo `id` del formulario podía sustituir ese código y provocar un error 500 durante la recuperación.

Revisión de navegador local en escritorio y a 390 × 844 px de Inicio, Marketplace, Objetos Perdidos, Cátedra, Apuntes, Restaurantes y Calendario, incluido el formulario de actividades. Se verificaron fotografías y distribución adaptable. Fotografías referenciales guardadas con la aplicación y créditos en `UsmpConnect/wwwroot/images/demo/CREDITOS.md`. El carné usa una ilustración sin datos personales.

Esta revisión utiliza una base SQLite temporal y servicios remotos desactivados. Después del merge, contrastar el campo `version` de `/health` con el commit que Render marca Live. Completar el acceso personal de Google y el ensayo de integraciones remotas en esa versión. La suspensión y el almacenamiento efímero de Render Free siguen siendo condiciones del alojamiento.

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


## Comprobación del 03/11/2026 (Lima)

Sobre el sitio público: /health y /Cuenta/Login respondieron 200; el POST de acceso demo con antiforgery recién obtenido llegó a Inicio. /Calendario respondió 200 e incluye evRepeticion (PR #37 desplegado). No se reprodujo el 400 en este intento; no se considera resuelta su causa.

Dos cargas de /Restaurantes y una búsqueda /Buscar/Sugerencias?q=pizza respondieron 200. El panel registró lectura remota Redis a las 10:06:23 y consulta remota Algolia a las 10:06:24. La aceptación previa de sincronización Algolia no prueba por sí sola indexación completa.

Pendientes: publicación/consumo con ACK de RabbitMQ e invalidación Redis tras reseña, recepción PieSocket en otro navegador, popup Google personal y revisión visual de la nueva presentación del chat. La verificación HTTP del login demo no sustituye Google real. Persistencia de SQLite/archivos de Render gratuito sigue siendo una limitación; no se contrató almacenamiento ni se migraron datos. Edición/borrado de una serie completa de calendario queda como mejora opcional, no implementada en este bloque.


## Recorrido remoto del 03/11/2026, 11:11–11:13 Lima

Se publicó desde María la reseña demo #17 en restaurante #1 (comentario marcado DEMO técnica 03/11). Redis confirmó invalidación remota a las 11:11:48; RabbitMQ productor a las 11:11:49, notificación persistida a las 11:11:50 y consumo con ACK a las 11:11:51. El resumen de notificaciones de la autora contenía confirmación. Se conserva la muestra identificada como demo.

PieSocket rechazó publicación: la implementación usaba /api/v3/publish. Comprobación directa sin credenciales: esa ruta devuelve 404 Not found, mientras /api/publish reconoce la API y devuelve 403 API key is required. Se corrige a ruta V3 documentada /api/publish y campo channel. Fuente: https://piehost.com/docs/3.0/rest-api. La recepción en navegador debe repetirse tras desplegar la corrección; no se declara aprobada.

En navegador publicado se observó el indicador durante la espera y elementos strong reales en nombres de la respuesta. Se detectó además una respuesta incorrecta: al pedir cafetería devolvió comida rápida; requiere revisar extracción de categoría. Google abrió correctamente accounts.google.com mediante Firebase, pero solicita credenciales del titular: pendiente completar personalmente la sesión y validar retorno. No se introdujeron credenciales Google.
