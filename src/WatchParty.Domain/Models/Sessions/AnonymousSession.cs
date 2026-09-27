using WatchParty.Domain.Common;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Sessions;

public sealed class AnonymousSession : AggregateRoot<AnonymousSessionId>
{
    public const int MaxDisplayNameLength = 50;

    private AnonymousSession()
    {
    }

    private AnonymousSession(AnonymousSessionId id, DateTimeOffset createdAt, DateTimeOffset expiresAt,
        string? displayName) : base(id)
    {
        DisplayName = displayName;
        CreatedAt = createdAt;
        LastSeenAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public string? DisplayName { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public static Result<AnonymousSession> Create(AnonymousSessionId id, DateTimeOffset createdAt, 
        DateTimeOffset expiresAt, string? displayName = null)
    {
        if (expiresAt <= createdAt)
        {
            return Result.Failure<AnonymousSession>(SessionErrors.ExpirationBeforeCreation);
        }

        var normalizedDisplayName = NormalizeDisplayName(displayName);
        return normalizedDisplayName.IsFailure
            ? Result.Failure<AnonymousSession>(normalizedDisplayName.Error)
            : new AnonymousSession(id, createdAt, expiresAt, normalizedDisplayName.Value);
    }

    public Result Rename(string displayName)
    {
        var normalizedDisplayName = NormalizeDisplayName(displayName);
        if (normalizedDisplayName.IsFailure)
        {
            return normalizedDisplayName;
        }

        DisplayName = normalizedDisplayName.Value;
        return Result.Success();
    }

    public Result Touch(DateTimeOffset seenAt, DateTimeOffset expiresAt)
    {
        if (seenAt < LastSeenAt)
        {
            return Result.Failure(SessionErrors.LastSeenMovedBackwards);
        }

        if (expiresAt <= seenAt)
        {
            return Result.Failure(SessionErrors.ExpirationBeforeLastSeen);
        }

        LastSeenAt = seenAt;
        ExpiresAt = expiresAt;
        return Result.Success();
    }

    public bool IsExpired(DateTimeOffset at) => at >= ExpiresAt;

    private static Result<string?> NormalizeDisplayName(string? displayName) =>
        TextRules.Optional(displayName, SessionErrors.DisplayNameTooLong, MaxDisplayNameLength);
}
