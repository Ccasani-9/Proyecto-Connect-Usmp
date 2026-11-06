# Datos de demostración para la revisión

## Apuntes y publicaciones

`Data/Demo/apuntes.json` contiene 24 materiales breves originales del equipo: ocho fichas existentes y 16 nuevas. Los PDF de ejemplo se generan con texto real sobre Python, cálculo, SQL, redes y algoritmos; no se presentan como documentos oficiales ni como archivos entregados por docentes.

`AmpliacionDemo` añade los nuevos recursos, dos artículos con estados distintos, un consejo de programación y un objeto encontrado. Una marca de versión en SQLite evita duplicarlos o recrearlos al reiniciar después de que alguien los elimine. No modifica publicaciones existentes. La marca y las inserciones se confirman juntas en una transacción.

Consultas preparadas: «Python funciones», «listas Python», «SQL», «derivadas», «WebSocket». Abrir el PDF y comprobar su contenido es parte de la demostración. El asistente sigue buscando metadatos de los apuntes, no interpreta automáticamente sus archivos.

Los archivos fuente se publican con la aplicación para que no dependan del disco efímero de subidas. Las aportaciones reales siguen sus rutas de archivo originales.

## Restaurantes

En bases nuevas se generan importes referenciales variados por rango: económico 9-12, medio 14-20 y premium 24-35 soles por persona. Son cifras ficticias razonables para la maqueta, no cotizaciones verificadas. En bases existentes se conservan los importes ya guardados para no sobrescribir ajustes del equipo.

## Modelos ML.NET

- Sentimiento: 120 comentarios etiquetados de demostración; incluye negaciones y experiencias mixtas.
- Recomendación: 200 interacciones sintéticas usuario-restaurante, sin duplicar pares. No son preferencias observadas de personas reales.
- Se conservan particiones de evaluación y un usuario desconocido para mostrar el respaldo por popularidad. Las métricas proceden de datos pequeños y sintéticos; no acreditan generalización a producción.
- Los nombres de modelo por defecto cambian a `sentimiento-resenas-demo-v2.zip` y `recomendacion-restaurantes-demo-v2.zip` para entrenar con los CSV ampliados. Si el entorno sobrescribe ModelPath, revisar ese valor antes de presentar; no basta cambiar el CSV para afirmar que el modelo guardado ya lo utiliza.
- Las nuevas reseñas no reentrenan por sí mismas el recomendador.

## Guías del asistente

El corpus añade cinco guías sobre acceso, apuntes/Cátedra, restaurantes, Marketplace/objetos y calendario/límites. PdfPig conserva su demostración con el manual PDF de varias páginas. Qdrant usa la versión del corpus para no mezclar textos antiguos con las fuentes actuales.

Verificación focalizada: 10 casos de contenido, conocimiento, sentimiento y recomendaciones superados. Incluye extraer el PDF de funciones y comprobar que una segunda aplicación de la actualización no recrea un apunte eliminado ni altera un gasto ajustado.
