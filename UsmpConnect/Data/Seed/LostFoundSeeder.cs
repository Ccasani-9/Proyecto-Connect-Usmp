using Microsoft.EntityFrameworkCore;
using UsmpConnect.Models;
using UsmpConnect.Services;

namespace UsmpConnect.Data.Seed;

public class LostFoundSeeder : IModuleSeeder
{
    public int Orden => 11;

    public async Task SeedAsync(SeedContext ctx)
    {
        var ids = await ctx.Db.Users.ToDictionaryAsync(u => u.Email!, u => u.Id);
        var hoy = HoraPeru.Ahora.Date;

        ObjetoPerdido Encontrado(string reportante, string titulo, CategoriaObjeto categoria, string pabellon, string lugar,
                                 DateTime fecha, CustodiaObjeto custodia, string? fotoUrl = null, string? whatsapp = null) =>
            new()
            {
                Tipo = TipoReporte.Encontrado, ReportanteId = ids[reportante], Titulo = titulo, Categoria = categoria,
                Pabellon = pabellon, Lugar = lugar, FechaHallazgo = fecha, Custodia = custodia, FotoUrl = fotoUrl, WhatsApp = whatsapp,
                FechaReporte = fecha.AddHours(5).AddMinutes(20) // hora de Lima → UTC, reportado 20 min después
            };

        ObjetoPerdido Perdido(string reportante, string titulo, CategoriaObjeto categoria, string pabellon, string lugar,
                              DateTime fecha, string whatsapp, string descripcion, string? fotoUrl = null) =>
            new()
            {
                Tipo = TipoReporte.Perdido, ReportanteId = ids[reportante], Titulo = titulo, Categoria = categoria,
                Pabellon = pabellon, Lugar = lugar, FechaHallazgo = fecha, WhatsApp = whatsapp, Descripcion = descripcion,
                FotoUrl = fotoUrl,
                FechaReporte = fecha.AddHours(7)
            };

        var carne = Encontrado(DemoUsers.Maria, "Carné Universitario — Ing. Civil", CategoriaObjeto.Carne,
            "Pabellón B", "Aula 204", hoy.AddDays(-2).AddHours(14).AddMinutes(30), CustodiaObjeto.GaritaPuerta1,
            "https://images.unsplash.com/photo-1589829545856-d10d557cf95f?auto=format&fit=crop&w=600&q=80");

        var audifonos = Encontrado(DemoUsers.Maria, "Audífonos Sony WH-1000XM4", CategoriaObjeto.Electronico,
            "Biblioteca Central", "Piso 2", hoy.AddHours(9).AddMinutes(15), CustodiaObjeto.GaritaPuerta2,
            "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=600&q=80", "912345670");

        var casaca = Encontrado(DemoUsers.Diego, "Casaca North Face Negra", CategoriaObjeto.Ropa,
            "Cafetería Principal", "Mesa junto a la ventana", hoy.AddDays(-3).AddHours(11), CustodiaObjeto.GaritaPuerta1,
            "https://images.unsplash.com/photo-1556905055-8f358a7a47b2?auto=format&fit=crop&w=600&q=80");
        casaca.Estado = EstadoObjeto.Devuelto;

        var cuaderno = Encontrado(DemoUsers.Lucia, "Cuaderno Azul — Cálculo II", CategoriaObjeto.Otros,
            "Pabellón A", "Aula 105", hoy.AddDays(-1).AddHours(16).AddMinutes(40), CustodiaObjeto.Secretaria,
            "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?auto=format&fit=crop&w=600&q=80");

        var cargador = Encontrado(DemoUsers.Alessandro, "Laptop Charger USB-C 65W", CategoriaObjeto.Electronico,
            "Laboratorios FIA", "Lab. C-204", hoy.AddHours(8).AddMinutes(5), CustodiaObjeto.GaritaPuerta3,
            "https://images.unsplash.com/photo-1583863788434-e58a36330cf0?auto=format&fit=crop&w=600&q=80", "987654321");

        var mochila = Encontrado(DemoUsers.Andres, "Mochila Totto Gris", CategoriaObjeto.Otros,
            "Losa deportiva", "Tribuna norte", hoy.AddDays(-4).AddHours(18), CustodiaObjeto.GaritaPuerta1,
            "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?auto=format&fit=crop&w=600&q=80");

        ctx.Db.ObjetosPerdidos.AddRange(carne, audifonos, casaca, cuaderno, cargador, mochila,
            Perdido(DemoUsers.Alessandro, "USB Kingston 32 GB negra", CategoriaObjeto.Electronico, "Laboratorios FIA", "Lab. C-205",
                hoy.AddDays(-1).AddHours(15), "987654321", "Tiene un llavero rojo y la carpeta \"Proyecto AED\".",
                "https://images.unsplash.com/photo-1624823183493-5f63901b0b57?auto=format&fit=crop&w=600&q=80"),
            Perdido(DemoUsers.Ana, "Llaves con llavero de Pikachu", CategoriaObjeto.Otros, "Cafetería Principal", "",
                hoy.AddDays(-2).AddHours(13), "967812345", "Son 3 llaves, una de ellas de candado.",
                "https://images.unsplash.com/photo-1582139329536-e7284fece509?auto=format&fit=crop&w=600&q=80"),
            Perdido(DemoUsers.Carlos, "Calculadora Casio fx-570 con mi nombre", CategoriaObjeto.Electronico, "Pabellón C", "Aula 301",
                hoy.AddDays(-5).AddHours(10), "956781234", "Tiene \"Carlos R.\" escrito con plumón en la tapa.",
                "https://images.unsplash.com/photo-1594980596870-8aa52a78d8cd?auto=format&fit=crop&w=600&q=80"));

        // Reclamos pendientes: Alessandro reclama los audífonos de María; Lucía reclama el cargador de Alessandro.
        ctx.Db.ReclamosObjetos.AddRange(
            new ReclamoObjeto
            {
                Objeto = audifonos, UsuarioId = ids[DemoUsers.Alessandro], CodigoAlumno = "2021123456",
                NombreCompleto = "Alessandro Morales Quispe", WhatsApp = "987654321", Fecha = DateTime.UtcNow.AddHours(-1),
                Descripcion = "Son negros, tienen un sticker de la USMP en el lado derecho y el estuche tiene mis iniciales AM."
            },
            new ReclamoObjeto
            {
                Objeto = cargador, UsuarioId = ids[DemoUsers.Lucia], CodigoAlumno = "2021118873",
                NombreCompleto = "Lucía Vargas Chávez", WhatsApp = "945678123", Fecha = DateTime.UtcNow.AddMinutes(-30),
                Descripcion = "Es un cargador Lenovo de 65 W, el cable tiene cinta aislante blanca cerca del conector."
            });

        await ctx.Db.SaveChangesAsync();
    }
}
