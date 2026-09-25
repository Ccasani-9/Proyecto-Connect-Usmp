using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsmpConnect.Models;

namespace UsmpConnect.Data;

public partial class ApplicationDbContext
{
    public DbSet<ArticuloMarketplace> Articulos => Set<ArticuloMarketplace>();
}

public class ArticuloMarketplaceConfiguration : IEntityTypeConfiguration<ArticuloMarketplace>
{
    public void Configure(EntityTypeBuilder<ArticuloMarketplace> builder)
    {
        builder.HasOne(a => a.Vendedor)
            .WithMany()
            .HasForeignKey(a => a.VendedorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.Categoria, a.Estado });
        builder.HasIndex(a => a.FechaPublicacion);
    }
}
