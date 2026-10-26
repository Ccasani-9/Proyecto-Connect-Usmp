using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using UsmpConnect.Services.Conocimiento;

namespace UsmpConnect.Tests;

public class ConocimientoTests
{
    private static byte[] ConstruirPdfManualFia()
    {
        using var builder = new PdfDocumentBuilder();
        var fontTitulo = builder.AddStandard14Font(Standard14Font.HelveticaBold);
        var fontTexto = builder.AddStandard14Font(Standard14Font.Helvetica);

        var p1 = builder.AddPage(PageSize.A4);
        p1.AddText("Manual Operativo de USMP Connect - Pagina 1: Calculadora de Promedios y Horario", 12, new PdfPoint(45, 780), fontTitulo);
        var lineasP1 = new[]
        {
            "1. Calculadora de Promedios FIA:",
            "En la pantalla de Inicio encontraras la Calculadora de Promedios integrada para cursos de la FIA.",
            "Permite ingresar el Promedio de Practicas (PP), el Examen Parcial (EP) y el Examen Final (EF).",
            "Verifica que los tres pesos porcentuales sumen exactamente 100% antes de calcular el resultado.",
            "La nota minima aprobatoria referencial en la calculadora de demostracion es 10.5 sobre 20.",
            "Si dejas vacia la casilla del Examen Final (EF), la herramienta calcula automaticamente que nota",
            "necesitas obtener en el examen final para alcanzar el promedio aprobatorio de 10.5, o te avisa si",
            "ya aprobaste el curso con tus notas previas o si requieres consultar el examen sustitutorio.",
            "",
            "2. Mi Horario y Mi Calendario Academico:",
            "En el modulo Mi Horario puedes revisar tus bloques semanales de teoria, practica y laboratorio",
            "con el aula o pabellon asignado dentro del campus de La Molina.",
            "En Mi Calendario se consolidan las entregas academicas, examenes parciales, finales y eventos",
            "universitarios cercanos para organizar tu semana de estudio."
        };
        var y1 = 748;
        foreach (var linea in lineasP1)
        {
            if (linea.Length > 0)
                p1.AddText(linea, 10, new PdfPoint(45, y1), fontTexto);
            y1 -= 20;
        }

        var p2 = builder.AddPage(PageSize.A4);
        p2.AddText("Manual Operativo de USMP Connect - Pagina 2: Restaurantes, Resenas ML y Comunidad", 12, new PdfPoint(45, 780), fontTitulo);
        var lineasP2 = new[]
        {
            "3. Mapa del Cuadrante FIA y Gestion de Resenas Estudiantiles:",
            "El modulo Restaurantes Cercanos abarca el cuadrante universitario alrededor del campus FIA:",
            "Av. La Fontana, Av. Flora Tristan, Av. Javier Prado Este y Av. Los Ingenieros.",
            "Cada local muestra su rango de precios, horario, distancia y ruta caminando desde la puerta FIA.",
            "Al abrir el detalle de un restaurante puedes publicar una resena con puntuacion de 1 a 5 estrellas.",
            "Tambien puedes editar o eliminar tu propia resena directamente desde la ventana del restaurante",
            "usando los botones Editar y Eliminar junto a tu comentario, o mediante los endpoints REST.",
            "Cada vez que creas o editas una resena, el modelo ML.NET clasifica automaticamente el sentimiento",
            "del comentario (Positivo o Negativo con su porcentaje de confianza), recalcula el promedio del",
            "local, invalida la cache en Redis y actualiza las recomendaciones personalizadas para alumnos.",
            "",
            "4. Convivencia y Seguridad en la Plataforma:",
            "Nunca compartas contrasenas, codigos completos de documento de identidad ni datos privados.",
            "Las publicaciones de Marketplace y Objetos Perdidos son responsabilidad de cada estudiante autor."
        };
        var y2 = 748;
        foreach (var linea in lineasP2)
        {
            if (linea.Length > 0)
                p2.AddText(linea, 10, new PdfPoint(45, y2), fontTexto);
            y2 -= 20;
        }

        return builder.Build();
    }

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

    private static string AsegurarPdfManualFia()
    {
        var rutaCorpusPdf = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "UsmpConnect", "Data", "Conocimiento", "manual-fia-usmp-connect.pdf"));
        if (!File.Exists(rutaCorpusPdf))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(rutaCorpusPdf)!);
            File.WriteAllBytes(rutaCorpusPdf, ConstruirPdfManualFia());
        }

        var rutaBinPdf = Path.Combine(AppContext.BaseDirectory, "Data", "Conocimiento", "manual-fia-usmp-connect.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(rutaBinPdf)!);
        File.Copy(rutaCorpusPdf, rutaBinPdf, overwrite: true);
        return rutaCorpusPdf;
    }

    [Fact]
    public async Task PdfPig_ExtraePaginasRealesDelManualPdfEnCorpus()
    {
        var rutaCorpusPdf = AsegurarPdfManualFia();

        var extractor = new DocumentoTextoExtractor();
        await using var stream = File.OpenRead(rutaCorpusPdf);
        var paginas = await extractor.ExtraerAsync(stream, "manual-fia-usmp-connect.pdf");

        Assert.Equal(2, paginas.Count);
        Assert.Equal(1, paginas[0].Numero);
        Assert.Contains("Calculadora de Promedios", paginas[0].Texto);
        Assert.Contains("10.5", paginas[0].Texto);
        Assert.Equal(2, paginas[1].Numero);
        Assert.Contains("Editar y Eliminar", paginas[1].Texto);
        Assert.Contains("ML.NET", paginas[1].Texto);
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
        AsegurarPdfManualFia();
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
