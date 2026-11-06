using System.Net;
using Microsoft.Extensions.DependencyInjection;
using UsmpConnect.Data;
using UsmpConnect.Data.Seed;
using UsmpConnect.Services;
using Xunit.Abstractions;

namespace UsmpConnect.Tests;

public class RecomendacionesRestaurantesTests(ITestOutputHelper salida)
{
    [Fact]
    public async Task Recomendador_EntrenaGuardaModeloRecomiendaPersonalizadoYUsaPopularidadEnFrio()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();

        var recomendador = factory.Services.GetRequiredService<IRecomendacionRestaurantesService>();
        Dictionary<int, string> nombres;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await recomendador.SincronizarConBaseDatosAsync(db);
            nombres = db.Restaurantes.ToDictionary(local => local.Id, local => local.Nombre);
        }

        Assert.True(recomendador.EstaListo);
        Assert.True(File.Exists(recomendador.ModelPath));
        Assert.NotNull(recomendador.UltimasMetricas);
        Assert.True(recomendador.UltimasMetricas.EjemplosEntrenamiento > 0);
        Assert.True(recomendador.UltimasMetricas.EjemplosPrueba > 0);

        Assert.Equal(400, recomendador.UltimasMetricas.EjemplosTotales);
        Assert.Equal(nombres.Count, recomendador.UltimasMetricas.Restaurantes);
        var candidatos = nombres.Keys;

        var recAlessandro = await recomendador.RecomendarAsync(DemoUsers.Alessandro, candidatos, topK: 3);
        Assert.Equal(3, recAlessandro.Count);
        Assert.All(recAlessandro, r => Assert.False(r.DeFrio));

        var recMaria = await recomendador.RecomendarAsync(DemoUsers.Maria, candidatos, topK: 3);
        Assert.Equal(3, recMaria.Count);
        Assert.All(recMaria, r => Assert.False(r.DeFrio));
        var coincidentes = recAlessandro.Select(x => x.RestauranteId).Intersect(recMaria.Select(x => x.RestauranteId));
        salida.WriteLine($"Alessandro: {string.Join(", ", recAlessandro.Select(x => $"{nombres[x.RestauranteId]} ({x.Score:F2})"))}");
        salida.WriteLine($"María: {string.Join(", ", recMaria.Select(x => $"{nombres[x.RestauranteId]} ({x.Score:F2})"))}");
        Assert.InRange(coincidentes.Count(), 0, 1);

        var recFrio = await recomendador.RecomendarAsync("usuario.nuevo.sin.historial@usmp.pe", candidatos, topK: 3);
        Assert.Equal(3, recFrio.Count);
        Assert.All(recFrio, r => Assert.True(r.DeFrio));
    }

    [Fact]
    public async Task VistasInicioRestaurantesYTecnologia_MuestranRecomendacionesYMetricas()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();

        // Usuario anónimo recibe opciones populares en frío
        var homeAnon = WebUtility.HtmlDecode(await client.GetStringAsync("/"));
        Assert.Contains("data-recomendacion-home", homeAnon);
        Assert.Contains("MÁS POPULARES", homeAnon);

        // Usuario demo con historial recibe recomendaciones personalizadas
        await DemoFactory.LoginAsync(client, DemoUsers.Alessandro);
        var homeAuth = WebUtility.HtmlDecode(await client.GetStringAsync("/"));
        Assert.Contains("PARA TI HOY", homeAuth);

        var foodSpots = WebUtility.HtmlDecode(await client.GetStringAsync("/Restaurantes"));
        Assert.Contains("data-recomendaciones-restaurantes", foodSpots);
        Assert.Contains("SUGERENCIAS PARA TI", foodSpots);

        var tecnologia = WebUtility.HtmlDecode(await client.GetStringAsync("/Tecnologia"));
        Assert.Contains("ML.NET · Recomendación de Restaurantes", tecnologia);
        Assert.Contains("data-ml-recomendacion-metricas", tecnologia);
    }
}
