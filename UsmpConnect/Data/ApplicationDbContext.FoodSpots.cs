using Microsoft.EntityFrameworkCore;
using UsmpConnect.Models;

namespace UsmpConnect.Data;

public partial class ApplicationDbContext
{
    public DbSet<Restaurante> Restaurantes => Set<Restaurante>();
    public DbSet<PlatoRestaurante> PlatosRestaurantes => Set<PlatoRestaurante>();
    public DbSet<ResenaRestaurante> ResenasRestaurantes => Set<ResenaRestaurante>();
}
