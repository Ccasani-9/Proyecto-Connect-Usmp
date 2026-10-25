# Búsqueda global y Algolia

Alcance aprobado: feature/busqueda-algolia desde develop tras PR #14.
- Buscar restaurantes, cursos/docentes, apuntes y artículos disponibles desde todas las páginas.
- Autocompletado con debounce, cancelación, clear, navegación teclado y resultados seguros mediante DOM.
- /Buscar entrega página accesible sin depender de JavaScript; /Buscar/Sugerencias entrega JSON público.
- Indexar los cuatro tipos; reconciliar objetos gestionados al arrancar y tras altas, cambios y bajas. Mantener fallback SQLite si no hay servicio cloud.
- No exponer claves al navegador ni confirmar sincronización si falla; sin nuevos indicadores técnicos para alumnos.
- Comprobar solo lo primordial: build, recorrido búsqueda local y reconciliación con HTTP simulado; no ampliar suite.

Verificación realizada:
- Build: 0 errores y 0 advertencias.
- Dos comprobaciones esenciales: búsqueda pública/cursos/disponibilidad y reconciliación browse/batch simulada.
- Navegador: autocompletado de Programación y navegación a página de resultados comprobados.
- Revisión breve sin bloqueos. Se recuerdan altas enviadas para retirar una baja inmediata aunque Algolia aún no haya indexado el alta.
- Credenciales cloud no configuradas en esta sesión: pendiente comprobar indexación en la cuenta del equipo. No se modifican claves.
