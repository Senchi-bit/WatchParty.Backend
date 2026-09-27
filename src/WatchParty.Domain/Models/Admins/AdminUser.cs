using WatchParty.Domain.Common;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Admins;

public sealed class AdminUser : AggregateRoot<AdminId>
{
    public const int MaxUsernameLength = 100;
    public const int MaxPasswordHashLength = 2048;

    private AdminUser()
    {
    }

    private AdminUser(AdminId id, string username, string passwordHash, DateTimeOffset createdAt)
        : base(id)
    {
        Username = username;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string Username { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<AdminUser> Create(string username, string passwordHash, DateTimeOffset createdAt)
    {
        var normalizedUsername = NormalizeUsername(username);
        if (normalizedUsername.IsFailure)
        {
            return Result.Failure<AdminUser>(normalizedUsername.Error);
        }

        var normalizedPasswordHash = ValidatePasswordHash(passwordHash);
        if (normalizedPasswordHash.IsFailure)
        {
            return Result.Failure<AdminUser>(normalizedPasswordHash.Error);
        }

        return new AdminUser(AdminId.New(), normalizedUsername.Value, normalizedPasswordHash.Value, createdAt);
    }

    public Result ChangePasswordHash(string passwordHash, DateTimeOffset changedAt)
    {
        var normalizedPasswordHash = ValidatePasswordHash(passwordHash);
        if (normalizedPasswordHash.IsFailure)
        {
            return normalizedPasswordHash;
        }

        PasswordHash = normalizedPasswordHash.Value;
        UpdatedAt = changedAt;
        return Result.Success();
    }

    public Result Disable(DateTimeOffset disabledAt)
    {
        if (!IsActive)
        {
            return Result.Failure(AdminErrors.AlreadyDisabled);
        }

        IsActive = false;
        UpdatedAt = disabledAt;
        return Result.Success();
    }

    public Result Enable(DateTimeOffset enabledAt)
    {
        if (IsActive)
        {
            return Result.Failure(AdminErrors.AlreadyActive);
        }

        IsActive = true;
        UpdatedAt = enabledAt;
        return Result.Success();
    }

    private static Result<string> NormalizeUsername(string username)
    {
        var normalized = TextRules.Required(
            username,
            AdminErrors.UsernameRequired,
            AdminErrors.UsernameTooLong,
            MaxUsernameLength);
        return normalized.IsFailure
            ? normalized
            : normalized.Value.ToLowerInvariant();
    }

    private static Result<string> ValidatePasswordHash(string passwordHash) =>
        TextRules.Required(
            passwordHash,
            AdminErrors.PasswordHashRequired,
            AdminErrors.PasswordHashTooLong,
            MaxPasswordHashLength);
}
