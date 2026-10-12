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
            // --- 1. LOCALES EMBLEMÁTICOS DENTRO Y FRENTE A LA FIA ---
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
                FotoUrl = "/images/restaurantes/cafeteria-central-fia.jpg",
                Latitud = -12.07203,
                Longitud = -76.94205,
                CalificacionPromedio = 4.6,
                TotalResenas = 2
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
                FotoUrl = "/images/restaurantes/dona-rossi.jpg",
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
                FotoUrl = "/images/restaurantes/dragon-dorado.jpg",
                Latitud = -12.07285,
                Longitud = -76.94420,
                CalificacionPromedio = 4.5,
                TotalResenas = 1
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
                FotoUrl = "/images/restaurantes/tambo-fontana.jpg",
                Latitud = -12.07310,
                Longitud = -76.94080,
                CalificacionPromedio = 4.4,
                TotalResenas = 1
            },

            // --- 2. RESTAURANTES REALES EXTRAÍDOS CON GOOGLE PLACES API (AV. LA FONTANA) ---
            new()
            {
                Nombre = "Puerto Central",
                Descripcion = "Cevichería y restaurante marino con pesca del día, arroz con mariscos, tríos marinos y chicharrones crujientes frente al campus.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana 1255, La Molina",
                DistanciaTexto = "A 100m frente a Puerta 1 FIA",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 10:00 AM - 5:30 PM",
                TelefonoWhatsApp = "994012345",
                FotoUrl = "/images/restaurantes/puerto-central.jpg",
                Latitud = -12.0729321,
                Longitud = -76.9420679,
                CalificacionPromedio = 4.5,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Tumbes Mar Pescados Y Mariscos",
                Descripcion = "Reconocido restaurante de comida marina norteña. Leche de tigre concentrada, ceviche mixto y causas rellenas a solo metros de la universidad.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana 1249, La Molina",
                DistanciaTexto = "A 101m cruzando La Fontana",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 10:00 AM - 5:00 PM",
                TelefonoWhatsApp = "981234567",
                FotoUrl = "/images/restaurantes/tumbes-mar-pescados-y-mariscos.jpg",
                Latitud = -12.072932,
                Longitud = -76.9421631,
                CalificacionPromedio = 4.1,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Pizza Raúl La Fontana",
                Descripcion = "Pizzas enteras y por porción, lasagnas y gaseosas heladas a precios súper accesibles para el presupuesto estudiantil.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana 1241, La Molina",
                DistanciaTexto = "A 103m cruzando Av. La Fontana",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 11:00 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "/images/restaurantes/pizza-raul-la-fontana-1241.jpg",
                Latitud = -12.072931,
                Longitud = -76.9422806,
                CalificacionPromedio = 4.0,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Mamma Tomato",
                Descripcion = "Pizzas artesanales con masa madre, ingredientes frescos e integrales. Excelente lugar para compartir en equipo en un ambiente moderno.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana 1233, La Molina",
                DistanciaTexto = "A 115m cruzando Av. La Fontana",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "012023300",
                FotoUrl = "/images/restaurantes/mamma-tomato.jpg",
                Latitud = -12.0730011,
                Longitud = -76.9424169,
                CalificacionPromedio = 4.0,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Dachi Café",
                Descripcion = "Cafetería de especialidad con excelente espresso, lattes, pastelería fina, tostadas y ambiente tranquilo con WiFi para avanzar trabajos.",
                Categoria = CategoriaRestaurante.CafeteriaHuarique,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana 1179, La Molina",
                DistanciaTexto = "A 156m en Av. La Fontana",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 8:00 AM - 9:00 PM",
                TelefonoWhatsApp = "987456123",
                FotoUrl = "/images/restaurantes/dachi.jpg",
                Latitud = -12.0729057,
                Longitud = -76.9431716,
                CalificacionPromedio = 5.0,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Isushi La Fontana",
                Descripcion = "Makis por tablas y combos variados (acevichados, furai, california) con rápida atención para almuerzos entre amigos.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana 1345, La Molina",
                DistanciaTexto = "A 171m en Av. La Fontana",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "948487532",
                FotoUrl = "/images/restaurantes/isushi.jpg",
                Latitud = -12.0729913,
                Longitud = -76.9408135,
                CalificacionPromedio = 3.9,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Amore La Molina",
                Descripcion = "Trattoria y pizzería con pastas artesanales, salsas tradicionales, focaccias y postres en un ambiente acogedor.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana 1125, La Molina",
                DistanciaTexto = "A 255m en Av. La Fontana",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:30 PM - 10:30 PM",
                TelefonoWhatsApp = "014378822",
                FotoUrl = "/images/restaurantes/amore-la-molina.jpg",
                Latitud = -12.0728816,
                Longitud = -76.9442291,
                CalificacionPromedio = 4.4,
                TotalResenas = 1
            },

            // --- 3. RESTAURANTES REALES EN AV. FLORA TRISTÁN (A PASOS DEL CAMPUS) ---
            new()
            {
                Nombre = "Santa Patricia (Panadería & Pastelería)",
                Descripcion = "Pan recién salido del horno, triples, empanadas artesanales, café y jugos naturales. El punto fijo para desayunar antes de las 8:00 AM.",
                Categoria = CategoriaRestaurante.PasteleriaPostres,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 443, La Molina",
                DistanciaTexto = "A 125m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 6:00 AM - 10:00 PM",
                TelefonoWhatsApp = "013491234",
                FotoUrl = "/images/restaurantes/santa-patricia.jpg",
                Latitud = -12.0709117,
                Longitud = -76.9419144,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Don Tito La Molina",
                Descripcion = "El pollo a la brasa más reconocido de la zona. Piel crocante, papas nativas doradas y ají de la casa para celebraciones en grupo.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 434, La Molina",
                DistanciaTexto = "A 126m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 11:30 AM - 10:30 PM",
                TelefonoWhatsApp = "016809200",
                FotoUrl = "/images/restaurantes/don-tito-la-molina.jpg",
                Latitud = -12.0710684,
                Longitud = -76.9414369,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Pastipan Flora Tristán",
                Descripcion = "Cafetería, tortas, pasteles, sándwiches calientes y jugos. Ideal para reuniones tranquilas de trabajo grupal.",
                Categoria = CategoriaRestaurante.PasteleriaPostres,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 362, La Molina",
                DistanciaTexto = "A 129m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 6:30 AM - 9:00 PM",
                TelefonoWhatsApp = "014492000",
                FotoUrl = "/images/restaurantes/pastipan-flora-trist-n.jpg",
                Latitud = -12.0718395,
                Longitud = -76.9408812,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Shimaya Ramen",
                Descripcion = "Ramen japonés con caldos cocinados por horas, gyozas crocantes y bowls teriyaki. Muy popular entre los alumnos de ingeniería.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 471, La Molina",
                DistanciaTexto = "A 158m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "012965152",
                FotoUrl = "/images/restaurantes/shimaya-ramen.jpg",
                Latitud = -12.0706083,
                Longitud = -76.9421576,
                CalificacionPromedio = 4.4,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Secretos Árabes",
                Descripcion = "Auténtica comida de Medio Oriente: shawarmas bien servidos de carne y pollo, falafel, hummus y enrollados al paso.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 318, La Molina",
                DistanciaTexto = "A 166m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 11:00 PM",
                TelefonoWhatsApp = "986341209",
                FotoUrl = "/images/restaurantes/secretos-rabes.jpg",
                Latitud = -12.0722306,
                Longitud = -76.9405361,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Cevichería El Verídico de Fidel",
                Descripcion = "Famoso por su Leche de Tigre de campeonato y ceviches norteños con choclo y camote glaseado. Excelente opción de fin de semana.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 527, La Molina",
                DistanciaTexto = "A 227m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 10:00 AM - 5:30 PM",
                TelefonoWhatsApp = "907477471",
                FotoUrl = "/images/restaurantes/cevicher-a-el-ver-dico-de-fide.jpg",
                Latitud = -12.070012,
                Longitud = -76.942396,
                CalificacionPromedio = 4.2,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Fuego & Brasa",
                Descripcion = "Pollo a la brasa dorado y crujiente, porciones generosas de papas fritas y ensaladas frescas a precio accesible.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 222, La Molina",
                DistanciaTexto = "A 312m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "998123456",
                FotoUrl = "/images/restaurantes/fuego-brasa.jpg",
                Latitud = -12.073277,
                Longitud = -76.9394743,
                CalificacionPromedio = 4.6,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Punto Italiano / Flora",
                Descripcion = "Pizzas al horno, lasagnas bolognesa y pastas frescas elaboradas con recetas italianas tradicionales.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 564, La Molina",
                DistanciaTexto = "A 317m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 11:00 PM",
                TelefonoWhatsApp = "902706964",
                FotoUrl = "/images/restaurantes/punto-italiano-flora.jpg",
                Latitud = -12.0692057,
                Longitud = -76.9424569,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Restaurante Huancahuasi La Molina",
                Descripcion = "Cocina tradicional andina y criolla: pachamanca de tres carnes, trucha frita, patasca y choclo con queso para ocasiones especiales.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Premium,
                Direccion = "Av. Flora Tristán 585, La Molina",
                DistanciaTexto = "A 321m en Av. Flora Tristán",
                RangoPrecios = "Premium",
                HorarioAtencion = "Lun - Dom: 8:00 AM - 6:00 PM",
                TelefonoWhatsApp = "015003901",
                FotoUrl = "/images/restaurantes/restaurante-huancahuasi-la-mol.jpg",
                Latitud = -12.069238,
                Longitud = -76.942813,
                CalificacionPromedio = 4.4,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Sanguchería El Chinito",
                Descripcion = "Tradición criolla desde 1960. Los clásicos sánguches de chicharrón con camote frito, sarza criolla de cebolla y ají molido.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 591, La Molina",
                DistanciaTexto = "A 351m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Mar - Dom: 7:00 AM - 9:00 PM",
                TelefonoWhatsApp = "981563378",
                FotoUrl = "/images/restaurantes/sangucheria-el-chinito.jpg",
                Latitud = -12.0689963,
                Longitud = -76.9429568,
                CalificacionPromedio = 4.1,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Anticuchos Bran - La Molina",
                Descripcion = "Anticuchos de corazón tiernos a la parrilla, rachi, pancita y choclos tiernos con papas doradas en un ambiente acogedor.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 615 A, La Molina",
                DistanciaTexto = "A 393m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Mar - Dom: 5:00 PM - 11:00 PM",
                TelefonoWhatsApp = "982345678",
                FotoUrl = "/images/restaurantes/anticuchos-bran-la-molina.jpg",
                Latitud = -12.0686665,
                Longitud = -76.9431732,
                CalificacionPromedio = 3.9,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Mero Loco 2",
                Descripcion = "Cevichería norteña de porciones generosas. Arroz chaufa de mariscos, jaleas mixtas crocantes y parihuelas reconstituyentes.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 619, La Molina",
                DistanciaTexto = "A 411m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 10:00 AM - 5:00 PM",
                TelefonoWhatsApp = "013495522",
                FotoUrl = "/images/restaurantes/mero-loco-2.jpg",
                Latitud = -12.0685292,
                Longitud = -76.9432745,
                CalificacionPromedio = 3.9,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "El Aguajal (La Molina)",
                Descripcion = "Especialistas en gastronomía de la Amazonía peruana: juane de gallina, tacacho con cecina y chorizo, chaufa amazónico y jugos de camu camu.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 624, La Molina",
                DistanciaTexto = "A 423m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 11:00 AM - 7:00 PM",
                TelefonoWhatsApp = "013491188",
                FotoUrl = "/images/restaurantes/el-aguajal-la-molina.jpg",
                Latitud = -12.0683208,
                Longitud = -76.9429489,
                CalificacionPromedio = 4.2,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Ohashi Maki",
                Descripcion = "Makis fusión nikkei en barra libre o tablas seleccionadas, yakisoba y alitas acevichadas para celebrar luego de entregas de proyectos.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 632, La Molina",
                DistanciaTexto = "A 437m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:30 PM - 10:30 PM",
                TelefonoWhatsApp = "989445566",
                FotoUrl = "/images/restaurantes/ohashi-maki.jpg",
                Latitud = -12.068214,
                Longitud = -76.9430379,
                CalificacionPromedio = 3.6,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "El Huerto Florido",
                Descripcion = "Restaurante con menús ejecutivos balanceados, ensaladas completas, pechugas a la plancha, extractos naturales y platos criollos caseros.",
                Categoria = CategoriaRestaurante.Saludable,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 660, La Molina",
                DistanciaTexto = "A 461m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 8:00 AM - 6:00 PM",
                TelefonoWhatsApp = "013490011",
                FotoUrl = "/images/restaurantes/el-huerto-florido.jpg",
                Latitud = -12.0680129,
                Longitud = -76.943139,
                CalificacionPromedio = 3.7,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Las Canastas - La Molina",
                Descripcion = "Pollo a la brasa con papas crocantes, parrilladas familiares y piqueos criollos para grupos grandes de compañeros de carrera.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Flora Tristán 682, La Molina",
                DistanciaTexto = "A 527m en Av. Flora Tristán",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 11:00 PM",
                TelefonoWhatsApp = "015183000",
                FotoUrl = "/images/restaurantes/las-canastas-la-molina.jpg",
                Latitud = -12.0674786,
                Longitud = -76.9434239,
                CalificacionPromedio = 4.2,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Puerto Máncora La Molina",
                Descripcion = "Restaurante norteño con langostinos al panko, ceviche mixto, parihuela y sudado de pescado fresco en Av. Javier Prado.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Javier Prado Este 6768, La Molina",
                DistanciaTexto = "A 583m cruzando hacia Javier Prado",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 10:00 AM - 6:00 PM",
                TelefonoWhatsApp = "013494488",
                FotoUrl = "/images/restaurantes/puerto-m-ncora-la-molina-saz-n.jpg",
                Latitud = -12.0676026,
                Longitud = -76.944938,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },

            new()
            {
                Nombre = "Las Tinajas Chicken & Grill",
                Descripcion = "Pollería y parrillas reconocidas en Javier Prado Este. Porciones generosas de pollo a la brasa, chicha morada casera y ensaladas.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Javier Prado Este 7084, La Molina",
                DistanciaTexto = "A 645m hacia Javier Prado",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 11:00 PM",
                TelefonoWhatsApp = "013492211",
                FotoUrl = "/images/restaurantes/las-tinajas-chicken-grill.jpg",
                Latitud = -12.0662196,
                Longitud = -76.9422381,
                CalificacionPromedio = 4.2,
                TotalResenas = 1
            },

            // --- 4. HUARIQUES Y MENÚS CASEROS DETECTADOS EN ZOOM ALTO (FLORA TRISTÁN Y LA FONTANA) ---
            new()
            {
                Nombre = "Pim Pam",
                Descripcion = "El clásico huarique de menú casero preferido por los estudiantes. Sopa de entrada, platos de fondo criollos bien servidos, refresco natural y atención rápida.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Jr. Bogotá 297 (Esq. Flora Tristán), La Molina",
                DistanciaTexto = "A 230m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 11:30 AM - 4:30 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "/images/restaurantes/pim-pam.jpg",
                Latitud = -12.069951,
                Longitud = -76.9419579,
                CalificacionPromedio = 4.4,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Oasis Café-Restaurante",
                Descripcion = "Menús ejecutivos y estudiantiles diarios, café pasado, lomo saltado, pollo al jugo y milanesas con ensalada fresca a precio muy cómodo.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 478, La Molina",
                DistanciaTexto = "A 180m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 8:00 AM - 8:00 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "/images/restaurantes/oasis-cafe-restaurante.jpg",
                Latitud = -12.070375,
                Longitud = -76.9418491,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Pastelería Las Delicias",
                Descripcion = "Empanadas de carne y pollo recién horneadas, sándwiches, alfajores artesanales, café caliente y jugos para el desayuno o break de media tarde.",
                Categoria = CategoriaRestaurante.PasteleriaPostres,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 509, La Molina",
                DistanciaTexto = "A 210m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 7:00 AM - 9:30 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "/images/restaurantes/pasteler-a-las-delicias.jpg",
                Latitud = -12.070232,
                Longitud = -76.9422749,
                CalificacionPromedio = 4.5,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Menú Rakus",
                Descripcion = "Huarique de menú universitario al toque de la esquina. Porciones contundentes, sazón casera y precios al alcance del bolsillo de los alumnos.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 394, La Molina",
                DistanciaTexto = "A 135m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 11:30 AM - 4:00 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "/images/restaurantes/men-rakus.jpg",
                Latitud = -12.0716738,
                Longitud = -76.9410831,
                CalificacionPromedio = 4.5,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Cevichería Miramar La Molina",
                Descripcion = "Cebichería y menú marino tradicional. Tríos marinos, arroz con mariscos, chilcano caliente y combinados con chicharrón de pescado a precio accesible.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 444, La Molina",
                DistanciaTexto = "A 140m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 9:30 AM - 5:00 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "/images/restaurantes/cevicher-a-miramar-la-molina.jpg",
                Latitud = -12.070923,
                Longitud = -76.9414678,
                CalificacionPromedio = 4.2,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Mr. Chino",
                Descripcion = "Chifa express frente a la puerta del campus universitario. Chaufas al wok, tallarín saltado y menús orientales contundentes servidos al instante.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana 1253, La Molina",
                DistanciaTexto = "A 95m cruzando Puerta 1 FIA",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 11:30 AM - 10:00 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "/images/restaurantes/mr-chino.jpg",
                Latitud = -12.0729084,
                Longitud = -76.9425046,
                CalificacionPromedio = 4.1,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Chifa El Dorado",
                Descripcion = "Combos de aeropuerto, sopa wantán familiar y menús chifa económicos para almorzar rápido entre turnos de clases.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 431, La Molina",
                DistanciaTexto = "A 130m en Av. Flora Tristán",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = null,
                FotoUrl = "/images/restaurantes/chifa-el-dorado.jpg",
                Latitud = -12.0710295,
                Longitud = -76.941843,
                CalificacionPromedio = 4.2,
                TotalResenas = 1
            }
        };

        db.Restaurantes.AddRange(restaurantes);
        await db.SaveChangesAsync();

        // --- RESEÑAS REALES DE ESTUDIANTES FIA USMP ---
        if (maria != null && juan != null && ana != null && carlos != null)
        {
            var resenas = new List<ResenaRestaurante>
            {
                // Cafetería Central
                new() { RestauranteId = restaurantes[0].Id, UsuarioId = maria.Id, Puntuacion = 5, Comentario = "Muy cómodo para no salir de la facultad entre clases, atienden rápido y el café salva vidas en parciales.", FechaUtc = DateTime.UtcNow.AddDays(-3) },
                new() { RestauranteId = restaurantes[0].Id, UsuarioId = juan.Id, Puntuacion = 4, Comentario = "Buena atención y limpio, solo que a la 1:00 PM se llena bastante de alumnos de civil y sistemas.", FechaUtc = DateTime.UtcNow.AddDays(-5) },

                // Doña Rossi
                new() { RestauranteId = restaurantes[1].Id, UsuarioId = carlos.Id, Puntuacion = 5, Comentario = "¡Doña Rossi es insuperable! El Lomo Saltado y el Arroz con pollo son los mejores de La Fontana. Recomendadísimo.", FechaUtc = DateTime.UtcNow.AddDays(-2) },
                new() { RestauranteId = restaurantes[1].Id, UsuarioId = ana.Id, Puntuacion = 5, Comentario = "Muy buena porción y sazón casera, perfecto para el presupuesto de nosotros los estudiantes.", FechaUtc = DateTime.UtcNow.AddDays(-1) },

                // Dragón Dorado
                new() { RestauranteId = restaurantes[2].Id, UsuarioId = juan.Id, Puntuacion = 5, Comentario = "El aeropuerto es inmenso, comemos 2 personas con un solo plato. Gran huarique para celebrar fin de ciclo.", FechaUtc = DateTime.UtcNow.AddDays(-7) },

                // Tambo
                new() { RestauranteId = restaurantes[3].Id, UsuarioId = ana.Id, Puntuacion = 4, Comentario = "Clave para comprar agua, energizantes o café antes de entrar a sustentar en los laboratorios de cómputo.", FechaUtc = DateTime.UtcNow.AddDays(-6) },

                // Puerto Central
                new() { RestauranteId = restaurantes[4].Id, UsuarioId = carlos.Id, Puntuacion = 5, Comentario = "El trío marino sale al toque y está frente a la puerta 1. Muy fresco y buen tamaño.", FechaUtc = DateTime.UtcNow.AddDays(-4) },

                // Tumbes Mar
                new() { RestauranteId = restaurantes[5].Id, UsuarioId = maria.Id, Puntuacion = 4, Comentario = "Buen ceviche clásico norteño con harta canchita y camote, recomendado para los viernes.", FechaUtc = DateTime.UtcNow.AddDays(-5) },

                // Pizza Raúl
                new() { RestauranteId = restaurantes[6].Id, UsuarioId = juan.Id, Puntuacion = 4, Comentario = "Súper económico para comprar una pizza grande entre 3 amigos cuando nos quedamos avanzando el proyecto.", FechaUtc = DateTime.UtcNow.AddDays(-8) },

                // Mamma Tomato
                new() { RestauranteId = restaurantes[7].Id, UsuarioId = ana.Id, Puntuacion = 4, Comentario = "La masa es buenísima y diferente a las comerciales, buen ambiente para conversar tranquilos.", FechaUtc = DateTime.UtcNow.AddDays(-3) },

                // Dachi Café
                new() { RestauranteId = restaurantes[8].Id, UsuarioId = maria.Id, Puntuacion = 5, Comentario = "El mejor café de especialidad cerca a la facultad. Se puede llevar la laptop y programar sin bulla.", FechaUtc = DateTime.UtcNow.AddDays(-2) },

                // Don Tito
                new() { RestauranteId = restaurantes[11].Id, UsuarioId = carlos.Id, Puntuacion = 5, Comentario = "El mejor pollo a la brasa cerca de la universidad. Para venir en grupo después de presentar proyectos.", FechaUtc = DateTime.UtcNow.AddDays(-4) },

                // Shimaya Ramen
                new() { RestauranteId = restaurantes[14].Id, UsuarioId = maria.Id, Puntuacion = 5, Comentario = "Ramen excelente y bien caliente. El caldo tonkotsu es buenazo en época de frío.", FechaUtc = DateTime.UtcNow.AddDays(-6) },

                // Secretos Árabes
                new() { RestauranteId = restaurantes[15].Id, UsuarioId = juan.Id, Puntuacion = 5, Comentario = "Los shawarmas son contundentes y salen al toque. Ideal si tienes solo 30 minutos de break.", FechaUtc = DateTime.UtcNow.AddDays(-1) }
            };

            db.ResenasRestaurantes.AddRange(resenas);
            await db.SaveChangesAsync();
        }
    }
}
