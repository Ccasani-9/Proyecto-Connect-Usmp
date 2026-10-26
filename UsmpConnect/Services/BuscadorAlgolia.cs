using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;

namespace UsmpConnect.Services;

public class AlgoliaItem
{
    [JsonPropertyName("objectID")]
    public string ObjectID { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty; // "Apunte", "Marketplace", "Restaurante", "Curso"

    [JsonPropertyName("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [JsonPropertyName("categoria")]
    public string Categoria { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("precio")]
    public decimal? Precio { get; set; }

    [JsonPropertyName("calificacion")]
    public double? Calificacion { get; set; }
}

public interface IBuscadorAlgolia
{
    Task<bool> SincronizarTodoAsync(ApplicationDbContext db, CancellationToken cancellationToken = default);
    Task<List<AlgoliaItem>> BuscarAsync(string query, ApplicationDbContext db, CancellationToken cancellationToken = default);
    bool IsConfigured { get; }
    string Estado { get; }
}

public sealed class BuscadorAlgolia(IConfiguration config, ILogger<BuscadorAlgolia> logger, HttpClient http) : IBuscadorAlgolia
{
    private static readonly SemaphoreSlim SyncGate = new(1, 1);
    // Algolia indexa en segundo plano: recordar altas enviadas permite retirar una baja
    // inmediata aunque browse todavía no vea el alta anterior. Protegido por SyncGate.
    private static readonly Dictionary<string, HashSet<string>> Enviados = new();
    private string? AppId => config["Algolia:ApplicationId"];
    private string? SearchKey => config["Algolia:SearchApiKey"];
    private string? WriteKey => config["Algolia:WriteApiKey"];
    private string Index => Uri.EscapeDataString(config["Algolia:IndexName"] ?? "usmp_connect");
    public bool IsConfigured => !string.IsNullOrWhiteSpace(AppId) && !string.IsNullOrWhiteSpace(SearchKey);
    public string Estado => IsConfigured ? "Algolia configurado" : "Búsqueda local SQLite";

    public async Task<bool> SincronizarTodoAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(AppId) || string.IsNullOrWhiteSpace(WriteKey)) return false;
        await SyncGate.WaitAsync(cancellationToken);
        try
        {
            var items = await CatalogoAsync(db, cancellationToken);
            var actuales = items.Select(i => i.ObjectID).ToHashSet(StringComparer.Ordinal);
            var anteriores = new HashSet<string>(StringComparer.Ordinal);
            string? cursor = null;
            do
            {
                object body = cursor is null ? new { attributesToRetrieve = new[] { "objectID" }, hitsPerPage = 1000 } : new { cursor };
                using var request = Request("browse", WriteKey!, body);
                using var response = await http.SendAsync(request, cancellationToken);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) break;
                response.EnsureSuccessStatusCode();
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                foreach (var hit in json.RootElement.GetProperty("hits").EnumerateArray())
                    if (hit.TryGetProperty("objectID", out var id) && EsPropio(id.GetString())) anteriores.Add(id.GetString()!);
                cursor = json.RootElement.TryGetProperty("cursor", out var next) ? next.GetString() : null;
            } while (!string.IsNullOrEmpty(cursor));

            var indice = $"{AppId}:{Index}";
            if (Enviados.TryGetValue(indice, out var pendientes)) anteriores.UnionWith(pendientes);
            Enviados[indice] = anteriores.Union(actuales).ToHashSet(StringComparer.Ordinal);

            var operations = items.Select(i => (object)new { action = "addObject", body = i }).ToList();
            operations.AddRange(anteriores.Except(actuales).Select(id => (object)new { action = "deleteObject", body = new { objectID = id } }));
            foreach (var batch in operations.Chunk(1000))
            {
                using var request = Request("batch", WriteKey!, new { requests = batch });
                using var response = await http.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            logger.LogInformation("Algolia aceptó la sincronización de {Cantidad} registros y la retirada de {Bajas} obsoletos", items.Count, anteriores.Except(actuales).Count());
            Enviados[indice] = actuales;
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException || ex is TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "No se pudo sincronizar Algolia; los datos siguen guardados en SQLite");
            return false;
        }
        finally { SyncGate.Release(); }
    }

