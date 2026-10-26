using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Recommender;
using Microsoft.ML.Trainers;
using UsmpConnect.Data;

namespace UsmpConnect.Services;

public class DatoEntrenamientoRecomendacion
{
    [LoadColumn(0)]
    public string UserId { get; set; } = string.Empty;

    [LoadColumn(1)]
    public float RestauranteId { get; set; }

    [LoadColumn(2)]
    public float Rating { get; set; }
}

public class SalidaPrediccionRecomendacion
{
    [ColumnName("Score")]
    public float Score { get; set; }
}

public class RecomendacionRestaurante
{
    public int RestauranteId { get; set; }
    public float Score { get; set; }
    public bool DeFrio { get; set; }
}

public class MetricasEntrenamientoRecomendacion
{
    public int EjemplosTotales { get; set; }
    public int EjemplosEntrenamiento { get; set; }
    public int EjemplosPrueba { get; set; }
    public int Usuarios { get; set; }
    public int Restaurantes { get; set; }
    public double? Rms { get; set; }
    public double? R2 { get; set; }
    public DateTime FechaEntrenamiento { get; set; } = DateTime.UtcNow;
}

public interface IRecomendacionRestaurantesService
{
    bool EstaHabilitado { get; }
    bool EstaListo { get; }
    string Estado { get; }
    string ModelPath { get; }
    MetricasEntrenamientoRecomendacion? UltimasMetricas { get; }
    bool EsUsuarioConocido(string? userId);
    Task InicializarAsync(CancellationToken cancellationToken = default);
    Task SincronizarConBaseDatosAsync(ApplicationDbContext db, CancellationToken cancellationToken = default);
    Task<List<RecomendacionRestaurante>> RecomendarAsync(
        string? userId,
        IEnumerable<int> candidatos,
        int topK,
        CancellationToken cancellationToken = default);
    Task<bool> ReentrenarAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Entrena y consume el modelo de recomendación de restaurantes por factorización de matrices (ML.NET)
/// sobre Data/ML/interacciones_restaurantes.csv, sincronizando los IDs de usuarios y restaurantes con SQLite.
/// </summary>
public sealed class RecomendacionRestaurantesService : IRecomendacionRestaurantesService, IDisposable
{
    private const string ScoreColumn = "Score";
    private const string LabelColumn = nameof(DatoEntrenamientoRecomendacion.Rating);

    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<RecomendacionRestaurantesService> _logger;

    private readonly MLContext _mlContext = new(seed: 42);
    private readonly SemaphoreSlim _cargaLock = new(1, 1);
    private readonly SemaphoreSlim _prediccionLock = new(1, 1);

    private PredictionEngine<DatoEntrenamientoRecomendacion, SalidaPrediccionRecomendacion>? _engine;
    private HashSet<string> _usuariosConocidos = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _aliasUsuario = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<int> _restaurantesConocidos = [];
    private Dictionary<int, float> _popularidadPorRestaurante = [];

