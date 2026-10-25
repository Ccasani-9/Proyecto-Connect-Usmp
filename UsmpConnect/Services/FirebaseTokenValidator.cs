using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace UsmpConnect.Services;

public record IdentidadGoogle(string Uid, string Email, string Nombre, string? FotoUrl);

public interface IFirebaseTokenValidator
{
    Task<IdentidadGoogle?> ValidarAsync(string idToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Valida ID tokens de Firebase con certificados públicos de Google. No requiere una clave privada
/// de servicio: comprueba firma RS256, proyecto, caducidad y proveedor antes de usar la identidad.
/// https://firebase.google.com/docs/auth/admin/verify-id-tokens#verify_id_tokens_using_a_third-party_jwt_library
/// </summary>
public sealed class FirebaseTokenValidator(HttpClient http, IMemoryCache cache, IConfiguration config) : IFirebaseTokenValidator
{
    private const string CertificatesUrl = "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com";
    private const string CacheKey = "firebase:public-certificates";
    private static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(30);

    public async Task<IdentidadGoogle?> ValidarAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var projectId = config["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(idToken) || idToken.Length > 16384)
            return null;

        var handler = new JsonWebTokenHandler { MapInboundClaims = false };
        if (!handler.CanReadToken(idToken)) return null;

        try
        {
            var untrusted = handler.ReadJsonWebToken(idToken);
            if (untrusted.Alg != SecurityAlgorithms.RsaSha256 || string.IsNullOrEmpty(untrusted.Kid)) return null;

            var certificates = await ObtenerCertificadosAsync(cancellationToken);
            if (!certificates.TryGetValue(untrusted.Kid, out var pem)) return null;
            using var certificate = X509Certificate2.CreateFromPem(pem);
            var result = await handler.ValidateTokenAsync(idToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new X509SecurityKey(certificate) { KeyId = untrusted.Kid },
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = $"https://securetoken.google.com/{projectId}",
                ValidateAudience = true,
                ValidAudience = projectId,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = ClockSkew
            });
            if (!result.IsValid || result.SecurityToken is not JsonWebToken token) return null;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (string.IsNullOrWhiteSpace(token.Subject) || token.Subject.Length > 128
                || !token.TryGetPayloadValue<long>("iat", out var issuedAt) || issuedAt <= 0 || issuedAt > now + ClockSkew.TotalSeconds
                || !token.TryGetPayloadValue<long>("auth_time", out var authTime) || authTime <= 0 || authTime > now + ClockSkew.TotalSeconds
                || !token.TryGetPayloadValue<bool>("email_verified", out var verified) || !verified
                || !token.TryGetPayloadValue<string>("email", out var email) || !MailAddress.TryCreate(email, out var address)
                || !token.TryGetPayloadValue<JsonElement>("firebase", out var firebase)
                || firebase.ValueKind != JsonValueKind.Object
                || !firebase.TryGetProperty("sign_in_provider", out var provider)
                || provider.ValueKind != JsonValueKind.String || provider.GetString() != "google.com")
                return null;

            token.TryGetPayloadValue<string>("name", out var nombre);
            token.TryGetPayloadValue<string>("picture", out var picture);
            var foto = Uri.TryCreate(picture, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? picture : null;
            return new IdentidadGoogle(token.Subject, address.Address.ToLowerInvariant(), nombre ?? "Estudiante", foto);
        }
        catch (Exception ex) when (ex is ArgumentException or JsonException or SecurityTokenException)
        {
            return null;
        }
    }

    private async Task<Dictionary<string, string>> ObtenerCertificadosAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out Dictionary<string, string>? certificates) && certificates is not null)
            return certificates;

        using var response = await http.GetAsync(CertificatesUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        certificates = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken)
            ?? throw new HttpRequestException("Google no devolvió certificados de autenticación.");
        cache.Set(CacheKey, certificates, response.Headers.CacheControl?.MaxAge ?? TimeSpan.FromMinutes(5));
        return certificates;
    }
}
