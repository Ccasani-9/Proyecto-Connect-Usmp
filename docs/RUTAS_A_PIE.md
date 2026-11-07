# Rutas a pie desde la FIA

1. En Google Cloud, habilita **Routes API** en un proyecto con facturación y crea una clave para el servidor. Restringe la clave a Routes API y configura una cuota acorde a la demo. La matriz consume un elemento por cada local consultado.
2. En Render, agrega `Routes__ApiKey` en **Environment**. Para desarrollo local, guarda `Routes:ApiKey` en los User Secrets de `UsmpConnect`. `GoogleMaps:ApiKey` corresponde al mapa en el navegador.
3. Tras desplegar, entra con un perfil demo. En Restaurantes, aplica un gasto máximo de S/15 y un límite de 8 minutos a pie. Comprueba los tiempos de Google Maps y que las sugerencias respeten ambos filtros.

La consulta parte de las coordenadas de la FIA. Envía hasta 12 destinos por solicitud, seleccionados por cercanía aproximada entre los locales que ya cumplen los demás filtros. La app admite hasta cuatro solicitudes de matriz por minuto y por instancia. El límite diario se controla con la cuota del proyecto de Google Cloud. Los resultados de Routes solo se usan en la petición actual.

ML.NET conserva el entrenamiento con el historial de valoraciones. El filtro de tiempo restringe los candidatos antes de que el recomendador les asigne puntuaciones. Los perfiles nuevos reciben sugerencias por popularidad entre los candidatos compatibles.

Si Google no puede calcular tiempos, el listado conserva la búsqueda, el gasto y la valoración, y muestra un aviso sobre el filtro de tiempo. Para demostrar la personalización, compara dos perfiles demo con el mismo presupuesto y límite de minutos. El orden solo puede variar si quedan suficientes candidatos compatibles que el modelo conoce.

[Documentación de la matriz](https://developers.google.com/maps/documentation/routes/compute_route_matrix) · [Cuotas y facturación](https://developers.google.com/maps/documentation/routes/usage-and-billing)
