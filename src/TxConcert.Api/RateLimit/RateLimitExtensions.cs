using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Settings;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace TxConcert.Api.RateLimit;

public static class RateLimitExtensions
{
    public static IServiceCollection AddAppRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RateLimitSettings settings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>()
            ?? new RateLimitSettings();

        if (!settings.Enabled)
            return services;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;

            // Default policy: 100 req / 60s
            options.AddFixedWindowLimiter(Constants.RateLimit.Names.Default, opt =>
            {
                opt.PermitLimit = Constants.RateLimit.Limits.DefaultLimit;
                opt.Window = Constants.RateLimit.Windows.Default;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });

            // Auth policy: 5 req / 15min
            options.AddFixedWindowLimiter(Constants.RateLimit.Names.Auth, opt =>
            {
                opt.PermitLimit = Constants.RateLimit.Limits.AuthLimit;
                opt.Window = Constants.RateLimit.Windows.Auth;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });

            // Public policy: 60 req / 60s
            options.AddFixedWindowLimiter(Constants.RateLimit.Names.Public, opt =>
            {
                opt.PermitLimit = Constants.RateLimit.Limits.PublicLimit;
                opt.Window = Constants.RateLimit.Windows.Public;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });

            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.ContentType = "application/json";
                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    ctx.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }
                await ctx.HttpContext.Response.WriteAsJsonAsync(new
                {
                    statusCode = 429,
                    message = "Too many requests. Please try again later."
                }, ct);
            };
        });

        return services;
    }
}
