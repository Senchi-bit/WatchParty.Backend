namespace WatchParty.Domain.Models.Videos;

public enum VideoStatus
{
    PendingUpload = 0,
    Uploaded = 1,
    Queued = 2,
    Transcoding = 3,
    Ready = 4,
    Failed = 5,
    Deleted = 6
}

public enum TranscodingStatus
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
