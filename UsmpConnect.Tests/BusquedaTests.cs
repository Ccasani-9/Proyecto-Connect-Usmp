using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;

namespace UsmpConnect.Tests;

public class BusquedaTests
{
    [Fact]
    public async Task BusquedaPublica_EncuentraCursosYRetiraArticulosVendidos()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        var cursos = await client.GetFromJsonAsync<List<AlgoliaItem>>("/Buscar/Sugerencias?q=programacion");
        Assert.Contains(cursos!, i => i.Tipo == "Cursos y docentes" && i.Url.StartsWith("/Catedra/Curso/"));
        Assert.False(factory.Services.GetRequiredService<EstadoIntegraciones>().Obtener("algolia")!.Correcta);
        string titulo;
        int id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var articulo = db.Articulos.First(a => a.Estado == EstadoPublicacion.Disponible);
            articulo.Titulo = "CalculadoraPruebaBusqueda";
            await db.SaveChangesAsync();
            titulo = articulo.Titulo; id = articulo.Id;
        }
        Assert.Contains((await client.GetFromJsonAsync<List<AlgoliaItem>>($"/Buscar/Sugerencias?q={titulo}"))!, i => i.ObjectID == $"market_{id}");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Articulos.Single(a => a.Id == id).Estado = EstadoPublicacion.Vendido;
            await db.SaveChangesAsync();
        }
        Assert.Empty((await client.GetFromJsonAsync<List<AlgoliaItem>>($"/Buscar/Sugerencias?q={titulo}"))!);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Buscar?q=programacion")).StatusCode);
    }

    [Fact]
    public async Task Sincronizar_IncluyeCursosYEliminaRegistrosObsoletosSinBorrarLosAjenos()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        using var scope = factory.Services.CreateScope();
        var transport = new AlgoliaTransport();
        using var http = new HttpClient(transport);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Algolia:ApplicationId"] = "test", ["Algolia:WriteApiKey"] = "test-write"
        }).Build();
        var service = new BuscadorAlgolia(config, NullLogger<BuscadorAlgolia>.Instance, http);
        Assert.True(await service.SincronizarTodoAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()));
        Assert.Contains(transport.Operations, op => op.Action == "deleteObject" && op.Id == "apunte_999999");
        Assert.DoesNotContain(transport.Operations, op => op.Action == "deleteObject" && op.Id == "otro_sistema");
        Assert.Contains(transport.Operations, op => op.Action == "addObject" && op.Id.StartsWith("curso_"));
    }

    [Theory]
    [InlineData("{\"hits\":\"invalid\"}")]
    [InlineData("{\"hits\":[{\"objectID\":4}]}")]
    public async Task RespuestaRemotaInvalida_UsaSQLiteYNoDeclaraExito(string respuesta)
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        using var scope = factory.Services.CreateScope();
        using var http = new HttpClient(new RespuestaTransport(respuesta));
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Algolia:ApplicationId"] = "test", ["Algolia:SearchApiKey"] = "test"
        }).Build();
        var estados = new EstadoIntegraciones();
        var service = new BuscadorAlgolia(config, NullLogger<BuscadorAlgolia>.Instance, http, estados);
        var resultado = await service.BuscarAsync("programacion", scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
        Assert.NotEmpty(resultado);
        Assert.False(estados.Obtener("algolia")!.Correcta);
    }

    private sealed class RespuestaTransport(string respuesta) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(respuesta) });
    }

    private sealed class AlgoliaTransport : HttpMessageHandler
    {
        public List<(string Action, string Id)> Operations { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/browse"))
                return new(HttpStatusCode.OK) { Content = new StringContent("{\"hits\":[{\"objectID\":\"apunte_999999\"},{\"objectID\":\"otro_sistema\"}]}") };
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            foreach (var op in json.RootElement.GetProperty("requests").EnumerateArray())
                Operations.Add((op.GetProperty("action").GetString()!, op.GetProperty("body").GetProperty("objectID").GetString()!));
            return new(HttpStatusCode.OK) { Content = new StringContent("{\"taskID\":1}") };
        }
    }
}
