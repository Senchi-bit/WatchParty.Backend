using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Sessions;

public static class SessionErrors
{
    public static readonly Error ExpirationBeforeCreation = Error.Validation(
        "Session.ExpirationBeforeCreation",
        "Срок сессии должен быть позже времени создания.");

    public static readonly Error LastSeenMovedBackwards = Error.Validation(
        "Session.LastSeenMovedBackwards",
        "Время последнего посещения не может сдвигаться назад.");

    public static readonly Error ExpirationBeforeLastSeen = Error.Validation(
        "Session.ExpirationBeforeLastSeen",
        "Срок сессии должен быть позже времени последнего посещения.");

    public static readonly Error DisplayNameTooLong = Error.Validation(
        "Session.DisplayNameTooLong",
        "Отображаемое имя должно быть не длиннее 50 символов.");
}
