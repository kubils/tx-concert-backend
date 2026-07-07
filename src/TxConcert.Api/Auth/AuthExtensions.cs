using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TxConcert.Api.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSettings jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? new JwtSettings();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = jwtSettings.Authority;
            options.Audience = jwtSettings.Audience;
            options.RequireHttpsMetadata = !string.IsNullOrEmpty(jwtSettings.Authority)
                && jwtSettings.Authority.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(jwtSettings.LocalSecret))
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.LocalSecret)),
                    ValidateIssuer = !string.IsNullOrEmpty(jwtSettings.Authority),
                    ValidIssuer = jwtSettings.Authority,
                    ValidateAudience = !string.IsNullOrEmpty(jwtSettings.Audience),
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            }
        })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(
            Constants.Jwt.ApiKeySchemeName, _ => { });

        services.AddAuthorization();

        return services;
    }
}
