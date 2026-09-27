namespace WatchParty.Domain.Models.Sessions;

public readonly record struct AnonymousSessionId(Guid Value)
{
    public static AnonymousSessionId New() => new(Guid.NewGuid());
}
