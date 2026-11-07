using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UsmpConnect.Data;

namespace UsmpConnect.Tests;

public class ValoracionesMapaDemoTests
{
    [Fact]
    public async Task Arranque_SiembraPromediosCoherentesYRespetaResenasEnElSiguienteArranque()
    {
        await using var factory = new DemoFactory();
        using var client = factory.CreateSessionClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var isushi = await db.Restaurantes.Include(r => r.Resenas).SingleAsync(r => r.Nombre == "Isushi La Fontana");
        Assert.Equal(2.5, isushi.CalificacionPromedio);
        Assert.Equal(2, isushi.TotalResenas);
        Assert.Equal(new[] { 2, 3 }, isushi.Resenas.Select(r => r.Puntuacion).Order().ToArray());
        Assert.All(isushi.Resenas, r => Assert.Contains("ficticia", r.Comentario));

        var satori = await db.Restaurantes.Include(r => r.Resenas).SingleAsync(r => r.Nombre == "Satori Nikkei");
        Assert.Equal(0, satori.TotalResenas);
        Assert.Empty(satori.Resenas);

        var dachi = await db.Restaurantes.Include(r => r.Resenas).SingleAsync(r => r.Nombre == "Dachi Café");
        Assert.Equal(5, Assert.Single(dachi.Resenas).Puntuacion);

        // Si una muestra se elimina después, un reinicio no debe volver a crearla.
        db.ResenasRestaurantes.Remove(isushi.Resenas.First());
        await db.SaveChangesAsync();
        await DbInitializer.InitializeAsync(factory.Services);
        Assert.Equal(1, await db.ResenasRestaurantes.CountAsync(r => r.RestauranteId == isushi.Id));
    }
}
