using WatchParty.Domain.Common;
using WatchParty.Domain.Models.Admins;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Videos;

public sealed class Video : AggregateRoot<VideoId>
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;
    public const int MaxFileNameLength = 255;
    public const int MaxContentTypeLength = 100;
    public const int MaxObjectKeyLength = 1024;

    private readonly List<VideoRendition> renditions = [];
    private readonly List<TranscodingAttempt> transcodingAttempts = [];

    private Video()
    {
    }

    private Video(
        VideoId id,
        AdminId uploadedByAdminId,
        string title,
        string? description,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string originalObjectKey,
        DateTimeOffset createdAt)
        : base(id)
    {
        UploadedByAdminId = uploadedByAdminId;
        Title = title;
        Description = description;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        OriginalObjectKey = originalObjectKey;
        Status = VideoStatus.PendingUpload;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public AdminId UploadedByAdminId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string OriginalFileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public string OriginalObjectKey { get; private set; } = string.Empty;

    public string? HlsManifestObjectKey { get; private set; }

    public TimeSpan? Duration { get; private set; }

    public VideoStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public IReadOnlyCollection<VideoRendition> Renditions => renditions;

    public IReadOnlyCollection<TranscodingAttempt> TranscodingAttempts => transcodingAttempts;

    public static Result<Video> CreatePendingUpload(
        AdminId uploadedByAdminId,
        string title,
        string? description,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string originalObjectKey,
        DateTimeOffset createdAt)
    {
        if (sizeBytes <= 0)
        {
            return Result.Failure<Video>(VideoErrors.InvalidSize);
        }

        var normalizedTitle = TextRules.Required(title, VideoErrors.TitleRequired, VideoErrors.TitleTooLong, MaxTitleLength);
        if (normalizedTitle.IsFailure)
        {
            return Result.Failure<Video>(normalizedTitle.Error);
        }

        var normalizedDescription = TextRules.Optional(description, VideoErrors.DescriptionTooLong, MaxDescriptionLength);
        if (normalizedDescription.IsFailure)
        {
            return Result.Failure<Video>(normalizedDescription.Error);
        }

        var normalizedFileName = TextRules.Required(
            originalFileName,
            VideoErrors.FileNameRequired,
            VideoErrors.FileNameTooLong,
            MaxFileNameLength);
        if (normalizedFileName.IsFailure)
        {
            return Result.Failure<Video>(normalizedFileName.Error);
        }

        var normalizedContentType = TextRules.Required(
            contentType,
            VideoErrors.ContentTypeRequired,
            VideoErrors.ContentTypeTooLong,
            MaxContentTypeLength);
        if (normalizedContentType.IsFailure)
        {
            return Result.Failure<Video>(normalizedContentType.Error);
        }

        var normalizedObjectKey = TextRules.Required(
            originalObjectKey,
            VideoErrors.ObjectKeyRequired,
            VideoErrors.ObjectKeyTooLong,
            MaxObjectKeyLength);
        if (normalizedObjectKey.IsFailure)
        {
            return Result.Failure<Video>(normalizedObjectKey.Error);
        }

        return new Video(
            VideoId.New(),
            uploadedByAdminId,
            normalizedTitle.Value,
            normalizedDescription.Value,
            normalizedFileName.Value,
            normalizedContentType.Value,
            sizeBytes,
            normalizedObjectKey.Value,
            createdAt);
    }

    public Result MarkUploaded(DateTimeOffset uploadedAt)
    {
        var status = EnsureStatus(VideoStatus.PendingUpload);
        if (status.IsFailure)
        {
            return status;
        }

        Status = VideoStatus.Uploaded;
        UpdatedAt = uploadedAt;
        return Result.Success();
    }

    public Result<TranscodingAttempt> QueueTranscoding(AdminId requestedByAdminId, DateTimeOffset queuedAt)
    {
        if (Status is not (VideoStatus.Uploaded or VideoStatus.Failed or VideoStatus.Ready))
        {
            return Result.Failure<TranscodingAttempt>(VideoErrors.CannotQueue);
        }

        if (transcodingAttempts.Any(attempt =>
                attempt.Status is TranscodingStatus.Queued or TranscodingStatus.Processing))
        {
            return Result.Failure<TranscodingAttempt>(VideoErrors.TranscodingAlreadyActive);
        }

        var attempt = TranscodingAttempt.Queue(Id, requestedByAdminId, transcodingAttempts.Count + 1, queuedAt);
        transcodingAttempts.Add(attempt);
        Status = VideoStatus.Queued;
        UpdatedAt = queuedAt;
        return attempt;
    }

    public Result StartTranscoding(TranscodingAttemptId attemptId, DateTimeOffset startedAt)
    {
        var status = EnsureStatus(VideoStatus.Queued);
        if (status.IsFailure)
        {
            return status;
        }

        var attempt = GetAttempt(attemptId);
        if (attempt.IsFailure)
        {
            return attempt;
        }

        var started = attempt.Value.Start(startedAt);
        if (started.IsFailure)
        {
            return started;
        }

        Status = VideoStatus.Transcoding;
        UpdatedAt = startedAt;
        return Result.Success();
    }

    public Result CompleteTranscoding(
        TranscodingAttemptId attemptId,
        string hlsManifestObjectKey,
        TimeSpan duration,
        IEnumerable<RenditionSpec> renditionSpecs,
        DateTimeOffset completedAt)
    {
        var status = EnsureStatus(VideoStatus.Transcoding);
        if (status.IsFailure)
        {
            return status;
        }

        if (duration <= TimeSpan.Zero)
        {
            return Result.Failure(VideoErrors.InvalidDuration);
        }

        var specs = renditionSpecs.ToArray();
        if (specs.Length == 0)
        {
            return Result.Failure(VideoErrors.RenditionRequired);
        }

        var createdRenditions = specs
            .Select(spec => VideoRendition.Create(
                Id,
                spec.Name,
                spec.PlaylistObjectKey,
                spec.Width,
                spec.Height,
                spec.Bitrate,
                completedAt))
            .ToArray();
        if (createdRenditions.Any(rendition => rendition.IsFailure))
        {
            return Result.Failure(ValidationError.FromResults(createdRenditions));
        }

        var manifestKey = TextRules.Required(
            hlsManifestObjectKey,
            VideoErrors.ObjectKeyRequired,
            VideoErrors.ObjectKeyTooLong,
            MaxObjectKeyLength);
        if (manifestKey.IsFailure)
        {
            return manifestKey;
        }

        var attempt = GetAttempt(attemptId);
        if (attempt.IsFailure)
        {
            return attempt;
        }

        var completed = attempt.Value.Complete(completedAt);
        if (completed.IsFailure)
        {
            return completed;
        }

        renditions.Clear();
        renditions.AddRange(createdRenditions.Select(rendition => rendition.Value));
        HlsManifestObjectKey = manifestKey.Value;
        Duration = duration;
        Status = VideoStatus.Ready;
        UpdatedAt = completedAt;
        return Result.Success();
    }

    public Result FailTranscoding(TranscodingAttemptId attemptId, string error, DateTimeOffset failedAt)
    {
        if (Status is not (VideoStatus.Queued or VideoStatus.Transcoding))
        {
            return Result.Failure(VideoErrors.CannotFail);
        }

        var attempt = GetAttempt(attemptId);
        if (attempt.IsFailure)
        {
            return attempt;
        }

        var failed = attempt.Value.Fail(error, failedAt);
        if (failed.IsFailure)
        {
            return failed;
        }

        Status = VideoStatus.Failed;
        UpdatedAt = failedAt;
        return Result.Success();
    }

    public Result UpdateMetadata(string title, string? description, DateTimeOffset updatedAt)
    {
        if (Status == VideoStatus.Deleted)
        {
            return Result.Failure(VideoErrors.Deleted);
        }

        var normalizedTitle = TextRules.Required(title, VideoErrors.TitleRequired, VideoErrors.TitleTooLong, MaxTitleLength);
        if (normalizedTitle.IsFailure)
        {
            return normalizedTitle;
        }

        var normalizedDescription = TextRules.Optional(description, VideoErrors.DescriptionTooLong, MaxDescriptionLength);
        if (normalizedDescription.IsFailure)
        {
            return normalizedDescription;
        }

        Title = normalizedTitle.Value;
        Description = normalizedDescription.Value;
        UpdatedAt = updatedAt;
        return Result.Success();
    }

    public Result MarkDeleted(DateTimeOffset deletedAt)
    {
        if (Status == VideoStatus.Deleted)
        {
            return Result.Failure(VideoErrors.AlreadyDeleted);
        }

        var activeAttempt = transcodingAttempts.SingleOrDefault(attempt =>
            attempt.Status is TranscodingStatus.Queued or TranscodingStatus.Processing);
        if (activeAttempt is not null)
        {
            var cancelled = activeAttempt.Cancel(deletedAt);
            if (cancelled.IsFailure)
            {
                return cancelled;
            }
        }

        Status = VideoStatus.Deleted;
        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
        return Result.Success();
    }

    private Result<TranscodingAttempt> GetAttempt(TranscodingAttemptId attemptId)
    {
        var attempt = transcodingAttempts.SingleOrDefault(candidate => candidate.Id == attemptId);
        return attempt is null
            ? Result.Failure<TranscodingAttempt>(VideoErrors.AttemptNotFound)
            : attempt;
    }

    private Result EnsureStatus(VideoStatus expected) =>
        Status == expected
            ? Result.Success()
            : Result.Failure(VideoErrors.InvalidStatus(expected));
}
