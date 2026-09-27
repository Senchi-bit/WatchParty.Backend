using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Rooms;

public static class RoomErrors
{
    public static readonly Error ExpirationBeforeCreation = Error.Validation(
        "Room.ExpirationBeforeCreation",
        "Срок комнаты должен быть позже времени создания.");

    public static readonly Error InvalidJoinCode = Error.Validation(
        "Room.InvalidJoinCode",
        "Код входа должен содержать ровно 6 латинских букв или цифр.");

    public static readonly Error DisplayNameRequired = Error.Validation(
        "Room.DisplayNameRequired",
        "Укажите имя участника.");

    public static readonly Error DisplayNameTooLong = Error.Validation(
        "Room.DisplayNameTooLong",
        "Имя участника должно быть не длиннее 50 символов.");

    public static readonly Error ParticipantLimitReached = Error.Conflict(
        "Room.ParticipantLimitReached",
        "Достигнут лимит участников комнаты.");

    public static readonly Error ParticipantNotFound = Error.NotFound(
        "Room.ParticipantNotFound",
        "Участник не принадлежит этой комнате.");

    public static readonly Error ParticipantAlreadyLeft = Error.Conflict(
        "Room.ParticipantAlreadyLeft",
        "Участник уже покинул комнату.");

    public static readonly Error LeaveBeforeJoin = Error.Validation(
        "Room.LeaveBeforeJoin",
        "Время выхода не может быть раньше времени входа.");

    public static readonly Error ParticipantAlreadyPresent = Error.Conflict(
        "Room.ParticipantAlreadyPresent",
        "Участник уже находится в комнате.");

    public static readonly Error AuthorNotPresent = Error.Conflict(
        "Room.AuthorNotPresent",
        "Сообщение может отправить только присутствующий участник.");

    public static readonly Error MessageRequired = Error.Validation(
        "Room.MessageRequired",
        "Укажите текст сообщения.");

    public static readonly Error MessageTooLong = Error.Validation(
        "Room.MessageTooLong",
        "Сообщение должно быть не длиннее 2000 символов.");

    public static readonly Error NotOpen = Error.Conflict(
        "Room.NotOpen",
        "Комната закрыта.");

    public static readonly Error Expired = Error.Conflict(
        "Room.Expired",
        "Срок комнаты истёк.");

    public static readonly Error HostRequired = Error.Conflict(
        "Room.HostRequired",
        "Закрыть комнату может только её создатель.");

    public static readonly Error CannotExpire = Error.Conflict(
        "Room.CannotExpire",
        "Истечь может только открытая комната.");

    public static readonly Error ExpireTooEarly = Error.Validation(
        "Room.ExpireTooEarly",
        "Комната не может истечь раньше назначенного срока.");
}
