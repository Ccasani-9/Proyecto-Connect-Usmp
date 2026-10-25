# Redis Cache Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans task-by-task.

**Goal:** Completar el uso demostrable de Redis en restaurantes, con invalidación coherente, precarga opcional y respaldo local.
**Architecture:** Un catálogo serializable en una sola clave compartida. El controlador filtra el catálogo en memoria (datos demo pequeños), de modo que toda reseña invalida la única copia. Un servicio scoped concentra carga, mapping, invalidación y precarga. Un semáforo compartido coordina cargas e invalidaciones dentro del proceso. Redis sigue siendo singleton y su conexión se libera al apagar.
**Tech Stack:** .NET 10, EF Core SQLite, StackExchange.Redis, xUnit, Redis 7 local en Docker para pruebas opcionales.
**Spec:** Bloque Redis acordado en la conversación; rama feature/redis-cache desde 9dcb9c3, merge del PR #13 en develop.

## Global Constraints
- PR hacia develop; no merge automático ni cambios en main.
- No añadir textos técnicos a la interfaz ni modificar su diseño.
- Conservar filtros de categoría y texto; el texto se interpreta como búsqueda literal sin distinguir mayúsculas.
- Precarga controlada por Redis:WarmupOnStartup. La caché tiene TTL de cinco minutos.
- Ninguna prueba usa credenciales cloud o borra llaves de otros entornos.

## Review Focus
- Publicar una reseña actualiza lista general, por categoría y por búsqueda.
- Distintas búsquedas no crean una clave por término ni colisiones con la palabra all.
- Una ausencia confirmada en Redis no revive una copia antigua en MemoryCache de otro proceso.
- Invalidar durante una desconexión debe eliminar la copia remota pendiente al reconectar.
- Precarga habilitada/deshabilitada y expiración funcionan sin depender de la UI.

## Tasks
- [x] 1. Reproducir con xUnit las listas filtradas obsoletas y con Redis real el fallback obsoleto tras una eliminación remota.
- [x] 2. Implementar CatalogoRestaurantesService, centralizar mapping y filtrar la copia común; invalidar al guardar una reseña.
- [x] 3. Corregir cache misses, invalidaciones pendientes durante desconexión y liberación del multiplexer. Probar TTL y fallback local.
- [x] 4. Añadir precarga opcional después de inicializar SQLite, con log del resultado; documentar configuración y pruebas opcionales.
- [ ] 5. Ejecutar pruebas/build, revisión independiente, publicar rama y abrir PR. Detenerse antes del merge humano.

## Verificación y cierre
- Cinco regresiones reproducidas antes de corregirlas.
- Suite ya ejecutada contra Redis 7.4.11 desechable: 38 pruebas aprobadas, ninguna omitida.
- Build ya ejecutado: 0 errores y 0 advertencias.
- Revisión independiente sin bloqueantes; corregida precisión documental sobre claves de prueba.
- Preferencia del usuario: limitar futuras pruebas a lo primordial; no ampliar ni repetir la suite sin motivo.
