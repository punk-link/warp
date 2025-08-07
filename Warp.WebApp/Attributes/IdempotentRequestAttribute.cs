using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Errors;

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
            var result = DomainErrors.RequestIdempotencyKeyViolation()
                .ToProblemDetails();

            context.Result = new ObjectResult(result)
            {
                StatusCode = StatusCodes.Status409Conflict,
                ContentTypes = { "application/problem+json" }
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

    
    private const int CacheInterval = 30;
    private const string IdempotencyHeader = "X-Idempotency-Key";
}