# Diseño de USMP Connect

Red social de la facultad dirigida a alumnos. Español de Perú; importes en soles. Conservar el lenguaje visual existente y las tareas de alumnos; herramientas técnicas no forman parte de sus recorridos cotidianos.

## Fuente de verdad visual
`UsmpConnect/wwwroot/css/site.css` define guinda `#6b1020`, guinda oscuro `#560b19`, fondo `#f4f5f7`, tarjetas blancas, borde `#e7e8ec`, texto `#17171c` y estados semánticos. Tipografía Inter con pesos para jerarquía. Reutilizar variables CSS, Bootstrap, `card-u`, `btn-guinda` y `btn-soft`; no agregar fuentes o paletas independientes por pantalla.

## Asistente FIA
Panel flotante no modal con región etiquetada, botón de abrir/cerrar, Escape y retorno de foco. Hereda los tokens del sitio a través de `asistente.css`, sin modificar otras pantallas. Su rasgo distintivo son las tarjetas de contenido de la facultad y las fuentes desplegables, generadas desde resultados de servidor.

`_AsistenteChat.cshtml` es el único propietario del widget. `asistente.js` gestiona sus estados: bienvenida, carga de historial, generación, respuesta completa, error y detención. `asistente.css` es una extensión acotada del tema, con scrollbar visible y espacio estable para el compositor.

Mensajes del modelo y de usuarios se renderizan con textContent, nunca HTML. Enlaces solo en tarjetas del servidor con rutas internas. El chat es transitorio y no comparte texto en URLs ni lo guarda en localStorage. Historial por sesión y usuario en el servidor, con seis intercambios; se pierde al reiniciar o expirar la sesión.

## Contratos de interacción
- Formularios: etiquetas y errores asociados; antiforgery; no validación nativa emergente; evitar envío duplicado.
- Streaming: NDJSON completo por líneas, indicador de espera y botón Detener, desplazamiento solo al permanecer al final, errores visibles junto al campo.
- Nueva conversación: confirmación dentro del panel, esperar respuesta del servidor antes de limpiar la vista.
- Accesibilidad: nombres en botones de iconos, foco visible, estado de generación anunciado sin repetir cada token, diseño adaptable con altura basada en viewport dinámico y safe areas.
- Un cierre del panel no destruye el historial. Una generación detenida o fallida no se presenta como respuesta completa ni se almacena como intercambio terminado.
