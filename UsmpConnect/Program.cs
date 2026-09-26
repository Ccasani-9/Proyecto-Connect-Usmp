using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;

var builder = WebApplication.CreateBuilder(args);

// Español de Perú, pero con punto decimal (los <input type="number"> envían "15.5").
var cultura = (CultureInfo)CultureInfo.GetCultureInfo("es-PE").Clone();
cultura.NumberFormat.NumberDecimalSeparator = ".";
cultura.NumberFormat.NumberGroupSeparator = ",";
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

// Render asigna el puerto por la variable PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------- Base de datos + Identity ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<UsmpClaimsFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.LogoutPath = "/Cuenta/Logout";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ---------- MVC: todo requiere login salvo [AllowAnonymous]; todo POST valida antiforgery ----------
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// ---------- Servicios compartidos por todos los módulos ----------
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddSingleton<IAlmacenArchivos, AlmacenArchivosLocal>();
builder.Services.AddServiciosNube();


// Subidas de hasta 25 MB (Banco de Apuntes).
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o => o.MultipartBodyLengthLimit = 26 * 1024 * 1024);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 26 * 1024 * 1024);

// Render termina HTTPS en su proxy.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

// Sincronización en segundo plano con Algolia si está configurado
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var algolia = scope.ServiceProvider.GetRequiredService<IBuscadorAlgolia>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (algolia.IsConfigured)
        {
            await algolia.SincronizarTodoAsync(db);
        }
    }
    catch
    {
        // Silencioso: no interrumpe el arranque
    }
});

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

// Módulos que aún no existen (404) muestran la página "En construcción".
app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");

app.UseStaticFiles(); // archivos subidos en wwwroot/uploads
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
