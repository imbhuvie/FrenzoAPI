using Serilog.Context;

namespace FrenZoAPI.Common.Middleware;

// Assigns one correlation id per request so all log lines for it tie together (R12: automatic identity).
public class CorrelationMiddleware
{
    // Client-supplied id header; honored when present so callers can trace end-to-end.
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Pushes the id into the Serilog context and mirrors it back on the response.
    public async Task InvokeAsync(HttpContext context)
    {
        string? incomingId = context.Request.Headers[CorrelationHeader];
        bool isMissingId = string.IsNullOrWhiteSpace(incomingId);
        string correlationId = isMissingId ? Guid.NewGuid().ToString("N") : incomingId!;

        context.Items[CorrelationHeader] = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
