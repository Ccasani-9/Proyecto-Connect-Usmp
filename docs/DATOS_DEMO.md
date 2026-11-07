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
- Recomendación: 400 valoraciones sintéticas entre usuarios y los 52 restaurantes. Se conservan las 200 iniciales y se añaden 20 por cada uno de los diez perfiles de alumnos. Los historiales simulan gustos distintos, por ejemplo cafeterías y chifas para Alessandro y menús criollos para María. Las categorías orientan la creación de la muestra. El recomendador sigue aprendiendo de pares usuario-restaurante con puntuaciones. El archivo de entrenamiento se mantiene separado de las reseñas públicas.
- Se conservan particiones de evaluación y un usuario desconocido para mostrar el respaldo por popularidad. Las métricas proceden de datos pequeños y sintéticos; no acreditan generalización a producción.
- Para mostrar la personalización, abre «Sugerencias para ti» con Alessandro y luego con María. En la prueba de integración con los 52 locales, los tres primeros de Alessandro fueron Shimaya Ramen, Isushi La Fontana y Chifa Moneda China. Los de María fueron Don Tito La Molina, Puerto Central y Cevichería Punta del Este.
- El modelo de sentimiento usa `sentimiento-resenas-demo-v2.zip` y el recomendador usa `recomendacion-restaurantes-demo-v3.zip`. La ruta nueva hace que el recomendador se entrene con el CSV ampliado cuando no existe un modelo guardado en esa ruta. Si Render sobrescribe `Recomendacion__ModelPath`, actualiza esa variable antes de presentar.
- Las nuevas reseñas no reentrenan por sí mismas el recomendador.
- Para demostrar la afinidad por comida, entra con Alessandro y mira «Sugerencias para ti» en Restaurantes. Deja cinco estrellas a Pizza Raúl La Fontana y Mamma Tomato. Al recargar, se priorizan otros locales de comida italiana, como Amore La Molina y Punto Italiano. La afinidad usa los platos y descripciones del catálogo y las reseñas guardadas. ML.NET ordena los locales que tienen la misma afinidad cuando dispone de predicciones.

### Evaluar el recomendador frente a popularidad

```powershell
dotnet test UsmpConnect.Tests --filter FullyQualifiedName~EvaluacionRecomendadorTests --logger 'console;verbosity=detailed'
```

La prueba reserva una valoración de 5 estrellas por perfil con al menos dos favoritos. Usa una semilla fija para la selección, salvo en el caso de Diego, donde reserva Punto Italiano (La Fontana) para mostrar el ejemplo de pastas. Entrena el modelo y calcula la popularidad sobre las valoraciones restantes. Ambos métodos ordenan los mismos locales que el perfil aún no ha puntuado en ese entrenamiento. Se cuenta un acierto cuando el favorito reservado aparece entre los tres primeros.

En la corrida con 400 valoraciones ficticias, ambos métodos acertaron en 4 de 15 perfiles. Diego tenía 5 estrellas en Pizza Raúl La Fontana y Mamma Tomato, además de otras valoraciones que se conservaron. El primer resultado de ML fue Amore La Molina, que ofrece pastas. El favorito reservado, Punto Italiano (La Fontana), quedó 20.º con ML y 21.º por popularidad de 21 candidatos. El ejemplo permite mostrar una sugerencia relacionada y el resultado del favorito reservado por separado. Las dos valoraciones de pizzerías por sí solas no explican la predicción.

## Guías del asistente

El corpus añade cinco guías sobre acceso, apuntes/Cátedra, restaurantes, Marketplace/objetos y calendario/límites. PdfPig conserva su demostración con el manual PDF de varias páginas. Qdrant usa la versión del corpus para no mezclar textos antiguos con las fuentes actuales.

Verificación focalizada: 10 casos de contenido, conocimiento, sentimiento y recomendaciones superados. Incluye extraer el PDF de funciones y comprobar que una segunda aplicación de la actualización no recrea un apunte eliminado ni altera un gasto ajustado.
