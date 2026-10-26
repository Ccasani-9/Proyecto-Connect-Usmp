using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UsmpConnect.Services.Conocimiento;

public sealed class QdrantKnowledgeClient
{
    public const string Modelo = "intfloat/multilingual-e5-small";
    private readonly HttpClient _http;
    private readonly string _coleccion;
    public bool EstaConfigurado { get; }

    public QdrantKnowledgeClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _coleccion = config["Qdrant:CollectionName"] ?? "usmp_fia_e5_v1";
        var endpoint = config["Qdrant:Endpoint"];
        var key = config["Qdrant:ApiKey"];
        EstaConfigurado = Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && uri.Scheme == "https"
            && string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment)
            && !string.IsNullOrWhiteSpace(key) && Regex.IsMatch(_coleccion, "^[a-zA-Z0-9_-]{1,64}$");
        if (EstaConfigurado)
        {
            _http.BaseAddress = new Uri(endpoint!.TrimEnd('/') + "/");
            _http.DefaultRequestHeaders.Add("api-key", key);
        }
    }

    public async Task SincronizarAsync(IReadOnlyList<FragmentoConocimiento> fragmentos, string version, CancellationToken ct)
    {
        if (!EstaConfigurado) throw new InvalidOperationException("Qdrant no está configurado.");
        using var respuesta = await _http.GetAsync($"collections/{_coleccion}", ct);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            using var crear = await _http.PutAsJsonAsync($"collections/{_coleccion}",
                new { vectors = new { size = 384, distance = "Cosine" } }, ct);
            // Otra instancia puede haber creado la colección entretanto; siempre comprobar el esquema después.
            if (!crear.IsSuccessStatusCode && crear.StatusCode is not HttpStatusCode.Conflict)
                crear.EnsureSuccessStatusCode();
            using var verificar = await _http.GetAsync($"collections/{_coleccion}", ct);
            verificar.EnsureSuccessStatusCode();
            await ValidarColeccionAsync(verificar, ct);
        }
        else
        {
            respuesta.EnsureSuccessStatusCode();
            await ValidarColeccionAsync(respuesta, ct);
        }

        foreach (var campo in new[] { "corpus", "model" })
        {
            using var indice = await _http.PutAsJsonAsync($"collections/{_coleccion}/index?wait=true",
                new { field_name = campo, field_schema = "keyword" }, ct);
            indice.EnsureSuccessStatusCode();
        }

        foreach (var lote in fragmentos.Chunk(32))
        {
            var points = lote.Select(f => new
            {
                // La versión evita que dos despliegues con corpus diferentes sobrescriban sus puntos.
                id = IdRemoto(f.Id, version),
                vector = new { text = f.Texto, model = Modelo },
                payload = new { corpus = version, model = Modelo, fragmento = f.Id.ToString(), fuente = f.Fuente, pagina = f.Pagina }
            });
            using var insertar = await _http.PutAsJsonAsync($"collections/{_coleccion}/points?wait=true", new { points }, ct);
            insertar.EnsureSuccessStatusCode();
        }
    }

    private static string IdRemoto(Guid id, string version)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"{version}:{id}"));
        return new Guid(hash.AsSpan(0, 16)).ToString();
    }

    private static async Task ValidarColeccionAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
        var vectors = json.RootElement.GetProperty("result").GetProperty("config").GetProperty("params").GetProperty("vectors");
        if (!vectors.TryGetProperty("size", out var size) || !size.TryGetInt32(out var dimensiones) || dimensiones != 384
            || !vectors.TryGetProperty("distance", out var distance) || distance.GetString() != "Cosine")
            throw new InvalidDataException("La colección Qdrant debe tener un vector de 384 dimensiones y distancia Cosine.");
    }

    public async Task<IReadOnlyList<Guid>> BuscarAsync(string consulta, string version, int limite, CancellationToken ct)
    {
        if (!EstaConfigurado) throw new InvalidOperationException("Qdrant no está configurado.");
        using var respuesta = await _http.PostAsJsonAsync($"collections/{_coleccion}/points/query", new
        {
            query = new { text = consulta, model = Modelo },
            filter = new { must = new object[] {
                new { key = "corpus", match = new { value = version } },
                new { key = "model", match = new { value = Modelo } } } },
            limit = Math.Clamp(limite, 1, 5),
            with_payload = true
        }, ct);
        respuesta.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
        var ids = new List<Guid>();
        foreach (var punto in json.RootElement.GetProperty("result").GetProperty("points").EnumerateArray())
        {
            var payload = punto.GetProperty("payload");
            if (payload.TryGetProperty("corpus", out var corpus) && corpus.GetString() == version
                && payload.TryGetProperty("model", out var model) && model.GetString() == Modelo
                && payload.TryGetProperty("fragmento", out var id) && Guid.TryParse(id.GetString(), out var guid))
                ids.Add(guid);
        }
        return ids;
    }
}
