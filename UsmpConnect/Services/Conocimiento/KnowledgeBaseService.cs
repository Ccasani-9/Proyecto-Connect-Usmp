using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace UsmpConnect.Services.Conocimiento;

public sealed class KnowledgeBaseService(CatalogoConocimiento catalogo, IHttpClientFactory clients,
    IConfiguration config, ILogger<KnowledgeBaseService> logger) : IKnowledgeBaseService
{
    private readonly SemaphoreSlim _sincronizacion = new(1, 1);
    private bool _sincronizado;
    private long _reintentarDesdeTicks;
    private static readonly HashSet<string> PalabrasComunes = new(
        "que como donde para los las del una unos unas con por hay quiero saber sobre puedo puedes favor dime tiene tienen esta este estos estas cual cuales".Split(' '));

    public async Task<ResultadoConocimiento> BuscarAsync(string consulta, int limite = 5, CancellationToken ct = default)
    {
        consulta = consulta.Trim();
        if (consulta.Length is < 3 or > 500) throw new ArgumentException("Escribe una consulta de entre 3 y 500 caracteres.");
        limite = Math.Clamp(limite, 1, 5);
        var corpus = await catalogo.ObtenerAsync(ct);
        var estado = "Qdrant sin configurar; búsqueda local por palabras.";
        if (corpus.Fragmentos.Count == 0)
            return new("local", "No hay documentos de referencia disponibles.", [], corpus.Advertencias);

        using var http = clients.CreateClient("QdrantConocimiento");
        var remoto = new QdrantKnowledgeClient(http, config);
        if (remoto.EstaConfigurado)
        {
            try
            {
                await _sincronizacion.WaitAsync(ct);
                try
                {
                    if (DateTimeOffset.UtcNow.UtcTicks < Interlocked.Read(ref _reintentarDesdeTicks))
                        return Local(corpus, consulta, limite, "Qdrant temporalmente no disponible; búsqueda local por palabras.");
                    if (!_sincronizado)
                    {
                        await remoto.SincronizarAsync(corpus.Fragmentos, corpus.Version, ct);
                        _sincronizado = true;
                    }
                }
                finally { _sincronizacion.Release(); }
                var ids = await remoto.BuscarAsync(consulta, corpus.Version, limite, ct);
                var conocidos = corpus.Fragmentos.ToDictionary(f => f.Id);
                var fuentes = ids.Distinct().Where(conocidos.ContainsKey).Select(id => Fuente(conocidos[id])).Take(limite).ToArray();
                return new("qdrant", "Consulta semántica completada en Qdrant.", fuentes, corpus.Advertencias);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidDataException or JsonException or InvalidOperationException or KeyNotFoundException)
            {
                Interlocked.Exchange(ref _reintentarDesdeTicks, DateTimeOffset.UtcNow.AddSeconds(30).UtcTicks);
                // No registrar mensajes de excepción que puedan incluir direcciones o credenciales.
                logger.LogWarning("Consulta Qdrant no disponible ({Tipo}); se usa el respaldo local.", ex.GetType().Name);
                estado = "Qdrant no disponible; búsqueda local por palabras. Revisa conexión, permisos, colección e inferencia.";
            }
        }
        return Local(corpus, consulta, limite, estado);
    }

    private static ResultadoConocimiento Local(CorpusConocimiento corpus, string consulta, int limite, string estado)
        => new("local", estado, BuscarLocal(corpus.Fragmentos, consulta, limite), corpus.Advertencias);

    private static FuenteConocimiento Fuente(FragmentoConocimiento fragmento) => new(fragmento.Fuente, fragmento.Pagina, fragmento.Texto);

    public static IReadOnlyList<FuenteConocimiento> BuscarLocal(IReadOnlyList<FragmentoConocimiento> fragmentos, string consulta, int limite)
    {
        var palabras = Tokenizar(consulta).Where(p => p.Length >= 3 && !PalabrasComunes.Contains(p)).ToHashSet();
        if (palabras.Count == 0) return [];
        return fragmentos.Select(f => new { Fragmento = f, Puntos = Tokenizar(f.Texto).Distinct().Count(palabras.Contains) })
            .Where(f => f.Puntos > 0).OrderByDescending(f => f.Puntos).ThenBy(f => f.Fragmento.Fuente, StringComparer.Ordinal)
            .Take(Math.Clamp(limite, 1, 5)).Select(f => Fuente(f.Fragmento)).ToArray();
    }

    private static IEnumerable<string> Tokenizar(string texto)
    {
        var limpio = new StringBuilder();
        foreach (var c in texto.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) limpio.Append(c);
        return Regex.Matches(limpio.ToString(), @"[\p{L}\p{N}]+").Select(m => m.Value);
    }
}
