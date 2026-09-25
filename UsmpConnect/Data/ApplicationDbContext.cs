using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Models;

namespace UsmpConnect.Data;

/// <summary>
/// Contexto de base de datos. Es <c>partial</c> para que cada módulo agregue sus DbSet
/// en su propio archivo (ej. <c>ApplicationDbContext.Marketplace.cs</c>) sin tocar este.
/// Las configuraciones (<see cref="IEntityTypeConfiguration{TEntity}"/>) de cualquier
/// módulo se registran solas.
/// </summary>
public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Seccion> Secciones => Set<Seccion>();
    public DbSet<Horario> Horarios => Set<Horario>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Nota> Notas => Set<Nota>();
    public DbSet<EventoAcademico> Eventos => Set<EventoAcademico>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<CasoMatricula> CasosMatricula => Set<CasoMatricula>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Curso>().HasIndex(c => c.Codigo).IsUnique();

        builder.Entity<Seccion>()
            .HasOne(s => s.Docente).WithMany()
            .HasForeignKey(s => s.DocenteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Matricula>().HasIndex(m => new { m.AlumnoId, m.SeccionId }).IsUnique();

        builder.Entity<Nota>()
            .HasOne(n => n.Matricula).WithOne(m => m.Nota)
            .HasForeignKey<Nota>(n => n.MatriculaId);

        builder.Entity<Notificacion>().HasIndex(n => new { n.UsuarioId, n.Leida });

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite no puede ordenar ni comparar decimales en SQL; se guardan como REAL.
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
    }
}
