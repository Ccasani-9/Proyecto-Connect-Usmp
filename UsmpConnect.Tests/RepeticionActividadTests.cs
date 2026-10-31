using UsmpConnect.Services;
namespace UsmpConnect.Tests;

public class RepeticionActividadTests
{
    [Fact]
    public async Task Formulario_GuardaRepeticionesPersonalesConEmoji()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(client);
        var pagina = await client.GetStringAsync("/Calendario");
        Assert.Contains("evRepeticion", pagina);
        var token = await DemoFactory.TokenAsync(client, "/Calendario");
        var datos = new List<KeyValuePair<string,string>>
        {
            new("__RequestVerificationToken", token), new("titulo", "Repaso semanal prueba"),
            new("fecha", "2026-11-02"), new("semanal", "true"), new("hasta", "2026-11-11"),
            new("dias", "1"), new("dias", "3"), new("emoji", "📚"), new("returnUrl", "/Calendario")
        };
        var respuesta = await client.PostAsync("/Eventos/Crear", new FormUrlEncodedContent(datos));
        Assert.Equal(System.Net.HttpStatusCode.Redirect, respuesta.StatusCode);
        using var scope = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.CreateScope(factory.Services);
        var db = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<UsmpConnect.Data.ApplicationDbContext>(scope.ServiceProvider);
        var guardados = db.Eventos.Where(e => e.Titulo == "📚 Repaso semanal prueba").ToList();
        Assert.Equal(4, guardados.Count);
        Assert.All(guardados, e => Assert.False(string.IsNullOrWhiteSpace(e.UsuarioId)));
    }

    [Fact]
    public void Semanal_SeleccionaDiasIncluyeLimitesSinDuplicar()
    {
        var fechas = RepeticionActividad.Fechas(new(2026, 11, 2), true, new(2026, 11, 11), [1, 3, 1]);
        Assert.Equal(new DateOnly[] { new(2026,11,2), new(2026,11,4), new(2026,11,9), new(2026,11,11) }, fechas);
    }
    [Fact]
    public void NoRepetida_ConservaFecha()
        => Assert.Single(RepeticionActividad.Fechas(new(2026,11,2), false, null, null));
    [Fact]
    public void RepeticionInvalida_NoGeneraActividades()
    {
        Assert.Throws<ArgumentException>(() => RepeticionActividad.Fechas(new(2026,11,2), true, null, [1]));
        Assert.Throws<ArgumentException>(() => RepeticionActividad.Fechas(new(2026,11,2), true, new(2028,1,1), [1]));
        Assert.Throws<ArgumentException>(() => RepeticionActividad.Fechas(new(2026,11,2), true, new(2026,11,9), []));
    }
}

