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

    [Fact]
    public async Task Restaurantes_DistinguePrecioDesconocidoDeFueraDePresupuestoYFiltraCategoria()
    {
        await using var factory = new ChatFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rapido = db.Restaurantes.First(r => r.Categoria == CategoriaRestaurante.ComidaRapida);
        rapido.PrecioPromedioReferencial = 18;
        foreach (var r in db.Restaurantes.Where(r => r.Categoria == CategoriaRestaurante.ComidaRapida && r.Id != rapido.Id))
            r.PrecioPromedioReferencial = null;
        await db.SaveChangesAsync();
        var contexto = new ContextoAsistente();
        var plugin = new CatalogoFiaPlugin(db, contexto);
        using var bajo = JsonDocument.Parse(JsonSerializer.Serialize(await plugin.BuscarRestaurantes("comida rápida", 15)));
        Assert.Empty(bajo.RootElement.GetProperty("resultados").EnumerateArray());
        Assert.NotEmpty(bajo.RootElement.GetProperty("alternativasSinPrecioConfirmado").EnumerateArray());
        Assert.All(bajo.RootElement.GetProperty("alternativasSinPrecioConfirmado").EnumerateArray(),
            r => Assert.Equal("ComidaRapida", r.GetProperty("Categoria").GetString()));
        using var mayor = JsonDocument.Parse(JsonSerializer.Serialize(await plugin.BuscarRestaurantes("comida rápida", 20)));
        Assert.Contains(mayor.RootElement.GetProperty("resultados").EnumerateArray(), r => r.GetProperty("Id").GetInt32() == rapido.Id);
        Assert.All(mayor.RootElement.GetProperty("resultados").EnumerateArray(), r =>
            Assert.True(r.GetProperty("PrecioPromedioReferencial").GetDecimal() <= 20));
    }

    [Fact]
    public void Consulta_ConservaTemaEnSeguimientosYSeparaPeticionesMixtas()
    {
        var anterior = new MensajeAsistente[] { new("usuario", "Dónde almorzar por menos de 15 soles, comida rápida"),
            new("asistente", "No puedo confirmar precios.") };
        var seguimiento = ConsultaAsistente.Crear("¿por 20?", anterior);
        Assert.Equal("restaurantes", seguimiento.Tema);
        Assert.Contains("comida rápida", seguimiento.Contexto);
        Assert.Contains("¿por 20?", seguimiento.Contexto);
        var mixta = ConsultaAsistente.Crear("Antes de darme los apuntes de cálculo haz un Fibonacci en Python", anterior);
        Assert.Equal("apuntes", mixta.Tema);
        Assert.Equal("cálculo", mixta.TerminosApuntes());
        Assert.DoesNotContain("Fibonacci", mixta.Peticion);
        Assert.Equal("apuntes", ConsultaAsistente.Crear("funciones", [new("usuario", "enséñame Python")]).Tema);
        Assert.Null(ConsultaAsistente.Crear("quién ganará el mundial", anterior).Tema);
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