    public async Task<List<AlgoliaItem>> BuscarAsync(string query, ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        var q = query.Trim();
        if (q.Length > 100) q = q[..100];
        // SQLite es la fuente de verdad para enlaces y disponibilidad, incluso si el índice está retrasado.
        var catalogo = await CatalogoAsync(db, cancellationToken);
        if (IsConfigured)
        {
            try
            {
                using var request = Request("query", SearchKey!, new { query = q, hitsPerPage = 12 }, search: true);
                using var response = await http.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                // Conserva el orden de relevancia de Algolia; nunca devuelve contenido borrado/vendido.
                var actuales = catalogo.ToDictionary(i => i.ObjectID);
                return json.RootElement.GetProperty("hits").EnumerateArray()
                    .Select(h => h.GetProperty("objectID").GetString())
                    .Where(id => id is not null && actuales.ContainsKey(id))
                    .Distinct().Select(id => actuales[id!]).Take(12).ToList();
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException || ex is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Algolia no respondió; se usa búsqueda local");
            }
        }
        return catalogo.Where(i => Contiene(i.Titulo, q) || Contiene(i.Descripcion, q) || Contiene(i.Categoria, q))
            .GroupBy(i => i.Tipo).SelectMany(g => g.Take(4)).Take(16).ToList();
    }

    private HttpRequestMessage Request(string action, string key, object body, bool search = false)
    {
        var host = search ? $"{AppId}-dsn.algolia.net" : $"{AppId}.algolia.net";
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://{host}/1/indexes/{Index}/{action}");
        request.Headers.Add("X-Algolia-Application-Id", AppId);
        request.Headers.Add("X-Algolia-API-Key", key);
        request.Content = JsonContent.Create(body);
        return request;
    }

    private static bool EsPropio(string? id) => id is not null && new[] { "apunte_", "market_", "rest_", "curso_" }.Any(id.StartsWith);
    internal static bool Contiene(string text, string q) => HoraPeru.Cultura.CompareInfo.IndexOf(text, q,
        System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace) >= 0;

    private static async Task<List<AlgoliaItem>> CatalogoAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var items = new List<AlgoliaItem>();
        var apuntes = await db.Apuntes.Include(a => a.Curso).AsNoTracking().ToListAsync(ct);
        items.AddRange(apuntes.Select(a => new AlgoliaItem
        {
            ObjectID = $"apunte_{a.Id}", Tipo = "Apuntes", Titulo = a.Titulo,
            Descripcion = $"{a.Curso.Nombre} · {a.Tipo.Nombre()} · Ciclo {a.Curso.Ciclo}", Categoria = a.Curso.Nombre,
            Url = $"/Apuntes?q={Uri.EscapeDataString(a.Titulo)}", Calificacion = a.Promedio
        }));
        var articulos = await db.Articulos.AsNoTracking().Where(a => a.Estado == UsmpConnect.Models.EstadoPublicacion.Disponible).ToListAsync(ct);
        items.AddRange(articulos.Select(a => new AlgoliaItem
        {
            ObjectID = $"market_{a.Id}", Tipo = "Marketplace", Titulo = a.Titulo,
            Descripcion = $"{a.Categoria} · {a.EstadoItem} · {a.NotasEntrega}", Categoria = a.Categoria.ToString(),
            Url = $"/Marketplace?q={Uri.EscapeDataString(a.Titulo)}", Precio = a.Precio
        }));
        var restaurantes = await db.Restaurantes.AsNoTracking().ToListAsync(ct);
        items.AddRange(restaurantes.Select(r => new AlgoliaItem
        {
            ObjectID = $"rest_{r.Id}", Tipo = "Restaurantes", Titulo = r.Nombre,
            Descripcion = $"{r.Direccion} · {r.Descripcion} · {r.RangoPrecios}", Categoria = r.Categoria.Nombre(),
            Url = $"/Restaurantes?q={Uri.EscapeDataString(r.Nombre)}#card-rest-{r.Id}", Calificacion = r.CalificacionPromedio
        }));
        var cursos = await db.Cursos.Include(c => c.Secciones).ThenInclude(s => s.Docente).AsNoTracking().ToListAsync(ct);
        items.AddRange(cursos.Select(c => new AlgoliaItem
        {
            ObjectID = $"curso_{c.Id}", Tipo = "Cursos y docentes", Titulo = c.Nombre,
            Descripcion = $"{c.Codigo} · Ciclo {c.Ciclo} · {string.Join(" · ", c.Secciones.Select(s => s.Docente.NombreDocente).Distinct())}",
            Categoria = "Cátedra & Tips", Url = $"/Catedra/Curso/{c.Id}"
        }));
        return items;
    }
}
