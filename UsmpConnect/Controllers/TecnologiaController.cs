using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.Services.Agente;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

[Authorize(Roles = Roles.Alumno)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TecnologiaController(
    ApplicationDbContext db, IRedisCacheService redis, IColaMensajes amqp,
    IBuscadorAlgolia algolia, IPieHostService pieHost,
    IAnalisisSentimientoService sentimiento, IRecomendacionRestaurantesService recomendador,
    IAsistenteService asistente, EstadoIntegraciones estados, IConfiguration config,
    IWebHostEnvironment entorno) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Mostrar el estado existente: abrir el panel no entrena modelos ni llama a proveedores de IA.
        var sqlite = await db.Database.CanConnectAsync(ct);
        var tablas = db.Model.GetEntityTypes().Select(e => e.GetTableName()).Where(n => n != null).Distinct().Count();
        var render = string.Equals(config["RENDER"], "true", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(config["RENDER_SERVICE_ID"]);
        var qdrantConfigurado = !string.IsNullOrWhiteSpace(config["Qdrant:ApiKey"])
            && Uri.TryCreate(config["Qdrant:Endpoint"], UriKind.Absolute, out var endpoint) && endpoint.Scheme == "https";
        var ultimaQdrant = estados.Obtener("qdrant");
        var ultimaGroq = estados.Obtener("groq");
        string Observacion(bool configurado, UltimaOperacionIntegracion? ultima) => ultima is not null
            ? $"{ultima.Descripcion} · {HoraPeru.DesdeUtc(ultima.FechaUtc):dd/MM HH:mm:ss} (Lima)"
            : configurado ? "Configurado; sin operación comprobada en este proceso" : "Sin configurar";
        var servicios = new List<ServicioStatusVM>
        {
            new("SQLite + Entity Framework Core", "Base de datos relacional", "SQLite",
                sqlite, sqlite ? $"Conexión comprobada · {tablas} tablas mapeadas" : "No se pudo abrir la conexión",
                "database", sqlite ? "tag-ok" : "tag-warn", "Datos del proyecto y referencias de cursos y docentes. El almacenamiento persistente depende del entorno de despliegue."),
            new("Redis Cloud", "Caché del catálogo", "Redis",
                estados.Obtener("redis")?.Correcta == true,
                Observacion(redis.IsConnected, estados.Obtener("redis")) + " | " + Observacion(redis.IsConnected, estados.Obtener("redis-invalidacion")),
                "lightning-charge-fill", estados.Obtener("redis")?.Correcta == true ? "tag-ok" : "tag-info",
                "La lectura registra su origen real. La invalidación es una operación separada; una conexión abierta no demuestra un hit."),
            new("CloudAMQP", "Mensajería asíncrona", "RabbitMQ",
                estados.Obtener("rabbit-consumo")?.Correcta == true,
                "Productor: " + Observacion(amqp.IsConnected, estados.Obtener("rabbit-publicacion")) +
                " | Consumidor: " + Observacion(amqp.IsConnected, estados.Obtener("rabbit-consumo")) +
                " | Notificación: " + Observacion(false, estados.Obtener("notificacion")),
                "envelope-paper-fill", estados.Obtener("rabbit-consumo")?.Correcta == true ? "tag-ok" : "tag-info",
                "Últimas operaciones de este proceso. Una notificación local no prueba consumo RabbitMQ remoto; comprobar el ACK y la reseña de demostración."),
            new("Algolia Search Engine", "Búsqueda global", "Algolia",
                estados.Obtener("algolia")?.Correcta == true, Observacion(algolia.IsConfigured, estados.Obtener("algolia")) +
                " | Índice: " + Observacion(algolia.IsConfigured, estados.Obtener("algolia-indice")),
                "search", estados.Obtener("algolia")?.Correcta == true ? "tag-ok" : "tag-info",
                "Indica si la última búsqueda respondió desde Algolia remoto o SQLite. No equivale a monitorización continua ni garantiza que el índice esté actualizado."),
            new("PieHost / PieSocket", "Avisos en tiempo real", "WebSockets",
                false, Observacion(pieHost.IsConfigured, estados.Obtener("piesocket")),
                "broadcast-pin", "tag-info", "Aceptar la publicación HTTP no prueba recepción WebSocket. Comprobar un aviso en otra pestaña sin recargar. No existe push local de respaldo."),
            new("ML.NET · Análisis de Sentimiento", "Clasificación de reseñas", "Modelo local",
                sentimiento.EstaListo, sentimiento.Estado, "cpu-fill", sentimiento.EstaListo ? "tag-ok" : "tag-info",
                "FeaturizeText y SdcaLogisticRegression con un dataset de demostración en español. Sus métricas no equivalen a validación con datos reales."),
            new("ML.NET · Recomendación de Restaurantes", "Filtrado colaborativo", "Modelo local",
                recomendador.EstaListo, recomendador.Estado, "stars", recomendador.EstaListo ? "tag-ok" : "tag-info",
                "MatrixFactorization con datos de demostración. Para usuarios sin historial se utilizan recomendaciones de popularidad."),
            new("Qdrant + PdfPig", "Fuentes documentales del asistente", "Qdrant Cloud / extracción local",
                ultimaQdrant?.Correcta == true, Observacion(qdrantConfigurado, ultimaQdrant), "file-earmark-text",
                ultimaQdrant?.Correcta == true ? "tag-ok" : "tag-info",
                "PdfPig extrae PDF con texto; Qdrant recupera fragmentos. Si falla la consulta remota se indica búsqueda local por palabras. El estado es la última observación de este proceso."),
            new("Groq + Semantic Kernel", "Asistente FIA para alumnos", "API de Groq",
                ultimaGroq?.Correcta == true, Observacion(asistente.EstaConfigurado, ultimaGroq), "chat-dots",
                ultimaGroq?.Correcta == true ? "tag-ok" : "tag-info",
                "Respuestas progresivas con herramientas de consulta y fuentes. Usa el chat y recarga el panel para ver su última operación; abrir el panel no consume cuota de IA."),
            new("Render / Docker", "Entorno de ejecución", render ? "Render" : entorno.EnvironmentName,
                render, render ? "Proceso ejecutándose en Render" : "Ejecución fuera de Render",
                "cloud", "tag-info", "El plan gratuito usa disco efímero: los cambios en SQLite y archivos subidos pueden perderse al reiniciar o desplegar. Los datos semilla permiten reconstruir la demo; no son una copia de los aportes de usuarios.")
        };
        var vm = new TecnologiaViewModel
        {
            TotalTablasSQLite = tablas,
            TotalUsuarios = sqlite ? await db.UserRoles.Join(db.Roles, u => u.RoleId, r => r.Id,
                (u, r) => new { u.UserId, r.Name }).Where(r => r.Name == Roles.Alumno).Select(r => r.UserId).Distinct().CountAsync(ct) : 0,
            TotalRestaurantes = sqlite ? await db.Restaurantes.CountAsync(ct) : 0,
            TotalApuntes = sqlite ? await db.Apuntes.CountAsync(ct) : 0,
            TotalArticulos = sqlite ? await db.Articulos.CountAsync(ct) : 0,
            MetricasSentimiento = sentimiento.UltimasMetricas,
            RutaModeloSentimiento = Path.GetFileName(sentimiento.ModelPath),
            MetricasRecomendacion = recomendador.UltimasMetricas,
            RutaModeloRecomendacion = Path.GetFileName(recomendador.ModelPath),
            Servicios = servicios
        };
        return View(vm);
    }
}
