namespace WatchParty.Domain.Models.Videos;

public readonly record struct VideoId(Guid Value)
{
    public static VideoId New() => new(Guid.NewGuid());
}

public readonly record struct VideoRenditionId(Guid Value)
{
    public static VideoRenditionId New() => new(Guid.NewGuid());
}

public readonly record struct TranscodingAttemptId(Guid Value)
{
    public static TranscodingAttemptId New() => new(Guid.NewGuid());
}
