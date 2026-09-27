using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Playback;

public static class PlaybackErrors
{
    public static readonly Error NegativePosition = Error.Validation(
        "Playback.NegativePosition",
        "Позиция воспроизведения не может быть отрицательной.");

    public static readonly Error NegativeRevision = Error.Validation(
        "Playback.NegativeRevision",
        "Ревизия воспроизведения не может быть отрицательной.");

    public static readonly Error UpdateMovedBackwards = Error.Validation(
        "Playback.UpdateMovedBackwards",
        "Время обновления воспроизведения не может сдвигаться назад.");
}
