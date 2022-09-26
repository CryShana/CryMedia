using CryMediaAPI.Audio.Models;
using CryMediaAPI.BaseClasses;

using System.Linq;
using System.Text.Json.Serialization;

namespace CryMediaAPI.Video.Models;

// prepare for source generation
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(VideoMetadata))]
[JsonSerializable(typeof(AudioMetadata))]
internal partial class SourceGenerationContext : JsonSerializerContext {}

public class VideoMetadata
{
    /// <summary>
    /// Video pixel format
    /// </summary>
    public string PixelFormat { get; set; }

    /// <summary>
    /// Video codec (long name)
    /// </summary>
    public string CodecLongName { get; set; }

    /// <summary>
    /// Video codec
    /// </summary>
    public string Codec { get; set; }

    /// <summary>
    /// Video width
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Video height
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Video duration in seconds
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// Average video framerate
    /// </summary>
    public double AvgFramerate { get; set; }

    /// <summary>
    /// Average video framerate in frequency format
    /// </summary>
    public string AvgFramerateText { get; set; }

    /// <summary>
    /// Average video bitrate
    /// </summary>
    public int BitRate { get; set; }

    /// <summary>
    /// Bits per sample
    /// </summary>
    public int BitDepth { get; set; }

    /// <summary>
    /// Pixel aspect ratio
    /// </summary>
    public string SampleAspectRatio { get; set; }

    /// <summary>
    /// Predicted frame count based on average framerate and duration
    /// </summary>
    public int PredictedFrameCount { get; set; }

    /// <summary>
    /// Get first video stream
    /// </summary>
    public MediaStream GetFirstVideoStream() => Streams.Where(x => x.IsVideo).FirstOrDefault();

    /// <summary>
    /// Get first audio stream
    /// </summary>
    public MediaStream GetFirstAudioStream() => Streams.Where(x => x.IsAudio).FirstOrDefault();

    /// <summary>
    /// Media streams inside the file. Can contain non-video streams as well.
    /// </summary>
    [JsonPropertyName("streams")]
    public MediaStream[] Streams { get; set; }

    /// <summary>
    /// File format information.
    /// </summary>
    [JsonPropertyName("format")]
    public VideoFormat Format { get; set; }

    public VideoMetadata() { }
}

public class VideoFormat
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; }

    [JsonPropertyName("nb_streams")]
    public long NbStreams { get; set; }

    [JsonPropertyName("nb_programs")]
    public long NbPrograms { get; set; }

    [JsonPropertyName("format_name")]
    public string FormatName { get; set; }

    [JsonPropertyName("format_long_name")]
    public string FormatLongName { get; set; }

    [JsonPropertyName("start_time")]
    public string StartTime { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("size")]
    public string Size { get; set; }

    [JsonPropertyName("bit_rate")]
    public string BitRate { get; set; }

    [JsonPropertyName("probe_score")]
    public long ProbeScore { get; set; }

    [JsonPropertyName("tags")]
    public VideoFormatTags Tags { get; set; }
}

public class VideoFormatTags
{
    [JsonPropertyName("major_brand")]
    public string MajorBrand { get; set; }

    [JsonPropertyName("minor_version")]
    public string MinorVersion { get; set; }

    [JsonPropertyName("compatible_brands")]
    public string CompatibleBrands { get; set; }

    [JsonPropertyName("creation_time")]
    public string CreationTime { get; set; }

    [JsonPropertyName("encoder")]
    public string Encoder { get; set; }
}