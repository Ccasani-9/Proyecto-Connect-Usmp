using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using UsmpConnect.Services.Conocimiento;

namespace UsmpConnect.Tests;

public class ConocimientoTests
{
    [Fact]
    public async Task Extractor_ConservaTextoYRechazaArchivoInvalido()
    {
        var extractor = new DocumentoTextoExtractor();
        using var texto = new MemoryStream(Encoding.UTF8.GetBytes("Cómo publicar apuntes en USMP Connect"));
        var paginas = await extractor.ExtraerAsync(texto, "guia.md");
        Assert.Contains("apuntes", paginas.Single().Texto);
        using var invalido = new MemoryStream([1, 2, 3]);
        await Assert.ThrowsAsync<InvalidDataException>(() => extractor.ExtraerAsync(invalido, "falso.pdf"));
        await Assert.ThrowsAsync<InvalidDataException>(() => extractor.ExtraerAsync(invalido, "programa.exe"));
    }

    private static string RutaPdfManualFia()
    {
        // El contenido versionado se copia al compilar, igual que en publicación.
        var ruta = Path.Combine(AppContext.BaseDirectory, "Data", "Conocimiento", "manual-fia-usmp-connect.pdf");
        Assert.True(File.Exists(ruta), "Falta el manual PDF versionado en el contenido de salida.");
        return ruta;
    }

    [Fact]
    public async Task PdfPig_ExtraePaginasRealesDelManualPdfEnCorpus()
    {
        var rutaCorpusPdf = RutaPdfManualFia();

        var extractor = new DocumentoTextoExtractor();
        await using var stream = File.OpenRead(rutaCorpusPdf);
        var paginas = await extractor.ExtraerAsync(stream, "manual-fia-usmp-connect.pdf");

        Assert.Equal(2, paginas.Count);
        Assert.Equal(1, paginas[0].Numero);
        Assert.Contains("Calculadora de Promedios", paginas[0].Texto);
        Assert.Contains("calculadorafiausmp.com", paginas[0].Texto);
        Assert.DoesNotContain("Calculadora de Promedios integrada", paginas[0].Texto);
        Assert.Equal(2, paginas[1].Numero);
        Assert.Contains("Editar y Eliminar", paginas[1].Texto);
        Assert.Contains("ML.NET", paginas[1].Texto);
        Assert.Contains("no reentrenan", paginas[1].Texto);
    }

    [Fact]
    public async Task BusquedaLocal_DevuelveFuenteRealYSinInventarCoincidencias()
    {
        var fragmentos = CatalogoConocimiento.Fragmentar("guia.md", [new PaginaDocumento(1, "Para publicar apuntes entra a la sección Apuntes y selecciona el curso.")]);
        var resultado = KnowledgeBaseService.BuscarLocal(fragmentos, "publicar apuntes", 5);
        Assert.Equal("guia.md", resultado.Single().Fuente);
        Assert.Empty(KnowledgeBaseService.BuscarLocal(fragmentos, "mundial fútbol", 5));
        var largo = CatalogoConocimiento.Fragmentar("largo.txt", [new PaginaDocumento(1, new string('a', 2100))]);
        Assert.All(largo, f => Assert.InRange(f.Texto.Length, 1, 1000));
        Assert.Equal(largo.Count, largo.Select(f => f.Id).Distinct().Count());
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Qdrant_UsaModeloConsistenteYFiltraVersionDelCorpus()
    {
        var handler = new QdrantHandler();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Qdrant:Endpoint"] = "https://qdrant.example",
            ["Qdrant:ApiKey"] = "clave-demo-prueba",
            ["Qdrant:CollectionName"] = "test_fia"
        }).Build();
        var cliente = new QdrantKnowledgeClient(new HttpClient(handler), config);
        var fragmentos = CatalogoConocimiento.Fragmentar("guia.md", [new PaginaDocumento(1, "Cómo publicar apuntes")]);
        await cliente.SincronizarAsync(fragmentos, "version-demo", default);
        await cliente.BuscarAsync("apuntes", "version-demo", 5, default);
        Assert.Contains(handler.Cuerpos, cuerpo => cuerpo.Contains("\"vector\"") && cuerpo.Contains(QdrantKnowledgeClient.Modelo));
        var consulta = JsonDocument.Parse(handler.Cuerpos.Last());
        Assert.Equal(QdrantKnowledgeClient.Modelo, consulta.RootElement.GetProperty("query").GetProperty("model").GetString());
        Assert.Equal("version-demo", consulta.RootElement.GetProperty("filter").GetProperty("must")[0].GetProperty("match").GetProperty("value").GetString());
    }

    [Fact]
    public async Task Consulta_RequiereSesionYPermiteAlumnoConRespaldoLocal()
    {
        RutaPdfManualFia();
        await using var factory = new ConocimientoFactory();
        using var client = factory.CreateSessionClient();
        var sinSesion = await client.GetAsync("/Conocimiento/Buscar?consulta=apuntes");
        Assert.Equal(HttpStatusCode.Redirect, sinSesion.StatusCode);
        await DemoFactory.LoginAsync(client);
        var respuesta = await client.GetAsync("/Conocimiento/Buscar?consulta=publicar%20apuntes");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal("local", json.RootElement.GetProperty("modo").GetString());
        Assert.NotEmpty(json.RootElement.GetProperty("fuentes").EnumerateArray());
        Assert.Contains(json.RootElement.GetProperty("fuentes").EnumerateArray(), f => f.GetProperty("fuente").GetString() == "guia-usmp-connect.md");

        var respuestaPdf = await client.GetAsync("/Conocimiento/Buscar?consulta=calculadora%20de%20promedios%2010.5");
        Assert.Equal(HttpStatusCode.OK, respuestaPdf.StatusCode);
        using var jsonPdf = JsonDocument.Parse(await respuestaPdf.Content.ReadAsStringAsync());
        Assert.Contains(jsonPdf.RootElement.GetProperty("fuentes").EnumerateArray(),
            f => f.GetProperty("fuente").GetString() == "manual-fia-usmp-connect.pdf" && f.GetProperty("pagina").GetInt32() == 1);

        var vacia = await client.GetAsync("/Conocimiento/Buscar?consulta=x");
        Assert.Equal(HttpStatusCode.BadRequest, vacia.StatusCode);
    }

    private sealed class ConocimientoFactory : DemoFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["Qdrant:Endpoint"] = "", ["Qdrant:ApiKey"] = "" }));
        }
    }

    private sealed class QdrantHandler : HttpMessageHandler
    {
        public List<string> Cuerpos { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.True(request.Headers.Contains("api-key"));
            if (request.Content is not null) Cuerpos.Add(await request.Content.ReadAsStringAsync(ct));
            var json = request.Method == HttpMethod.Get
                ? "{\"result\":{\"config\":{\"params\":{\"vectors\":{\"size\":384,\"distance\":\"Cosine\"}}}}}"
                : "{\"result\":{\"points\":[]},\"status\":\"ok\"}";
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }
}
