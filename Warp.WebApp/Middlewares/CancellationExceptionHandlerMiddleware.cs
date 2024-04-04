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
                // TODO: add ProblemDetails support. See #20.
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;          
            }
        }

        private readonly RequestDelegate _next;
    }
}
