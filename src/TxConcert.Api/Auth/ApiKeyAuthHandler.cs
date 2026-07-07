using TxConcert.Domain.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace TxConcert.Api.Auth;

public sealed class ApiKeyAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Constants.Headers.AdminApiKey, out Microsoft.Extensions.Primitives.StringValues apiKeyHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        string? expectedKey = configuration[Constants.Security.AdminApiKeyConfigPath];
        if (string.IsNullOrEmpty(expectedKey))
            return Task.FromResult(AuthenticateResult.Fail("API key authentication is not configured."));

        string providedKey = apiKeyHeader.ToString();
        if (!string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, "api-key-user"),
            new(ClaimTypes.Role, "Admin")
        ];

        ClaimsIdentity identity = new(claims, Constants.Jwt.ApiKeySchemeName);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, Constants.Jwt.ApiKeySchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
