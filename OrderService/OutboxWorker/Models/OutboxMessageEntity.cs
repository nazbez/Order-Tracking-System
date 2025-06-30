using System.Diagnostics.CodeAnalysis;

namespace OutboxWorker.Models;

[ExcludeFromCodeCoverage]
public sealed class OutboxMessageEntity
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public DateTimeOffset OccurredOnUtc { get; set; }
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
