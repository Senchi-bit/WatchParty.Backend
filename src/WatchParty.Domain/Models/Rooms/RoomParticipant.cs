using WatchParty.Domain.Common;
using WatchParty.Domain.Models.Sessions;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Rooms;

public sealed class RoomParticipant : Entity<RoomParticipantId>
{
    public const int MaxDisplayNameLength = 50;

    private RoomParticipant()
    {
    }

    private RoomParticipant(RoomParticipantId id, RoomId roomId, AnonymousSessionId sessionId, string displayName,
        ParticipantRole role, DateTimeOffset joinedAt) : base(id)
    {
        RoomId = roomId;
        SessionId = sessionId;
        DisplayName = displayName;
        Role = role;
        JoinedAt = joinedAt;
    }

    public RoomId RoomId { get; private set; }

    public AnonymousSessionId SessionId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public ParticipantRole Role { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    public DateTimeOffset? LeftAt { get; private set; }

    public bool IsPresent => LeftAt is null;

    internal static Result<RoomParticipant> Join(RoomId roomId, AnonymousSessionId sessionId, string displayName,
        ParticipantRole role, DateTimeOffset joinedAt)
    {
        var normalizedDisplayName = NormalizeDisplayName(displayName);
        return normalizedDisplayName.IsFailure
            ? Result.Failure<RoomParticipant>(normalizedDisplayName.Error)
            : new RoomParticipant(
                RoomParticipantId.New(),
                roomId,
                sessionId,
                normalizedDisplayName.Value,
                role,
                joinedAt);
    }

    internal Result Leave(DateTimeOffset leftAt)
    {
        if (!IsPresent)
        {
            return Result.Failure(RoomErrors.ParticipantAlreadyLeft);
        }

        if (leftAt < JoinedAt)
        {
            return Result.Failure(RoomErrors.LeaveBeforeJoin);
        }

        LeftAt = leftAt;
        return Result.Success();
    }

    internal Result Rejoin(string displayName, DateTimeOffset joinedAt)
    {
        if (IsPresent)
        {
            return Result.Failure(RoomErrors.ParticipantAlreadyPresent);
        }

        var normalizedDisplayName = NormalizeDisplayName(displayName);
        if (normalizedDisplayName.IsFailure)
        {
            return normalizedDisplayName;
        }

        DisplayName = normalizedDisplayName.Value;
        JoinedAt = joinedAt;
        LeftAt = null;
        return Result.Success();
    }

    private static Result<string> NormalizeDisplayName(string displayName) =>
        TextRules.Required(
            displayName,
            RoomErrors.DisplayNameRequired,
            RoomErrors.DisplayNameTooLong,
            MaxDisplayNameLength);
}
