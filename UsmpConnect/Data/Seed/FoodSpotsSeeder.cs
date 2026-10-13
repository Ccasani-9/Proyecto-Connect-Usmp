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
                HorarioAtencion = "Lun: 8:30 AM - 7:00 PM | Mar-Vie: 8:30 AM - 9:00 PM | Sáb: 9:00 AM - 3:00 PM",
                TelefonoWhatsApp = "987654321",
                FotoUrl = "/images/restaurantes/cafeteria-central-fia.jpg",
                Latitud = -12.07203,
                Longitud = -76.94205,
                CalificacionPromedio = 4.6,
                TotalResenas = 2
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
            },
            new()
            {
                Nombre = "Pardos Chicken La Molina",
                Descripcion = "El auténtico pollo a la brasa peruano de receta tradicional con papas crujientes, ensalada fresca y salsas de la casa en Av. Javier Prado.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. Javier Prado Este 6495 · Esq. Jr. Los Civiles",
                DistanciaTexto = "A 900m de la FIA (11 min a pie)",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 11:00 PM",
                TelefonoWhatsApp = "981234561",
                FotoUrl = "/images/restaurantes/pardos-chicken-la-molina.jpg",
                Latitud = -12.068524,
                Longitud = -76.9489191,
                CalificacionPromedio = 4.2,
                TotalResenas = 5
            },
            new()
            {
                Nombre = "Roll Star Sushi Bar",
                Descripcion = "Especialistas en makis fusionados, rolls acevichados, gyozas y cocina nikkei en formato barra para compartir con amigos de la universidad.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana esq. Jr. Loreto · Cerca de ISIL",
                DistanciaTexto = "A 650m de la FIA (8 min a pie)",
                RangoPrecios = "Medio",
                HorarioAtencion = "Mar - Dom: 1:00 PM - 10:30 PM",
                TelefonoWhatsApp = "987112233",
                FotoUrl = "/images/restaurantes/roll-star-sushi-bar.jpg",
                Latitud = -12.0727725,
                Longitud = -76.9480586,
                CalificacionPromedio = 3.7,
                TotalResenas = 3
            },
            new()
            {
                Nombre = "Pastelería Mariate",
                Descripcion = "Pastelería fina y cafetería artesanal. Famosa por sus tortas de chocolate, pies de limón, alfajores de maicena y café espresso.",
                Categoria = CategoriaRestaurante.CafeteriaHuarique,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana 996 · Frente a ISIL La Molina",
                DistanciaTexto = "A 600m de la FIA (7 min a pie)",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Sáb: 8:30 AM - 8:30 PM",
                TelefonoWhatsApp = "993445566",
                FotoUrl = "/images/restaurantes/pasteleria-mariate-la-fontana.jpg",
                Latitud = -12.0727867,
                Longitud = -76.9476893,
                CalificacionPromedio = 4.7,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Punto Italiano (La Fontana)",
                Descripcion = "Pastas artesanales, lasagñas, pizzas al horno de leña y postres tradicionales italianos en un ambiente acogedor sobre la Av. La Fontana.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana 848 · Esq. Av. Los Ingenieros",
                DistanciaTexto = "A 850m de la FIA (10 min a pie)",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 12:30 PM - 10:30 PM",
                TelefonoWhatsApp = "984556677",
                FotoUrl = "/images/restaurantes/punto-italiano-la-fontana.jpg",
                Latitud = -12.0731566,
                Longitud = -76.9500156,
                CalificacionPromedio = 4.5,
                TotalResenas = 4
            },
            new()
            {
                Nombre = "Chifa Moneda China",
                Descripcion = "Chifa tradicional cantonés con platos gigantes taipá, wantanes crocantes, arroz chaufa de la casa y combos ideales para grupos estudiantiles.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana 824 · Altura Av. Los Ingenieros",
                DistanciaTexto = "A 900m de la FIA (11 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "991223344",
                FotoUrl = "/images/restaurantes/chifa-moneda-china.jpg",
                Latitud = -12.0731789,
                Longitud = -76.9501775,
                CalificacionPromedio = 4.0,
                TotalResenas = 3
            },
            new()
            {
                Nombre = "Starbucks La Fontana",
                Descripcion = "Cafetería con Wi-Fi veloz, frappuccinos, bebidas frías, sándwiches y ambiente tranquilo ideal para estudiar o hacer proyectos de facultad.",
                Categoria = CategoriaRestaurante.CafeteriaHuarique,
                Rango = RangoPrecio.Medio,
                Direccion = "Av. La Fontana esq. Av. Los Ingenieros",
                DistanciaTexto = "A 900m de la FIA (11 min a pie)",
                RangoPrecios = "Medio",
                HorarioAtencion = "Lun - Dom: 7:00 AM - 10:00 PM",
                TelefonoWhatsApp = "998877665",
                FotoUrl = "/images/restaurantes/starbucks-la-fontana.jpg",
                Latitud = -12.0732198,
                Longitud = -76.9504108,
                CalificacionPromedio = 4.4,
                TotalResenas = 5
            },
            new()
            {
                Nombre = "Sanguchón Campesino",
                Descripcion = "Sánguches generosos de lechón al horno, pavo criollo, lomo ahumado y hamburguesas artesanales servidas con cremas peruanas.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Javier Prado Este 7095 · Cerca a Flora Tristán",
                DistanciaTexto = "A 800m de la FIA (10 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 5:00 PM - 2:00 AM",
                TelefonoWhatsApp = "981992288",
                FotoUrl = "/images/restaurantes/sanguchon-campesino.jpg",
                Latitud = -12.0654946,
                Longitud = -76.9423352,
                CalificacionPromedio = 3.9,
                TotalResenas = 3
            },
            new()
            {
                Nombre = "Full Sanguchón La Fontana",
                Descripcion = "Huarique nocturno de salchipapas monstruo, hamburguesas caseras, alitas broaster y combos rápidos a buen precio para estudiantes.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. La Fontana 836 · Cerca a Los Ingenieros",
                DistanciaTexto = "A 880m de la FIA (11 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 5:30 PM - 1:00 AM",
                TelefonoWhatsApp = "982334455",
                FotoUrl = "/images/restaurantes/full-sanguchon-la-fontana.jpg",
                Latitud = -12.073168,
                Longitud = -76.9501224,
                CalificacionPromedio = 3.7,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Chifa Ruyi",
                Descripcion = "Chifa clásico de la Av. Flora Tristán, con wantanes rellenos, tallarines taypa, sopas wantán calientes y menú ejecutivo al mediodía.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 715 · Urb. Santa Patricia",
                DistanciaTexto = "A 750m de la FIA (9 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "987445511",
                FotoUrl = "/images/restaurantes/chifa-ruyi.jpg",
                Latitud = -12.0661127,
                Longitud = -76.9445243,
                CalificacionPromedio = 4.1,
                TotalResenas = 2
            },
            new()
            {
                Nombre = "Macartur Parrillas & Menú",
                Descripcion = "Sazón casera criolla, parrilladas personales, menús del día y guisos norteños a pasos de la Facultad de Ingeniería.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Calle Carhuaquero 103 · Altura Av. La Fontana",
                DistanciaTexto = "A 120m de la FIA (2 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 11:30 AM - 5:00 PM",
                TelefonoWhatsApp = "993112233",
                FotoUrl = "/images/restaurantes/macartur-parrillas.jpg",
                Latitud = -12.072951,
                Longitud = -76.9419045,
                CalificacionPromedio = 4.0,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Club Sandwich Flora Tristán",
                Descripcion = "Sánguches triples, club sándwich con papas fritas, cafés americanos, jugos naturales y hamburguesas al paso.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 690 · Santa Patricia",
                DistanciaTexto = "A 600m de la FIA (7 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 8:00 AM - 10:00 PM",
                TelefonoWhatsApp = "981223344",
                FotoUrl = "/images/restaurantes/club-sandwich-flora.jpg",
                Latitud = -12.0671733,
                Longitud = -76.9435731,
                CalificacionPromedio = 3.9,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Satori Nikkei",
                Descripcion = "Alta cocina de fusión peruano-japonesa, makis de autor con flameado, ceviches nikkei y tiraditos de pesca del día.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Premium,
                Direccion = "Av. Javier Prado Este 6895 · Esq. Flora Tristán",
                DistanciaTexto = "A 700m de la FIA (9 min a pie)",
                RangoPrecios = "Premium",
                HorarioAtencion = "Mar - Dom: 12:30 PM - 11:00 PM",
                TelefonoWhatsApp = "994556677",
                FotoUrl = "/images/restaurantes/satori-nikkei.jpg",
                Latitud = -12.0664806,
                Longitud = -76.9443094,
                CalificacionPromedio = 4.8,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Grido Helado Flora Tristán",
                Descripcion = "Heladería accesible con gran variedad de sabores de crema, paletas, tortas heladas y sundae para refrescarse saliendo de clases.",
                Categoria = CategoriaRestaurante.CafeteriaHuarique,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 404 · Cerca Av. La Fontana",
                DistanciaTexto = "A 200m de la FIA (3 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 11:00 AM - 10:00 PM",
                TelefonoWhatsApp = "987665544",
                FotoUrl = "/images/restaurantes/grido-helado-flora.jpg",
                Latitud = -12.0714566,
                Longitud = -76.9411793,
                CalificacionPromedio = 4.6,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Cremoladas Angélica",
                Descripcion = "Las cremoladas de pura fruta más famosas de La Molina: fresa, mango, lúcuma, maracuyá y combinados tropicales.",
                Categoria = CategoriaRestaurante.CafeteriaHuarique,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 295 · Santa Patricia",
                DistanciaTexto = "A 400m de la FIA (5 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 10:00 AM - 7:00 PM",
                TelefonoWhatsApp = "993223311",
                FotoUrl = "/images/restaurantes/cremoladas-angelica.jpg",
                Latitud = -12.0686117,
                Longitud = -76.942785,
                CalificacionPromedio = 4.2,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Cevichería Punta del Este",
                Descripcion = "Huarique marino escondido en Santa Patricia: ceviche carretillero con chicharrón de calamar, arroz con mariscos y leche de tigre.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Calle Arequipa 140 · Urb. Santa Patricia",
                DistanciaTexto = "A 550m de la FIA (7 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Mar - Dom: 10:30 AM - 5:00 PM",
                TelefonoWhatsApp = "981445566",
                FotoUrl = "/images/restaurantes/cevicher-a-punta-del-este.jpg",
                Latitud = -12.0687511,
                Longitud = -76.9470315,
                CalificacionPromedio = 4.3,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Chifa Lun Ma",
                Descripcion = "Sabor oriental clásico con wantanes crocantes, pollo Chi Jau Kay, Tipakay y porciones generosas para compartir.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Flora Tristán 729 · Santa Patricia",
                DistanciaTexto = "A 780m de la FIA (10 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:30 PM",
                TelefonoWhatsApp = "992112244",
                FotoUrl = "/images/restaurantes/chifa-lun-ma.jpg",
                Latitud = -12.0659037,
                Longitud = -76.9446104,
                CalificacionPromedio = 4.1,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Chifa Yin Dou",
                Descripcion = "Chifa de barrio acogedor en calle Arequipa, famoso por su Kam Lu Wantán, tallarín saltado y menú al paso bien servido.",
                Categoria = CategoriaRestaurante.Chifa,
                Rango = RangoPrecio.Economico,
                Direccion = "Jr. Arequipa 188 · Urb. Santa Patricia",
                DistanciaTexto = "A 500m de la FIA (6 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 12:00 PM - 10:00 PM",
                TelefonoWhatsApp = "987556611",
                FotoUrl = "/images/restaurantes/chifa-yin-dou.jpg",
                Latitud = -12.069347,
                Longitud = -76.946812,
                CalificacionPromedio = 4.0,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Dacsabor Restaurant & Menú",
                Descripcion = "Menú criollo diario con entrada y segundo, platos ejecutivos y refresco incluido sobre la Av. Javier Prado.",
                Categoria = CategoriaRestaurante.MenuCriollo,
                Rango = RangoPrecio.Economico,
                Direccion = "Av. Javier Prado Este 6377 · Cerca Los Ingenieros",
                DistanciaTexto = "A 880m de la FIA (11 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Sáb: 11:30 AM - 4:30 PM",
                TelefonoWhatsApp = "993334411",
                FotoUrl = "/images/restaurantes/dacsabor-restaurant.jpg",
                Latitud = -12.0687304,
                Longitud = -76.9494634,
                CalificacionPromedio = 3.5,
                TotalResenas = 1
            },
            new()
            {
                Nombre = "Don Lucho (Salchipapas & Broaster)",
                Descripcion = "Clásico huarique nocturno de salchipapas campesinas, alitas broaster crocantes, hamburguesas y cremas variadas.",
                Categoria = CategoriaRestaurante.ComidaRapida,
                Rango = RangoPrecio.Economico,
                Direccion = "Calle Madre de Dios 270 · Urb. Santa Patricia",
                DistanciaTexto = "A 550m de la FIA (7 min a pie)",
                RangoPrecios = "Económico",
                HorarioAtencion = "Lun - Dom: 5:00 PM - 1:00 AM",
                TelefonoWhatsApp = "981223399",
                FotoUrl = "/images/restaurantes/don-lucho-salchipapas.jpg",
                Latitud = -12.0690156,
                Longitud = -76.9457379,
                CalificacionPromedio = 4.4,
                TotalResenas = 1
            },
        };

        db.Restaurantes.AddRange(restaurantes);
        await db.SaveChangesAsync();

        // --- RESEÑAS REALES DE ESTUDIANTES FIA USMP ---
        if (maria != null && juan != null && ana != null && carlos != null)
        {
            var dict = restaurantes.ToDictionary(r => r.Nombre, r => r.Id);
            var resenas = new List<ResenaRestaurante>();

            void AgregarResena(string nombre, string usuarioId, int puntuacion, string comentario, int diasAtras)
            {
                if (dict.TryGetValue(nombre, out var restId))
                {
                    resenas.Add(new ResenaRestaurante
                    {
                        RestauranteId = restId,
                        UsuarioId = usuarioId,
                        Puntuacion = puntuacion,
                        Comentario = comentario,
                        FechaUtc = DateTime.UtcNow.AddDays(-diasAtras)
                    });
                }
            }

            // Cafetería Central FIA (Campus)
            AgregarResena("Cafetería Central FIA (Campus)", maria.Id, 5, "Muy cómodo para no salir de la facultad entre clases, atienden rápido y el café salva vidas en parciales.", 3);
            AgregarResena("Cafetería Central FIA (Campus)", juan.Id, 4, "Buena atención y limpio, solo que a la 1:00 PM se llena bastante de alumnos de civil y sistemas.", 5);

            // Puerto Central
            AgregarResena("Puerto Central", carlos.Id, 5, "El trío marino sale al toque y está frente a la puerta 1. Muy fresco y buen tamaño.", 4);

            // Tumbes Mar Pescados Y Mariscos
            AgregarResena("Tumbes Mar Pescados Y Mariscos", maria.Id, 4, "Buen ceviche clásico norteño con harta canchita y camote, recomendado para los viernes.", 5);

            // Pizza Raúl La Fontana
            AgregarResena("Pizza Raúl La Fontana", juan.Id, 4, "Súper económico para comprar una pizza grande entre 3 amigos cuando nos quedamos avanzando el proyecto.", 8);

            // Mamma Tomato
            AgregarResena("Mamma Tomato", ana.Id, 4, "La masa es buenísima y diferente a las comerciales, buen ambiente para conversar tranquilos.", 3);

            // Dachi Café
            AgregarResena("Dachi Café", maria.Id, 5, "El mejor café de especialidad cerca a la facultad. Se puede llevar la laptop y programar sin bulla.", 2);

            // Don Tito La Molina
            AgregarResena("Don Tito La Molina", carlos.Id, 5, "El mejor pollo a la brasa cerca de la universidad. Para venir en grupo después de presentar proyectos.", 4);

            // Shimaya Ramen
            AgregarResena("Shimaya Ramen", maria.Id, 5, "Ramen excelente y bien caliente. El caldo tonkotsu es buenazo en época de frío.", 6);

            // Secretos Árabes
            AgregarResena("Secretos Árabes", juan.Id, 5, "Los shawarmas son contundentes y salen al toque. Ideal si tienes solo 30 minutos de break.", 1);

            // Pardos Chicken La Molina
            AgregarResena("Pardos Chicken La Molina", carlos.Id, 5, "El clásico de Javier Prado con Los Civiles. El pellejito crocante y el ají Pardos no fallan nunca.", 2);
            AgregarResena("Pardos Chicken La Molina", maria.Id, 4, "Buen ambiente y rápido el servicio para almorzar entre patas después de un examen.", 5);

            // Roll Star Sushi Bar
            AgregarResena("Roll Star Sushi Bar", ana.Id, 4, "Los makis acevichados y el roll furai están buenazos, cerca a ISIL y cruzando de la FIA.", 3);

            // Punto Italiano (La Fontana)
            AgregarResena("Punto Italiano (La Fontana)", juan.Id, 5, "La lasaña bolognesa y los fetuccinis al pesto son de primera. De los mejores sitios de pastas de La Fontana.", 4);

            // Menú Rakus
            AgregarResena("Menú Rakus", carlos.Id, 5, "El menú estudiantil es bien despachado y la sazón es casera. Clave para los días largos de laboratorio.", 2);

            // Don Lucho (Salchipapas & Broaster)
            AgregarResena("Don Lucho (Salchipapas & Broaster)", ana.Id, 4, "Las mejores salchipapas y cremas para bajar el hambre después de clases en la tarde.", 1);

            db.ResenasRestaurantes.AddRange(resenas);
            await db.SaveChangesAsync();
        }
    }
}
