using Microsoft.ML;
using Microsoft.ML.Data;

namespace UsmpConnect.Services;

public class DatoEntrenamientoSentimiento
{
    [LoadColumn(0)]
    public string Comentario { get; set; } = string.Empty;

    [LoadColumn(1)]
    public bool Label { get; set; }
}

public class EntradaPrediccionSentimiento
{
    public string Comentario { get; set; } = string.Empty;
}

public class SalidaPrediccionSentimiento
{
    [ColumnName("PredictedLabel")]
    public bool Label { get; set; }

    [ColumnName("Probability")]
    public float Probability { get; set; }
}

public class ResultadoSentimiento
{
    public const string EtiquetaPositivo = "Positivo";
    public const string EtiquetaNegativo = "Negativo";

    public bool Analizado { get; set; }
    public bool EsPositivo { get; set; }
    public double Probabilidad { get; set; }

    public string? Etiqueta => Analizado
        ? (EsPositivo ? EtiquetaPositivo : EtiquetaNegativo)
        : null;

    public DateTime FechaEvaluacion { get; set; } = DateTime.UtcNow;
}

public class MetricasEntrenamientoSentimiento
{
    public int EjemplosTotales { get; set; }
    public int EjemplosEntrenamiento { get; set; }
    public int EjemplosPrueba { get; set; }
    public int Positivos { get; set; }
    public int Negativos { get; set; }
    public double Accuracy { get; set; }
    public double F1Score { get; set; }
    public double? LogLoss { get; set; }
    public DateTime FechaEntrenamiento { get; set; } = DateTime.UtcNow;
}

public interface IAnalisisSentimientoService
{
    bool EstaHabilitado { get; }
    bool EstaListo { get; }
    string Estado { get; }
    string ModelPath { get; }
    MetricasEntrenamientoSentimiento? UltimasMetricas { get; }
    Task InicializarAsync(CancellationToken cancellationToken = default);
    Task<ResultadoSentimiento> AnalizarAsync(string texto, CancellationToken cancellationToken = default);
    Task<bool> ReentrenarAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Entrena y consume el modelo de análisis de sentimiento (ML.NET) sobre Data/ML/comentarios_resenas.csv.
/// Persiste el modelo entrenado en disco (.zip) y lo reutiliza de forma segura para subprocesos.
/// </summary>
public sealed class AnalisisSentimientoService : IAnalisisSentimientoService, IDisposable
{
    private const string LabelColumn = "Label";
    private const string FeaturesColumn = "Features";
    private const string ScoreColumn = "Score";
    private const string ProbabilityColumn = "Probability";

    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<AnalisisSentimientoService> _logger;

    private readonly MLContext _mlContext = new(seed: 42);
    private readonly SemaphoreSlim _cargaLock = new(1, 1);
    private readonly SemaphoreSlim _prediccionLock = new(1, 1);

    private PredictionEngine<EntradaPrediccionSentimiento, SalidaPrediccionSentimiento>? _engine;

    public AnalisisSentimientoService(
        IConfiguration config,
        IHostEnvironment env,
        ILogger<AnalisisSentimientoService> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    public bool EstaHabilitado => _config.GetValue("Sentimiento:Enabled", true);
    public bool EstaListo => _engine is not null;
    public MetricasEntrenamientoSentimiento? UltimasMetricas { get; private set; }

    public string Estado => !EstaHabilitado
        ? "Deshabilitado"
        : (EstaListo
            ? (UltimasMetricas is not null
                ? $"Modelo ML.NET Activo (Accuracy {UltimasMetricas.Accuracy:P0})"
                : "Modelo ML.NET Activo")
            : "Pendiente de inicialización");

    public string ModelPath => ResolverRuta(
        _config["Sentimiento:ModelPath"] ?? "App_Data/sentimiento-resenas.zip");

    private string TrainingDataPath => ResolverRuta(
        _config["Sentimiento:TrainingDataPath"] ?? "Data/ML/comentarios_resenas.csv");

    public Task InicializarAsync(CancellationToken cancellationToken = default)
    {
        if (!EstaHabilitado)
            return Task.CompletedTask;

        return AsegurarModeloAsync(cancellationToken);
    }

    public async Task<ResultadoSentimiento> AnalizarAsync(string texto, CancellationToken cancellationToken = default)
    {
        var resultado = new ResultadoSentimiento { FechaEvaluacion = DateTime.UtcNow };
        if (string.IsNullOrWhiteSpace(texto) || !EstaHabilitado)
            return resultado;

        await AsegurarModeloAsync(cancellationToken);

        var engine = _engine;
        if (engine is null)
            return resultado;

        await _prediccionLock.WaitAsync(cancellationToken);
        try
        {
            var prediccion = engine.Predict(new EntradaPrediccionSentimiento { Comentario = texto.Trim() });
            resultado.Analizado = true;
            resultado.EsPositivo = prediccion.Label;
            var prob = prediccion.Label ? prediccion.Probability : 1f - prediccion.Probability;
            resultado.Probabilidad = Math.Clamp(Math.Round(prob, 4), 0.0, 1.0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al clasificar el sentimiento de la reseña.");
        }
        finally
        {
            _prediccionLock.Release();
        }

        return resultado;
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
                    CalcularMetricasSiFaltan();
                    return;
                }
            }

            if (!_config.GetValue("Sentimiento:TrainOnStartup", true))
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
            _logger.LogWarning("No se encontró el dataset de entrenamiento en '{DataPath}'.", dataPath);
            return false;
        }

