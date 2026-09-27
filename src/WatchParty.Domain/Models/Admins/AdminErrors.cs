using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Admins;

public static class AdminErrors
{
    public static readonly Error UsernameRequired = Error.Validation(
        "Admin.UsernameRequired",
        "Укажите имя администратора.");

    public static readonly Error UsernameTooLong = Error.Validation(
        "Admin.UsernameTooLong",
        "Имя администратора должно быть не длиннее 100 символов.");

    public static readonly Error PasswordHashRequired = Error.Validation(
        "Admin.PasswordHashRequired",
        "Укажите хеш пароля.");

    public static readonly Error PasswordHashTooLong = Error.Validation(
        "Admin.PasswordHashTooLong",
        "Хеш пароля слишком длинный.");

    public static readonly Error AlreadyDisabled = Error.Conflict(
        "Admin.AlreadyDisabled",
        "Администратор уже отключён.");

    public static readonly Error AlreadyActive = Error.Conflict(
        "Admin.AlreadyActive",
        "Администратор уже активен.");
}
