using System.ComponentModel.DataAnnotations;

namespace FrenZoAPI.DTOs.Chat;

// Cursor pagination over the chatlist (stable under new arrivals, unlike offset pages).
public class ChatlistQuery
{
    // Returns rows strictly older than this UTC instant; null starts from the newest.
    public DateTime? Cursor { get; set; }

    // Rows per page; clamped in the service to DefaultPageSize..MaxPageSize.
    [Range(1, 50)]
    public int PageSize { get; set; } = 20;
}
