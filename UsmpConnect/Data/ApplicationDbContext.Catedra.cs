using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsmpConnect.Models;

namespace UsmpConnect.Data;

public partial class ApplicationDbContext
{
    public DbSet<Tip> Tips => Set<Tip>();
    public DbSet<VotoTip> VotosTips => Set<VotoTip>();
    public DbSet<ReporteTip> ReportesTips => Set<ReporteTip>();
}

public class TipConfiguration : IEntityTypeConfiguration<Tip>
{
    public void Configure(EntityTypeBuilder<Tip> builder)
    {
        builder.HasOne(t => t.Curso).WithMany()
            .HasForeignKey(t => t.CursoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Autor).WithMany()
            .HasForeignKey(t => t.AutorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Docente).WithMany()
            .HasForeignKey(t => t.DocenteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.CursoId, t.Reportes });
    }
}

public class VotoTipConfiguration : IEntityTypeConfiguration<VotoTip>
{
    public void Configure(EntityTypeBuilder<VotoTip> builder)
    {
        builder.HasOne(v => v.Tip).WithMany(t => t.Votos)
            .HasForeignKey(v => v.TipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Usuario).WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.TipId, v.UsuarioId }).IsUnique();
    }
}

public class ReporteTipConfiguration : IEntityTypeConfiguration<ReporteTip>
{
    public void Configure(EntityTypeBuilder<ReporteTip> builder)
    {
        builder.HasOne(r => r.Tip).WithMany()
            .HasForeignKey(r => r.TipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Usuario).WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.TipId, r.UsuarioId }).IsUnique();
    }
}
