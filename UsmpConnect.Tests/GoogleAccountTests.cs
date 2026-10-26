using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;

namespace UsmpConnect.Tests;

public class GoogleAccountTests
{
    [Fact]
    public async Task TokenGoogleValido_ExigeAntiforgeryAntesDeCrearSesion()
    {
        await using var factory = new IdentityFactory(new("csrf-uid", "csrf@usmp.pe", "Ana", null));
        using var client = factory.CreateSessionClient();
        // El mismo cuerpo tendría éxito: solo cambia la presencia del token CSRF.
        var response = await client.PostAsJsonAsync("/Cuenta/GoogleLogin", new { idToken = "token-validado-por-el-doble-de-prueba" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(client)).StatusCode);
    }

    [Fact]
    public async Task NuevaCuenta_TieneRolAlumnoSinPasswordNiCodigoInventadoYNoSeDuplica()
    {
        await using var factory = new IdentityFactory(new("nuevo-uid", "nueva@usmp.pe", "Ana", null));
        using (var client = factory.CreateSessionClient()) Assert.Equal(HttpStatusCode.OK, (await SignInAsync(client)).StatusCode);
        using (var client = factory.CreateSessionClient()) Assert.Equal(HttpStatusCode.OK, (await SignInAsync(client)).StatusCode);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync("nueva@usmp.pe");
        Assert.NotNull(user);
        Assert.True(await users.IsInRoleAsync(user, Roles.Alumno));
        Assert.False(await users.HasPasswordAsync(user));
        Assert.Null(user.CodigoAlumno);
        Assert.Equal(user.Id, (await users.FindByLoginAsync("Firebase", "nuevo-uid"))?.Id);
        Assert.Single(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.Where(u => u.Email == "nueva@usmp.pe"));
    }

    [Theory]
    [InlineData("alumno-uid", "alessandro.morales@usmp.pe", Roles.Alumno)]
    [InlineData("profesor-uid", "jramirez@usmp.pe", Roles.Profesor)]
    public async Task CuentaExistente_ConservaRoles(string uid, string email, string role)
    {
        await using var factory = new IdentityFactory(new(uid, email, "Nombre de Google", null));
        using var client = factory.CreateSessionClient();
        Assert.Equal(role == Roles.Alumno ? HttpStatusCode.OK : HttpStatusCode.Forbidden, (await SignInAsync(client)).StatusCode);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.Equal(new[] { role }, await users.GetRolesAsync(user!));
    }

    [Fact]
    public async Task CuentaAnteriorSinRol_RecibeAlumno()
    {
        await using var factory = new IdentityFactory(new("anterior", "alessandro.morales@usmp.pe", "Alessandro", null));
        using var client = factory.CreateSessionClient();
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByEmailAsync("alessandro.morales@usmp.pe"))!;
            Assert.True((await users.RemoveFromRoleAsync(user, Roles.Alumno)).Succeeded);
        }
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(client)).StatusCode);
        using var verify = factory.Services.CreateScope();
        var manager = verify.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.True(await manager.IsInRoleAsync((await manager.FindByEmailAsync("alessandro.morales@usmp.pe"))!, Roles.Alumno));
    }

    [Fact]
    public async Task CuentaBloqueada_NoPuedeEntrarConGoogle()
    {
        await using var factory = new IdentityFactory(new("bloqueado", "alessandro.morales@usmp.pe", "Alessandro", null));
        using var client = factory.CreateSessionClient();
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByEmailAsync("alessandro.morales@usmp.pe"))!;
            await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddHours(1));
        }
        var response = await SignInAsync(client);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task CorreoAjenoAlDominio_NoCreaCuenta()
    {
        await using var factory = new IdentityFactory(new("externo", "alumno@example.com", "Ana", null));
        using var client = factory.CreateSessionClient();
        var response = await SignInAsync(client);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.False(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.Any(u => u.Email == "alumno@example.com"));
    }

    [Fact]
    public async Task FalloDeRed_NoCreaSesion()
    {
        await using var factory = new IdentityFactory(null, true);
        using var client = factory.CreateSessionClient();
        var response = await SignInAsync(client);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    private static async Task<HttpResponseMessage> SignInAsync(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("RequestVerificationToken");
        client.DefaultRequestHeaders.Add("RequestVerificationToken", await DemoFactory.TokenAsync(client));
        return await client.PostAsJsonAsync("/Cuenta/GoogleLogin", new { idToken = "token-validado-por-el-doble-de-prueba" });
    }

    private sealed class IdentityFactory(IdentidadGoogle? identity, bool networkFailure = false) : DemoFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IFirebaseTokenValidator>();
                services.AddSingleton<IFirebaseTokenValidator>(new VerifiedToken(identity, networkFailure));
            });
        }
    }

    // Las firmas/claims se prueban con claves RSA reales en FirebaseTokenValidatorTests.
    private sealed class VerifiedToken(IdentidadGoogle? identity, bool networkFailure) : IFirebaseTokenValidator
    {
        public Task<IdentidadGoogle?> ValidarAsync(string token, CancellationToken cancellationToken = default) =>
            networkFailure ? throw new HttpRequestException("Red no disponible") : Task.FromResult(identity);
    }
}
