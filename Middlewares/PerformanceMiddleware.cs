using System.Diagnostics;

namespace ECommerceBackend.Middlewares;

public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var elapsedMs = stopwatch.ElapsedMilliseconds;

        _logger.LogInformation(
            "PERF: {method} {path} executed in {time} ms",
            context.Request.Method,
            context.Request.Path,
            elapsedMs
        );
    }
}

