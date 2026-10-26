using System.Net;

namespace UsmpConnect.Tests;

public class CierreDemoTests
{
    [Fact]
    public async Task DocenteNoIniciaSesion_YAlumnoAccedeAPanelYHorario()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        var token = await DemoFactory.TokenAsync(client);
        var docente = await client.PostAsync("/Cuenta/Login", new FormUrlEncodedContent(new Dictionary<string,string>
        {
            ["Email"]="jramirez@usmp.pe", ["Password"]="Usmp2026!", ["__RequestVerificationToken"]=token
        }));
        Assert.Equal(HttpStatusCode.OK, docente.StatusCode);
        Assert.Contains("alumnos", await docente.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Horario")).StatusCode);
        await DemoFactory.LoginAsync(client);
        var panel = WebUtility.HtmlDecode(await client.GetStringAsync("/Tecnologia"));
        Assert.Contains("Qdrant", panel);
        Assert.Contains("Groq", panel);
        Assert.Contains("/swagger", panel);
        Assert.DoesNotContain("En Vivo (Producción)", panel);
        Assert.DoesNotContain("menos de 2 milisegundos", panel);
        foreach (var ruta in new[] {"/", "/Horario", "/Catedra", "/Apuntes", "/Marketplace", "/Restaurantes", "/ObjetosPerdidos", "/Calendario"})
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(ruta)).StatusCode);
    }
}
