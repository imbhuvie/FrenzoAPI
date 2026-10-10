using System.Net;
using System.Text.Json;
using FrenZoAPI.Common.Responses;

namespace FrenZoAPI.Common.Middleware;

// Last-resort handler: unexpected failures become a uniform 500 envelope (R11: business code stays catch-free).
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Runs the pipeline; only the unhandled escapes here since services translate their own known failures.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            string correlationId = ReadCorrelationId(context);
            _logger.LogError(exception, "Unhandled exception {CorrelationId} on {Method} {Path}.", correlationId, context.Request.Method, context.Request.Path);
            await WriteFailureAsync(context, correlationId);
        }
    }

    // Reads the id assigned by CorrelationMiddleware (R12: automatic per-request identity).
    private static string ReadCorrelationId(HttpContext context)
    {
        object? storedId = context.Items["X-Correlation-Id"];
        bool hasStoredId = storedId is string;
        if (hasStoredId)
            return (string)storedId!;
        return "none";
    }

    // Writes a safe 500 without stack traces; the id lets the user quote the log line (R6: no framework calls inside the condition).
    private static async Task WriteFailureAsync(HttpContext context, string correlationId)
    {
        bool isResponseStarted = context.Response.HasStarted;
        if (isResponseStarted)
            return;

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        ApiResponse<object> failure = ApiResponse<object>.Fail("An unexpected error occurred.", new { correlationId });
        string payload = JsonSerializer.Serialize(failure);
        await context.Response.WriteAsync(payload);
    }
}