    public RecomendacionRestaurantesService(
        IConfiguration config,
        IHostEnvironment env,
        ILogger<RecomendacionRestaurantesService> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    public bool EstaHabilitado => _config.GetValue("Recomendacion:Enabled", true);
    public bool EstaListo => _engine is not null;
    public MetricasEntrenamientoRecomendacion? UltimasMetricas { get; private set; }

    public string Estado => !EstaHabilitado
        ? "Deshabilitado"
        : (EstaListo
            ? (UltimasMetricas is not null
                ? $"Factorización de Matrices Activa (RMSE {UltimasMetricas.Rms?.ToString("0.00") ?? "no disponible"})"
                : "Factorización de Matrices Activa")
            : "Pendiente de inicialización");

    public string ModelPath => ResolverRuta(
        _config["Recomendacion:ModelPath"] ?? "App_Data/recomendacion-restaurantes.zip");

    private string TrainingDataPath => ResolverRuta(
        _config["Recomendacion:TrainingDataPath"] ?? "Data/ML/interacciones_restaurantes.csv");

    public bool EsUsuarioConocido(string? userId)
    {
        var clave = NormalizarUsuario(userId);
        return !string.IsNullOrWhiteSpace(clave) && _usuariosConocidos.Contains(clave);
    }

    public Task InicializarAsync(CancellationToken cancellationToken = default)
    {
        if (!EstaHabilitado)
            return Task.CompletedTask;

        return AsegurarModeloAsync(cancellationToken);
    }

    public async Task SincronizarConBaseDatosAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        if (!EstaHabilitado)
            return;

        var usuarios = await db.Users.AsNoTracking()
            .Select(u => new { u.Id, u.Email, u.UserName })
            .ToListAsync(cancellationToken);

        await _cargaLock.WaitAsync(cancellationToken);
        try
        {
            foreach (var u in usuarios)
            {
                var canonico = u.Email ?? u.UserName;
                if (!string.IsNullOrWhiteSpace(u.Id) && !string.IsNullOrWhiteSpace(canonico))
                {
                    _aliasUsuario[u.Id] = canonico;
                    _aliasUsuario[canonico] = canonico;
                }
            }
        }
        finally
        {
            _cargaLock.Release();
        }

        await AsegurarModeloAsync(cancellationToken);
    }

    public async Task<List<RecomendacionRestaurante>> RecomendarAsync(
        string? userId,
        IEnumerable<int> candidatos,
        int topK,
        CancellationToken cancellationToken = default)
    {
        var listaCandidatos = candidatos.Distinct().ToList();
        var limite = Math.Max(0, topK);
        if (!EstaHabilitado || limite == 0 || listaCandidatos.Count == 0)
            return [];

        await AsegurarModeloAsync(cancellationToken);

        var engine = _engine;
        var claveUsuario = NormalizarUsuario(userId);
        var conocidosEnCatalogo = _restaurantesConocidos
            .Intersect(listaCandidatos)
            .OrderBy(id => id)
            .ToList();

        await _prediccionLock.WaitAsync(cancellationToken);
        try
        {
            if (engine is not null
                && !string.IsNullOrWhiteSpace(claveUsuario)
                && _usuariosConocidos.Contains(claveUsuario)
                && conocidosEnCatalogo.Count > 0)
            {
                var predicciones = new List<RecomendacionRestaurante>(conocidosEnCatalogo.Count);
                foreach (var restId in conocidosEnCatalogo)
                {
                    var pred = engine.Predict(new DatoEntrenamientoRecomendacion
                    {
                        UserId = claveUsuario,
                        RestauranteId = restId,
                        Rating = 0
                    });
                    predicciones.Add(new RecomendacionRestaurante
                    {
                        RestauranteId = restId,
                        Score = float.IsNaN(pred.Score) ? 0f : pred.Score,
                        DeFrio = false
                    });
                }

                return predicciones
                    .OrderByDescending(r => r.Score)
                    .ThenBy(r => r.RestauranteId)
                    .Take(limite)
                    .ToList();
            }

            // Arranque en frío: usuarios sin historial o sesión anónima reciben los locales más populares.
            return listaCandidatos
                .Select(id => new RecomendacionRestaurante
                {
                    RestauranteId = id,
                    Score = _popularidadPorRestaurante.GetValueOrDefault(id, 4.0f),
                    DeFrio = true
                })
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.RestauranteId)
                .Take(limite)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar recomendaciones de restaurantes para '{UserId}'.", userId);
            return [];
        }
        finally
        {
            _prediccionLock.Release();
        }
    }

    public async Task<bool> ReentrenarAsync(CancellationToken cancellationToken = default)
    {
        if (!EstaHabilitado)
            return false;

        await _cargaLock.WaitAsync(cancellationToken);
        try
        {
            return EntrenarYGuardar();
        }
        finally
        {
            _cargaLock.Release();
        }
    }