        try
        {
            var datos = _mlContext.Data.LoadFromTextFile<DatoEntrenamientoSentimiento>(
                dataPath,
                separatorChar: ',',
                hasHeader: true,
                allowQuoting: true);

            var pipeline = ConstruirPipeline();
            var metricas = EvaluarDataset(datos, pipeline);

            var modelo = pipeline.Fit(datos);

            var dir = Path.GetDirectoryName(ModelPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var esquemaPrediccion = _mlContext.Data
                .LoadFromEnumerable(Array.Empty<EntradaPrediccionSentimiento>()).Schema;
            _mlContext.Model.Save(modelo, esquemaPrediccion, ModelPath);

            CargarModelo(ModelPath);
            UltimasMetricas = metricas;

            _logger.LogInformation(
                "Modelo ML.NET de sentimiento entrenado ({Total} ejemplos, Accuracy {Accuracy:P1}) y guardado en '{ModelPath}'.",
                metricas.EjemplosTotales,
                metricas.Accuracy,
                ModelPath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al entrenar el modelo ML.NET de sentimiento.");
            return false;
        }
    }

    private IEstimator<ITransformer> ConstruirPipeline() =>
        _mlContext.Transforms.Text
            .FeaturizeText(FeaturesColumn, nameof(DatoEntrenamientoSentimiento.Comentario))
            .Append(_mlContext.BinaryClassification.Trainers
                .SdcaLogisticRegression(labelColumnName: LabelColumn, featureColumnName: FeaturesColumn));

    private MetricasEntrenamientoSentimiento EvaluarDataset(IDataView datos, IEstimator<ITransformer> pipeline)
    {
        var metricas = new MetricasEntrenamientoSentimiento { FechaEntrenamiento = DateTime.UtcNow };
        foreach (var fila in _mlContext.Data.CreateEnumerable<DatoEntrenamientoSentimiento>(datos, reuseRowObject: false))
        {
            metricas.EjemplosTotales++;
            if (fila.Label)
                metricas.Positivos++;
            else
                metricas.Negativos++;
        }

        if (metricas.EjemplosTotales >= 10)
        {
            var split = _mlContext.Data.TrainTestSplit(datos, testFraction: 0.2, seed: 42);
            var modeloVal = pipeline.Fit(split.TrainSet);
            var eval = _mlContext.BinaryClassification.Evaluate(
                modeloVal.Transform(split.TestSet),
                labelColumnName: LabelColumn,
                scoreColumnName: ScoreColumn,
                probabilityColumnName: ProbabilityColumn);

            metricas.EjemplosEntrenamiento = (int)(split.TrainSet.GetRowCount() ?? (metricas.EjemplosTotales - 12));
            metricas.EjemplosPrueba = (int)(split.TestSet.GetRowCount() ?? 12);
            metricas.Accuracy = Math.Round(eval.Accuracy, 4);
            metricas.F1Score = Math.Round(eval.F1Score, 4);
            metricas.LogLoss = Math.Round(eval.LogLoss, 4);
        }

        return metricas;
    }

    private void CalcularMetricasSiFaltan()
    {
        if (UltimasMetricas is not null || !File.Exists(TrainingDataPath))
            return;

        try
        {
            var datos = _mlContext.Data.LoadFromTextFile<DatoEntrenamientoSentimiento>(
                TrainingDataPath,
                separatorChar: ',',
                hasHeader: true,
                allowQuoting: true);
            UltimasMetricas = EvaluarDataset(datos, ConstruirPipeline());
        }
        catch { }
    }

    private bool CargarModelo(string modelPath)
    {
        try
        {
            var modelo = _mlContext.Model.Load(modelPath, out var esquema);
            _engine = _mlContext.Model.CreatePredictionEngine<EntradaPrediccionSentimiento, SalidaPrediccionSentimiento>(
                modelo, esquema);
            return true;
        }
        catch (Exception ex)
        {
            _engine = null;
            _logger.LogWarning(ex, "No se pudo cargar el modelo ML.NET desde '{ModelPath}'.", modelPath);
            return false;
        }
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
