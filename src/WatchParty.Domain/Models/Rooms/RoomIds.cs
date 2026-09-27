namespace WatchParty.Domain.Models.Rooms;

public readonly record struct RoomId(Guid Value)
{
    public static RoomId New() => new(Guid.NewGuid());
}

public readonly record struct RoomParticipantId(Guid Value)
{
    public static RoomParticipantId New() => new(Guid.NewGuid());
}
