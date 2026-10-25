using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UsmpConnect.Controllers;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Tests;

public class RestaurantesCacheTests
{
    [Fact]
    public async Task PrecargaHabilitada_SeEjecutaDuranteElArranque()
    {
        await using var factory = new WarmupFactory();
        using var client = factory.CreateSessionClient();
        var cache = factory.Services.GetRequiredService<IRedisCacheService>();
        var items = await cache.GetAsync<List<RestauranteItemVM>>(CatalogoRestaurantesService.CacheKey);
        Assert.NotNull(items);
        Assert.NotEmpty(items);
    }

    [RedisFact]
    [Trait("Category", "Redis")]
    public async Task ResenarConRedisReal_ActualizaTodosLosFiltros()
    {
        await using var factory = new RedisFactory();
        using var client = factory.CreateSessionClient();
        var cache = factory.Services.GetRequiredService<IRedisCacheService>();
        Assert.True(cache.IsConnected);
        await cache.RemoveAsync(CatalogoRestaurantesService.CacheKey);
        try { await VerificarResenaAsync(factory); }
        finally { await cache.RemoveAsync(CatalogoRestaurantesService.CacheKey); }
    }

    [Fact]
    public async Task CatalogoCompartido_SeReutilizaHastaInvalidar()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        await CatalogoAsync(factory);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Restaurantes.Single(r => r.Id == 1).Nombre = "Nombre cambiado sin invalidar";
            await db.SaveChangesAsync();
        }
        Assert.Empty((await CatalogoAsync(factory, q: "Nombre cambiado sin invalidar")).Restaurantes);
        using (var scope = factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<CatalogoRestaurantesService>().InvalidarAsync();
        Assert.Single((await CatalogoAsync(factory, q: "Nombre cambiado sin invalidar")).Restaurantes);
    }

    [Fact]
    public async Task PrecargaOpcional_PueblaLaMismaClaveQueUsaElListado()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
        Assert.Null(await cache.GetAsync<List<RestauranteItemVM>>(CatalogoRestaurantesService.CacheKey));
        await scope.ServiceProvider.GetRequiredService<CatalogoRestaurantesService>().PrecalentarCacheAsync();
        var items = await cache.GetAsync<List<RestauranteItemVM>>(CatalogoRestaurantesService.CacheKey);
        Assert.NotNull(items);
        Assert.NotEmpty(items);
        Assert.Equal(items.Count, (await CatalogoAsync(factory)).Restaurantes.Count);
    }

    [Fact]
    public async Task Resenar_ActualizaListadoGeneralCategoriaYBusqueda()
    {
        await using var factory = new DemoFactory();
        await VerificarResenaAsync(factory);
    }

    private static async Task VerificarResenaAsync(DemoFactory factory)
    {
        using var client = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(client);
        CategoriaRestaurante categoria;
        string nombre;
        using (var scope = factory.Services.CreateScope())
        {
            var r = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Restaurantes.Single(r => r.Id == 1);
            categoria = r.Categoria;
            nombre = r.Nombre;
        }
        var before = (await CatalogoAsync(factory)).Restaurantes.Single(r => r.Id == 1);
        await CatalogoAsync(factory, categoria);
        await CatalogoAsync(factory, q: nombre);
        await CatalogoAsync(factory, categoria, nombre);
        var token = await DemoFactory.TokenAsync(client, "/Restaurantes");
        var response = await client.PostAsync("/Restaurantes/Resenar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["RestauranteId"] = "1", ["Puntuacion"] = "1", ["Comentario"] = "Una reseña nueva para comprobar la caché",
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        foreach (var vm in new[] { await CatalogoAsync(factory), await CatalogoAsync(factory, categoria),
                     await CatalogoAsync(factory, q: nombre), await CatalogoAsync(factory, categoria, nombre) })
        {
            var after = vm.Restaurantes.Single(r => r.Id == 1);
            Assert.Equal(before.TotalResenas + 1, after.TotalResenas);
            Assert.Equal((await DetalleAsync(client)).Calificacion, after.CalificacionPromedio);
        }
    }

    [Fact]
    public async Task BuscarPalabraAll_NoReutilizaPorErrorElListadoSinFiltros()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        var todos = (await CatalogoAsync(factory)).Restaurantes;
        var filtrados = (await CatalogoAsync(factory, q: "all")).Restaurantes;
        var expected = todos.Where(r => r.Nombre.Contains("all", StringComparison.OrdinalIgnoreCase)
            || r.Direccion.Contains("all", StringComparison.OrdinalIgnoreCase)
            || r.Descripcion.Contains("all", StringComparison.OrdinalIgnoreCase)).Select(r => r.Id);
        Assert.Equal(expected, filtrados.Select(r => r.Id));
    }

    private static async Task<FoodSpotsIndexVM> CatalogoAsync(DemoFactory factory, CategoriaRestaurante? categoria = null, string? q = null)
    {
        using var scope = factory.Services.CreateScope();
        var controller = ActivatorUtilities.CreateInstance<FoodSpotsController>(scope.ServiceProvider);
        return Assert.IsType<FoodSpotsIndexVM>(Assert.IsType<ViewResult>(await controller.Index(categoria, q)).Model);
    }

    private static async Task<(double Calificacion, int Total)> DetalleAsync(HttpClient client)
    {
        using var json = System.Text.Json.JsonDocument.Parse(await client.GetStringAsync("/Restaurantes/Detalle/1"));
        return (json.RootElement.GetProperty("calificacionPromedio").GetDouble(), json.RootElement.GetProperty("totalResenas").GetInt32());
    }

    private sealed class WarmupFactory : DemoFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:WarmupOnStartup"] = "true"
            }));
        }
    }

    private sealed class RedisFactory : DemoFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:ConnectionString"] = Environment.GetEnvironmentVariable("USMP_TEST_REDIS")
            }));
        }
    }
}
