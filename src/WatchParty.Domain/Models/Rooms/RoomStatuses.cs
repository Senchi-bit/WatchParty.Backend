namespace WatchParty.Domain.Models.Rooms;

public enum RoomStatus
{
    Open = 0,
    Closed = 1,
    Expired = 2
}

public enum ParticipantRole
{
    Host = 0,
    Viewer = 1
}
