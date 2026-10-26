# Asistente FIA para alumnos

Diseño aprobado en conversación: USMP Connect es una red social para alumnos. El asistente encuentra apuntes, cursos/consejos, restaurantes y artículos disponibles, y explica la plataforma mediante fuentes de conocimiento. Si una consulta mezcla temas válidos y ajenos, atiende la parte válida; si todo es ajeno, orienta brevemente al alumno. Puede responder un saludo casual sin convertirse en asistente general. No inventa precios, normativa ni publicaciones.

## Implementación propuesta
Chat flotante integrado en el layout autenticado, estilo guinda existente, panel adaptable a móvil, mensajes como texto seguro, fuentes y tarjetas enlazadas generadas por el servidor. Semantic Kernel orquesta herramientas de solo lectura sobre el catálogo y Qdrant; Groq genera la respuesta progresiva. Historial por sesión y usuario, máximo seis intercambios, botón nueva conversación. No se envían contraseñas, teléfonos, correos, notas personales ni conversaciones ajenas al proveedor.

Instrucciones del sistema distinguen contenido de fuentes de instrucciones; prohíben obedecer instrucciones incrustadas en documentos o publicaciones. Los permisos reales de las herramientas limitan el acceso independientemente del prompt. Sin API key de Groq o ante error no se simula una respuesta del modelo: se informa y se permite reintentar.

POST Preguntar y Limpiar con autenticación y antiforgery; GET Historial privado/no cache. Streaming NDJSON con eventos texto, fin y error; cancelación al desconectar, límite de longitud y tiempo. No guardar respuestas incompletas. Evitar envíos simultáneos de la misma sesión. Historial en memoria de servidor mediante ASP.NET Session: se pierde al reiniciar; apropiado para demo de una instancia.

## Interfaz
Reutilizar variables guinda/card/border/text y tipografía Inter existentes. Botón flotante identificable como Asistente FIA, panel no modal con cierre/Escape y retorno del foco. Indicador de espera, detener generación, error persistente junto al formulario y nueva conversación con confirmación propia. Desplazamiento acompaña la respuesta solo si el alumno está al final. Carrusel horizontal de tarjetas con enlaces internos de servidor. Sin HTML o enlaces arbitrarios producidos por el modelo. Fuentes visibles por nombre y página.

## Validación
Pruebas enfocadas de herramientas, aislamiento de historial, antiforgery, prompt mixto con proveedor simulado y lectura del flujo. Comprobación en navegador de escritorio y móvil. Una consulta real con Groq queda sujeta a la API key del equipo y a la disponibilidad del modelo. PR a develop sin merge automático.
