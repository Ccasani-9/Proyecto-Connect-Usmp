using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UsmpConnect.Controllers;
using UsmpConnect.Data;

namespace UsmpConnect.Tests;

public class DemoFactory : WebApplicationFactory<CuentaController>
{
    // Cada host usa una BD y ruta de modelo nuevas y nunca reinicia archivos del checkout.
    public string DatabasePath { get; } = Path.Combine(Path.GetTempPath(), $"usmp-tests-{Guid.NewGuid():N}.db");
    public string SentimentModelPath { get; } = Path.Combine(Path.GetTempPath(), $"usmp-sentimiento-{Guid.NewGuid():N}.zip");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:ResetOnStartup"] = "false",
            ["Redis:ConnectionString"] = "",
            ["Redis:WarmupOnStartup"] = "false",
            ["CloudAMQP:Url"] = "",
            ["CloudAMQP:DelayMinSeconds"] = "0",
            ["CloudAMQP:DelayMaxSeconds"] = "0",
            ["Algolia:ApplicationId"] = "",
            ["PieHost:ApiKey"] = "",
            ["Sentimiento:ModelPath"] = SentimentModelPath
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite($"Data Source={DatabasePath}"));
        });
    }

    public HttpClient CreateSessionClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost")
    });

    public static async Task<string> TokenAsync(HttpClient client, string path = "/Cuenta/Login")
    {
        var html = await client.GetStringAsync(path);
        return WebUtility.HtmlDecode(Regex.Match(html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    }

    public static async Task LoginAsync(HttpClient client, string email = "alessandro.morales@usmp.pe")
    {
        var token = await TokenAsync(client);
        var result = await client.PostAsync("/Cuenta/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Password"] = "Usmp2026!", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
    }
}
