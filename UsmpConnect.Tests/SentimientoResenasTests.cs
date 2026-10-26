using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using UsmpConnect.Data.Seed;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Tests;

public class SentimientoResenasTests
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task AnalizarSentimiento_EntrenaGuardaModeloYClasificaPositivoYNegativo()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();

        var servicio = factory.Services.GetRequiredService<IAnalisisSentimientoService>();
        await servicio.InicializarAsync();

        Assert.True(servicio.EstaListo);
        Assert.True(File.Exists(servicio.ModelPath));
        Assert.NotNull(servicio.UltimasMetricas);
        Assert.True(servicio.UltimasMetricas.EjemplosTotales >= 40);

        var pos = await servicio.AnalizarAsync("Excelente sazón casera, atención súper rápida y precios muy económicos.");
        Assert.True(pos.Analizado);
        Assert.Equal(ResultadoSentimiento.EtiquetaPositivo, pos.Etiqueta);
        Assert.True(pos.Probabilidad > 0.5);

        var neg = await servicio.AnalizarAsync("Pésimo servicio, la comida llegó completamente fría, grasosa y sin sabor.");
        Assert.True(neg.Analizado);
        Assert.Equal(ResultadoSentimiento.EtiquetaNegativo, neg.Etiqueta);
        Assert.True(neg.Probabilidad > 0.5);

        var htmlTecnologia = await client.GetStringAsync("/Tecnologia");
        Assert.Contains("ML.NET · Análisis de Sentimiento", htmlTecnologia);
        Assert.Contains("data-ml-sentimiento-metricas", htmlTecnologia);
    }

    [Fact]
    public async Task PublicarResena_ClasificaSentimientoYLoPersisteEnDetalle()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(client, DemoUsers.Alessandro);

        var resp = await client.PostAsJsonAsync("/api/restaurantes/1/resenas", new GuardarResenaRequest
        {
            Puntuacion = 5,
            Comentario = "Excelente sazón, menú universitario muy rico y atención súper rápida."
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var creada = await resp.Content.ReadFromJsonAsync<ResenaDto>(JsonOpts);
        Assert.NotNull(creada);
        Assert.Equal(ResultadoSentimiento.EtiquetaPositivo, creada.Sentimiento);
        Assert.NotNull(creada.ProbabilidadSentimiento);

        var detalle = await client.GetFromJsonAsync<RestauranteDetalleDto>("/api/restaurantes/1", JsonOpts);
        Assert.NotNull(detalle);
        Assert.Contains(detalle.Resenas, r => r.Id == creada.Id && r.Sentimiento == ResultadoSentimiento.EtiquetaPositivo);
    }
}
