using System.Text;
using System.Text.Json;

namespace UsmpConnect.Services;

public interface IPieHostService
{
    Task PublicarEventoAsync(string canal, string evento, object datos);
    string? WebSocketClientUrl { get; }
    bool IsConfigured { get; }
    string Estado { get; }
}

public class PieHostService : IPieHostService
{
    private readonly HttpClient _http;
    private readonly EstadoIntegraciones? _estados;
    private readonly ILogger<PieHostService> _logger;
    private readonly string? _apiKey;
    private readonly string? _apiSecret;
    private readonly string? _clusterId;
    private readonly string? _wsUrl;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey) && !string.IsNullOrWhiteSpace(_apiSecret);
    public string Estado => IsConfigured ? "PieSocket configurado; recepción por comprobar" : "Sin WebSocket configurado";
    public string? WebSocketClientUrl => _wsUrl;

    public PieHostService(IConfiguration config, ILogger<PieHostService> logger, EstadoIntegraciones? estados = null)
    {
        _logger = logger;
        _estados = estados;
        _apiKey = config["PieHost:ApiKey"];
        _apiSecret = config["PieHost:ApiSecret"];
        _clusterId = config["PieHost:ClusterId"] ?? "free.blr2";
        _wsUrl = config["PieHost:WebSocketUrl"];
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public async Task PublicarEventoAsync(string canal, string evento, object datos)
    {
        if (!IsConfigured)
        {
            _estados?.Registrar("piesocket", false, "Sin configuración de publicación WebSocket");
            _logger.LogInformation("PieHost no configurado. Evento {Evento} procesado localmente.", evento);
            return;
        }

        try
        {
            var payload = new
            {
                key = _apiKey,
                secret = _apiSecret,
                channelId = canal,
                message = JsonSerializer.Serialize(new
                {
                    evento,
                    datos,
                    timestamp = DateTime.UtcNow
                })
            };

            var url = $"https://{_clusterId}.piesocket.com/api/v3/publish";
            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var resp = await _http.PostAsync(url, content);
            if (resp.IsSuccessStatusCode)
            {
                _estados?.Registrar("piesocket", true, "PieSocket aceptó la publicación; comprobar recepción en el navegador");
                _logger.LogInformation("Evento {Evento} transmitido por WebSocket en PieHost.", evento);
            }
            else
            {
                _estados?.Registrar("piesocket", false, "PieSocket rechazó la publicación");
                _logger.LogWarning("PieHost publish respondió con status: {Code}", resp.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _estados?.Registrar("piesocket", false, "No se pudo completar la publicación remota");
            _logger.LogWarning(ex, "Error al emitir evento por PieHost.");
        }
    }
}
