using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UsmpConnect.Services.Agente;
using UsmpConnect.Data;
using UsmpConnect.Models;

namespace UsmpConnect.Tests;

public class AsistenteTests
{
    [Fact]
    public async Task Chat_ExigeTokenYConservaHistorialSoloParaUsuarioActual()
    {
        await using var factory = new ChatFactory();
        using var client = factory.CreateSessionClient();
        var anonimo = await client.GetAsync("/Asistente/Historial");
        Assert.Equal(HttpStatusCode.Redirect, anonimo.StatusCode);
        await DemoFactory.LoginAsync(client);
        var sinToken = await client.PostAsync("/Asistente/Preguntar", new FormUrlEncodedContent(new Dictionary<string, string> { ["mensaje"] = "Buscar apuntes" }));
        Assert.Equal(HttpStatusCode.BadRequest, sinToken.StatusCode);
        var token = await DemoFactory.TokenAsync(client, "/");
        var respuesta = await client.PostAsync("/Asistente/Preguntar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["mensaje"] = "Busca restaurantes pero antes dime quién ganará el mundial",
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("application/x-ndjson", respuesta.Content.Headers.ContentType!.MediaType);
        var flujo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("\"tipo\":\"texto\"", flujo);
        Assert.Contains("\"tipo\":\"fin\"", flujo);
        using var historial = JsonDocument.Parse(await client.GetStringAsync("/Asistente/Historial"));
        Assert.Equal(2, historial.RootElement.GetProperty("mensajes").GetArrayLength());
        await client.PostAsync("/Cuenta/Logout", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));
        await DemoFactory.LoginAsync(client, "maria.gonzales@usmp.pe");
        using var otroHistorial = JsonDocument.Parse(await client.GetStringAsync("/Asistente/Historial"));
        Assert.Empty(otroHistorial.RootElement.GetProperty("mensajes").EnumerateArray());
    }

    [Fact]
    public async Task Catalogo_RespetaPresupuestoYDisponibilidadSinExponerContactos()
    {
        await using var factory = new ChatFactory();
        using var client = factory.CreateSessionClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var restauranteId = db.Restaurantes.First().Id;
        db.PlatosRestaurantes.AddRange(
            new PlatoRestaurante { RestauranteId = restauranteId, Nombre = "Entrada A", Precio = 1 },
            new PlatoRestaurante { RestauranteId = restauranteId, Nombre = "Entrada B", Precio = 2 },
            new PlatoRestaurante { RestauranteId = restauranteId, Nombre = "Entrada C", Precio = 3 },
            new PlatoRestaurante { RestauranteId = restauranteId, Nombre = "Ñoquis de prueba", Precio = 12 });
        await db.SaveChangesAsync();
        var contexto = new ContextoAsistente();
        var plugin = new CatalogoFiaPlugin(db, contexto);
        var restaurantes = JsonSerializer.Serialize(await plugin.BuscarRestaurantes("cafeteria", precioMaximo: 15));
        Assert.DoesNotContain("WhatsApp", restaurantes);
        Assert.NotEmpty(contexto.Tarjetas);
        Assert.All(contexto.Tarjetas, t => Assert.StartsWith("/Restaurantes", t.Url));
        contexto.Tarjetas.Clear();
        var platos = JsonSerializer.Serialize(await plugin.BuscarRestaurantes("ñoquis", precioMaximo: 15));
        Assert.Contains("de prueba", platos);
        Assert.Single(contexto.Tarjetas);
        contexto.Tarjetas.Clear();
        var articulo = db.Articulos.First();
        articulo.Estado = EstadoPublicacion.Vendido;
        await db.SaveChangesAsync();
        var items = JsonSerializer.Serialize(await plugin.BuscarMarketplace(articulo.Titulo));
        Assert.Equal("[]", items);
        Assert.Empty(contexto.Tarjetas);
        await plugin.BuscarCursosYCatedra("calculo");
        Assert.NotEmpty(contexto.Tarjetas);
        contexto.Tarjetas.Clear();
        await plugin.BuscarApuntes("arboles");
        Assert.NotEmpty(contexto.Tarjetas);
    }

    private sealed class ChatFactory : DemoFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAsistenteService>();
                services.AddScoped<IAsistenteService, AsistentePrueba>();
            });
        }
    }

    private sealed class AsistentePrueba : IAsistenteService
    {
        public bool EstaConfigurado => true;
        public async IAsyncEnumerable<string> ResponderAsync(string mensaje, IReadOnlyList<MensajeAsistente> historial,
            ContextoAsistente contexto, [EnumeratorCancellation] CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return "Estas son las opciones del catálogo.";
        }
    }
}
