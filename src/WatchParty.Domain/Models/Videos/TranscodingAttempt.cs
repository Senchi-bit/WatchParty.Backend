using WatchParty.Domain.Common;
using WatchParty.Domain.Models.Admins;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Videos;

public sealed class TranscodingAttempt : Entity<TranscodingAttemptId>
{
    public const int MaxErrorLength = 4000;

    private TranscodingAttempt()
    {
    }

    private TranscodingAttempt(
        TranscodingAttemptId id,
        VideoId videoId,
        AdminId requestedByAdminId,
        int attemptNumber,
        DateTimeOffset queuedAt)
        : base(id)
    {
        VideoId = videoId;
        RequestedByAdminId = requestedByAdminId;
        AttemptNumber = attemptNumber;
        Status = TranscodingStatus.Queued;
        QueuedAt = queuedAt;
    }

    public VideoId VideoId { get; private set; }

    public AdminId RequestedByAdminId { get; private set; }

    public int AttemptNumber { get; private set; }

    public TranscodingStatus Status { get; private set; }

    public string? Error { get; private set; }

    public DateTimeOffset QueuedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    internal static TranscodingAttempt Queue(VideoId videoId, AdminId requestedByAdminId, int attemptNumber,
        DateTimeOffset queuedAt) =>
        new(TranscodingAttemptId.New(), videoId, requestedByAdminId, attemptNumber, queuedAt);

    internal Result Start(DateTimeOffset startedAt)
    {
        if (Status != TranscodingStatus.Queued)
        {
            return Result.Failure(VideoErrors.AttemptCannotStart);
        }

        Status = TranscodingStatus.Processing;
        StartedAt = startedAt;
        return Result.Success();
    }

    internal Result Complete(DateTimeOffset completedAt)
    {
        if (Status != TranscodingStatus.Processing)
        {
            return Result.Failure(VideoErrors.AttemptCannotComplete);
        }

        Status = TranscodingStatus.Completed;
        CompletedAt = completedAt;
        return Result.Success();
    }

    internal Result Fail(string error, DateTimeOffset failedAt)
    {
        if (Status is not (TranscodingStatus.Queued or TranscodingStatus.Processing))
        {
            return Result.Failure(VideoErrors.AttemptCannotFail);
        }

        var normalizedError = TextRules.Required(
            error,
            VideoErrors.TranscodingErrorRequired,
            VideoErrors.TranscodingErrorTooLong,
            MaxErrorLength);
        if (normalizedError.IsFailure)
        {
            return normalizedError;
        }

        Error = normalizedError.Value;
        Status = TranscodingStatus.Failed;
        CompletedAt = failedAt;
        return Result.Success();
    }

    internal Result Cancel(DateTimeOffset cancelledAt)
    {
        if (Status is not (TranscodingStatus.Queued or TranscodingStatus.Processing))
        {
            return Result.Failure(VideoErrors.AttemptCannotCancel);
        }

        Status = TranscodingStatus.Cancelled;
        CompletedAt = cancelledAt;
        return Result.Success();
    }
}
