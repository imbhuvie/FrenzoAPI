namespace FrenZoAPI.Common.Responses;

// Standard envelope for all API responses.
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }

    // Builds a success response with optional data.
    public static ApiResponse<T> Ok(T? data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    // Builds a failure response with an optional error payload.
    public static ApiResponse<T> Fail(string message, object? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}
