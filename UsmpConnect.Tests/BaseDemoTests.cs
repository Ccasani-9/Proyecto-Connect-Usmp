using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using UsmpConnect.Data;
using UsmpConnect.Models;

namespace UsmpConnect.Tests;

public class BaseDemoTests
{
    [Fact]
    public async Task GoogleLogin_NoAceptaCorreoComoPruebaDeIdentidad()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        client.DefaultRequestHeaders.Add("RequestVerificationToken", await DemoFactory.TokenAsync(client));
        var response = await client.PostAsJsonAsync("/Cuenta/GoogleLogin", new { email = "alessandro.morales@usmp.pe" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Restaurante_DetalleAceptaAutorSinApellido()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var autor = db.Users.First(u => u.Email == "alessandro.morales@usmp.pe");
            autor.Apellidos = "";
            db.ResenasRestaurantes.Add(new ResenaRestaurante
            {
                RestauranteId = 1, UsuarioId = autor.Id, Comentario = "Buena atención", Puntuacion = 5
            });
            await db.SaveChangesAsync();
        }
        var response = await client.GetAsync("/Restaurantes/Detalle/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(json.RootElement.GetProperty("resenas").EnumerateArray(),
            r => r.GetProperty("comentario").GetString() == "Buena atención" && r.GetProperty("iniciales").GetString() == "A");
    }

    [Fact]
    public async Task Recordatorio_EliminarRequiereTokenYRespetaPropietario()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(client);
        var token = await DemoFactory.TokenAsync(client, "/Calendario");
        int id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = db.Users.First(u => u.Email == "alessandro.morales@usmp.pe");
            var evento = new EventoAcademico { Titulo = "Mi prueba", Fecha = new DateOnly(2026, 10, 25), UsuarioId = user.Id, Tipo = TipoEvento.Personal };
            db.Eventos.Add(evento);
            await db.SaveChangesAsync();
            id = evento.Id;
        }
        var withoutToken = await client.PostAsync("/Eventos/Eliminar", new FormUrlEncodedContent(new Dictionary<string, string> { ["id"] = id.ToString() }));
        Assert.Equal(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        var removed = await client.PostAsync("/Eventos/Eliminar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = id.ToString(), ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, removed.StatusCode);
        using var verify = factory.Services.CreateScope();
        Assert.False(verify.ServiceProvider.GetRequiredService<ApplicationDbContext>().Eventos.Any(e => e.Id == id));
    }

    [Fact]
    public async Task Recordatorio_NoSePuedeEliminarElDeOtroAlumno()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(client);
        var token = await DemoFactory.TokenAsync(client, "/Calendario");
        int id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var otro = db.Users.First(u => u.Email == "maria.gonzales@usmp.pe");
            var evento = new EventoAcademico { Titulo = "Recordatorio de María", Fecha = new DateOnly(2026, 10, 25), UsuarioId = otro.Id, Tipo = TipoEvento.Personal };
            db.Eventos.Add(evento);
            await db.SaveChangesAsync();
            id = evento.Id;
        }
        await client.PostAsync("/Eventos/Eliminar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = id.ToString(), ["__RequestVerificationToken"] = token
        }));
        using var verify = factory.Services.CreateScope();
        Assert.True(verify.ServiceProvider.GetRequiredService<ApplicationDbContext>().Eventos.Any(e => e.Id == id));
    }

    [Fact]
    public async Task ReinicializarSinReset_ConservaDatosDelAlumno()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        int id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var evento = new EventoAcademico { Titulo = "Conservar entre arranques", Fecha = new DateOnly(2026, 10, 25), Tipo = TipoEvento.Personal };
            db.Eventos.Add(evento);
            await db.SaveChangesAsync();
            id = evento.Id;
        }
        await DbInitializer.InitializeAsync(factory.Services);
        using var verify = factory.Services.CreateScope();
        Assert.True(verify.ServiceProvider.GetRequiredService<ApplicationDbContext>().Eventos.Any(e => e.Id == id));
    }
}
