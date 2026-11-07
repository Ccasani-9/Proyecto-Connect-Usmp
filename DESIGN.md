# Diseño de USMP Connect

Red social de la facultad dirigida a alumnos. Español de Perú; importes en soles. Conservar el lenguaje visual existente y las tareas de alumnos; herramientas técnicas no forman parte de sus recorridos cotidianos.

## Fuente de verdad visual
`UsmpConnect/wwwroot/css/site.css` define guinda `#6b1020`, guinda oscuro `#560b19`, fondo `#f4f5f7`, tarjetas blancas, borde `#e7e8ec`, texto `#17171c` y estados semánticos. Tipografía Inter con pesos para jerarquía. Reutilizar variables CSS, Bootstrap, `card-u`, `btn-guinda` y `btn-soft`; no agregar fuentes o paletas independientes por pantalla.

## Asistente FIA
Panel flotante no modal con región etiquetada, botón de abrir/cerrar, Escape y retorno de foco. Hereda los tokens del sitio a través de `asistente.css`, sin modificar otras pantallas. Su rasgo distintivo son las tarjetas de contenido de la facultad generadas desde resultados del servidor. Las guías de ayuda se usan para redactar respuestas, y el chat entrega solo texto y tarjetas.

`_AsistenteChat.cshtml` es el único propietario del widget. `asistente.js` gestiona sus estados: bienvenida, carga de historial, generación, respuesta completa, error y detención. `asistente.css` es una extensión acotada del tema, con scrollbar visible y espacio estable para el compositor.

Mensajes del modelo y de usuarios se renderizan con textContent, nunca HTML. Enlaces solo en tarjetas del servidor con rutas internas. El chat es transitorio y no comparte texto en URLs ni lo guarda en localStorage. Historial por sesión y usuario en el servidor, con seis intercambios; se pierde al reiniciar o expirar la sesión.

## Contratos de interacción
- El panel `/Tecnologia` es diagnóstico para alumnos autenticados y sustentación. Reutiliza `card-u`, tarjetas Bootstrap y tags del tema; las etiquetas largas se envuelven. Los estados distinguen configuración de última operación y muestran hora de Lima; no ejecuta consultas de IA al abrirse. Swagger y pruebas de fuentes se acceden desde ese panel, sin agregar controles técnicos a las pantallas sociales.
- Formularios: etiquetas y errores asociados; antiforgery; no validación nativa emergente; evitar envío duplicado.
- Streaming: NDJSON completo por líneas, indicador de espera y botón Detener, desplazamiento solo al permanecer al final, errores visibles junto al campo.
- Nueva conversación: confirmación dentro del panel, esperar respuesta del servidor antes de limpiar la vista.
- Accesibilidad: nombres en botones de iconos, foco visible, estado de generación anunciado sin repetir cada token, diseño adaptable con altura basada en viewport dinámico y safe areas.
- Un cierre del panel no destruye el historial. Una generación detenida o fallida no se presenta como respuesta completa ni se almacena como intercambio terminado.

## Gasto referencial de restaurantes
El catálogo coloca los filtros del listado antes del mapa. Los controles de zona se agrupan con el mapa. Las tarjetas de la grilla muestran foto, nombre, categoría, valoración, gasto aproximado y cercanía. El modal reúne descripción, dirección, horarios, rutas y reseñas. Un precio desconocido se muestra como gasto por confirmar y queda fuera del filtro de presupuesto.

## Fotos y acceso

Marketplace y Objetos Perdidos usan fotografías locales de referencia en publicaciones demo. Se identifican junto a la foto y conservan la misma geometría al cargar o fallar. Una publicación nueva sin imagen muestra un estado sin foto, sin atribuirle una fotografía de otro objeto. Los archivos de ejemplo viven en `/images/demo/`; eliminar un reporte solo retira archivos subidos a `/uploads/lostfound/`.

`acceso.js` comparte la espera y prevención de doble envío en inicio y cierre de sesión. Un formulario vencido se renueva mediante navegación GET sin repetir la operación. Google presenta errores dentro del formulario, con recuperación explícita cuando debe renovarse.

## Calendario y lectura del asistente

Calendario reserva 100px al pie para separar acciones del lanzador flotante. Actividades semanales con días seleccionados y final obligatorio (máximo 366 días); se materializan como actividades independientes y eliminar afecta solo esa fecha. Emoji de paleta acotada integrado al título para mantener compatibilidad con Inicio sin migración. Formulario desplazable en pantallas bajas. El asistente presenta listas mediante nodos DOM con textContent y tarjetas verticales completas, sin HTML del modelo ni carruseles truncados.

El asistente admite negrita y código en línea mediante elementos strong/code creados con textContent (sin HTML del modelo). Un indicador fijo sobre el compositor permanece visible durante toda la petición y el streaming; desaparece en finalización, error o cancelación. Respeta movimiento reducido.
