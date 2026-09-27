using System.Text.Json.Serialization;
using WatchParty.Domain.Models.Rooms;
using WatchParty.Domain.Models.Videos;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Playback;

public enum PlaybackStatus
{
    Paused = 0,
    Playing = 1
}

public sealed record PlaybackSnapshot
{
    [JsonConstructor]
    public PlaybackSnapshot(
        VideoId videoId,
        PlaybackStatus status,
        TimeSpan position,
        long revision,
        DateTimeOffset updatedAt,
        RoomParticipantId updatedByParticipantId)
    {
        VideoId = videoId;
        Status = status;
        Position = position;
        Revision = revision;
        UpdatedAt = updatedAt;
        UpdatedByParticipantId = updatedByParticipantId;
    }

    public VideoId VideoId { get; init; }

    public PlaybackStatus Status { get; init; }

    public TimeSpan Position { get; init; }

    public long Revision { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public RoomParticipantId UpdatedByParticipantId { get; init; }

    public static Result<PlaybackSnapshot> Create(
        VideoId videoId,
        PlaybackStatus status,
        TimeSpan position,
        long revision,
        DateTimeOffset updatedAt,
        RoomParticipantId updatedByParticipantId)
    {
        var validation = Validate(position, revision);
        return validation.IsFailure
            ? Result.Failure<PlaybackSnapshot>(validation.Error)
            : new PlaybackSnapshot(videoId, status, position, revision, updatedAt, updatedByParticipantId);
    }

    public Result<PlaybackSnapshot> Apply(
        PlaybackStatus status,
        TimeSpan position,
        RoomParticipantId participantId,
        DateTimeOffset updatedAt)
    {
        if (updatedAt < UpdatedAt)
        {
            return Result.Failure<PlaybackSnapshot>(PlaybackErrors.UpdateMovedBackwards);
        }

        var validation = Validate(position, Revision + 1);
        return validation.IsFailure
            ? Result.Failure<PlaybackSnapshot>(validation.Error)
            : new PlaybackSnapshot(VideoId, status, position, Revision + 1, updatedAt, participantId);
    }

    private static Result Validate(TimeSpan position, long revision)
    {
        if (position < TimeSpan.Zero)
        {
            return Result.Failure(PlaybackErrors.NegativePosition);
        }

        return revision < 0
            ? Result.Failure(PlaybackErrors.NegativeRevision)
            : Result.Success();
    }
}
