using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

/// <summary>
/// Marketplace Comunitario: Tablón de compra, venta y donación entre alumnos de la USMP.
/// </summary>
public class MarketplaceController(ApplicationDbContext db, IAlmacenArchivos almacen) : Controller
{
    private const long MaxImagenBytes = 5 * 1024 * 1024; // 5 MB para fotos

    [HttpGet]
    public async Task<IActionResult> Index(string? q, CategoriaMarketplace? categoria)
    {
        var query = db.Articulos
            .AsNoTracking()
            .Include(a => a.Vendedor)
            .OrderByDescending(a => a.FechaPublicacion)
            .AsQueryable();

        if (categoria.HasValue)
            query = query.Where(a => a.Categoria == categoria.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var texto = q.Trim().ToLower();
            query = query.Where(a =>
                a.Titulo.ToLower().Contains(texto) ||
                (a.NotasEntrega != null && a.NotasEntrega.ToLower().Contains(texto)) ||
                a.Vendedor.Nombres.ToLower().Contains(texto) ||
                a.Vendedor.Apellidos.ToLower().Contains(texto));
        }

        var articulos = await query.ToListAsync();

        var vm = new MarketplaceIndexViewModel
        {
            Articulos = articulos,
            Busqueda = q,
            CategoriaFiltro = categoria
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publicar(PublicarArticuloInputModel input, IFormFile? imagen)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Por favor revisa los datos ingresados en el formulario.";
            return RedirectToAction(nameof(Index));
        }

        string? rutaImagen = null;
        if (imagen != null && imagen.Length > 0)
        {
            try
            {
                var extensionesValidas = AlmacenArchivosLocal.Imagenes;
                var resultado = await almacen.GuardarAsync(imagen, "marketplace", extensionesValidas, MaxImagenBytes);
                rutaImagen = resultado.Url;
            }
            catch (ArchivoInvalidoException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        var articulo = new ArticuloMarketplace
        {
            Titulo = input.Titulo.Trim(),
            Categoria = input.Categoria,
            Precio = input.Precio,
            EstadoItem = input.EstadoItem,
            Estado = EstadoPublicacion.Disponible,
            ImagenUrl = rutaImagen,
            WhatsApp = input.WhatsApp.Trim().Replace(" ", "").Replace("+", ""),
            NotasEntrega = input.NotasEntrega?.Trim(),
            VendedorId = User.GetUserId(),
            FechaPublicacion = DateTime.UtcNow
        };

        db.Articulos.Add(articulo);
        await db.SaveChangesAsync();

        TempData["Ok"] = "¡Artículo publicado correctamente en el Marketplace!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoPublicacion nuevoEstado)
    {
        var articulo = await db.Articulos.FindAsync(id);
        if (articulo == null)
            return NotFound();

        if (articulo.VendedorId != User.GetUserId())
            return Forbid();

        articulo.Estado = nuevoEstado;
        await db.SaveChangesAsync();

        TempData["Ok"] = $"Estado de '{articulo.Titulo}' actualizado a {nuevoEstado}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var articulo = await db.Articulos.FindAsync(id);
        if (articulo == null)
            return NotFound();

        if (articulo.VendedorId != User.GetUserId())
            return Forbid();

        db.Articulos.Remove(articulo);
        await db.SaveChangesAsync();

        TempData["Ok"] = "Artículo eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
