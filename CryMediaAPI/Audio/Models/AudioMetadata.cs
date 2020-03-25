using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CryMediaAPI.Audio.Models
{
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

        public long PredictedSampleCount { get; set; }


        [JsonPropertyName("streams")]
        public AudioStream[] Streams { get; set; }

        [JsonPropertyName("format")]
        public AudioFormat Format { get; set; }
    }
    public partial class AudioFormat
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

    public partial class AudioStream
    {
        [JsonPropertyName("index")]
        public long Index { get; set; }

        [JsonPropertyName("codec_name")]
        public string CodecName { get; set; }

        [JsonPropertyName("codec_long_name")]
        public string CodecLongName { get; set; }

        [JsonPropertyName("codec_type")]
        public string CodecType { get; set; }

        [JsonPropertyName("codec_time_base")]
        public string CodecTimeBase { get; set; }

        [JsonPropertyName("codec_tag_string")]
        public string CodecTagString { get; set; }

        [JsonPropertyName("codec_tag")]
        public string CodecTag { get; set; }

        [JsonPropertyName("sample_fmt")]
        public string SampleFmt { get; set; }

        [JsonPropertyName("sample_rate")]
        public string SampleRate { get; set; }

        [JsonPropertyName("channels")]
        public int Channels { get; set; }

        [JsonPropertyName("channel_layout")]
        public string ChannelLayout { get; set; }

        [JsonPropertyName("bits_per_sample")]
        public int BitsPerSample { get; set; }

        [JsonPropertyName("r_frame_rate")]
        public string RFrameRate { get; set; }

        [JsonPropertyName("avg_frame_rate")]
        public string AvgFrameRate { get; set; }

        [JsonPropertyName("time_base")]
        public string TimeBase { get; set; }

        [JsonPropertyName("start_pts")]
        public long StartPts { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("duration_ts")]
        public long DurationTs { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("bit_rate")]
        public string BitRate { get; set; }

        [JsonPropertyName("disposition")]
        public Dictionary<string, long> Disposition { get; set; }

        [JsonPropertyName("tags")]
        public Tags Tags { get; set; }
    }

    public partial class Tags
    {
        [JsonPropertyName("encoder")]
        public string Encoder { get; set; }
    }

}
