using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UsmpConnect.Data;
using UsmpConnect.Models;

namespace UsmpConnect.Tests;

public class PresentacionFinalTests
{
    [Fact]
    public async Task CatedraYApuntes_PublicarVotarCalificarYDescargar()
    {
        await using var factory = new DemoFactory();
        using var autor = factory.CreateSessionClient();
        using var lector = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(autor);
        await DemoFactory.LoginAsync(lector, "maria.gonzales@usmp.pe");
        var token = await DemoFactory.TokenAsync(autor, "/Catedra");
        var aporte = await autor.PostAsync("/Catedra/Aportar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["CursoId"] = "2", ["Texto"] = "Repasa los ejercicios de límites y revisa cada paso antes de entregar.",
            ["Dificultad"] = "3", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, aporte.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tip = await db.Tips.SingleAsync(t => t.Texto.StartsWith("Repasa los ejercicios de límites"));
        token = await DemoFactory.TokenAsync(lector, "/Catedra/Curso/2");
        var voto = await lector.PostAsync("/Catedra/Votar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = tip.Id.ToString(), ["valor"] = "1", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.OK, voto.StatusCode);
        await db.Entry(tip).ReloadAsync();
        Assert.Equal(1, tip.Utiles);
        var apunte = await db.Apuntes.FirstAsync(a => a.NombreArchivo == "demo-python-funciones.pdf");
        // Alessandro califica un recurso de María y luego modifica su valoración.
        token = await DemoFactory.TokenAsync(autor, "/Apuntes");
        foreach (var estrellas in new[] { "5", "2" })
        {
            var nota = await autor.PostAsync("/Apuntes/Calificar", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["id"] = apunte.Id.ToString(), ["estrellas"] = estrellas, ["__RequestVerificationToken"] = token
            }));
            Assert.Equal(HttpStatusCode.Redirect, nota.StatusCode);
        }
        await db.Entry(apunte).ReloadAsync();
        Assert.Equal(1, apunte.NumCalificaciones);
        Assert.Equal(2, apunte.SumaEstrellas);
        var descarga = await autor.GetAsync($"/Apuntes/Descargar/{apunte.Id}");
        Assert.Equal(HttpStatusCode.OK, descarga.StatusCode);
        Assert.Equal("application/pdf", descarga.Content.Headers.ContentType!.MediaType);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString((await descarga.Content.ReadAsByteArrayAsync())[..4]));
        await db.Entry(apunte).ReloadAsync();
        Assert.Equal(1, apunte.Descargas);
    }

    [Fact]
    public async Task AccesoConFormularioVencido_PermiteRenovarloSinDesactivarAntiforgery()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        var login = await client.PostAsync("/Cuenta/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "alessandro.morales@usmp.pe", ["Password"] = "Usmp2026!"
        }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Contains("aviso=formulario-vencido", login.Headers.Location!.OriginalString);
        var renovado = await client.GetStringAsync(login.Headers.Location);
        Assert.Contains("El formulario de acceso venció", renovado);
        var google = await client.PostAsync("/Cuenta/GoogleLogin", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, google.StatusCode);
        Assert.Contains("\"recargar\":true", await google.Content.ReadAsStringAsync());
        await DemoFactory.LoginAsync(client);
        var logout = await client.PostAsync("/Cuenta/Logout", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Calendario")).StatusCode);
        var token = await DemoFactory.TokenAsync(client, "/");
        await client.PostAsync("/Cuenta/Logout", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Calendario")).StatusCode);
    }

    [Fact]
    public async Task FotosDemo_CarganYEliminarUnReporteNoBorraLaFotografiaCompartida()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(client, "maria.gonzales@usmp.pe");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var fotos = await db.Articulos.Select(a => a.ImagenUrl).ToListAsync();
        Assert.All(fotos, foto => Assert.StartsWith("/images/demo/", foto));
        foreach (var foto in fotos.Distinct())
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(foto)).StatusCode);
        var reporte = await db.ObjetosPerdidos.SingleAsync(o => o.Titulo == "Audífonos Sony WH-1000XM4");
        var token = await DemoFactory.TokenAsync(client, "/ObjetosPerdidos");
        var eliminado = await client.PostAsync("/ObjetosPerdidos/Eliminar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = reporte.Id.ToString(), ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, eliminado.StatusCode);
        Assert.False(await db.ObjetosPerdidos.AnyAsync(o => o.Id == reporte.Id));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(reporte.FotoUrl)).StatusCode);
        await DbInitializer.InitializeAsync(factory.Services);
        Assert.False(await db.ObjetosPerdidos.AnyAsync(o => o.Id == reporte.Id));
        var inicio = WebUtility.HtmlDecode(await client.GetStringAsync("/"));
        Assert.DoesNotContain("huarique", inicio.ToLowerInvariant());
    }

    [Fact]
    public async Task Marketplace_PublicarCambiarEstadoYEliminar_RespetaAutorYValidaEstado()
    {
        await using var factory = new DemoFactory();
        using var vendedor = factory.CreateSessionClient();
        using var otro = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(vendedor);
        await DemoFactory.LoginAsync(otro, "maria.gonzales@usmp.pe");
        var token = await DemoFactory.TokenAsync(vendedor, "/Marketplace");
        var publicado = await vendedor.PostAsync("/Marketplace/Publicar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Titulo"] = "Artículo de prueba final", ["Categoria"] = "LibrosGuias", ["Precio"] = "18.5",
            ["EstadoItem"] = "Usado", ["WhatsApp"] = "987654321", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, publicado.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var articulo = await db.Articulos.SingleAsync(a => a.Titulo == "Artículo de prueba final");
        var datos = new Dictionary<string, string> { ["id"] = articulo.Id.ToString(), ["nuevoEstado"] = "999", ["__RequestVerificationToken"] = token };
        Assert.Equal(HttpStatusCode.BadRequest, (await vendedor.PostAsync("/Marketplace/CambiarEstado", new FormUrlEncodedContent(datos))).StatusCode);
        datos["nuevoEstado"] = "Reservado";
        Assert.Equal(HttpStatusCode.Redirect, (await vendedor.PostAsync("/Marketplace/CambiarEstado", new FormUrlEncodedContent(datos))).StatusCode);
        await db.Entry(articulo).ReloadAsync();
        Assert.Equal(EstadoPublicacion.Reservado, articulo.Estado);
        datos["__RequestVerificationToken"] = await DemoFactory.TokenAsync(otro, "/Marketplace");
        var ajeno = await otro.PostAsync("/Marketplace/Eliminar", new FormUrlEncodedContent(datos));
        Assert.Equal(HttpStatusCode.Redirect, ajeno.StatusCode);
        Assert.Contains("AccesoDenegado", ajeno.Headers.Location!.OriginalString);
        datos["__RequestVerificationToken"] = token;
        Assert.Equal(HttpStatusCode.Redirect, (await vendedor.PostAsync("/Marketplace/Eliminar", new FormUrlEncodedContent(datos))).StatusCode);
        Assert.False(await db.Articulos.AnyAsync(a => a.Id == articulo.Id));
    }

    [Fact]
    public async Task ObjetoEncontrado_ReclamarAprobarYConfirmarEntrega_NotificaALosAlumnos()
    {
        await using var factory = new DemoFactory();
        using var reportante = factory.CreateSessionClient();
        using var propietario = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(reportante, "maria.gonzales@usmp.pe");
        await DemoFactory.LoginAsync(propietario);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reclamo = await db.ReclamosObjetos.Include(r => r.Objeto)
            .SingleAsync(r => r.Objeto.Titulo == "Audífonos Sony WH-1000XM4");
        var token = await DemoFactory.TokenAsync(reportante, "/ObjetosPerdidos");
        var aprobado = await reportante.PostAsync("/ObjetosPerdidos/ResolverReclamo", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = reclamo.Id.ToString(), ["aprobar"] = "true", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, aprobado.StatusCode);
        await db.Entry(reclamo).ReloadAsync();
        Assert.Equal(EstadoReclamo.Aprobado, reclamo.Estado);
        token = await DemoFactory.TokenAsync(propietario, "/ObjetosPerdidos?tab=mis-solicitudes");
        var confirmado = await propietario.PostAsync("/ObjetosPerdidos/ConfirmarRecepcion", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = reclamo.Id.ToString(), ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, confirmado.StatusCode);
        await db.Entry(reclamo.Objeto).ReloadAsync();
        Assert.Equal(EstadoObjeto.Devuelto, reclamo.Objeto.Estado);
        Assert.True(await db.Notificaciones.AnyAsync(n => n.UsuarioId == reclamo.UsuarioId && n.Mensaje.Contains("Aprobaron")));
    }
}
