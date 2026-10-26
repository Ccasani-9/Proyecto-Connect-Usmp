# Render: configuración de la demo

## Estado revisado el 26 de octubre de 2026

Servicio UsmpConnect, Docker, plan Free y despliegue automático desde `develop`. Se conserva esa rama; el cambio a `main` se realizará explícitamente al preparar la versión estable. No se contrataron recursos ni se lanzaron despliegues durante la preparación.

En Environment se guardaron Groq__ApiKey, Groq__Model, Qdrant__Endpoint, Qdrant__ApiKey y Qdrant__CollectionName mediante **Save only**. También se cambió Database__ResetOnStartup de true a false. Esto prepara los valores pero no cambia el proceso en ejecución: entrarán en vigor con el siguiente despliegue. No copiar las claves a este documento ni al repositorio.

Ya existían claves de configuración de Algolia, Redis, CloudAMQP y PieHost. Su presencia no demuestra que cada operación remota funcione: revisar la demostración correspondiente después del despliegue. Los valores existentes se conservaron.

## Publicación de estos ajustes

El PR #24 ya fue fusionado y Render publicó `ba2649c` correctamente. Groq y Qdrant se comprobaron en el sitio público. Después se guardó `/health` como Health Check Path; Render inició y completó otro despliegue automático del mismo commit por ese ajuste. Consultar [Verificación de la publicación](VERIFICACION_PUBLICACION.md) para distinguir los flujos comprobados de los pendientes de demostración.

1. Revisar y fusionar el PR hacia `develop` con Create a merge commit.
2. Render iniciará el despliegue por la configuración de auto-deploy existente. Esperar a que el nuevo commit figure Live.
3. Comprobar `/health` (200), iniciar sesión como alumno y abrir `/Tecnologia`.
4. Hacer una consulta de restaurantes en el asistente y una de fuentes; recargar `/Tecnologia` para ver las últimas observaciones de Groq y Qdrant en ese servidor.
5. Publicar una reseña de demostración y comprobar notificación persistida y, si está configurado, aviso WebSocket. Buscar un apunte y comprobar la respuesta del buscador.

El blueprint define `/health` para servicios nuevos o sincronizados por Blueprint. En un servicio gestionado manualmente, ajustar Health Check Path a `/health` en Settings una vez publicado el código que expone esa ruta; no anticiparlo sobre una versión que aún responde 404.

Las variables con `sync: false` se solicitan al crear un Blueprint, pero no se incorporan automáticamente a servicios existentes. Por eso las claves nuevas se añadieron directamente al servicio desde el Dashboard.

## Almacenamiento y límites

El plan Free de Render usa filesystem efímero y no admite discos persistentes. SQLite (`/app/data/usmpconnect.db`), imágenes subidas (`/app/wwwroot/uploads`) y modelos ML generados (`/app/App_Data`) pueden perderse al reiniciar, desplegar o suspender la instancia. Los modelos se regeneran y los datos semilla reconstruyen la demo, pero los aportes de usuarios y archivos subidos no se recuperan con el seeder.

Database__ResetOnStartup=false impide que la propia aplicación borre deliberadamente los datos al arrancar; no convierte ese disco en persistente. No usar ResetOnStartup=true durante una demostración con aportes que se quieran conservar. No se habilitó almacenamiento de pago ni se migró la base de datos.

Los documentos de referencia en Data/Conocimiento forman parte de la imagen y se vuelven a publicar con el código. El historial de chat también es temporal: queda en memoria del proceso y expira por sesión.

## Diagnóstico y acceso

La aplicación está dirigida a alumnos. Las cuentas docentes de los datos de ejemplo siguen existiendo como referencias de cursos y consejos, pero no pueden iniciar sesión. No se convierten automáticamente en alumnos ni se borran sus relaciones de Cátedra.

`/Tecnologia` requiere cuenta de alumno y no realiza llamadas a Groq/Qdrant ni entrena modelos al abrirse. Muestra configuración y últimas operaciones observadas con hora de Lima. No es un monitor continuo de disponibilidad y su memoria se reinicia con el proceso. `/health` solo confirma que la aplicación terminó de arrancar y atiende HTTP; no prueba todos los proveedores externos.

## Fuentes oficiales

- [Plan gratuito y almacenamiento efímero](https://render.com/docs/free)
- [Variables y opciones de guardado](https://render.com/docs/configure-environment-variables)
- [Blueprints y sync: false](https://render.com/docs/blueprint-spec)
- [Health checks](https://render.com/docs/health-checks)
