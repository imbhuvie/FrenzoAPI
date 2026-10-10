using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class TblMedium
{
    public Guid MediaId { get; set; }

    public short MediaType { get; set; }

    public string Url { get; set; } = null!;

    public string ThumbnailUrl { get; set; } = null!;

    public long? Duration { get; set; }

    public long Size { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDelete { get; set; }
}
