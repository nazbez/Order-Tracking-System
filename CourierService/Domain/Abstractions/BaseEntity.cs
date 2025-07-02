using System.Diagnostics.CodeAnalysis;

namespace Domain.Abstractions;

[ExcludeFromCodeCoverage]
public abstract class BaseEntity<TId> where TId : struct
{
    public TId Id { get; protected internal set; }
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }
}
