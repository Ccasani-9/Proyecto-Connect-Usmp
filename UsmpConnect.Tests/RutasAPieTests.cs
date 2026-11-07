using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UsmpConnect.Data;
using UsmpConnect.Services;

namespace UsmpConnect.Tests;

public class RutasAPieTests
{
    [Fact]
    public async Task Rutas_FiltraLaListaYLasSugerenciasYConservaPresupuestoSiFallaGoogle()
    {
        using var google = new TransporteRoutes();
        await using var factory = new RutasFactory(google);
        using var client = factory.CreateSessionClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var locales = db.Restaurantes.OrderBy(r => r.Id).Take(4).ToArray();
        for (var i = 0; i < locales.Length; i++)
        {
            locales[i].Nombre = $"RutaPrueba {i}";
            locales[i].Latitud = RutasAPieService.LatitudFia + (i + 1) * 0.001;
            locales[i].Longitud = RutasAPieService.LongitudFia;
            locales[i].PrecioPromedioReferencial = i == 3 ? 20 : 12;
        }
        await db.SaveChangesAsync();
        await scope.ServiceProvider.GetRequiredService<CatalogoRestaurantesService>().InvalidarAsync();

        const string consulta = "/Restaurantes?q=RutaPrueba&gastoMaximo=15&minutosMaximos=8";
        await client.GetStringAsync(consulta);
        Assert.Equal(0, google.Peticiones);

        await DemoFactory.LoginAsync(client);
        var filtrada = await client.GetStringAsync(consulta);
        Assert.Equal(1, google.Peticiones);
        Assert.Contains("7 min a pie", filtrada);
        Assert.Contains($"id=\"card-rest-{locales[0].Id}\"", filtrada);
        foreach (var excluido in locales.Skip(1))
            Assert.DoesNotContain($"id=\"card-rest-{excluido.Id}\"", filtrada);
        var sugerencias = filtrada.Split("data-recomendaciones-restaurantes>")[1].Split("<!-- Grilla de Restaurantes -->")[0];
        Assert.Contains("RutaPrueba 0", sugerencias);
        Assert.DoesNotContain("RutaPrueba 1", sugerencias);
        Assert.DoesNotContain("RutaPrueba 2", sugerencias);
        Assert.DoesNotContain("RutaPrueba 3", sugerencias);
        Assert.DoesNotContain("solo-clave-prueba", filtrada);

        google.Fallar = true;
        var respaldo = await client.GetStringAsync(consulta);
        Assert.Contains("Se mantienen los demás filtros", WebUtility.HtmlDecode(respaldo));
        Assert.Contains($"id=\"card-rest-{locales[1].Id}\"", respaldo);
        Assert.DoesNotContain($"id=\"card-rest-{locales[3].Id}\"", respaldo);
    }

    private sealed class RutasFactory(TransporteRoutes google) : DemoFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["Routes:ApiKey"] = "solo-clave-prueba" }));
            builder.ConfigureServices(services => services.AddHttpClient(RutasAPieService.Cliente)
                .ConfigurePrimaryHttpMessageHandler(() => google));
        }
    }

    private sealed class TransporteRoutes : HttpMessageHandler
    {
        public int Peticiones { get; private set; }
        public bool Fallar { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Peticiones++;
            Assert.Equal("routes.googleapis.com", request.RequestUri!.Host);
            Assert.Equal("solo-clave-prueba", Assert.Single(request.Headers.GetValues("X-Goog-Api-Key")));
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            Assert.Equal("WALK", json.RootElement.GetProperty("travelMode").GetString());
            Assert.Single(json.RootElement.GetProperty("origins").EnumerateArray());
            Assert.Equal(3, json.RootElement.GetProperty("destinations").GetArrayLength());
            if (Fallar) return new(HttpStatusCode.ServiceUnavailable);
            return new(HttpStatusCode.OK) { Content = new StringContent("""
                [
                  {"destinationIndex":2,"status":{"code":5},"condition":"ROUTE_NOT_FOUND"},
                  {"destinationIndex":1,"status":{},"condition":"ROUTE_EXISTS","duration":"541s"},
                  {"status":{},"condition":"ROUTE_EXISTS","duration":"390s"}
                ]
                """) };
        }
    }
}
