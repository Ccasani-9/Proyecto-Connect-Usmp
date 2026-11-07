# Mejorar las consultas del Asistente FIA

Alcance: resolver los fallos de la conversación aportada por el usuario, conservando el asistente como buscador y guía de la comunidad estudiantil.

Diagnóstico: los 52 locales publicados carecen de platos con precios numéricos. El filtro de presupuesto los elimina y el modelo interpreta ausencia de precios como ausencia de locales. La categoría no forma parte de la búsqueda. La selección obligatoria de herramientas solo mira el mensaje actual y permite consultar la guía en lugar del catálogo solicitado.

- [x] Guardar PrecioPromedioReferencial por local, con ampliación SQLite idempotente y valores demo por rango (10/16/25), conservando importes existentes. Exponerlo en catálogo/API/UI y usarlo para filtrar por presupuesto sin depender de platos; separar alternativas de precio desconocido.
- [x] Recuperar el tema en continuaciones cortas y exigir la herramienta del catálogo pertinente en preguntas mixtas.
- [x] Buscar apuntes por términos, devolver alternativas reales si no hay coincidencias y aclarar qué información tiene el asistente.
- [x] Mejorar las instrucciones y ampliar la guía sobre capacidades, precios desconocidos, seguimientos y temas ajenos.
- [x] Pruebas mínimas de filtros/contexto y conversación con Groq real usando BD temporal; revisar y crear PR hacia develop.

No se inventarán precios, apuntes, contenido de archivos ni recetas. No se agregan roles o permisos. No se convierte el chat en tutor general ni se cambia su interfaz.

Ajuste solicitado por el usuario: presupuesto sobre gasto promedio por local, sin carta detallada. La caché cambia a v2 para descartar copias que no contienen el nuevo campo. Las estimaciones se identifican como demo; no se representan como precios reales.

## Evidencia
- 6 pruebas focalizadas (asistente y API) superadas.
- Conversación real con Groq: búsqueda de comida rápida por 15 devuelve 4 tarjetas de locales de gasto referencial 10; seguimiento por 20 incorpora locales de gasto 16 y conserva categoría.
- El modelo inicialmente volvió a generar código pese al prompt, y una continuación agotó su respuesta sin contenido. Se aisló la consulta válida de apuntes y se presenta su resultado estructurado directamente; se aumentó el margen de salida del modelo para las demás consultas.
- Tras ese cambio, los tres turnos de apuntes mixtos, Python y funciones completaron sin errores: dos materiales de cálculo, ausencia explícita de Python y conservación de Python en el seguimiento. No se generan ejercicios ni se promete leer archivos que no se han extraído.
- UI local: importe referencial presente en catálogo y modal; se separa del renglón de estrellas para conservar espacio.
- Desarrollo y verificaciones sobre SQLite temporal, con Redis, Algolia, RabbitMQ y PieSocket remotos desactivados; no se modificaron reseñas ni datos del sitio publicado.
