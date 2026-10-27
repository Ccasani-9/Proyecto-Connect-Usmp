# Correcciones de demostración del PR 27

Alcance aprobado por el usuario: corregir el origen mostrado del catálogo, las afirmaciones del manual y la eliminación inmediata de reseñas. Revisar además la ausencia del chat.

- [x] Verificar el chat publicado: oculto sin sesión, visible y abre tras login demo. Sin fallo reproducido; conservar autenticación y diseño.
- [x] Mantener el indicador del catálogo al abrir detalles; aclarar que registra su carga inicial.
- [x] Confirmar antes de DELETE, siguiendo la confirmación existente de Marketplace.
- [x] Corregir y regenerar el PDF de dos páginas desde un script de autoría separado. Las pruebas solo leen el contenido versionado.
- [x] Verificar extracción del PDF y API con pruebas focalizadas; inspeccionar ambas páginas renderizadas.
- [ ] Commit, push y PR hacia develop; revisión y merge a cargo del usuario.

Validación: 7 pruebas de ConocimientoTests y RestaurantesApiTests superadas. El chequeo del PDF falló antes de corregir su contenido y pasó después. La comprobación publicada del chat solo abrió el panel con cuenta demo, sin enviar consultas ni crear publicaciones. No se desplegaron estas correcciones durante la revisión.
