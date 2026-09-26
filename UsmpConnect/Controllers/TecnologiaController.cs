using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

[AllowAnonymous]
public class TecnologiaController(
    ApplicationDbContext db,
    IRedisCacheService redis,
    IColaMensajes amqp,
    IBuscadorAlgolia algolia,
    IPieHostService pieHost) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vm = new TecnologiaViewModel
        {
            TotalTablasSQLite = 11,
            TotalUsuarios = await db.Users.CountAsync(),
            TotalRestaurantes = await db.Restaurantes.CountAsync(),
            TotalApuntes = await db.Apuntes.CountAsync(),
            TotalArticulos = await db.Articulos.CountAsync(),
            Servicios = new List<ServicioStatusVM>
            {
                new(
                    "SQLite + Entity Framework Core",
                    "Base de Datos Relacional",
                    "Local / Volumen Docker en Render",
                    true,
                    "Operativo (11 tablas mapeadas)",
                    "database",
                    "tag-ok",
                    "Almacenamiento persistente de usuarios, notas, horarios, apuntes, marketplace, objetos perdidos y huariques."
                ),
                new(
                    "Redis Cloud",
                    "Memoria Caché Ultrarrápida",
                    "cloud.redis.io (Puerto 19955)",
                    redis.IsConnected,
                    redis.Estado,
                    "lightning-charge-fill",
                    redis.IsConnected ? "tag-ok" : "tag-warn",
                    "Caché en memoria de consultas frecuentes de Food & Spots y cursos para respuesta en menos de 2 milisegundos."
                ),
                new(
                    "CloudAMQP",
                    "Broker de Mensajería Asíncrona (RabbitMQ)",
                    "customer.cloudamqp.com (Jackal vHost)",
                    amqp.IsConnected,
                    amqp.Estado,
                    "envelope-paper-fill",
                    amqp.IsConnected ? "tag-ok" : "tag-info",
                    "Cola en segundo plano para procesar notificaciones, auditoría de reseñas y sincronización asíncrona de eventos."
                ),
                new(
                    "Algolia Search Engine",
                    "Motor de Búsqueda Inteligente",
                    "dashboard.algolia.com (App ID: PRHV...)",
                    algolia.IsConfigured,
                    algolia.Estado,
                    "search",
                    algolia.IsConfigured ? "tag-ok" : "tag-info",
                    "Indexación y búsqueda instantánea (as-you-type con tolerancia a errores ortográficos) en apuntes, marketplace y comida."
                ),
                new(
                    "PieHost / PieSocket",
                    "WebSockets en Tiempo Real",
                    "piehost.com (Cluster free.blr2)",
                    pieHost.IsConfigured,
                    pieHost.Estado,
                    "broadcast-pin",
                    pieHost.IsConfigured ? "tag-ok" : "tag-info",
                    "Push en tiempo real: emisión de notificaciones en vivo y sincronización de reseñas y votos sin recargar la página."
                ),
                new(
                    "Render Cloud PaaS",
                    "Despliegue y Orquestación Docker",
                    "usmpconnect.onrender.com",
                    true,
                    "En Vivo (Producción)",
                    "cloud-check-fill",
                    "tag-ok",
                    "Servidor web ASP.NET Core 10 sobre Linux en contenedor Docker con CI/CD automático desde la rama main de GitHub."
                )
            }
        };

        return View(vm);
    }
}
