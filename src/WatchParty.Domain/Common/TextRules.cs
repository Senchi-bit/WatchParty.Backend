using WatchParty.SharedKernel;

namespace WatchParty.Domain.Common;

internal static class TextRules
{
    public static Result<string> Required(string? value, Error required, Error tooLong, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Result.Failure<string>(required);
        }

        return normalized.Length > maxLength ? Result.Failure<string>(tooLong) : normalized;
    }

    public static Result<string?> Optional(string? value, Error tooLong, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success<string?>(null);
        }

        var required = Required(value, tooLong, tooLong, maxLength);
        return required.IsFailure ? Result.Failure<string?>(required.Error) : required.Value;
    }
}