    private async Task AsegurarModeloAsync(CancellationToken cancellationToken)
    {
        if (_engine is not null)
            return;

        await _cargaLock.WaitAsync(cancellationToken);
        try
        {
            if (_engine is not null)
                return;

            if (File.Exists(ModelPath))
            {
                if (CargarModelo(ModelPath))
                {
                    CargarVocabularioYMetricasSiFaltan();
                    return;
                }
            }

            if (!_config.GetValue("Recomendacion:TrainOnStartup", true))
                return;

            EntrenarYGuardar();
        }
        finally
        {
            _cargaLock.Release();
        }
    }

    private bool EntrenarYGuardar()
    {
        var dataPath = TrainingDataPath;
        if (!File.Exists(dataPath))
        {
            _logger.LogWarning("No se encontró el dataset de recomendaciones en '{DataPath}'.", dataPath);
            return false;
        }

        try
        {
            var datos = _mlContext.Data.LoadFromTextFile<DatoEntrenamientoRecomendacion>(
                dataPath,
                separatorChar: ',',
                hasHeader: true,
                allowQuoting: true);

            CargarVocabulario(datos);

            var pipeline = ConstruirPipeline();
            var metricas = EvaluarConSeparacion(datos, pipeline);

            var modelo = pipeline.Fit(datos);

            var dir = Path.GetDirectoryName(ModelPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            _mlContext.Model.Save(modelo, datos.Schema, ModelPath);
            CargarModelo(ModelPath);
            UltimasMetricas = metricas;

            _logger.LogInformation(
                "Modelo ML.NET de recomendación entrenado ({Total} interacciones, {Usuarios} usuarios, {Locales} locales, RMSE {Rms:0.000}) y guardado en '{ModelPath}'.",
                metricas.EjemplosTotales,
                metricas.Usuarios,
                metricas.Restaurantes,
                metricas.Rms,
                ModelPath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al entrenar el modelo ML.NET de recomendación.");
            return false;
        }
    }

    private IEstimator<ITransformer> ConstruirPipeline()
    {
        var userIdKey = nameof(DatoEntrenamientoRecomendacion.UserId) + "Key";
        var restIdKey = nameof(DatoEntrenamientoRecomendacion.RestauranteId) + "Key";

        return _mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: userIdKey,
                inputColumnName: nameof(DatoEntrenamientoRecomendacion.UserId))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: restIdKey,
                inputColumnName: nameof(DatoEntrenamientoRecomendacion.RestauranteId)))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                new MatrixFactorizationTrainer.Options
                {
                    MatrixColumnIndexColumnName = userIdKey,
                    MatrixRowIndexColumnName = restIdKey,
                    LabelColumnName = LabelColumn,
                    NumberOfIterations = _config.GetValue("Recomendacion:NumberOfIterations", 30),
                    ApproximationRank = _config.GetValue("Recomendacion:ApproximationRank", 8),
                    LearningRate = _config.GetValue("Recomendacion:LearningRate", 0.05f),
                    Quiet = true
                }));
    }

    private MetricasEntrenamientoRecomendacion EvaluarConSeparacion(IDataView datos, IEstimator<ITransformer> pipeline)
    {
        var total = _mlContext.Data.CreateEnumerable<DatoEntrenamientoRecomendacion>(datos, reuseRowObject: false).Count();
        var metricas = new MetricasEntrenamientoRecomendacion
        {
            EjemplosTotales = total,
            Usuarios = _usuariosConocidos.Count,
            Restaurantes = _restaurantesConocidos.Count,
            FechaEntrenamiento = DateTime.UtcNow
        };

        if (total >= 10)
        {
            // Evaluación con partición separada de entrenamiento (80%) y prueba (20%)
            var split = _mlContext.Data.TrainTestSplit(datos, testFraction: 0.2, seed: 42);
            var modeloValidacion = pipeline.Fit(split.TrainSet);
            var prediccionesTest = modeloValidacion.Transform(split.TestSet);
            var eval = _mlContext.Regression.Evaluate(
                prediccionesTest,
                labelColumnName: LabelColumn,
                scoreColumnName: ScoreColumn);

            var countTrain = _mlContext.Data.CreateEnumerable<DatoEntrenamientoRecomendacion>(split.TrainSet, reuseRowObject: false).Count();
            var countTest = _mlContext.Data.CreateEnumerable<DatoEntrenamientoRecomendacion>(split.TestSet, reuseRowObject: false).Count();

            metricas.EjemplosEntrenamiento = countTrain;
            metricas.EjemplosPrueba = countTest;
            metricas.Rms = double.IsFinite(eval.RootMeanSquaredError) ? Math.Round(eval.RootMeanSquaredError, 4) : null;
            metricas.R2 = double.IsFinite(eval.RSquared) ? Math.Round(eval.RSquared, 4) : null;
        }

        return metricas;
    }

    private void CargarVocabulario(IDataView datos)
    {
        var usuarios = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var restaurantes = new HashSet<int>();
        var suma = new Dictionary<int, double>();
        var conteo = new Dictionary<int, int>();

        foreach (var fila in _mlContext.Data.CreateEnumerable<DatoEntrenamientoRecomendacion>(datos, reuseRowObject: false))
        {
            if (string.IsNullOrWhiteSpace(fila.UserId) || fila.RestauranteId <= 0)
                continue;

            var userKey = fila.UserId.Trim();
            usuarios.Add(userKey);

            var restId = (int)fila.RestauranteId;
            restaurantes.Add(restId);

            suma[restId] = suma.GetValueOrDefault(restId) + fila.Rating;
            conteo[restId] = conteo.GetValueOrDefault(restId) + 1;
        }

        _usuariosConocidos = usuarios;
        _restaurantesConocidos = restaurantes;
        _popularidadPorRestaurante = restaurantes.ToDictionary(
            id => id,
            id => (float)Math.Round(suma[id] / conteo[id], 2));
    }

    private void CargarVocabularioYMetricasSiFaltan()
    {
        if (!File.Exists(TrainingDataPath))
            return;

        try
        {
            var datos = _mlContext.Data.LoadFromTextFile<DatoEntrenamientoRecomendacion>(
                TrainingDataPath,
                separatorChar: ',',
                hasHeader: true,
                allowQuoting: true);

            if (_usuariosConocidos.Count == 0 || _restaurantesConocidos.Count == 0)
                CargarVocabulario(datos);

            UltimasMetricas ??= EvaluarConSeparacion(datos, ConstruirPipeline());
        }
        catch { }
    }

    private bool CargarModelo(string modelPath)
    {
        try
        {
            var modelo = _mlContext.Model.Load(modelPath, out var esquema);
            _engine = _mlContext.Model.CreatePredictionEngine<DatoEntrenamientoRecomendacion, SalidaPrediccionRecomendacion>(
                modelo, esquema);
            return true;
        }
        catch (Exception ex)
        {
            _engine = null;
            _logger.LogWarning(ex, "No se pudo cargar el modelo de recomendación desde '{ModelPath}'.", modelPath);
            return false;
        }
    }

    private string? NormalizarUsuario(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var limpio = userId.Trim();
        return _aliasUsuario.TryGetValue(limpio, out var canonico) ? canonico : limpio;
    }

    private string ResolverRuta(string ruta)
    {
        if (Path.IsPathRooted(ruta))
            return ruta;

        var candidata = Path.Combine(_env.ContentRootPath, ruta);
        if (File.Exists(candidata) || ruta.StartsWith("App_Data", StringComparison.OrdinalIgnoreCase))
            return candidata;

        var alternativa = Path.Combine(AppContext.BaseDirectory, ruta);
        return File.Exists(alternativa) ? alternativa : candidata;
    }

    public void Dispose()
    {
        _engine?.Dispose();
        _cargaLock.Dispose();
        _prediccionLock.Dispose();
    }
}
