using UsmpConnect.Models;

namespace UsmpConnect.Data.Seed;

/// <summary>Correos de los usuarios de prueba, para que otros seeders los puedan usar.</summary>
public static class DemoUsers
{
    public const string Password = "Usmp2026!";

    // Alumnos
    public const string Alessandro = "alessandro.morales@usmp.pe";
    public const string Maria = "maria.gonzales@usmp.pe";
    public const string Carlos = "carlos.rojas@usmp.pe";
    public const string Ana = "ana.lopez@usmp.pe";
    public const string Juan = "juan.perez@usmp.pe";
    public const string Pedro = "pedro.mendoza@usmp.pe";
    public const string Sofia = "sofia.torres@usmp.pe";
    public const string Diego = "diego.flores@usmp.pe";
    public const string Lucia = "lucia.vargas@usmp.pe";
    public const string Andres = "andres.kuroda@usmp.pe";

    // Docentes
    public const string Ramirez = "jramirez@usmp.pe";
    public const string Torres = "ltorres@usmp.pe";
    public const string Sanchez = "msanchez@usmp.pe";
    public const string Lopez = "rlopez@usmp.pe";
    public const string Herrera = "cherrera@usmp.pe";
}

public class BaseSeeder : IModuleSeeder
{
    public const string Periodo = "2026-II";

    public int Orden => 0;

