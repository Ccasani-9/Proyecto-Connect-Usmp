using Microsoft.EntityFrameworkCore;
using UsmpConnect.Models;

namespace UsmpConnect.Data.Seed;

public class FoodSpotsSeeder : IModuleSeeder
{
    public int Orden => 45;

    public async Task SeedAsync(SeedContext ctx)
    {
        var db = ctx.Db;
        if (await db.Restaurantes.AnyAsync())
            return;

        var maria = await ctx.Users.FindByEmailAsync(DemoUsers.Maria);
        var juan = await ctx.Users.FindByEmailAsync(DemoUsers.Juan);
        var ana = await ctx.Users.FindByEmailAsync(DemoUsers.Ana);
        var carlos = await ctx.Users.FindByEmailAsync(DemoUsers.Carlos);

        var r1 = new Restaurante
        {
            Nombre = "Cafetería Central FIA (Campus)",
            Descripcion = "La cafetería del campus universitario. Desayunos, sándwiches, menús ejecutivos, café pasado y snacks entre horas de clase.",
            Categoria = CategoriaRestaurante.CafeteriaHuarique,
            Rango = RangoPrecio.Economico,
            Direccion = "Campus FIA USMP · Al costado del Pabellón B",
            DistanciaTexto = "Dentro del Campus FIA (0 min)",
            RangoPrecios = "S/ 5.00 - S/ 13.00",
            HorarioAtencion = "Lun - Sáb: 7:30 AM - 8:30 PM",
            TelefonoWhatsApp = "987654321",
            FotoUrl = "https://images.unsplash.com/photo-1554118811-1e0d58224f24?auto=format&fit=crop&w=600&q=80",
            Latitud = -12.0722,
            Longitud = -76.9532,
            CalificacionPromedio = 4.6,
            TotalResenas = 3
        };

        var r2 = new Restaurante
        {
            Nombre = "Huarique Doña Rossi",
            Descripcion = "El clásico huarique de los estudiantes de sistemas e industrial. Platos bien servidos, sazón casera y menú económico.",
            Categoria = CategoriaRestaurante.MenuCriollo,
            Rango = RangoPrecio.Economico,
            Direccion = "Av. La Fontana 1270 · Frente a Puerta 1 FIA",
            DistanciaTexto = "A 40m cruzando Av. La Fontana",
            RangoPrecios = "S/ 9.50 - S/ 14.00",
            HorarioAtencion = "Lun - Vie: 11:30 AM - 4:30 PM",
            TelefonoWhatsApp = "991234567",
            FotoUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=600&q=80",
            Latitud = -12.0718,
            Longitud = -76.9528,
            CalificacionPromedio = 4.9,
            TotalResenas = 2
        };

        var r3 = new Restaurante
        {
            Nombre = "Chifa Dragón Dorado La Fontana",
            Descripcion = "Chifa rápido con porciones contundentes para grupos de estudio. Aeropuertos gigantes, sopa wantán especial y chaufas al wok.",
            Categoria = CategoriaRestaurante.Chifa,
            Rango = RangoPrecio.Economico,
            Direccion = "Av. La Fontana 1310",
            DistanciaTexto = "A 1 cuadra de la FIA (100m)",
            RangoPrecios = "S/ 11.00 - S/ 18.00",
            HorarioAtencion = "Lun - Dom: 12:00 PM - 10:00 PM",
            TelefonoWhatsApp = "978112233",
            FotoUrl = "https://images.unsplash.com/photo-1525755662778-989d0524087e?auto=format&fit=crop&w=600&q=80",
            Latitud = -12.0714,
            Longitud = -76.9521,
            CalificacionPromedio = 4.5,
            TotalResenas = 2
        };

        var r4 = new Restaurante
        {
            Nombre = "Sanguchería El Chinito (La Molina)",
            Descripcion = "Famoso por su chicharrón crocante con camote frito y sarza criolla. Ideal para desayunos antes de exámenes o almuerzos rápidos.",
            Categoria = CategoriaRestaurante.ComidaRapida,
            Rango = RangoPrecio.Medio,
            Direccion = "Av. La Fontana 1180",
            DistanciaTexto = "A 2 minutos a pie (180m)",
            RangoPrecios = "S/ 14.00 - S/ 22.00",
            HorarioAtencion = "Lun - Dom: 7:00 AM - 9:00 PM",
            TelefonoWhatsApp = "982334455",
            FotoUrl = "https://images.unsplash.com/photo-1509722747041-616f39b57569?auto=format&fit=crop&w=600&q=80",
            Latitud = -12.0731,
            Longitud = -76.9546,
            CalificacionPromedio = 4.8,
            TotalResenas = 1
        };

        var r5 = new Restaurante
        {
            Nombre = "Huarique El Punto Criollo",
            Descripcion = "Almuerzos caseros, seco de res, lomo saltado y milanesas con papas fritas. Menú universitario rápido con chicha morada.",
            Categoria = CategoriaRestaurante.MenuCriollo,
            Rango = RangoPrecio.Economico,
            Direccion = "Calle Los Mirtos 142 · Frente a la FIA",
            DistanciaTexto = "A 80m de la entrada peatonal",
            RangoPrecios = "S/ 9.00 - S/ 13.50",
            HorarioAtencion = "Lun - Sáb: 11:30 AM - 5:00 PM",
            TelefonoWhatsApp = "965443322",
            FotoUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=600&q=80",
            Latitud = -12.0726,
            Longitud = -76.9522,
            CalificacionPromedio = 4.7,
            TotalResenas = 1
        };

        var r6 = new Restaurante
        {
            Nombre = "Tambo+ / Snack Fontana",
            Descripcion = "Bebidas, sánguches empaquetados, empanadas, café caliente y snacks rápidos para recargar energías entre laboratorios.",
            Categoria = CategoriaRestaurante.ComidaRapida,
            Rango = RangoPrecio.Economico,
            Direccion = "Av. La Fontana con Calle Los Biólogos",
            DistanciaTexto = "A 120m de la facultad",
            RangoPrecios = "S/ 4.00 - S/ 11.00",
            HorarioAtencion = "Lun - Dom: 24 Horas",
            TelefonoWhatsApp = "998877665",
            FotoUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=600&q=80",
            Latitud = -12.0719,
            Longitud = -76.9539,
            CalificacionPromedio = 4.4,
            TotalResenas = 1
        };

        db.Restaurantes.AddRange(r1, r2, r3, r4, r5, r6);
        await db.SaveChangesAsync();

        // Platos de muestra
        var platos = new List<PlatoRestaurante>
        {
            new() { RestauranteId = r1.Id, Nombre = "Menú Universitario (Sopa + Segundo + Refresco)", Precio = 9.50m, Descripcion = "Varía diario: Arroz con pollo, Estofado de res o Lentejas con milanesa.", EsPopular = true },
            new() { RestauranteId = r1.Id, Nombre = "Empanada de Carne / Pollo al Horno", Precio = 4.50m, Descripcion = "Relleno jugoso artesanal con ají de la casa.", EsPopular = true },
            new() { RestauranteId = r1.Id, Nombre = "Café Americano / Capuchino", Precio = 5.00m, Descripcion = "Grano 100% Chanchamayo recién pasado.", EsPopular = false },

            new() { RestauranteId = r2.Id, Nombre = "Lomo Saltado Casero con Papas Nativas", Precio = 12.00m, Descripcion = "Flambeado al wok con cebolla crujiente, tomate y arroz graneado.", EsPopular = true },
            new() { RestauranteId = r2.Id, Nombre = "Ají de Gallina Cremoso", Precio = 10.50m, Descripcion = "Pechuga deshilachada con crema de ají amarillo, aceituna y huevo.", EsPopular = true },
            new() { RestauranteId = r2.Id, Nombre = "Menú Criollo Completo (Entrada + Fondo + Jarra)", Precio = 9.50m, Descripcion = "La opción más pedida por los alumnos de la FIA.", EsPopular = true },

            new() { RestauranteId = r3.Id, Nombre = "Aeropuerto de Pollo y Carne Especial", Precio = 14.50m, Descripcion = "Chaufa con fideos chinos, frejol chino, huevo y trozos de carne al sillao.", EsPopular = true },
            new() { RestauranteId = r3.Id, Nombre = "Chaufa de Pollo al Wok", Precio = 11.50m, Descripcion = "Porción bien taipá servida con wantanes fritos.", EsPopular = true },

            new() { RestauranteId = r4.Id, Nombre = "Sándwich de Chicharrón Completo", Precio = 16.50m, Descripcion = "Chicharrón de cerdo tierno, camote frito en pan francés crocante.", EsPopular = true },
            new() { RestauranteId = r4.Id, Nombre = "Jugo Especial con Leche y Algarrobina", Precio = 8.50m, Descripcion = "Papaya, plátano, fresa, leche y miel de abeja.", EsPopular = false },

            new() { RestauranteId = r5.Id, Nombre = "Quinoa Power Bowl con Pollo Teriyaki", Precio = 16.00m, Descripcion = "Base de quinua mixta, palta hass, tomate cherry y pollo caramelizado.", EsPopular = true },
            new() { RestauranteId = r5.Id, Nombre = "Wrap de Pollo y Palta", Precio = 13.50m, Descripcion = "Tortilla integral con pollo deshilachado, lechuga y vinagreta de mostaza miel.", EsPopular = false }
        };

        db.PlatosRestaurantes.AddRange(platos);

        // Reseñas de estudiantes
        if (maria != null && juan != null && ana != null && carlos != null)
        {
            var resenas = new List<ResenaRestaurante>
            {
                new() { RestauranteId = r1.Id, UsuarioId = maria.Id, Puntuacion = 5, Comentario = "Muy cómodo para no salir de la facultad entre clases, atienden rápido y el café salva vidas en parciales.", FechaUtc = DateTime.UtcNow.AddDays(-3) },
                new() { RestauranteId = r1.Id, UsuarioId = juan.Id, Puntuacion = 4, Comentario = "El menú es bueno y económico, solo que a la 1:00 PM se llena bastante.", FechaUtc = DateTime.UtcNow.AddDays(-5) },
                new() { RestauranteId = r2.Id, UsuarioId = carlos.Id, Puntuacion = 5, Comentario = "¡Doña Rossi es insuperable! El Lomo Saltado y el Arroz con pollo son los mejores de La Fontana. Recomendadísimo.", FechaUtc = DateTime.UtcNow.AddDays(-2) },
                new() { RestauranteId = r2.Id, UsuarioId = ana.Id, Puntuacion = 5, Comentario = "Muy buena porción por 9.50 soles, perfecto para el presupuesto de nosotros los estudiantes.", FechaUtc = DateTime.UtcNow.AddDays(-1) },
                new() { RestauranteId = r3.Id, UsuarioId = juan.Id, Puntuacion = 5, Comentario = "El aeropuerto es inmenso, comemos 2 personas con un solo plato. Gran huarique para celebrar el fin de ciclo.", FechaUtc = DateTime.UtcNow.AddDays(-7) }
            };

            db.ResenasRestaurantes.AddRange(resenas);
        }

        await db.SaveChangesAsync();
    }
}
