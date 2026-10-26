# Base de conocimiento FIA

## Diseño acordado
USMP Connect es una red social para alumnos. Este bloque prepara fuentes para el asistente sin introducir profesores, administradores ni pantallas de gestión. El equipo añade documentos PDF, Markdown o TXT a Data/Conocimiento y reinicia o despliega el proyecto para actualizar el corpus. Los apuntes de alumnos no se indexan automáticamente.

El asistente de la siguiente rama buscará apuntes, cursos, consejos, restaurantes y Marketplace, y explicará cómo usar la plataforma. Puede conversar brevemente, pero debe centrarse en esos temas. En consultas mixtas atiende la parte válida; una petición ajena no bloquea la búsqueda. No inventa datos ni reglamentos. Las herramientas solo consultan contenido permitido al alumno.

## Arquitectura
Fuentes versionadas en archivos, extracción PdfPig, fragmentos con fuente y página, índice Qdrant Cloud opcional y búsqueda local por palabras como respaldo explícito. Se descarta el diseño anterior de panel Profesor y tablas SQLite porque ya no hay cargas web ni gestión mutable. El catálogo fuente se carga al primer uso y permanece estable hasta reiniciar el proceso. Una versión del corpus filtra puntos remotos obsoletos sin borrar colecciones ni datos de otros índices.

Límites por archivo: 5 MB, 200.000 caracteres y 250 fragmentos; máximo 32 documentos y 1.000.000 caracteres en el corpus. Fragmentos de 1.000 caracteres y solapamiento de 150, con número de página para PDF. PDF escaneado sin texto se identifica como no compatible con esta extracción, sin OCR.

Qdrant REST con HTTPS, API key en servidor, colección dedicada, coseno y 384 dimensiones. Modelo fijo intfloat/multilingual-e5-small, el mismo para consulta e inserción. No modificar colecciones incompatibles. Búsqueda autenticada devuelve hasta cinco fuentes; el modo remoto solo se anuncia tras completar la consulta. La falta de credenciales o red permite búsqueda local, sin afirmar que es semántica. No indexar documentos en cada petición; sincronización una vez por proceso, reintento con espera tras fallar.

Documentos semilla describen funcionalidades verificadas del proyecto y se identifican como demostración. No se inventa normativa académica. El corpus contiene únicamente material que todos los alumnos pueden consultar.

## Entrega
Servicio IKnowledgeBaseService para el futuro plugin. Endpoint GET /Conocimiento/Buscar?consulta=... autenticado como diagnóstico, sin escrituras públicas. README explica configuración y demostración. No se añade aún el widget; alcance de la segunda rama. Pruebas enfocadas de extracción, búsqueda con fuente, aislamiento de versión del corpus y contrato HTTP remoto. PR a develop sin merge automático.

Referencias: https://qdrant.tech/documentation/embeddings/qdrant-cloud-inference/ y https://uglytoad.github.io/PdfPig/.
