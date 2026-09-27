namespace WatchParty.Domain.Common;

public abstract class Entity<TId> where TId : notnull
{
    protected Entity()
    {
    }

    protected Entity(TId id) => Id = id;

    public TId Id { get; protected set; } = default!;
}

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id) : base(id)
    {
    }
}