    public async Task SeedAsync(SeedContext ctx)
    {
        var db = ctx.Db;

        // ---------- Usuarios ----------
        var u = new Dictionary<string, ApplicationUser>();

        async Task Alumno(string email, string nombres, string apellidos, string codigo, string escuela)
        {
            var user = new ApplicationUser
            {
                UserName = email, Email = email, EmailConfirmed = true,
                Nombres = nombres, Apellidos = apellidos, CodigoAlumno = codigo, Escuela = escuela
            };
            await ctx.Users.CreateAsync(user, DemoUsers.Password);
            await ctx.Users.AddToRoleAsync(user, Roles.Alumno);
            u[email] = user;
        }

        async Task Docente(string email, string titulo, string nombres, string apellidos, string escuela)
        {
            var user = new ApplicationUser
            {
                UserName = email, Email = email, EmailConfirmed = true,
                Titulo = titulo, Nombres = nombres, Apellidos = apellidos, Escuela = escuela
            };
            await ctx.Users.CreateAsync(user, DemoUsers.Password);
            await ctx.Users.AddToRoleAsync(user, Roles.Profesor);
            u[email] = user;
        }

        const string sistemas = "Ing. de Computación y Sistemas";
        await Alumno(DemoUsers.Alessandro, "Alessandro", "Morales Quispe", "2021123456", sistemas);
        await Alumno(DemoUsers.Maria, "María", "Gonzales Ríos", "2021104512", sistemas);
        await Alumno(DemoUsers.Carlos, "Carlos", "Rojas Paredes", "2020117733", "Ing. Electrónica");
        await Alumno(DemoUsers.Ana, "Ana", "López Huamán", "2021109821", sistemas);
        await Alumno(DemoUsers.Juan, "Juan", "Pérez Salazar", "2022100456", "Ing. Civil");
        await Alumno(DemoUsers.Pedro, "Pedro", "Mendoza Castro", "2020132214", "Derecho");
        await Alumno(DemoUsers.Sofia, "Sofía", "Torres Vega", "2022114590", "Arquitectura");
        await Alumno(DemoUsers.Diego, "Diego", "Flores Ramos", "2021120087", sistemas);
        await Alumno(DemoUsers.Lucia, "Lucía", "Vargas Chávez", "2021118873", sistemas);
        await Alumno(DemoUsers.Andres, "Andrés", "Kuroda Díaz", "2022101199", sistemas);

        await Docente(DemoUsers.Ramirez, "Ing.", "Jorge", "Ramírez Soto", sistemas);
        await Docente(DemoUsers.Torres, "Ing.", "Luis", "Torres Campos", sistemas);
        await Docente(DemoUsers.Sanchez, "Ing.", "Marta", "Sánchez Luna", sistemas);
        await Docente(DemoUsers.Lopez, "Ing.", "Raúl", "López Medina", sistemas);
        await Docente(DemoUsers.Herrera, "Lic.", "Carmen", "Herrera Nieto", "Humanidades");

        // ---------- Cursos ----------
        Curso C(string codigo, string nombre, string corto, int creditos, int ciclo) =>
            new() { Codigo = codigo, Nombre = nombre, NombreCorto = corto, Creditos = creditos, Ciclo = ciclo };

        var com1 = C("HU-101", "Comunicación I", "Comunicación I", 3, 1);
        var calc = C("MA-101", "Cálculo Diferencial", "Cálculo Dif.", 4, 1);
        var intro = C("IS-101", "Introducción a la Ing. de Software", "Intro. Ing. Soft.", 3, 1);
        var fis1 = C("FI-102", "Física I", "Física I", 4, 2);
        var prog1 = C("CS-101", "Programación I", "Programación I", 4, 2);
        var calcInt = C("MA-102", "Cálculo Integral", "Cálculo Int.", 4, 2);
        var aed = C("CS-201", "Algoritmos y Estructuras de Datos", "AED", 4, 3);
        var discreta = C("MA-201", "Matemática Discreta", "Mat. Discreta", 3, 3);
        var bd1 = C("CS-301", "Base de Datos I", "Base de Datos I", 4, 4);
        var poo = C("CS-302", "Programación Orientada a Objetos", "POO", 4, 4);
        var redes = C("CS-401", "Redes de Computadoras", "Redes", 4, 5);
        var so = C("CS-402", "Sistemas Operativos", "Sist. Operativos", 4, 5);
        db.Cursos.AddRange(com1, calc, intro, fis1, prog1, calcInt, aed, discreta, bd1, poo, redes, so);

        // ---------- Secciones y horarios ----------
        Seccion S(Curso curso, string codigo, string docente, params (DayOfWeek dia, int ini, int fin, string aula)[] horas)
        {
            var s = new Seccion { Curso = curso, Codigo = codigo, Periodo = Periodo, DocenteId = u[docente].Id };
            foreach (var h in horas)
                s.Horarios.Add(new Horario { Dia = h.dia, HoraInicio = new TimeOnly(h.ini, 0), HoraFin = new TimeOnly(h.fin, 0), Aula = h.aula });
            db.Secciones.Add(s);
            return s;
        }

        var sCom1 = S(com1, "01A", DemoUsers.Herrera, (DayOfWeek.Monday, 8, 10, "Aula C-105"), (DayOfWeek.Friday, 10, 12, "Aula C-105"));
        var sCalc = S(calc, "01A", DemoUsers.Ramirez, (DayOfWeek.Monday, 10, 12, "Aula A-302"), (DayOfWeek.Wednesday, 8, 10, "Aula A-302"));
        var sIntro = S(intro, "01B", DemoUsers.Sanchez, (DayOfWeek.Wednesday, 14, 16, "Aula B-201"), (DayOfWeek.Friday, 8, 10, "Aula B-201"));
        var sFis1 = S(fis1, "02A", DemoUsers.Torres, (DayOfWeek.Tuesday, 8, 10, "Lab. F-101"), (DayOfWeek.Thursday, 8, 10, "Lab. F-101"));
        var sAed = S(aed, "04T", DemoUsers.Ramirez, (DayOfWeek.Tuesday, 14, 17, "Lab. C-204"), (DayOfWeek.Thursday, 11, 13, "Aula A-210"));
        S(aed, "04G", DemoUsers.Torres, (DayOfWeek.Monday, 14, 17, "Lab. C-205"), (DayOfWeek.Wednesday, 11, 13, "Aula A-211"));
        S(aed, "04M", DemoUsers.Sanchez, (DayOfWeek.Tuesday, 18, 21, "Lab. C-204"), (DayOfWeek.Friday, 18, 20, "Aula A-210"));
        S(prog1, "02B", DemoUsers.Lopez, (DayOfWeek.Monday, 16, 19, "Lab. C-201"));
        S(prog1, "02C", DemoUsers.Sanchez, (DayOfWeek.Thursday, 16, 19, "Lab. C-202"));
        S(calcInt, "02A", DemoUsers.Ramirez, (DayOfWeek.Tuesday, 10, 12, "Aula A-305"));
        S(discreta, "03A", DemoUsers.Torres, (DayOfWeek.Friday, 14, 16, "Aula A-110"));
        S(bd1, "04A", DemoUsers.Lopez, (DayOfWeek.Wednesday, 16, 19, "Lab. C-301"));
        S(poo, "04B", DemoUsers.Lopez, (DayOfWeek.Thursday, 14, 16, "Lab. C-302"));
        S(redes, "05A", DemoUsers.Torres, (DayOfWeek.Monday, 18, 21, "Lab. R-101"));
        S(so, "05A", DemoUsers.Sanchez, (DayOfWeek.Wednesday, 18, 21, "Lab. C-303"));

        // ---------- Matrículas y notas ----------
        void Matricular(string alumno, Seccion seccion, decimal? ep = null, decimal? ef = null)
        {
            var m = new Matricula { AlumnoId = u[alumno].Id, Seccion = seccion };
            if (ep.HasValue || ef.HasValue)
                m.Nota = new Nota { EP = ep, EF = ef, ActualizadoEn = DateTime.UtcNow };
            db.Matriculas.Add(m);
        }

        Matricular(DemoUsers.Alessandro, sAed, 15, 16);
        Matricular(DemoUsers.Alessandro, sCalc, 12, 11);
        Matricular(DemoUsers.Alessandro, sIntro, 14);
        Matricular(DemoUsers.Alessandro, sFis1);
        Matricular(DemoUsers.Alessandro, sCom1);

        var companeros = new[]
        {
            (DemoUsers.Maria, 17m, 18m), (DemoUsers.Ana, 13m, 12m), (DemoUsers.Diego, 9m, 11m),
            (DemoUsers.Lucia, 16m, 15m), (DemoUsers.Andres, 11m, 0m)
        };
        foreach (var (email, ep, ef) in companeros)
        {
            Matricular(email, sAed, ep, ef == 0 ? null : ef);
            Matricular(email, sCalc, ep - 1);
            Matricular(email, sIntro);
            Matricular(email, sFis1);
        }
        Matricular(DemoUsers.Carlos, sFis1, 14);
        Matricular(DemoUsers.Juan, sCalc, 10);

        // ---------- Calendario académico 2026-II ----------
        EventoAcademico E(int mes, int dia, string titulo, TipoEvento tipo) =>
            new() { Fecha = new DateOnly(2026, mes, dia), Titulo = titulo, Tipo = tipo };

        db.Eventos.AddRange(
            E(8, 17, "Inicio de clases 2026-II", TipoEvento.Institucional),
            E(8, 28, "Plazo final add/drop", TipoEvento.Institucional),
            E(8, 30, "Feriado — Santa Rosa de Lima", TipoEvento.Institucional),
            E(9, 30, "Entrega Proyecto AED — Avance 1", TipoEvento.Entrega),
            E(10, 5, "Inicio de exámenes parciales", TipoEvento.Evaluacion),
            E(10, 8, "Feriado — Combate de Angamos", TipoEvento.Institucional),
            E(10, 9, "Fin de exámenes parciales", TipoEvento.Evaluacion),
            E(10, 23, "Entrega Proyecto Ing. de Software", TipoEvento.Entrega),
            E(11, 20, "Entrega final Proyecto AED", TipoEvento.Entrega),
            E(11, 30, "Inicio de exámenes finales", TipoEvento.Evaluacion),
            E(12, 4, "Fin de exámenes finales", TipoEvento.Evaluacion),
            E(12, 11, "Examen sustitutorio", TipoEvento.Evaluacion));

        db.Eventos.Add(new EventoAcademico
        {
            UsuarioId = u[DemoUsers.Alessandro].Id, Fecha = new DateOnly(2026, 9, 29),
            Titulo = "Reunión grupal — Proyecto Software", Tipo = TipoEvento.Personal
        });

        // ---------- Notificaciones de bienvenida ----------
        foreach (var user in u.Values)
        {
            db.Notificaciones.Add(new Notificacion
            {
                UsuarioId = user.Id, Icono = "stars",
                Mensaje = "¡Bienvenido a USMP Connect! Explora Marketplace, Cátedra & Tips y el Banco de Apuntes.",
                Fecha = DateTime.UtcNow.AddDays(-2)
            });
        }
        db.Notificaciones.AddRange(
            new Notificacion { UsuarioId = u[DemoUsers.Alessandro].Id, Icono = "journal-check", Url = "/Notas", Mensaje = "Ing. Ramírez registró tus notas de Algoritmos y Estructuras de Datos.", Fecha = DateTime.UtcNow.AddHours(-20) },
            new Notificacion { UsuarioId = u[DemoUsers.Alessandro].Id, Icono = "calendar-event", Mensaje = "Recuerda: el 30 de setiembre es la entrega del Proyecto AED.", Fecha = DateTime.UtcNow.AddHours(-3) });

        await db.SaveChangesAsync();
    }
}
