using WatchParty.Domain.Common;
using WatchParty.SharedKernel;

namespace WatchParty.Domain.Models.Videos;

public sealed class VideoRendition : Entity<VideoRenditionId>
{
    public const int MaxNameLength = 50;
    public const int MaxObjectKeyLength = 1024;

    private VideoRendition()
    {
    }

    private VideoRendition(VideoRenditionId id, VideoId videoId, string name, string playlistObjectKey,
        int width, int height, int bitrate, DateTimeOffset createdAt) : base(id)
    {
        VideoId = videoId;
        Name = name;
        PlaylistObjectKey = playlistObjectKey;
        Width = width;
        Height = height;
        Bitrate = bitrate;
        CreatedAt = createdAt;
    }

    public VideoId VideoId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string PlaylistObjectKey { get; private set; } = string.Empty;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Bitrate { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    internal static Result<VideoRendition> Create(VideoId videoId, string name, string playlistObjectKey,
        int width, int height, int bitrate, DateTimeOffset createdAt)
    {
        if (width <= 0 || height <= 0)
        {
            return Result.Failure<VideoRendition>(VideoErrors.InvalidDimensions);
        }

        if (bitrate <= 0)
        {
            return Result.Failure<VideoRendition>(VideoErrors.InvalidBitrate);
        }

        var normalizedName = TextRules.Required(name, VideoErrors.RenditionNameRequired,
            VideoErrors.RenditionNameTooLong, MaxNameLength);
        
        if (normalizedName.IsFailure)
        {
            return Result.Failure<VideoRendition>(normalizedName.Error);
        }

        var normalizedKey = TextRules.Required(playlistObjectKey, VideoErrors.ObjectKeyRequired,
            VideoErrors.ObjectKeyTooLong, MaxObjectKeyLength);
        
        if (normalizedKey.IsFailure)
        {
            return Result.Failure<VideoRendition>(normalizedKey.Error);
        }

        return new VideoRendition(VideoRenditionId.New(), videoId, normalizedName.Value, normalizedKey.Value,
            width, height, bitrate, createdAt);
    }
}

public sealed record RenditionSpec(string Name, string PlaylistObjectKey, int Width, int Height, int Bitrate);
