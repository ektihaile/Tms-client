using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8]; 
        context.Response.Headers.Append("X-Correlation-Id", correlationId); 

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("--> Entry: Method={Method}, Path={Path}, CorrelationId={CorrelationId}", 
            context.Request.Method, context.Request.Path, correlationId); 

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop(); 

           
            _logger.LogInformation("<-- Exit: StatusCode={StatusCode}, Elapsed={ElapsedMs}ms, CorrelationId={CorrelationId}", 
                context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId); // [cite: 95]
        }
    }
}