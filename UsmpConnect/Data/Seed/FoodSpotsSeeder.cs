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
        {
            return;
        }

        var maria = await ctx.Users.FindByEmailAsync(DemoUsers.Maria);
        var juan = await ctx.Users.FindByEmailAsync(DemoUsers.Juan);
        var ana = await ctx.Users.FindByEmailAsync(DemoUsers.Ana);
        var carlos = await ctx.Users.FindByEmailAsync(DemoUsers.Carlos);

        var restaurantes = new List<Restaurante>
        {
            // 1. Huariques y Comedores emblemáticos de los alumnos FIA
            new()
            {
                Nombre = "Cafetería Central FIA (Campus)",
                Descripcion = "La cafetería dentro del campus universitario. Desayunos, combos estudiantiles, sándwiches, café y snacks rápidos entre horas de clase y laboratorio.",
                Categoria = CategoriaRestaurante.CafeteriaHuarique,
                Rango = RangoPrecio.Economico,
                Direccion = "Campus FIA USMP · Al costado del Pabellón B",
                DistanciaTexto = "Dentro del Campus FIA (0 min)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 7:30 AM - 8:30 PM",
                TelefonoWhatsApp = "987654321",
                FotoUrl = "https://images.unsplash.com/photo-1554118811-1e0d58224f24?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.07203,
                Longitud = -76.94205,
                CalificacionPromedio = 4.6,
                TotalResenas = 3
            },
            new()
            {
                Nombre = "Huarique Doña Rossi",
                Descripcion = "El clásico huarique preferido por los estudiantes de sistemas e industrial. Platos bien taipá, sazón casera norteña y atención al toque frente al campus.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana 1270 · Frente a Puerta 1 FIA",
                DistanciaTexto = "A 40m cruzando Av. La Fontana",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Vie: 11:30 AM - 4:30 PM",
                TelefonoWhatsApp = "991234567",
                FotoUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.07270,
                Longitud = -76.94235,
                CalificacionPromedio = 4.9,
                TotalResenas = 2
            },
            new()
            {
                Nombre = "Chifa Dragón Dorado La Fontana",
                Descripcion = "Chifa rápido con porciones contundentes para grupos de estudio. Aeropuertos gigantes, sopa wantán especial y chaufas al wok recién salidos.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana 1310 · Urb. Santa Patricia",
                DistanciaTexto = "A 1 cuadra de la FIA (120m)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:00 PM",
                TelefonoWhatsApp = "978112233",
                FotoUrl = "https://images.unsplash.com/photo-1525755662778-989d0524087e?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.07285,
                Longitud = -76.94420,
                CalificacionPromedio = 4.5,
                TotalResenas = 2
            },
            new()
            {
                Nombre = "Tambo+ / Snack Fontana",
                Descripcion = "Bebidas frías, sánguches listos, empanadas, café caliente y snacks rápidos para recargar energías entre entregas de laboratorio.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana con Av. Flora Tristán",
                DistanciaTexto = "A 100m en el cruce con Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 24 Horas",
                TelefonoWhatsApp = "998877665",
                FotoUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.07310,
                Longitud = -76.94080,
                CalificacionPromedio = 4.4,
                TotalResenas = 1
            },

            // 2. Restaurantes Reales obtenidos con Google Places API (Av. La Fontana & Flora Tristán)
            new()
            {
                Nombre = "Don Tito La Molina",
                Descripcion = "Uno de los pollos a la brasa más reconocidos de La Molina. Crujiente, bien dorado, papas nativas y cremas de la casa para almuerzos en grupo.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 434, La Molina",
                DistanciaTexto = "A 125m del campus FIA",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 11:30 AM - 10:30 PM",
                TelefonoWhatsApp = "016809200",
                FotoUrl = "https://images.unsplash.com/photo-1598515214211-89d3c73ae83b?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.071068,
                Longitud = -76.941437,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Shimaya Ramen - Flora Tristán",
                Descripcion = "Especialidad en ramen japonés tradicional, gyozas crocantes y bowls teriyaki. Excelente ambiente para desconectar después de parciales.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 471, La Molina",
                DistanciaTexto = "A 150m caminando de la FIA",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "012965152",
                FotoUrl = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.070608,
                Longitud = -76.942158,
                CalificacionPromedio = 4.4,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Pastelería & Panadería Santa Patricia",
                Descripcion = "Pan recién horneado, empanadas artesanales, triples, pasteles y café pasado para desayunos tempranos antes de entrar al pabellón.",
                Categoria = CategoriaRestaurante.PasteleriaPostres,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 443, La Molina",
                DistanciaTexto = "A 120m de la puerta principal",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 6:00 AM - 10:00 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.070912,
                Longitud = -76.941914,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Pastipan Flora Tristán",
                Descripcion = "Cafetería y panadería tradicional. Postres individuales, tortas, sándwiches calientes y jugos naturales a un paso de la universidad.",
                Categoria = CategoriaRestaurante.PasteleriaPostres,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 362, La Molina",
                DistanciaTexto = "A 130m cruzando Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 6:30 AM - 9:00 PM",
                TelefonoWhatsApp = "014492000",
                FotoUrl = "https://images.unsplash.com/photo-1517433670267-08bbd4be890f?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.071840,
                Longitud = -76.940881,
                CalificacionPromedio = 4.3,
                TotalResenas = 0
            },
            new()
            {
                Nombre = "Isushi La Fontana",
                Descripcion = "Makis por tablas y combos variados, acevichados, furai y california. Opción rápida y moderna para compartir entre compañeros.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana 1345, La Molina",
                DistanciaTexto = "A 170m en Av. La Fontana",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "948487532",
                FotoUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.072991,
                Longitud = -76.940814,
                CalificacionPromedio = 3.9,
                TotalResenas = 0
            },
            new()
            {
                Nombre = "Pizza Raúl La Fontana",
                Descripcion = "Pizzas enteras y por porción, lasagnas y bebidas a precios muy accesibles para el bolsillo estudiantil.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana 1241, La Molina",
                DistanciaTexto = "A 100m cruzando la pista de la FIA",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 11:00 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.072890,
                Longitud = -76.942950,
                CalificacionPromedio = 4.0,
                TotalResenas = 0
            },
            new()
            {
                Nombre = "Sanguchería El Chinito (Flora Tristán)",
                Descripcion = "Tradición criolla desde 1960. Sándwiches de chicharrón, pavo asado y lechón con camote frito y sarza criolla de cebolla.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 591, La Molina",
                DistanciaTexto = "A 350m en Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Mar - Dom: 7:00 AM - 9:00 PM",
                TelefonoWhatsApp = "981563378",
                FotoUrl = "https://images.unsplash.com/photo-1509722747041-616f39b57569?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.068996,
                Longitud = -76.942957,
                CalificacionPromedio = 4.1,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Restaurante Huancahuasi La Molina",
                Descripcion = "Cocina tradicional andina y criolla: pachamanca, trucha frita, patasca y choclo con queso para celebraciones o visitas familiares.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Premium,
                Direccion = "Av. Flora Tristán 585, La Molina",
                DistanciaTexto = "A 320m de la FIA",
                RangoPrecios = "Premium",
                HorarioAtencion = "Lun - Dom: 8:00 AM - 6:00 PM",
                TelefonoWhatsApp = "015003901",
                FotoUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=600&q=80",
                Latitud = -12.069238,
                Longitud = -76.942813,
                CalificacionPromedio = 4.4,
                TotalResenas = 0
            }
        };

        db.Restaurantes.AddRange(restaurantes);
        await db.SaveChangesAsync();

        // Reseñas de estudiantes
        if (maria != null && juan != null && ana != null && carlos != null)
        {
            var rCafeteria = restaurantes[0].Id;
            var rDonaRossi = restaurantes[1].Id;
            var rChifa = restaurantes[2].Id;
            var rDonTito = restaurantes[4].Id;
            var rShimaya = restaurantes[5].Id;

            var resenas = new List<ResenaRestaurante>
            {
                new() { RestauranteId = rCafeteria, UsuarioId = maria.Id, Puntuacion = 5, Comentario = "Muy cómodo para no salir de la facultad entre clases, atienden rápido y el café salva vidas en parciales.", FechaUtc = DateTime.UtcNow.AddDays(-3) },
                new() { RestauranteId = rCafeteria, UsuarioId = juan.Id, Puntuacion = 4, Comentario = "Buena atención y limpio, solo que a la 1:00 PM se llena bastante de alumnos de civil y sistemas.", FechaUtc = DateTime.UtcNow.AddDays(-5) },
                new() { RestauranteId = rDonaRossi, UsuarioId = carlos.Id, Puntuacion = 5, Comentario = "¡Doña Rossi es insuperable! El Lomo Saltado y el Arroz con pollo son los mejores de La Fontana. Recomendadísimo.", FechaUtc = DateTime.UtcNow.AddDays(-2) },
                new() { RestauranteId = rDonaRossi, UsuarioId = ana.Id, Puntuacion = 5, Comentario = "Muy buena porción y sazón casera, perfecto para el presupuesto de nosotros los estudiantes.", FechaUtc = DateTime.UtcNow.AddDays(-1) },
                new() { RestauranteId = rChifa, UsuarioId = juan.Id, Puntuacion = 5, Comentario = "El aeropuerto es inmenso, comemos 2 personas con un solo plato. Gran huarique para celebrar fin de ciclo.", FechaUtc = DateTime.UtcNow.AddDays(-7) },
                new() { RestauranteId = rDonTito, UsuarioId = carlos.Id, Puntuacion = 5, Comentario = "El mejor pollo a la brasa cerca de la universidad. Para venir en grupo después de presentar proyectos.", FechaUtc = DateTime.UtcNow.AddDays(-4) },
                new() { RestauranteId = rShimaya, UsuarioId = maria.Id, Puntuacion = 5, Comentario = "Ramen excelente y bien caliente. El caldo tonkotsu es buenazo.", FechaUtc = DateTime.UtcNow.AddDays(-6) }
            };

            db.ResenasRestaurantes.AddRange(resenas);
            await db.SaveChangesAsync();
        }
    }
}
