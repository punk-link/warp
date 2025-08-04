using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Warp.WebApp.Attributes;


/// <summary>
/// Ensures API endpoints are idempotent by checking for an idempotency header
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class IdempotentRequestAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyHeader, out var idempotencyKey))
        {
            await next();
            return;
        }

        var cacheKey = $"idempotency:{idempotencyKey}";
        var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        
        if (await cache.GetStringAsync(cacheKey) is not null)
        {
            var result = "";
            context.Result = new Microsoft.AspNetCore.Mvc.ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json"
            };
            return;
        }

        await cache.SetStringAsync(cacheKey, "true", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheInterval)
        });

        await next();
        
        await cache.RemoveAsync(cacheKey);
    }


    private const string IdempotencyHeader = "X-Idempotency-Key";
    private const int CacheInterval = 30;
}