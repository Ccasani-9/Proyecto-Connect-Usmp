using Microsoft.EntityFrameworkCore;
using UsmpConnect.Models;

namespace UsmpConnect.Data.Seed;

public class MarketplaceSeeder : IModuleSeeder
{
    public int Orden => 30;

    public async Task SeedAsync(SeedContext ctx)
    {
        var db = ctx.Db;

        var juan = await ctx.Users.FindByEmailAsync(DemoUsers.Juan);
        var carlos = await ctx.Users.FindByEmailAsync(DemoUsers.Carlos);
        var pedro = await ctx.Users.FindByEmailAsync(DemoUsers.Pedro);
        var ana = await ctx.Users.FindByEmailAsync(DemoUsers.Ana);
        var sofia = await ctx.Users.FindByEmailAsync(DemoUsers.Sofia);

        if (juan == null || carlos == null || pedro == null || ana == null || sofia == null)
            return;

        var articulos = new List<ArticuloMarketplace>
        {
            new()
            {
                Titulo = "Calculadora Científica Casio fx-991LA X",
                Categoria = CategoriaMarketplace.CalculadorasHardware,
                Precio = 65.00m,
                EstadoItem = EstadoItem.Usado,
                Estado = EstadoPublicacion.Disponible,
                WhatsApp = "51987654321",
                NotasEntrega = "Disponible en biblioteca central de 2 a 6 pm.",
                VendedorId = juan.Id,
                FechaPublicacion = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Titulo = "Bata de laboratorio — solo usada 1 ciclo",
                Categoria = CategoriaMarketplace.BatasUniformes,
                Precio = null, // Donación gratis
                EstadoItem = EstadoItem.Donacion,
                Estado = EstadoPublicacion.Disponible,
                WhatsApp = "51987654322",
                NotasEntrega = "Entrega en Pabellón B o Cafetería de FIA.",
                VendedorId = carlos.Id,
                FechaPublicacion = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Titulo = "Arduino Uno R3 + Kit sensores completo",
                Categoria = CategoriaMarketplace.CalculadorasHardware,
                Precio = 120.00m,
                EstadoItem = EstadoItem.Usado,
                Estado = EstadoPublicacion.Reservado,
                WhatsApp = "51987654323",
                NotasEntrega = "Laboratorio F-101 los martes o jueves.",
                VendedorId = carlos.Id,
                FechaPublicacion = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                Titulo = "Cuarto individual cerca campus — Santa Anita",
                Categoria = CategoriaMarketplace.AlquilerCuartos,
                Precio = 550.00m,
                EstadoItem = EstadoItem.Nuevo,
                Estado = EstadoPublicacion.Disponible,
                WhatsApp = "51987654324",
                NotasEntrega = "A 5 minutos a pie de la FIA. Incluye servicios y Wi-Fi.",
                VendedorId = pedro.Id,
                FechaPublicacion = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Titulo = "Tabla periódica plastificada + formulario",
                Categoria = CategoriaMarketplace.LibrosGuias,
                Precio = 15.00m,
                EstadoItem = EstadoItem.ComoNuevo,
                Estado = EstadoPublicacion.Disponible,
                WhatsApp = "51987654325",
                NotasEntrega = "Cualquier día en el campus.",
                VendedorId = ana.Id,
                FechaPublicacion = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Titulo = "Apuntes de Física I — completos (impreso)",
                Categoria = CategoriaMarketplace.LibrosGuias,
                Precio = null, // Donación gratis
                EstadoItem = EstadoItem.Donacion,
                Estado = EstadoPublicacion.Disponible,
                WhatsApp = "51987654326",
                NotasEntrega = "Coordinar entrega en biblioteca central.",
                VendedorId = sofia.Id,
                FechaPublicacion = DateTime.UtcNow.AddDays(-6)
            }
        };

        db.Articulos.AddRange(articulos);
        await db.SaveChangesAsync();
    }
}
