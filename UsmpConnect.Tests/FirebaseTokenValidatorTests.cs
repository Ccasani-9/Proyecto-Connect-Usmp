using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UsmpConnect.Services;

namespace UsmpConnect.Tests;

public class FirebaseTokenValidatorTests : IDisposable
{
    private readonly RSA rsa = RSA.Create(2048);
    private readonly MemoryCache cache = new(new MemoryCacheOptions());
    private readonly X509Certificate2 certificate;
    private readonly HttpClient http;
    private readonly CertificatesHandler certificates;
    private readonly FirebaseTokenValidator validator;

    public FirebaseTokenValidatorTests()
    {
        certificate = new CertificateRequest("CN=Firebase Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
            .CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        certificates = new CertificatesHandler(JsonSerializer.Serialize(new Dictionary<string, string> { ["test-key"] = certificate.ExportCertificatePem() }));
        http = new HttpClient(certificates);
        validator = new FirebaseTokenValidator(http, cache, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Firebase:ProjectId"] = "usmp-test"
        }).Build());
    }

    [Fact]
    public async Task AceptaSoloLaIdentidadDelTokenFirmadoYCacheaCertificados()
    {
        var token = Token();
        var identity = await validator.ValidarAsync(token);
        Assert.NotNull(identity);
        Assert.Equal("firebase-user-1", identity.Uid);
        Assert.Equal("alumno@usmp.pe", identity.Email);
        Assert.Equal("Ana", identity.Nombre);
        Assert.NotNull(await validator.ValidarAsync(token));
        Assert.Equal(1, certificates.Requests);
    }

    [Theory]
    [InlineData("aud", "otro-proyecto")]
    [InlineData("iss", "https://securetoken.google.com/otro-proyecto")]
    [InlineData("sub", "")]
    [InlineData("email_verified", false)]
    [InlineData("email", "")]
    [InlineData("firebase", "password")]
    [InlineData("exp", "expired")]
    [InlineData("iat", "future")]
    [InlineData("auth_time", "future")]
    public async Task RechazaClaimsInvalidos(string field, object value)
    {
        Assert.Null(await validator.ValidarAsync(Token(field, value)));
    }

    [Fact]
    public async Task RechazaFirmaAjenaAunqueElCorreoYElKidSeanCorrectos()
    {
        using var other = RSA.Create(2048);
        Assert.Null(await validator.ValidarAsync(Token(signingKey: other)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("esto-no-es-un-token")]
    [InlineData("eyJhbGciOiJub25lIn0.eyJlbWFpbCI6ImFsdW1ub0B1c21wLnBlIn0.")]
    public async Task RechazaTokensNoFirmadosOMalformados(string token) => Assert.Null(await validator.ValidarAsync(token));

    private string Token(string? field = null, object? value = null, RSA? signingKey = null)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var claims = new Dictionary<string, object>
        {
            ["sub"] = "firebase-user-1", ["email"] = "alumno@usmp.pe", ["email_verified"] = true,
            ["name"] = "Ana", ["iat"] = now - 10, ["auth_time"] = now - 20,
            ["firebase"] = new Dictionary<string, string> { ["sign_in_provider"] = "google.com" }
        };
        if (field == "firebase") claims[field] = new Dictionary<string, string> { ["sign_in_provider"] = (string)value! };
        else if (field is "iat" or "auth_time") claims[field] = now + 3600;
        else if (field is not null && field is not ("aud" or "iss" or "exp")) claims[field] = value!;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = field == "iss" ? (string)value! : "https://securetoken.google.com/usmp-test",
            Audience = field == "aud" ? (string)value! : "usmp-test",
            IssuedAt = field == "iat" ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow.AddSeconds(-10),
            NotBefore = DateTime.UtcNow.AddHours(-2),
            Expires = field == "exp" ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddMinutes(30),
            Claims = claims,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(signingKey ?? rsa) { KeyId = "test-key" }, SecurityAlgorithms.RsaSha256)
        });
    }

    public void Dispose() { http.Dispose(); cache.Dispose(); certificate.Dispose(); rsa.Dispose(); }

    private sealed class CertificatesHandler(string json) : HttpMessageHandler
    {
        public int Requests { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
            response.Headers.CacheControl = new() { MaxAge = TimeSpan.FromHours(1) };
            return Task.FromResult(response);
        }
    }
}
