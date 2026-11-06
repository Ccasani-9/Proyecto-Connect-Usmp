using Microsoft.Extensions.DependencyInjection;
using UsmpConnect.Data;
using UsmpConnect.Data.Seed;
using UsmpConnect.Services.Conocimiento;

namespace UsmpConnect.Tests;

public class ContenidoDemoTests
{
    [Fact]
    public async Task Ampliacion_TieneContenidoRealYNoDuplicaNiReponeAportesEliminados()
    {
        await using var factory = new DemoFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(24, db.Apuntes.Count());
        var python = db.Apuntes.First(a => a.NombreArchivo == "demo-python-funciones.pdf");
        python.Curso = db.Cursos.Find(python.CursoId)!;
        using var pdf = new MemoryStream(python.GenerarPdfEjemplo());
        var texto = await new DocumentoTextoExtractor().ExtraerAsync(pdf, "funciones.pdf");
        Assert.Contains("return", string.Join(" ", texto.Select(p => p.Texto)));
        var local = db.Restaurantes.First();
        local.PrecioPromedioReferencial = 13;
        db.Apuntes.Remove(python);
        await db.SaveChangesAsync();
        await AmpliacionDemo.AplicarAsync(db);
        Assert.Equal(23, db.Apuntes.Count());
        Assert.Equal(13m, db.Restaurantes.Find(local.Id)!.PrecioPromedioReferencial);
        Assert.Equal(2, db.Articulos.Count(a => a.Titulo.StartsWith("DEMO -")));
    }
}
