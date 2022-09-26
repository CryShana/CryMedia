using CryMediaAPI.BaseClasses;

using System.Linq;
using System.Text.Json.Serialization;

namespace CryMediaAPI.Audio.Models;

public class AudioMetadata
{
    /// <summary>
    /// Audio sample format
    /// </summary>
    public string SampleFormat { get; set; }

    /// <summary>
    /// Audio codec (long name)
    /// </summary>
    public string CodecLongName { get; set; }

    /// <summary>
    /// Audio codec
    /// </summary>
    public string Codec { get; set; }

    /// <summary>
    /// Audio channel count
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Audio sample rate
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Audio duration in seconds
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// Average audio bitrate
    /// </summary>
    public int BitRate { get; set; }

    /// <summary>
    /// Bits per sample
    /// </summary>
    public int BitDepth { get; set; }

    /// <summary>
    /// Predicted sample count based on sample rate and duration
    /// </summary>
    public long PredictedSampleCount { get; set; }

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
    public AudioFormat Format { get; set; }

    public AudioMetadata() { }
}
public class AudioFormat
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
}

public class Tags
{
    [JsonPropertyName("encoder")]
    public string Encoder { get; set; }
}

