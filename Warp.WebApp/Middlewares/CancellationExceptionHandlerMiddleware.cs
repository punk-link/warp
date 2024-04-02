

namespace Warp.WebApp.Middlewares
{
    public class CancellationExceptionHandlerMiddleware
    {
        public CancellationExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException) 
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;          
            }
        }

        private readonly RequestDelegate _next;
    }
}
