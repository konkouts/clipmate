using System;

namespace ClipMate.Models;

public class ClipboardSnippet
{
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsPinned { get; set; } = false;
}