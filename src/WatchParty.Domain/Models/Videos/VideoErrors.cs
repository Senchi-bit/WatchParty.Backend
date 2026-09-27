using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Videos;

public static class VideoErrors
{
    public static readonly Error TitleRequired = Error.Validation("Video.TitleRequired", "Укажите название видео.");
    public static readonly Error TitleTooLong = Error.Validation("Video.TitleTooLong", "Название видео должно быть не длиннее 200 символов.");
    public static readonly Error DescriptionTooLong = Error.Validation("Video.DescriptionTooLong", "Описание видео должно быть не длиннее 2000 символов.");
    public static readonly Error FileNameRequired = Error.Validation("Video.FileNameRequired", "Укажите имя исходного файла.");
    public static readonly Error FileNameTooLong = Error.Validation("Video.FileNameTooLong", "Имя исходного файла слишком длинное.");
    public static readonly Error ContentTypeRequired = Error.Validation("Video.ContentTypeRequired", "Укажите тип содержимого.");
    public static readonly Error ContentTypeTooLong = Error.Validation("Video.ContentTypeTooLong", "Тип содержимого слишком длинный.");
    public static readonly Error ObjectKeyRequired = Error.Validation("Video.ObjectKeyRequired", "Укажите ключ объекта.");
    public static readonly Error ObjectKeyTooLong = Error.Validation("Video.ObjectKeyTooLong", "Ключ объекта слишком длинный.");
    public static readonly Error InvalidSize = Error.Validation("Video.InvalidSize", "Размер видео должен быть положительным.");
    public static readonly Error InvalidDuration = Error.Validation("Video.InvalidDuration", "Длительность видео должна быть положительной.");
    public static readonly Error RenditionRequired = Error.Validation("Video.RenditionRequired", "Нужна хотя бы одна HLS-дорожка.");
    public static readonly Error InvalidDimensions = Error.Validation("Video.InvalidDimensions", "Размеры дорожки должны быть положительными.");
    public static readonly Error InvalidBitrate = Error.Validation("Video.InvalidBitrate", "Битрейт дорожки должен быть положительным.");
    public static readonly Error RenditionNameRequired = Error.Validation("Video.RenditionNameRequired", "Укажите имя дорожки.");
    public static readonly Error RenditionNameTooLong = Error.Validation("Video.RenditionNameTooLong", "Имя дорожки слишком длинное.");
    public static readonly Error CannotQueue = Error.Conflict("Video.CannotQueue", "Видео нельзя поставить в очередь транскодирования.");
    public static readonly Error TranscodingAlreadyActive = Error.Conflict("Video.TranscodingAlreadyActive", "У видео уже есть активная попытка транскодирования.");
    public static readonly Error CannotFail = Error.Conflict("Video.CannotFail", "Транскодирование нельзя завершить ошибкой в текущем статусе.");
    public static readonly Error Deleted = Error.Conflict("Video.Deleted", "Удалённое видео нельзя изменить.");
    public static readonly Error AlreadyDeleted = Error.Conflict("Video.AlreadyDeleted", "Видео уже удалено.");
    public static readonly Error AttemptNotFound = Error.NotFound("Video.AttemptNotFound", "Попытка транскодирования не принадлежит этому видео.");
    public static readonly Error AttemptCannotStart = Error.Conflict("Video.AttemptCannotStart", "Можно запустить только попытку в очереди.");
    public static readonly Error AttemptCannotComplete = Error.Conflict("Video.AttemptCannotComplete", "Можно завершить только выполняющуюся попытку.");
    public static readonly Error AttemptCannotFail = Error.Conflict("Video.AttemptCannotFail", "Ошибкой можно завершить только попытку в очереди или в работе.");
    public static readonly Error AttemptCannotCancel = Error.Conflict("Video.AttemptCannotCancel", "Можно отменить только попытку в очереди или в работе.");
    public static readonly Error TranscodingErrorRequired = Error.Validation("Video.TranscodingErrorRequired", "Укажите ошибку транскодирования.");
    public static readonly Error TranscodingErrorTooLong = Error.Validation("Video.TranscodingErrorTooLong", "Текст ошибки транскодирования слишком длинный.");

    public static Error InvalidStatus(VideoStatus expected) =>
        Error.Conflict("Video.InvalidStatus", $"Видео должно быть в статусе «{ToRussian(expected)}».");

    private static string ToRussian(VideoStatus status) => status switch
    {
        VideoStatus.PendingUpload => "ожидание загрузки",
        VideoStatus.Uploaded => "загружено",
        VideoStatus.Queued => "в очереди",
        VideoStatus.Transcoding => "транскодирование",
        VideoStatus.Ready => "готово",
        VideoStatus.Failed => "ошибка",
        VideoStatus.Deleted => "удалено",
        _ => status.ToString()
    };
}
