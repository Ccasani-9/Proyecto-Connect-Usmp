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
            await db.Database.EnsureDeletedAsync();

        if (!await db.Database.EnsureCreatedAsync())
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
