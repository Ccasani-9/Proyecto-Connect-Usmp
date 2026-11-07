using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.RateLimiting;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Services;

public record ResultadoRutasAPie(IReadOnlyDictionary<int, int> MinutosPorLocal, int Consultados, string? Aviso = null);

public sealed class RutasAPieService(IHttpClientFactory clientes, IConfiguration config,
    ILogger<RutasAPieService> logger) : IDisposable
{
    public const string Cliente = "GoogleRoutes";
    public const int MaximoDestinos = 12;
    public const double LatitudFia = -12.07203;
    public const double LongitudFia = -76.94205;
    public bool Disponible => !string.IsNullOrWhiteSpace(config["Routes:ApiKey"]);

    private readonly FixedWindowRateLimiter _limite = new(new FixedWindowRateLimiterOptions
    {
        PermitLimit = 4,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0,
        AutoReplenishment = true
    });

    public async Task<ResultadoRutasAPie> ConsultarAsync(IReadOnlyList<RestauranteItemVM> locales, CancellationToken ct)
    {
        var destinos = locales.Where(r => r.Latitud is >= -90 and <= 90 && r.Longitud is >= -180 and <= 180
                && (r.Latitud != 0 || r.Longitud != 0))
            .OrderBy(r => Math.Pow(r.Latitud - LatitudFia, 2) + Math.Pow(r.Longitud - LongitudFia, 2))
            .ThenBy(r => r.Id).Take(MaximoDestinos).ToArray();
        if (destinos.Length == 0) return new(new Dictionary<int, int>(), 0);

        using var permiso = _limite.AttemptAcquire(1);
        if (!permiso.IsAcquired)
            return new(new Dictionary<int, int>(), 0, "Hay varias consultas en curso. Intenta de nuevo en un minuto.");

        var apiKey = config["Routes:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return new(new Dictionary<int, int>(), 0, "La consulta de rutas a pie no está disponible.");

        try
        {
            using var http = clientes.CreateClient(Cliente);
            using var solicitud = new HttpRequestMessage(HttpMethod.Post, "https://routes.googleapis.com/distanceMatrix/v2:computeRouteMatrix")
            {
                Content = JsonContent.Create(new
                {
                    origins = new[] { Punto(LatitudFia, LongitudFia) },
                    destinations = destinos.Select(r => Punto(r.Latitud, r.Longitud)).ToArray(),
                    travelMode = "WALK"
                })
            };
            solicitud.Headers.Add("X-Goog-Api-Key", apiKey);
            solicitud.Headers.Add("X-Goog-FieldMask", "destinationIndex,status,condition,duration");
            using var respuesta = await http.SendAsync(solicitud, ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                logger.LogWarning("Google Routes rechazó la consulta HTTP {Estado}.", (int)respuesta.StatusCode);
                return Fallo();
            }

            var rutas = await respuesta.Content.ReadFromJsonAsync<ElementoRuta[]>(cancellationToken: ct);
            var minutos = new Dictionary<int, int>();
            foreach (var ruta in rutas ?? [])
            {
                // Google omite algunos campos con valor por defecto, incluido el índice cero.
                if (ruta.Status?.Code is > 0 || ruta.Condition != "ROUTE_EXISTS"
                    || ruta.DestinationIndex < 0 || ruta.DestinationIndex >= destinos.Length
                    || ruta.Duration is not { } duracion || !duracion.EndsWith('s')
                    || !double.TryParse(duracion[..^1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var segundos)
                    || !double.IsFinite(segundos) || segundos < 0 || segundos > int.MaxValue)
                    continue;
                minutos[destinos[ruta.DestinationIndex].Id] = (int)Math.Ceiling(segundos / 60);
            }
            return new(minutos, destinos.Length);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException || ex is OperationCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning("No se pudieron consultar las rutas a pie ({Tipo}).", ex.GetType().Name);
            return Fallo();
        }
    }

    private static ResultadoRutasAPie Fallo() => new(new Dictionary<int, int>(), 0,
        "No se pudieron consultar los tiempos a pie. Se mantienen los demás filtros.");

    private static object Punto(double latitude, double longitude) => new { waypoint = new { location = new { latLng = new { latitude, longitude } } } };

    private sealed record EstadoRuta(int Code);
    private sealed record ElementoRuta(int DestinationIndex, EstadoRuta? Status, string? Condition, string? Duration);

    public void Dispose() => _limite.Dispose();
}
