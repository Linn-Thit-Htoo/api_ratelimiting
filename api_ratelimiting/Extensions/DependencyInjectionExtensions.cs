using api_ratelimiting.Utils;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace api_ratelimiting.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, WebApplicationBuilder builder)
        {
            builder.Services.AddControllers().AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddRateLimiter(opt =>
            {
                opt.OnRejected = async (context, cs) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    var result = BaseResponse<object>.Fail("Rate limit exceeded.", HttpStatusCode.TooManyRequests);
                    await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(result), cancellationToken: cs);
                };

                // token bucket (if use queue, then the overflow requests will be queued and not return 429.)
                opt.AddTokenBucketLimiter("token-bucket", opt =>
                {
                    opt.TokenLimit = 10; // max token = 10
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // FIFO
                    opt.QueueLimit = 5; // at most 5 requests can be queued
                    opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1); // every 1 minute, tokens are added to the bucket.
                    opt.TokensPerPeriod = 10; // 10 tokens are added every 1 minute.
                    opt.AutoReplenishment = true;
                });

                // fixed window (if use queue, then have to wait Window timeframe after the requests exceeded.)
                opt.AddFixedWindowLimiter("fixed-window", opt =>
                {
                    opt.PermitLimit = 10;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 5;
                });

                // Concurrency Limiter
                opt.AddConcurrencyLimiter("concurrency", opt =>
                {
                    opt.PermitLimit = 10;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 5;
                });
            });

            return services;
        }
    }
}
