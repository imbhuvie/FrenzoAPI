using System.ComponentModel.DataAnnotations;

namespace FrenZoAPI.DTOs.Chat;

// Paging for GET message history (newest page first; cursor walks older).
public class MessageHistoryQuery
{
    // Returns messages strictly older than this UTC instant; null starts from the newest.
    public DateTime? Cursor { get; set; }

    // Rows per page; clamped in the service to DefaultHistoryPageSize..MaxHistoryPageSize.
    [Range(1, 100)]
    public int PageSize { get; set; } = 30;
}
