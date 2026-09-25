using Microsoft.EntityFrameworkCore;
using UsmpConnect.Models;

namespace UsmpConnect.Data.Seed;

public class ApuntesSeeder : IModuleSeeder
{
    public int Orden => 21;

    public async Task SeedAsync(SeedContext ctx)
    {
        var ids = await ctx.Db.Users.ToDictionaryAsync(u => u.Email!, u => u.Id);
        var cursos = await ctx.Db.Cursos.ToDictionaryAsync(c => c.Codigo);
        var ahora = DateTime.UtcNow;

        // Sin archivo subido: el PDF se genera al verlo o descargarlo (ver Apunte.GenerarPdfEjemplo).
        Apunte A(string titulo, string curso, string profesor, TipoDocumento tipo, string autor, string descripcion,
                 double promedio, int calificaciones, int descargas, int diasAtras)
        {
            var apunte = new Apunte
            {
                Titulo = titulo, Curso = cursos[curso], Profesor = profesor, Tipo = tipo, AutorId = ids[autor],
                Descripcion = descripcion, Extension = ".pdf", NombreArchivo = $"{titulo}.pdf",
                SumaEstrellas = (int)Math.Round(promedio * calificaciones), NumCalificaciones = calificaciones,
                Descargas = descargas, Fecha = ahora.AddDays(-diasAtras)
            };
            apunte.Bytes = apunte.GenerarPdfEjemplo().Length;
            return apunte;
        }

        ctx.Db.Apuntes.AddRange(
            A("Resumen Completo - Árboles y Grafos", "CS-201", "Ing. Ramírez", TipoDocumento.Resumen, DemoUsers.Maria,
                "Árboles binarios de búsqueda, AVL, recorridos, BFS, DFS y Dijkstra con ejemplos paso a paso.", 4.8, 127, 342, 3),
            A("Examen Parcial 2025-II Resuelto", "MA-101", "Ing. Torres", TipoDocumento.Examen, DemoUsers.Carlos,
                "Las 5 preguntas del parcial con la solución completa: límites, continuidad y derivadas.", 4.6, 98, 260, 6),
            A("Guía de Laboratorio 3 - Circuitos RC", "FI-102", "Ing. Sánchez", TipoDocumento.GuiaLab, DemoUsers.Ana,
                "Procedimiento, tabla de datos y preguntas del laboratorio de carga y descarga de un condensador.", 4.9, 156, 410, 9),
            A("Formulario de Integrales Completo", "MA-101", "Ing. Torres", TipoDocumento.Resumen, DemoUsers.Pedro,
                "Integrales inmediatas, por partes, sustitución trigonométrica y fracciones parciales en dos páginas.", 4.5, 72, 198, 12),
            A("Resumen Final - Ing. de Software", "IS-101", "Ing. López", TipoDocumento.Resumen, DemoUsers.Sofia,
                "Modelos de proceso, requisitos, casos de uso y diagramas UML para el examen final.", 4.7, 89, 175, 15),
            A("Guía Lab 5 - Movimiento Armónico", "FI-102", "Ing. Sánchez", TipoDocumento.GuiaLab, DemoUsers.Diego,
                "Péndulo simple y sistema masa-resorte: cálculo del periodo y análisis de error.", 4.3, 54, 120, 18),
            A("Examen Final 2025-I Resuelto", "CS-201", "Ing. Ramírez", TipoDocumento.Examen, DemoUsers.Lucia,
                "Examen final con pilas, colas, árboles y ordenamiento, resuelto en C++ con comentarios.", 4.9, 201, 530, 21),
            A("Apuntes de Clase Semana 1-8", "HU-101", "Lic. Herrera", TipoDocumento.Apuntes, DemoUsers.Andres,
                "Apuntes ordenados de las primeras 8 semanas: tipos de texto, cohesión, coherencia y redacción académica.", 4.2, 41, 88, 25));

        await ctx.Db.SaveChangesAsync();
    }
}
