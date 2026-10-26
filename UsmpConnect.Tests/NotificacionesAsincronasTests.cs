using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using UsmpConnect.Data;
using UsmpConnect.Services;

namespace UsmpConnect.Tests;

public class NotificacionesAsincronasTests
{
    [Fact]
    public async Task Resenar_EncolaConsumePersisteNotificacionYRenderizaCampanitaEnLayout()
    {
        var spyPieHost = new SpyPieHostService();
        await using var factory = new NotiFactory(spyPieHost);
        using var client = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(client);

        var token = await DemoFactory.TokenAsync(client, "/Restaurantes");
        var resp = await client.PostAsync("/Restaurantes/Resenar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["RestauranteId"] = "1",
            ["Puntuacion"] = "5",
            ["Comentario"] = "Excelente sazón y atención rápida para el almuerzo.",
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);

        // Esperar brevemente al consumidor en segundo plano
        var procesada = await spyPieHost.EsperarEventoAsync(TimeSpan.FromSeconds(5));
        Assert.True(procesada);
        Assert.Equal("nueva_resena", spyPieHost.UltimoEvento);
        Assert.DoesNotContain("UsuarioId", spyPieHost.UltimosDatos);
        Assert.DoesNotContain("MensajeNotificacion", spyPieHost.UltimosDatos);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notis = await db.Notificaciones.AsNoTracking().ToListAsync();
            Assert.Contains(notis, n => n.Icono == "star-fill" && n.Mensaje.Contains("fue procesada y publicada"));
        }

        var html = await client.GetStringAsync("/");
        Assert.Contains("id=\"notificaciones\"", html);
        Assert.Contains("fue procesada y publicada", html);
        var resumen = await client.GetStringAsync("/Notificaciones/Resumen");
        Assert.Contains("fue procesada y publicada", resumen);
        using var otroClient = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(otroClient, "maria.gonzales@usmp.pe");
        var resumenAjeno = await otroClient.GetStringAsync("/Notificaciones/Resumen");
        Assert.DoesNotContain("fue procesada y publicada", resumenAjeno);
        using var anonimo = factory.CreateSessionClient();
        var sinSesion = await anonimo.GetAsync("/Notificaciones/Resumen");
        Assert.Equal(HttpStatusCode.Redirect, sinSesion.StatusCode);
    }

    [Fact]
    public async Task ProcesarSobreAsync_EsIdempotenteAnteReentregas()
    {
        var spyPieHost = new SpyPieHostService();
        await using var factory = new NotiFactory(spyPieHost);
        using var client = factory.CreateSessionClient(); // inicializa el host y seed

        string userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            userId = (await db.Users.AsNoTracking().FirstAsync()).Id;
        }

        var consumidor = new ConsumidorNotificacionesService(
            factory.Services.GetRequiredService<IColaMensajes>(),
            factory.Services.GetRequiredService<IServiceScopeFactory>(),
            factory.Services.GetRequiredService<IConfiguration>(),
            NullLogger<ConsumidorNotificacionesService>.Instance);

        var evento = new EventoResenaCola(999, 1, "Cafetería Central FIA", userId, "Alessandro Morales", 5, 4.9, 12, DateTime.UtcNow);
        var envelope = new MensajeCola(nameof(EventoResenaCola), JsonSerializer.Serialize(evento), DateTime.UtcNow);

        await consumidor.ProcesarSobreAsync(envelope);
        await consumidor.ProcesarSobreAsync(envelope);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var count = await db.Notificaciones.CountAsync(n => n.UsuarioId == userId && n.Mensaje.Contains("#999"));
            Assert.Equal(1, count);
        }
    }

    private sealed class NotiFactory(IPieHostService pieHost) : DemoFactory
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPieHostService>();
                services.AddSingleton(pieHost);
            });
        }
    }

    private sealed class SpyPieHostService : IPieHostService
    {
        private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string? UltimoEvento { get; private set; }
        public string UltimosDatos { get; private set; } = "";
        public string? WebSocketClientUrl => null;
        public bool IsConfigured => true;
        public string Estado => "Spy";

        public Task PublicarEventoAsync(string canal, string evento, object datos)
        {
            UltimoEvento = evento;
            UltimosDatos = JsonSerializer.Serialize(datos);
            _tcs.TrySetResult(true);
            return Task.CompletedTask;
        }

        public async Task<bool> EsperarEventoAsync(TimeSpan timeout)
        {
            var completed = await Task.WhenAny(_tcs.Task, Task.Delay(timeout));
            return completed == _tcs.Task && await _tcs.Task;
        }
    }
}
