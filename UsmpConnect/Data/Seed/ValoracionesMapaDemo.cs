using Microsoft.EntityFrameworkCore;
using UsmpConnect.Models;

namespace UsmpConnect.Data.Seed;

internal static class ValoracionesMapaDemo
{
    private static readonly (string Nombre, int[] Puntuaciones)[] Ejemplos =
    [
        ("Isushi La Fontana", [2, 3]),
        ("El Huerto Florido", [3, 3]),
        ("Chifa Moneda China", [2, 4]),
        ("Dacsabor Restaurant & Menú", [3, 4]),
        ("Santa Patricia (Panadería & Pastelería)", [4, 4]),
        ("Chifa Yin Dou", [4, 5]),
        ("Satori Nikkei", []),
        ("Macartur Parrillas & Menú", [])
    ];

    public static async Task<bool> AplicarAsync(ApplicationDbContext db)
    {
        var maria = await db.Users.SingleOrDefaultAsync(u => u.Email == DemoUsers.Maria);
        var carlos = await db.Users.SingleOrDefaultAsync(u => u.Email == DemoUsers.Carlos);
        if (maria is null || carlos is null) return false;

        await db.Database.ExecuteSqlRawAsync(
            "CREATE TABLE IF NOT EXISTS DemoActualizaciones (Version TEXT PRIMARY KEY, AplicadaUtc TEXT NOT NULL);");
        await using var transaccion = await db.Database.BeginTransactionAsync();
        var pendiente = await db.Database.ExecuteSqlRawAsync("""
            INSERT OR IGNORE INTO DemoActualizaciones (Version, AplicadaUtc)
            VALUES ('valoraciones-mapa-v1', datetime('now'));
            """);
        if (pendiente == 0) return false;

        var nombres = Ejemplos.Select(e => e.Nombre).ToArray();
        var locales = await db.Restaurantes.Where(r => nombres.Contains(r.Nombre) && !r.Resenas.Any()).ToListAsync();
        var autores = new[] { maria.Id, carlos.Id };
        foreach (var local in locales)
        {
            var ejemplo = Ejemplos.Single(e => e.Nombre == local.Nombre);
            for (var i = 0; i < ejemplo.Puntuaciones.Length; i++)
            {
                db.ResenasRestaurantes.Add(new ResenaRestaurante
                {
                    RestauranteId = local.Id,
                    UsuarioId = autores[i],
                    Puntuacion = ejemplo.Puntuaciones[i],
                    Comentario = "Reseña de demostración. La puntuación es ficticia y sirve para probar el mapa.",
                    FechaUtc = DateTime.UtcNow
                });
            }
            local.TotalResenas = ejemplo.Puntuaciones.Length;
            local.CalificacionPromedio = local.TotalResenas == 0 ? 0 : Math.Round(ejemplo.Puntuaciones.Average(), 1);
        }

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return locales.Count > 0;
    }
}
