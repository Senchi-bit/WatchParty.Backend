using WatchParty.Domain.Common;
using WatchParty.Domain.Models.Rooms;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Rooms.Chat;

public readonly record struct ChatMessageId(Guid Value)
{
    public static ChatMessageId New() => new(Guid.NewGuid());
}

public sealed class ChatMessage : Entity<ChatMessageId>
{
    public const int MaxBodyLength = 2000;

    private ChatMessage()
    {
    }

    private ChatMessage(ChatMessageId id, RoomId roomId, RoomParticipantId authorId, string body, 
        DateTimeOffset sentAt) : base(id)
    {
        RoomId = roomId;
        AuthorId = authorId;
        Body = body;
        SentAt = sentAt;
    }

    public RoomId RoomId { get; private set; }

    public RoomParticipantId AuthorId { get; private set; }

    public string Body { get; private set; } = string.Empty;

    public DateTimeOffset SentAt { get; private set; }

    internal static Result<ChatMessage> Create(RoomId roomId, RoomParticipantId authorId, string body, 
        DateTimeOffset sentAt)
    {
        var normalizedBody = TextRules.Required(
            body,
            RoomErrors.MessageRequired,
            RoomErrors.MessageTooLong,
            MaxBodyLength);
        return normalizedBody.IsFailure ? Result.Failure<ChatMessage>(normalizedBody.Error)
            : new ChatMessage(ChatMessageId.New(), roomId, authorId, normalizedBody.Value, sentAt);
    }
}
