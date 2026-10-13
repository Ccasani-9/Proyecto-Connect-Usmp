using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        // Mientras no usemos migraciones, la BD se recrea en cada arranque para que
        // siempre coincida con los modelos de todas las ramas.
        if (config.GetValue("Database:ResetOnStartup", true))
        {
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
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error al sincronizar datos de Cafetería Central");
        }

        if (!creada)
            return;

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
