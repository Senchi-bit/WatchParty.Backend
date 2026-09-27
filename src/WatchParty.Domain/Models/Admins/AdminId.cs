namespace WatchParty.Domain.Models.Admins;

public readonly record struct AdminId(Guid Value)
{
    public static AdminId New() => new(Guid.NewGuid());
}
