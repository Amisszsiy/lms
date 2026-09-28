namespace WebAPI.Middlewares
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleUnhandledExceptionAsync(ctx, ex);
            }
        }

        private static async Task HandleUnhandledExceptionAsync(HttpContext ctx, Exception ex)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                message = ex.Message,
                code = "ERR"
            };

            await ctx.Response.WriteAsJsonAsync(response);
        }
    }
}
