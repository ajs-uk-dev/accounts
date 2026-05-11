namespace Accounts.SharedKernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : struct
{
    private readonly List<IDomainEvent> _events = new();

    public IReadOnlyList<IDomainEvent> DomainEvents => _events;

    protected AggregateRoot() { }

    protected AggregateRoot(TId id, DateTimeOffset now) : base(id, now) { }

    protected void Raise(IDomainEvent @event) => _events.Add(@event);

    public void ClearEvents() => _events.Clear();

    /// <summary>Helper used by static factory methods to set identity + timestamps after <c>new()</c>.</summary>
    protected void AssignIdentity(TId id, DateTimeOffset now)
    {
        Id = id;
        CreatedAt = now;
        UpdatedAt = now;
    }
}
