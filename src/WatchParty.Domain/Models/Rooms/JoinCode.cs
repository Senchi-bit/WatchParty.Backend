using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Rooms;

public sealed record JoinCode
{
    public const int Length = 6;

    private JoinCode(string value) => Value = value;

    public string Value { get; }

    public static Result<JoinCode> Create(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return normalized.Length == Length && normalized.All(char.IsAsciiLetterOrDigit)
            ? new JoinCode(normalized)
            : Result.Failure<JoinCode>(RoomErrors.InvalidJoinCode);
    }

    internal static JoinCode Restore(string value) => new(value);

    public override string ToString() => Value;
}
