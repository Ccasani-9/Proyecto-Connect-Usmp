using Microsoft.EntityFrameworkCore;
using UsmpConnect.Models;

namespace UsmpConnect.Data.Seed;

public class CatedraSeeder : IModuleSeeder
{
    public int Orden => 20;

    public async Task SeedAsync(SeedContext ctx)
    {
        var ids = await ctx.Db.Users.ToDictionaryAsync(u => u.Email!, u => u.Id);
        var cursos = await ctx.Db.Cursos.ToDictionaryAsync(c => c.Codigo, c => c.Id);
        var ahora = DateTime.UtcNow;

        Tip T(string curso, string? docente, string autor, string texto, int dificultad, int? claridad, Exigencia? exigencia,
              int utiles, int noUtiles, double diasAtras) =>
            new()
            {
                CursoId = cursos[curso], DocenteId = docente is null ? null : ids[docente], AutorId = ids[autor],
                Texto = texto, Dificultad = dificultad, Claridad = claridad, Exigencia = exigencia,
                Utiles = utiles, NoUtiles = noUtiles, Fecha = ahora.AddDays(-diasAtras)
            };

        ctx.Db.Tips.AddRange(
            // Algoritmos y Estructuras de Datos: las tres secciones
            T("CS-201", DemoUsers.Ramirez, DemoUsers.Maria,
                "Para el parcial, enfóquense en los ejercicios de árboles binarios y grafos. El libro de Cormen es clave, capítulos 10-12. ¡Hagan los laboratorios, valen 30%!",
                4, 4, Exigencia.Alta, 47, 3, 2),
            T("CS-201", DemoUsers.Torres, DemoUsers.Carlos,
                "Recomiendo ir a las asesorías los martes. El Ing. Torres deja práctica calificada cada semana, no falten. La bibliografía complementaria de Sedgewick ayuda mucho para el final.",
                3, 3, Exigencia.Media, 32, 1, 5),
            T("CS-201", DemoUsers.Sanchez, DemoUsers.Ana,
                "El laboratorio 3 sobre ordenamiento es el más difícil. Practiquen QuickSort y MergeSort con casos borde. Usen el compilador online que recomienda el profesor.",
                4, 5, Exigencia.MuyAlta, 28, 5, 7),
            T("CS-201", DemoUsers.Ramirez, DemoUsers.Diego,
                "El Ing. Ramírez explica muy bien con la pizarra, pero avanza rápido. Graben la clase (él lo permite) y repasen el mismo día.",
                4, 5, Exigencia.Alta, 15, 2, 9),
            T("CS-201", DemoUsers.Torres, DemoUsers.Lucia,
                "Las prácticas del Ing. Torres son parecidas a los ejercicios de LeetCode nivel fácil-medio. Si practicas ahí, llegas bien.",
                3, 4, Exigencia.Media, 9, 0, 12),
            T("CS-201", DemoUsers.Sanchez, DemoUsers.Andres,
                "La Ing. Sánchez corrige hasta el estilo del código: nombres claros, sin variables globales y comentarios. Cuiden eso y suman puntos.",
                5, 5, Exigencia.MuyAlta, 12, 1, 15),
            T("CS-201", null, DemoUsers.Alessandro,
                "Consejo general: armen grupo de estudio desde la semana 1. El proyecto final es en equipo y se nota quién estudió junto.",
                4, null, null, 6, 0, 1),

            // Otros cursos
            T("MA-101", DemoUsers.Ramirez, DemoUsers.Maria,
                "Hagan todos los ejercicios del Stewart de límites y derivadas. El parcial siempre trae un problema de optimización.",
                4, 4, Exigencia.Alta, 21, 2, 20),
            T("MA-101", null, DemoUsers.Juan,
                "Los videos de Julioprofe salvan para regla de la cadena y derivación implícita.",
                3, null, null, 18, 1, 25),
            T("FI-102", DemoUsers.Torres, DemoUsers.Carlos,
                "En los laboratorios de Física I lleven la guía impresa y la calculadora. El informe se entrega el mismo día.",
                4, 4, Exigencia.Media, 14, 0, 10),
            T("FI-102", DemoUsers.Torres, DemoUsers.Ana,
                "Dinámica y trabajo-energía son lo que más viene en el parcial. Repasen los problemas resueltos del Serway.",
                4, 3, Exigencia.Alta, 11, 3, 18),
            T("IS-101", DemoUsers.Sanchez, DemoUsers.Diego,
                "El curso es de exposiciones y un proyecto grupal. Elijan bien a su grupo y empiecen el documento de requisitos temprano.",
                2, 5, Exigencia.Media, 8, 0, 14),
            T("HU-101", DemoUsers.Herrera, DemoUsers.Lucia,
                "La Lic. Herrera valora mucho la ortografía. Revisen sus ensayos con calma antes de entregar.",
                2, 5, Exigencia.Baja, 10, 1, 30),
            T("CS-101", DemoUsers.Lopez, DemoUsers.Andres,
                "Si nunca programaste, practica en casa todos los días aunque sea 30 minutos. Los ejercicios de bucles y arreglos son la base de todo.",
                3, 4, Exigencia.Media, 25, 1, 40),
            T("CS-301", DemoUsers.Lopez, DemoUsers.Pedro,
                "Normalización y SQL con JOINs es lo que más pesa. Practiquen consultas en SQLite o en el laboratorio.",
                4, 4, Exigencia.Alta, 7, 0, 22));

        await ctx.Db.SaveChangesAsync();
    }
}
