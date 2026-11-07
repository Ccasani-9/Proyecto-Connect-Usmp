using Microsoft.EntityFrameworkCore;

namespace UsmpConnect.Data.Seed;

/// <summary>Completa fotos de ejemplos conocidos una sola vez, conservando subidas y publicaciones eliminadas.</summary>
internal static class PresentacionDemo
{
    private static readonly (string Titulo, string Autor, string Foto)[] Articulos =
    [
        ("Calculadora Científica Casio fx-991LA X", DemoUsers.Juan, "calculadora"),
        ("Bata de laboratorio — solo usada 1 ciclo", DemoUsers.Carlos, "bata"),
        ("Arduino Uno R3 + Kit sensores completo", DemoUsers.Carlos, "arduino"),
        ("Cuarto individual cerca campus — Santa Anita", DemoUsers.Pedro, "habitacion"),
        ("Tabla periódica plastificada + formulario", DemoUsers.Ana, "tabla-periodica"),
        ("Apuntes de Física I — completos (impreso)", DemoUsers.Sofia, "apuntes"),
        ("DEMO - Manual de SQL disponible", DemoUsers.Maria, "cuaderno"),
        ("DEMO - Calculadora ya vendida", DemoUsers.Maria, "calculadora")
    ];

    private static readonly (string Titulo, string Autor, string Foto)[] Objetos =
    [
        ("Carné Universitario — Ing. Civil", DemoUsers.Maria, "sin-foto.svg"),
        ("Audífonos Sony WH-1000XM4", DemoUsers.Maria, "audifonos.jpg"),
        ("Casaca North Face Negra", DemoUsers.Diego, "casaca.jpg"),
        ("Mochila Totto Gris", DemoUsers.Andres, "mochila.jpg"),
        ("Cuaderno Azul — Cálculo II", DemoUsers.Lucia, "cuaderno.jpg"),
        ("Laptop Charger USB-C 65W", DemoUsers.Alessandro, "cargador.jpg"),
        ("USB Kingston 32 GB negra", DemoUsers.Alessandro, "usb.jpg"),
        ("Llaves con llavero de Pikachu", DemoUsers.Ana, "llaves.jpg"),
        ("Calculadora Casio fx-570 con mi nombre", DemoUsers.Carlos, "calculadora.jpg"),
        ("DEMO - Cuaderno de programación encontrado", DemoUsers.Maria, "cuaderno.jpg")
    ];

    public static async Task<bool> AplicarAsync(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            "CREATE TABLE IF NOT EXISTS DemoActualizaciones (Version TEXT PRIMARY KEY, AplicadaUtc TEXT NOT NULL);");
        await using var transaccion = await db.Database.BeginTransactionAsync();
        var pendiente = await db.Database.ExecuteSqlRawAsync("""
            INSERT OR IGNORE INTO DemoActualizaciones (Version, AplicadaUtc)
            VALUES ('presentacion-fotos-v1', datetime('now'));
            """);
        if (pendiente == 0) return false;

        foreach (var ejemplo in Articulos)
        {
            var articulo = await db.Articulos.FirstOrDefaultAsync(a =>
                a.Titulo == ejemplo.Titulo && a.Vendedor.Email == ejemplo.Autor && a.ImagenUrl == null);
            if (articulo is not null) articulo.ImagenUrl = $"/images/demo/{ejemplo.Foto}.jpg";
        }
        foreach (var ejemplo in Objetos)
        {
            var objeto = await db.ObjetosPerdidos.FirstOrDefaultAsync(o =>
                o.Titulo == ejemplo.Titulo && o.Reportante.Email == ejemplo.Autor);
            if (objeto is not null && (objeto.FotoUrl is null || objeto.FotoUrl.StartsWith("https://images.unsplash.com/", StringComparison.Ordinal)))
                objeto.FotoUrl = $"/images/demo/{ejemplo.Foto}";
        }

        var restaurantes = await db.Restaurantes.ToListAsync();
        foreach (var restaurante in restaurantes)
            restaurante.Descripcion = restaurante.Descripcion.Replace("Huarique", "Restaurante").Replace("huarique", "restaurante");
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return true;
    }
}
