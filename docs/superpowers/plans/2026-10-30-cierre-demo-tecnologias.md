# Cierre de demostración de tecnologías — plan actualizado

Fecha: 30/10/2026. Alcance aprobado: mejorar una maqueta universitaria para alumnos, con pruebas focalizadas y ramas desde develop. Microsoft/Entra fue cancelado expresamente; no implementar ni solicitar credenciales.

1. Google personal mediante Firebase: PR #33. Mantener validación del token, autoría y prevención de vinculación por correo sin prueba de control.
2. Datos de demostración, corpus y ML: PR #34. 24 apuntes originales, 120 ejemplos de sentimiento, 200 interacciones sintéticas y guías del producto. Preservar aportes y precios existentes.
3. Asistente: PR #35. Continuaciones con presupuesto/categoría, horarios de almuerzo y salida legible sin interpretar HTML del modelo.
4. Evidencia de integraciones: rama codex/evidencia-integraciones-demo. Registrar origen real, fallos y operaciones separadas de Redis, Algolia, RabbitMQ y PieSocket. Un envío aceptado no prueba recepción.
5. Sustentación y cierre: documentación incluida en el bloque 4. Seguir docs/GUIA_SUSTENTACION.md para prueba remota tras despliegue y actualizar docs/VERIFICACION_PUBLICACION.md con resultados reales.

Estado: código preparado y comprobaciones locales realizadas por bloque. Merges humanos pendientes; no fusionar automáticamente. Orden recomendado: #33, #34, #35 y bloque 4. Reutilizar PR #31 develop→main si continúa abierto. La prueba real del popup Google y los recorridos remotos en Render quedan pendientes de despliegue; no confundir respaldos locales con integración remota comprobada.
