using WatchParty.Domain.Common;
using WatchParty.Domain.Models.Rooms.Chat;
using WatchParty.Domain.Models.Sessions;
using WatchParty.Domain.Models.Videos;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Rooms;

public sealed class Room : AggregateRoot<RoomId>
{
    public const int MaxParticipants = 100;

    private readonly List<RoomParticipant> participants = [];
    private readonly List<ChatMessage> messages = [];

    private Room()
    {
    }

    private Room(RoomId id, JoinCode joinCode, VideoId videoId, AnonymousSessionId hostSessionId, RoomParticipant host,
        DateTimeOffset createdAt, DateTimeOffset expiresAt) : base(id)
    {
        JoinCode = joinCode;
        VideoId = videoId;
        HostSessionId = hostSessionId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        Status = RoomStatus.Open;
        participants.Add(host);
        HostParticipantId = host.Id;
    }

    public JoinCode JoinCode { get; private set; } = null!;

    public VideoId VideoId { get; private set; }

    public AnonymousSessionId HostSessionId { get; private set; }

    public RoomParticipantId HostParticipantId { get; private set; }

    public RoomStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public IReadOnlyCollection<RoomParticipant> Participants => participants;

    public IReadOnlyCollection<ChatMessage> Messages => messages;

    public static Result<Room> Create(JoinCode joinCode, VideoId videoId, AnonymousSessionId hostSessionId,
        string hostDisplayName, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        if (expiresAt <= createdAt)
        {
            return Result.Failure<Room>(RoomErrors.ExpirationBeforeCreation);
        }

        var roomId = RoomId.New();
        var host = RoomParticipant.Join(roomId, hostSessionId, hostDisplayName,
            ParticipantRole.Host, createdAt);
        
        return host.IsFailure ? Result.Failure<Room>(host.Error)
            : new Room(roomId, joinCode, videoId, hostSessionId, host.Value, createdAt, expiresAt);
    }

    public Result<RoomParticipant> Join(AnonymousSessionId sessionId, string displayName, DateTimeOffset joinedAt)
    {
        var open = EnsureOpen(joinedAt);
        if (open.IsFailure)
        {
            return Result.Failure<RoomParticipant>(open.Error);
        }

        var existing = participants.SingleOrDefault(participant => participant.SessionId == sessionId);
        if (existing is not null)
        {
            if (!existing.IsPresent)
            {
                var rejoined = existing.Rejoin(displayName, joinedAt);
                if (rejoined.IsFailure)
                {
                    return Result.Failure<RoomParticipant>(rejoined.Error);
                }
            }

            return existing;
        }

        if (participants.Count(participant => participant.IsPresent) >= MaxParticipants)
        {
            return Result.Failure<RoomParticipant>(RoomErrors.ParticipantLimitReached);
        }

        var participant = RoomParticipant.Join(Id, sessionId, displayName, 
            ParticipantRole.Viewer, joinedAt);
        
        if (participant.IsFailure)
        {
            return participant;
        }

        participants.Add(participant.Value);
        return participant;
    }

    public Result Leave(RoomParticipantId participantId, DateTimeOffset leftAt)
    {
        var participant = GetParticipant(participantId);
        return participant.IsFailure ? participant : participant.Value.Leave(leftAt);
    }

    public Result<ChatMessage> AddMessage(RoomParticipantId authorId, string body, DateTimeOffset sentAt)
    {
        var open = EnsureOpen(sentAt);
        if (open.IsFailure)
        {
            return Result.Failure<ChatMessage>(open.Error);
        }

        var author = GetParticipant(authorId);
        if (author.IsFailure)
        {
            return Result.Failure<ChatMessage>(author.Error);
        }

        if (!author.Value.IsPresent)
        {
            return Result.Failure<ChatMessage>(RoomErrors.AuthorNotPresent);
        }

        var message = ChatMessage.Create(Id, authorId, body, sentAt);
        if (message.IsFailure)
        {
            return message;
        }

        messages.Add(message.Value);
        return message;
    }

    public Result Close(AnonymousSessionId requestedBySessionId, DateTimeOffset closedAt)
    {
        var open = EnsureOpen(closedAt);
        if (open.IsFailure)
        {
            return open;
        }

        if (requestedBySessionId != HostSessionId)
        {
            return Result.Failure(RoomErrors.HostRequired);
        }

        Status = RoomStatus.Closed;
        ClosedAt = closedAt;
        return Result.Success();
    }

    public Result Expire(DateTimeOffset expiredAt)
    {
        if (Status != RoomStatus.Open)
        {
            return Result.Failure(RoomErrors.CannotExpire);
        }

        if (expiredAt < ExpiresAt)
        {
            return Result.Failure(RoomErrors.ExpireTooEarly);
        }

        Status = RoomStatus.Expired;
        ClosedAt = expiredAt;
        return Result.Success();
    }

    private Result<RoomParticipant> GetParticipant(RoomParticipantId participantId)
    {
        var participant = participants.SingleOrDefault(candidate => candidate.Id == participantId);
        return participant is null
            ? Result.Failure<RoomParticipant>(RoomErrors.ParticipantNotFound)
            : participant;
    }

    private Result EnsureOpen(DateTimeOffset at)
    {
        if (Status != RoomStatus.Open)
        {
            return Result.Failure(RoomErrors.NotOpen);
        }

        return at >= ExpiresAt
            ? Result.Failure(RoomErrors.Expired)
            : Result.Success();
    }
}
