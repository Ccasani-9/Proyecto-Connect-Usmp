using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UsmpConnect.Data.Seed;
using UsmpConnect.Models;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Tests;

public class RestaurantesApiTests
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task SwaggerYConsultasPublicas_RespondenCorrectamente()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();

        var swaggerResp = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, swaggerResp.StatusCode);
        var swaggerJson = await swaggerResp.Content.ReadAsStringAsync();
        Assert.Contains("/api/restaurantes", swaggerJson);
        Assert.Contains("/api/resenas/{id}", swaggerJson);

        var todos = await client.GetFromJsonAsync<List<RestauranteItemVM>>("/api/restaurantes", JsonOpts);
        Assert.NotNull(todos);
        Assert.NotEmpty(todos);

        var chifas = await client.GetFromJsonAsync<List<RestauranteItemVM>>(
            $"/api/restaurantes?categoria={CategoriaRestaurante.Chifa}", JsonOpts);
        Assert.NotNull(chifas);
        Assert.All(chifas, r => Assert.Equal(CategoriaRestaurante.Chifa, r.Categoria));

        var detalle = await client.GetFromJsonAsync<RestauranteDetalleDto>("/api/restaurantes/1", JsonOpts);
        Assert.NotNull(detalle);
        Assert.Equal(1, detalle.Id);
        Assert.False(string.IsNullOrWhiteSpace(detalle.Nombre));

        var noExiste = await client.GetAsync("/api/restaurantes/99999");
        Assert.Equal(HttpStatusCode.NotFound, noExiste.StatusCode);
    }

    [Fact]
    public async Task CicloDeVidaResenaApi_CrearEditarEliminarYProtegerAutoria()
    {
        await using var factory = new DemoFactory();
        using var anonClient = factory.CreateSessionClient();

        // Sin sesión debe devolver 401 directo (sin redirección HTML)
        var unauthResp = await anonClient.PostAsJsonAsync("/api/restaurantes/1/resenas", new GuardarResenaRequest
        {
            Puntuacion = 5,
            Comentario = "Intento sin iniciar sesión."
        });
        Assert.Equal(HttpStatusCode.Unauthorized, unauthResp.StatusCode);

        using var autorClient = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(autorClient, DemoUsers.Alessandro);

        // Crear reseña por API
        var createResp = await autorClient.PostAsJsonAsync("/api/restaurantes/1/resenas", new GuardarResenaRequest
        {
            Puntuacion = 5,
            Comentario = "Excelente menú ejecutivo cerca de la facultad."
        });
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var creada = await createResp.Content.ReadFromJsonAsync<ResenaDto>(JsonOpts);
        Assert.NotNull(creada);
        Assert.Equal(5, creada.Puntuacion);

        // Otro usuario no puede editar ni eliminar la reseña ajena (403 Forbidden)
        using var otroClient = factory.CreateSessionClient();
        await DemoFactory.LoginAsync(otroClient, DemoUsers.Maria);

        var forbidPut = await otroClient.PutAsJsonAsync($"/api/resenas/{creada.Id}", new GuardarResenaRequest
        {
            Puntuacion = 1,
            Comentario = "Intentando modificar reseña de otro alumno."
        });
        Assert.Equal(HttpStatusCode.Forbidden, forbidPut.StatusCode);

        var forbidDel = await otroClient.DeleteAsync($"/api/resenas/{creada.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, forbidDel.StatusCode);

        // El propio autor sí puede editar su reseña
        var putResp = await autorClient.PutAsJsonAsync($"/api/resenas/{creada.Id}", new GuardarResenaRequest
        {
            Puntuacion = 4,
            Comentario = "Muy buena atención, actualizado desde la API."
        });
        Assert.Equal(HttpStatusCode.OK, putResp.StatusCode);
        var editada = await putResp.Content.ReadFromJsonAsync<ResenaDto>(JsonOpts);
        Assert.NotNull(editada);
        Assert.Equal(4, editada.Puntuacion);
        Assert.Equal("Muy buena atención, actualizado desde la API.", editada.Comentario);

        // El propio autor puede eliminar su reseña
        var delResp = await autorClient.DeleteAsync($"/api/resenas/{creada.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);

        // Verificar que ya no existe (404)
        var delAgain = await autorClient.DeleteAsync($"/api/resenas/{creada.Id}");
        Assert.Equal(HttpStatusCode.NotFound, delAgain.StatusCode);
    }
}
