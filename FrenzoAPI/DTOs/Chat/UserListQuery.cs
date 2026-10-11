using System.ComponentModel.DataAnnotations;

namespace FrenZoAPI.DTOs.Chat;

// Paging + name search for GET api/users (self is always excluded server-side).
public class UserListQuery
{
    [MaxLength(100)]
    public string? Search { get; set; }

    // Rows per page; clamped in the service to DefaultPageSize..MaxPageSize.
    [Range(1, 50)]
    public int PageSize { get; set; } = 20;
}
