using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data.Seed;
using UsmpConnect.Models;

namespace UsmpConnect.Data;

/// <summary>
/// Datos de prueba de un módulo. Cualquier clase que implemente esta interfaz se ejecuta
/// automáticamente al crear la base de datos, en orden ascendente de <see cref="Orden"/>.
/// El módulo base usa Orden = 0; los demás módulos deben usar 10 o más.
/// </summary>
public interface IModuleSeeder
{
    int Orden { get; }
    Task SeedAsync(SeedContext ctx);
}

public record SeedContext(
    ApplicationDbContext Db,
    UserManager<ApplicationUser> Users,
    IWebHostEnvironment Env);

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var config = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        // Reinicio destructivo solo para una demo desechable, activado explícitamente.
        if (config.GetValue("Database:ResetOnStartup", false))
        {
            logger.LogWarning("ResetOnStartup activo: se reiniciarán la base de datos y los archivos de la demo.");
            await db.Database.EnsureDeletedAsync();

            // Los archivos subidos pertenecían a la BD anterior. Se vacía la carpeta sin borrarla:
            // en Docker el usuario de la app no tiene permiso sobre wwwroot, solo sobre wwwroot/uploads.
            var uploads = new DirectoryInfo(Path.Combine(sp.GetRequiredService<IWebHostEnvironment>().WebRootPath, "uploads"));
            if (uploads.Exists)
            {
                foreach (var carpeta in uploads.EnumerateDirectories())
                    carpeta.Delete(recursive: true);
                foreach (var archivo in uploads.EnumerateFiles())
                    archivo.Delete();
            }
        }

        var creada = await db.Database.EnsureCreatedAsync();

        if (!creada)
        {
            try
            {
                var cafeteria = await db.Restaurantes.FirstOrDefaultAsync(r => r.Nombre.Contains("Cafetería Central FIA"));
                if (cafeteria != null)
                {
                    bool modificado = false;
                    if (cafeteria.Direccion != null && cafeteria.Direccion.Contains("Pabellón B"))
                    {
                        cafeteria.Direccion = "Dentro del Campus FIA";
                        modificado = true;
                    }
                    if (cafeteria.HorarioAtencion != null && cafeteria.HorarioAtencion.Contains("Mar-Vie"))
                    {
                        cafeteria.HorarioAtencion = "Lun - Vie: 8:00 AM - 9:00 PM · Sáb: 8:30 AM - 1:00 PM";
                        modificado = true;
                    }
                    if (modificado)
                    {
                        await db.SaveChangesAsync();
                    }
                }

                // Sincronizar cursos complementarios de los ciclos 1 al 10 y carreras FIA
                var cursosFia = new (string Codigo, string Nombre, string Corto, int Creditos, int Ciclo, string DocenteEmail)[]
                {
                    ("IS-301", "Ingeniería de Requerimientos", "Ing. Requerimientos", 3, 6, DemoUsers.Sanchez),
                    ("CB-401", "Seguridad en Redes y Hacking Ético", "Hacking Ético", 4, 6, DemoUsers.Torres),
                    ("CS-403", "Desarrollo de Aplicaciones Web y Móviles", "Desarrollo Web/Móvil", 4, 7, DemoUsers.Lopez),
                    ("CD-401", "Minería de Datos y Analítica", "Minería Datos", 4, 7, DemoUsers.Ramirez),
                    ("CS-501", "Inteligencia Artificial", "Inteligencia Art.", 4, 8, DemoUsers.Torres),
                    ("IA-401", "Robótica y Visión Computacional", "Robótica", 4, 8, DemoUsers.Lopez),
                    ("CS-502", "Cloud Computing y Arquitectura", "Cloud y Arq.", 4, 9, DemoUsers.Ramirez),
                    ("IS-501", "Taller de Proyectos de Fin de Carrera", "Taller Proyectos", 4, 10, DemoUsers.Sanchez),
                    ("CI-201", "Topografía y Geodesia", "Topografía", 4, 3, DemoUsers.Torres),
                    ("CI-301", "Resistencia de Materiales", "Resist. Materiales", 4, 4, DemoUsers.Ramirez),
                    ("IN-201", "Procesos Industriales I", "Procesos Ind.", 4, 3, DemoUsers.Lopez),
                    ("IN-301", "Ingeniería de Métodos", "Ing. Métodos", 4, 4, DemoUsers.Sanchez),
                    ("AR-101", "Taller de Diseño Arquitectónico I", "Diseño Arq. I", 4, 1, DemoUsers.Herrera),
                    ("AE-101", "Fundamentos de Navegación Aérea", "Navegación Aérea", 3, 1, DemoUsers.Torres)
                };

                var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                foreach (var cf in cursosFia)
                {
                    var cursoExistente = await db.Cursos.FirstOrDefaultAsync(c => c.Codigo == cf.Codigo);
                    if (cursoExistente == null)
                    {
                        cursoExistente = new Curso
                        {
                            Codigo = cf.Codigo,
                            Nombre = cf.Nombre,
                            NombreCorto = cf.Corto,
                            Creditos = cf.Creditos,
                            Ciclo = cf.Ciclo
                        };
                        db.Cursos.Add(cursoExistente);
                        await db.SaveChangesAsync();

                        var docente = await userManager.FindByEmailAsync(cf.DocenteEmail);
                        if (docente != null)
                        {
                            var seccion = new Seccion
                            {
                                CursoId = cursoExistente.Id,
                                Codigo = "01A",
                                Periodo = BaseSeeder.Periodo,
                                DocenteId = docente.Id
                            };
                            db.Secciones.Add(seccion);
                            await db.SaveChangesAsync();
                        }
                    }
                }

                // Asegurar tips auténticos si algún curso aún no tiene opiniones
                void AgregarTipSiFalta(string codCurso, string autorEmail, string? docenteEmail, string texto, int dificultad, int? claridad, Exigencia? exigencia, int utiles)
                {
                    var c = db.Cursos.FirstOrDefault(x => x.Codigo == codCurso);
                    if (c != null && !db.Tips.Any(t => t.CursoId == c.Id))
                    {
                        var autor = userManager.FindByEmailAsync(autorEmail).GetAwaiter().GetResult();
                        var doc = docenteEmail != null ? userManager.FindByEmailAsync(docenteEmail).GetAwaiter().GetResult() : null;
                        if (autor != null)
                        {
                            db.Tips.Add(new Tip
                            {
                                CursoId = c.Id,
                                AutorId = autor.Id,
                                DocenteId = doc?.Id,
                                Texto = texto,
                                Dificultad = dificultad,
                                Claridad = claridad,
                                Exigencia = exigencia,
                                Utiles = utiles,
                                Fecha = DateTime.UtcNow.AddDays(-5)
                            });
                        }
                    }
                }

                AgregarTipSiFalta("MA-102", DemoUsers.Maria, DemoUsers.Ramirez, "Métodos de integración (por partes y fracciones parciales) es el 70% del examen parcial. Hagan la guía completa.", 4, 4, Exigencia.Alta, 19);
                AgregarTipSiFalta("CS-302", DemoUsers.Diego, DemoUsers.Lopez, "Polimorfismo, interfaces y patrones de diseño (Singleton y Factory) entran fijo. Diseñen bien el diagrama UML antes de tirar código.", 3, 5, Exigencia.Media, 15);
                AgregarTipSiFalta("CS-401", DemoUsers.Carlos, DemoUsers.Torres, "Subnetting VLSM y configuración en Cisco Packet Tracer vienen en todas las prácticas. Dominen cálculo de máscaras en binario.", 4, 4, Exigencia.Alta, 22);
                AgregarTipSiFalta("CS-402", DemoUsers.Alessandro, DemoUsers.Sanchez, "Procesos, hilos y semáforos de sincronización con memoria compartida. Hagan los laboratorios en Linux (Ubuntu/Debian).", 4, 4, Exigencia.Alta, 17);
                AgregarTipSiFalta("IS-301", DemoUsers.Ana, DemoUsers.Sanchez, "El documento de especificación SRS según estándar IEEE 830 debe ser muy consistente. No contradigan requerimientos funcionales con las pantallas.", 3, 5, Exigencia.Media, 14);
                AgregarTipSiFalta("CS-403", DemoUsers.Alessandro, DemoUsers.Lopez, "Para el proyecto final usen React o Flutter con ASP.NET Core. Empiecen el backend y la base de datos temprano.", 3, 4, Exigencia.Media, 26);
                AgregarTipSiFalta("CS-501", DemoUsers.Maria, DemoUsers.Torres, "Búsqueda A* y algoritmos genéticos son preguntas fijas en la primera evaluación. Entiendan la teoría antes de implementar.", 4, 4, Exigencia.Alta, 18);
                AgregarTipSiFalta("CI-201", DemoUsers.Juan, DemoUsers.Torres, "Las prácticas con estación total en el patio de la FIA requieren precisión milimétrica. Cuiden mucho el equipo y anoten todo en la libreta de campo.", 4, 4, Exigencia.Alta, 12);
                AgregarTipSiFalta("IN-201", DemoUsers.Lucia, DemoUsers.Lopez, "Diagramas DOP y DAP entran de todas maneras. Apréndanse bien la simbología estándar ASME de operaciones e inspecciones.", 3, 4, Exigencia.Media, 11);
                AgregarTipSiFalta("AR-101", DemoUsers.Sofia, DemoUsers.Herrera, "Taller de Diseño exige maquetas muy limpias y buenos conceptos espaciales. Tomen apuntes en todas las críticas de láminas.", 4, 5, Exigencia.Alta, 14);
                AgregarTipSiFalta("AE-101", DemoUsers.Carlos, DemoUsers.Torres, "Cartas de navegación VFR, rumbos magnéticos y altimetría. Estúdienlo con tiempo porque la teoría es densa pero fascinante.", 3, 4, Exigencia.Media, 9);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error al sincronizar datos complementarios de Cátedra");
            }
            return;
        }

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in new[] { Roles.Alumno, Roles.Profesor })
            await roleManager.CreateAsync(new IdentityRole(rol));

        var ctx = new SeedContext(db, sp.GetRequiredService<UserManager<ApplicationUser>>(), sp.GetRequiredService<IWebHostEnvironment>());

        var seeders = typeof(DbInitializer).Assembly.GetTypes()
            .Where(t => typeof(IModuleSeeder).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .Select(t => (IModuleSeeder)ActivatorUtilities.CreateInstance(sp, t))
            .OrderBy(s => s.Orden);

        foreach (var seeder in seeders)
        {
            logger.LogInformation("Sembrando datos: {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(ctx);
            db.ChangeTracker.Clear();
        }
    }
}
