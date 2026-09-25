using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsmpConnect.Models;

namespace UsmpConnect.Data;

public partial class ApplicationDbContext
{
    public DbSet<Apunte> Apuntes => Set<Apunte>();
    public DbSet<CalificacionApunte> CalificacionesApuntes => Set<CalificacionApunte>();
}

public class ApunteConfiguration : IEntityTypeConfiguration<Apunte>
{
    public void Configure(EntityTypeBuilder<Apunte> builder)
    {
        builder.HasOne(a => a.Curso).WithMany()
            .HasForeignKey(a => a.CursoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Autor).WithMany()
            .HasForeignKey(a => a.AutorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CalificacionApunteConfiguration : IEntityTypeConfiguration<CalificacionApunte>
{
    public void Configure(EntityTypeBuilder<CalificacionApunte> builder)
    {
        builder.HasOne(c => c.Apunte).WithMany()
            .HasForeignKey(c => c.ApunteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Usuario).WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ApunteId, c.UsuarioId }).IsUnique();
    }
}
