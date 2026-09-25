using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsmpConnect.Models;

namespace UsmpConnect.Data;

public partial class ApplicationDbContext
{
    public DbSet<ObjetoPerdido> ObjetosPerdidos => Set<ObjetoPerdido>();
    public DbSet<ReclamoObjeto> ReclamosObjetos => Set<ReclamoObjeto>();
}

public class ObjetoPerdidoConfiguration : IEntityTypeConfiguration<ObjetoPerdido>
{
    public void Configure(EntityTypeBuilder<ObjetoPerdido> builder)
    {
        builder.HasOne(o => o.Reportante).WithMany()
            .HasForeignKey(o => o.ReportanteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.Tipo, o.Estado });
    }
}

public class ReclamoObjetoConfiguration : IEntityTypeConfiguration<ReclamoObjeto>
{
    public void Configure(EntityTypeBuilder<ReclamoObjeto> builder)
    {
        builder.HasOne(r => r.Objeto).WithMany(o => o.Reclamos)
            .HasForeignKey(r => r.ObjetoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Usuario).WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
