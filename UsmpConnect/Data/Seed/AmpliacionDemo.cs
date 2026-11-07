using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UsmpConnect.Models;

namespace UsmpConnect.Data.Seed;

public record DocumentoDemo(string Clave, string Titulo, string Curso, string Descripcion, string[] Lineas, bool Existente);

public static class ContenidoApuntesDemo
{
    private static readonly Lazy<IReadOnlyList<DocumentoDemo>> Documentos = new(() =>
        JsonSerializer.Deserialize<List<DocumentoDemo>>(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Data", "Demo", "apuntes.json")))
        ?? throw new InvalidDataException("No se pudo leer el contenido demo."));
    public static IReadOnlyList<DocumentoDemo> Todos => Documentos.Value;
    public static DocumentoDemo? Buscar(string archivo) => Todos.FirstOrDefault(d => d.Clave == archivo);
}

/// <summary>Actualización única y transaccional; no vuelve a crear ejemplos eliminados por alumnos.</summary>
public static class AmpliacionDemo
{
    public static async Task AplicarAsync(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS DemoActualizaciones (Version TEXT PRIMARY KEY, AplicadaUtc TEXT NOT NULL);");
        await using var tx = await db.Database.BeginTransactionAsync();
        using (var comando = db.Database.GetDbConnection().CreateCommand())
        {
            comando.Transaction = tx.GetDbTransaction();
            comando.CommandText = "SELECT COUNT(*) FROM DemoActualizaciones WHERE Version='contenido-v1';";
            if (Convert.ToInt64(await comando.ExecuteScalarAsync()) != 0) return;
        }
        var cursos = await db.Cursos.ToDictionaryAsync(c => c.Codigo);
        var autor = await db.Users.FirstOrDefaultAsync(u => u.Email == DemoUsers.Maria);
        if (autor is null) throw new InvalidOperationException("Falta la cuenta demo para añadir recursos.");
        var archivosOriginales = ContenidoApuntesDemo.Todos.Where(d => d.Existente).Select(d => d.Clave).ToArray();
        var anteriores = await db.Apuntes.Include(a => a.Curso)
            .Where(a => a.ArchivoUrl == null && archivosOriginales.Contains(a.NombreArchivo)).ToListAsync();
        foreach (var apunte in anteriores) apunte.Bytes = apunte.GenerarPdfEjemplo().Length;
        foreach (var documento in ContenidoApuntesDemo.Todos.Where(d => !d.Existente))
        {
            if (await db.Apuntes.AnyAsync(a => a.NombreArchivo == documento.Clave)) continue;
            if (!cursos.TryGetValue(documento.Curso, out var curso)) throw new InvalidDataException($"Falta curso demo {documento.Curso}.");
            var apunte = new Apunte { Titulo = documento.Titulo, NombreArchivo = documento.Clave,
                Curso = curso, Profesor = "Equipo de demostración", AutorId = autor.Id,
                Descripcion = documento.Descripcion, Tipo = TipoDocumento.Apuntes, Extension = ".pdf" };
            apunte.Bytes = apunte.GenerarPdfEjemplo().Length;
            db.Apuntes.Add(apunte);
        }
        db.Articulos.AddRange(
            new ArticuloMarketplace { Titulo = "DEMO - Manual de SQL disponible", Categoria = CategoriaMarketplace.LibrosGuias,
                Precio = 18, EstadoItem = EstadoItem.Usado, Estado = EstadoPublicacion.Disponible,
                VendedorId = autor.Id, NotasEntrega = "Ejemplo del equipo; no es una oferta real." },
            new ArticuloMarketplace { Titulo = "DEMO - Calculadora ya vendida", Categoria = CategoriaMarketplace.CalculadorasHardware,
                Precio = 35, EstadoItem = EstadoItem.Usado, Estado = EstadoPublicacion.Vendido,
                VendedorId = autor.Id, NotasEntrega = "Permite comprobar que el asistente excluye artículos vendidos." });
        db.Tips.Add(new Tip { CursoId = cursos["CS-101"].Id, AutorId = autor.Id, Dificultad = 2,
            Texto = "Consejo demo: para practicar funciones en Python, separa entradas, cálculo y salida; comprueba listas vacías y casos límite.", Fecha = DateTime.UtcNow });
        db.ObjetosPerdidos.Add(new ObjetoPerdido { Titulo = "DEMO - Cuaderno de programación encontrado", Tipo = TipoReporte.Encontrado,
            Categoria = CategoriaObjeto.Otros, ReportanteId = autor.Id, Pabellon = "Biblioteca", Lugar = "Mesa de estudio",
            Custodia = CustodiaObjeto.Reportante, Descripcion = "Ejemplo ficticio del equipo para mostrar un reporte y su autoría.", FechaHallazgo = DateTime.Today });
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("INSERT INTO DemoActualizaciones (Version,AplicadaUtc) VALUES ('contenido-v1', datetime('now'));");
        await tx.CommitAsync();
    }
}
