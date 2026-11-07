using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.ML;
using UsmpConnect.Data;
using UsmpConnect.Data.Seed;
using UsmpConnect.Services;
using Xunit.Abstractions;

namespace UsmpConnect.Tests;

public class EvaluacionRecomendadorTests(ITestOutputHelper salida)
{
    [Fact]
    public async Task ComparaTopTresContraPopularidadConValoracionesReservadas()
    {
        await using var factory = new DemoFactory();
        using var scope = factory.Services.CreateScope();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var nombres = await db.Restaurantes.ToDictionaryAsync(r => r.Id, r => r.Nombre);
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var ml = new MLContext();
        var csv = Path.Combine(env.ContentRootPath, "Data", "ML", "interacciones_restaurantes.csv");
        var datos = ml.Data.CreateEnumerable<DatoEntrenamientoRecomendacion>(
            ml.Data.LoadFromTextFile<DatoEntrenamientoRecomendacion>(csv, ',', hasHeader: true), false).ToArray();

        var pasta = nombres.Single(r => r.Value == "Punto Italiano (La Fontana)").Key;
        var aleatorio = new Random(42);
        var reservadas = datos.GroupBy(r => r.UserId).OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => g.Where(r => r.Rating > 4.5f).OrderBy(r => r.RestauranteId).ToArray())
            .Where(g => g.Length >= 2)
            .Select(g => g[0].UserId == DemoUsers.Diego
                ? g.Single(r => r.RestauranteId == pasta)
                : g[aleatorio.Next(g.Length)])
            .ToArray();
        var entrenamiento = datos.Except(reservadas).ToArray();
        var catalogo = entrenamiento.Select(r => (int)r.RestauranteId).Distinct().ToArray();
        Assert.NotEmpty(reservadas);
        Assert.All(reservadas, r => Assert.Contains((int)r.RestauranteId, catalogo));

        var csvTemporal = Path.GetTempFileName();
        var modeloTemporal = Path.ChangeExtension(csvTemporal, ".zip");
        try
        {
            await File.WriteAllLinesAsync(csvTemporal, new[] { "UserId,RestauranteId,Rating" }.Concat(
                entrenamiento.Select(r => FormattableString.Invariant($"{r.UserId},{r.RestauranteId},{r.Rating}"))));
            var configEvaluacion = new ConfigurationBuilder().AddConfiguration(config)
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Recomendacion:TrainingDataPath"] = csvTemporal,
                    ["Recomendacion:ModelPath"] = modeloTemporal
                }).Build();
            using var servicio = new RecomendacionRestaurantesService(configEvaluacion, env,
                NullLogger<RecomendacionRestaurantesService>.Instance);
            await servicio.InicializarAsync();
            Assert.True(servicio.EstaListo);

            var aciertosModelo = 0;
            var aciertosPopularidad = 0;
            foreach (var objetivo in reservadas)
            {
                var vistos = entrenamiento.Where(r => r.UserId == objetivo.UserId)
                    .Select(r => (int)r.RestauranteId).ToHashSet();
                var candidatos = catalogo.Where(id => !vistos.Contains(id)).ToArray();
                var personalizado = await servicio.RecomendarAsync(objetivo.UserId, candidatos, candidatos.Length);
                var popular = await servicio.RecomendarAsync(null, candidatos, candidatos.Length);
                Assert.Equal(candidatos.Length, personalizado.Count);
                Assert.Equal(candidatos.Length, popular.Count);
                Assert.All(personalizado, r => Assert.False(r.DeFrio));
                Assert.All(popular, r => Assert.True(r.DeFrio));

                var puestoModelo = personalizado.FindIndex(r => r.RestauranteId == objetivo.RestauranteId) + 1;
                var puestoPopular = popular.FindIndex(r => r.RestauranteId == objetivo.RestauranteId) + 1;
                Assert.InRange(puestoModelo, 1, candidatos.Length);
                Assert.InRange(puestoPopular, 1, candidatos.Length);
                if (puestoModelo <= 3) aciertosModelo++;
                if (puestoPopular <= 3) aciertosPopularidad++;

                if (objetivo.UserId == DemoUsers.Diego)
                {
                    var pizzerias = nombres.Where(r => r.Value is "Pizza Raúl La Fontana" or "Mamma Tomato").ToArray();
                    Assert.Equal(2, pizzerias.Length);
                    Assert.All(pizzerias, r => Assert.Contains(entrenamiento,
                        fila => fila.UserId == objetivo.UserId && fila.RestauranteId == r.Key && fila.Rating == 5));
                    salida.WriteLine($"Diego: {nombres[(int)objetivo.RestauranteId]} quedó en puesto {puestoModelo} con ML y {puestoPopular} por popularidad (de {candidatos.Length}).");
                    salida.WriteLine($"Top 3 de Diego con ML: {string.Join(", ", personalizado.Take(3).Select(r => nombres[r.RestauranteId]))}.");
                }
            }

            salida.WriteLine($"Aciertos en top 3: ML {aciertosModelo}/{reservadas.Length}, popularidad {aciertosPopularidad}/{reservadas.Length}.");
        }
        finally
        {
            File.Delete(csvTemporal);
            File.Delete(modeloTemporal);
        }
    }
}
