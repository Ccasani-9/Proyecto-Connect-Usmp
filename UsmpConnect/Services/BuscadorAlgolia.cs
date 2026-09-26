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
    Task SincronizarTodoAsync(ApplicationDbContext db);
    Task<List<AlgoliaItem>> BuscarAsync(string query, ApplicationDbContext db);
    bool IsConfigured { get; }
    string Estado { get; }
}

public class BuscadorAlgolia : IBuscadorAlgolia
{
    private readonly HttpClient _http;
    private readonly ILogger<BuscadorAlgolia> _logger;
    private readonly string? _appId;
    private readonly string? _searchKey;
    private readonly string? _writeKey;
    private readonly string _indexName;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_appId) && !string.IsNullOrWhiteSpace(_searchKey);
    public string Estado => IsConfigured ? "Algolia Search Engine (Activo)" : "Búsqueda Local SQLite (Fallback)";

    public BuscadorAlgolia(IConfiguration config, ILogger<BuscadorAlgolia> logger)
    {
        _logger = logger;
        _appId = config["Algolia:ApplicationId"];
        _searchKey = config["Algolia:SearchApiKey"];
        _writeKey = config["Algolia:WriteApiKey"];
        _indexName = config["Algolia:IndexName"] ?? "usmp_connect";
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public async Task SincronizarTodoAsync(ApplicationDbContext db)
    {
        if (string.IsNullOrWhiteSpace(_appId) || string.IsNullOrWhiteSpace(_writeKey))
        {
            _logger.LogInformation("Algolia write key no configurada. Omitiendo sincronización en la nube.");
            return;
        }

        try
        {
            var items = new List<AlgoliaItem>();

            // 1. Apuntes
            var apuntes = await db.Apuntes.Include(a => a.Curso).AsNoTracking().ToListAsync();
            items.AddRange(apuntes.Select(a => new AlgoliaItem
            {
                ObjectID = $"apunte_{a.Id}",
                Tipo = "Apunte",
                Titulo = a.Titulo,
                Descripcion = $"{a.Curso.Nombre} · {a.Tipo.Nombre()} · Ciclo {a.Curso.Ciclo}",
                Categoria = a.Curso.Nombre,
                Url = "/Apuntes",
                Calificacion = a.Promedio
            }));

            // 2. Marketplace
            var articulos = await db.Articulos.AsNoTracking().ToListAsync();
            items.AddRange(articulos.Select(art => new AlgoliaItem
            {
                ObjectID = $"market_{art.Id}",
                Tipo = "Marketplace",
                Titulo = art.Titulo,
                Descripcion = $"{art.Categoria} · {art.EstadoItem} · {art.NotasEntrega}",
                Categoria = art.Categoria.ToString(),
                Url = "/Marketplace",
                Precio = art.Precio
            }));

            // 3. Restaurantes (FoodSpots)
            var restaurantes = await db.Restaurantes.AsNoTracking().ToListAsync();
            items.AddRange(restaurantes.Select(r => new AlgoliaItem
            {
                ObjectID = $"rest_{r.Id}",
                Tipo = "Food & Spots",
                Titulo = r.Nombre,
                Descripcion = $"{r.Direccion} · {r.DistanciaTexto} · {r.RangoPrecios}",
                Categoria = r.Categoria.ToString(),
                Url = "/FoodSpots",
                Calificacion = r.CalificacionPromedio
            }));

            // Subir a Algolia mediante REST Batch API
            var batchRequest = new
            {
                requests = items.Select(item => new
                {
                    action = "addObject",
                    body = item
                })
            };

            var url = $"https://{_appId}.algolia.net/1/indexes/{_indexName}/batch";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-Algolia-Application-Id", _appId);
            request.Headers.Add("X-Algolia-API-Key", _writeKey);
            request.Content = new StringContent(JsonSerializer.Serialize(batchRequest), System.Text.Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Sincronizados {Count} elementos en Algolia índice '{Index}' exitosamente.", items.Count, _indexName);
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Algolia batch falló con código {Code}: {Error}", response.StatusCode, err);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al sincronizar con Algolia.");
        }
    }

    public async Task<List<AlgoliaItem>> BuscarAsync(string query, ApplicationDbContext db)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<AlgoliaItem>();

        var q = query.Trim();

        // 1. Intentar Algolia si está configurado
        if (IsConfigured)
        {
            try
            {
                var searchUrl = $"https://{_appId}-dsn.algolia.net/1/indexes/{_indexName}/query";
                var request = new HttpRequestMessage(HttpMethod.Post, searchUrl);
                request.Headers.Add("X-Algolia-Application-Id", _appId);
                request.Headers.Add("X-Algolia-API-Key", _searchKey);
                request.Content = new StringContent(JsonSerializer.Serialize(new { query = q, hitsPerPage = 10 }), System.Text.Encoding.UTF8, "application/json");

                var resp = await _http.SendAsync(request);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("hits", out var hitsEl))
                    {
                        var lista = JsonSerializer.Deserialize<List<AlgoliaItem>>(hitsEl.GetRawText());
                        if (lista != null && lista.Count > 0)
                            return lista;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al consultar Algolia. Cayendo a búsqueda local SQLite.");
            }
        }

        // 2. Fallback local SQLite
        var resultados = new List<AlgoliaItem>();

        var apuntes = await db.Apuntes.Include(a => a.Curso).AsNoTracking()
            .Where(a => EF.Functions.Like(a.Titulo, $"%{q}%") || EF.Functions.Like(a.Curso.Nombre, $"%{q}%"))
            .Take(4)
            .ToListAsync();
        resultados.AddRange(apuntes.Select(a => new AlgoliaItem
        {
            ObjectID = $"apunte_{a.Id}",
            Tipo = "Apunte",
            Titulo = a.Titulo,
            Descripcion = $"{a.Curso.Nombre} · {a.Tipo}",
            Categoria = a.Curso.Nombre,
            Url = "/Apuntes"
        }));

        var articulos = await db.Articulos.AsNoTracking()
            .Where(art => EF.Functions.Like(art.Titulo, $"%{q}%"))
            .Take(4)
            .ToListAsync();
        resultados.AddRange(articulos.Select(art => new AlgoliaItem
        {
            ObjectID = $"market_{art.Id}",
            Tipo = "Marketplace",
            Titulo = art.Titulo,
            Descripcion = $"S/ {art.Precio:N2} · {art.NotasEntrega}",
            Categoria = art.Categoria.ToString(),
            Url = "/Marketplace",
            Precio = art.Precio
        }));

        var restos = await db.Restaurantes.AsNoTracking()
            .Where(r => EF.Functions.Like(r.Nombre, $"%{q}%") || EF.Functions.Like(r.Direccion, $"%{q}%"))
            .Take(4)
            .ToListAsync();
        resultados.AddRange(restos.Select(r => new AlgoliaItem
        {
            ObjectID = $"rest_{r.Id}",
            Tipo = "Food & Spots",
            Titulo = r.Nombre,
            Descripcion = $"{r.DistanciaTexto} · {r.RangoPrecios}",
            Categoria = r.Categoria.ToString(),
            Url = "/FoodSpots",
            Calificacion = r.CalificacionPromedio
        }));

        return resultados;
    }
}
