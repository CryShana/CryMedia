using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CryMediaAPI.Video.Models
{
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

        public int PredictedFrameCount { get; set; }

        
        /// <summary>
        /// Media streams inside the video file. Can contain non-video streams as well.
        /// </summary>
        [JsonPropertyName("streams")]
        public VideoStream[] Streams { get; set; }

        /// <summary>
        /// Video file format information.
        /// </summary>
        [JsonPropertyName("format")]
        public VideoFormat Format { get; set; }
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
        public DateTimeOffset CreationTime { get; set; }

        [JsonPropertyName("encoder")]
        public string Encoder { get; set; }
    }

    public class VideoStream
    {
        [JsonPropertyName("index")]
        public long Index { get; set; }

        [JsonPropertyName("codec_name")]
        public string CodecName { get; set; }

        [JsonPropertyName("codec_long_name")]
        public string CodecLongName { get; set; }

        [JsonPropertyName("profile")]
        public string Profile { get; set; }

        [JsonPropertyName("codec_type")]
        public string CodecType { get; set; }

        [JsonPropertyName("codec_time_base")]
        public string CodecTimeBase { get; set; }

        [JsonPropertyName("codec_tag_string")]
        public string CodecTagString { get; set; }

        [JsonPropertyName("codec_tag")]
        public string CodecTag { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("coded_width")]
        public long? CodedWidth { get; set; }

        [JsonPropertyName("coded_height")]
        public long? CodedHeight { get; set; }

        [JsonPropertyName("has_b_frames")]
        public long? HasBFrames { get; set; }

        [JsonPropertyName("sample_aspect_ratio")]
        public string SampleAspectRatio { get; set; }

        [JsonPropertyName("display_aspect_ratio")]
        public string DisplayAspectRatio { get; set; }

        [JsonPropertyName("pix_fmt")]
        public string PixFmt { get; set; }

        [JsonPropertyName("level")]
        public long? Level { get; set; }

        [JsonPropertyName("color_range")]
        public string ColorRange { get; set; }

        [JsonPropertyName("color_space")]
        public string ColorSpace { get; set; }

        [JsonPropertyName("color_transfer")]
        public string ColorTransfer { get; set; }

        [JsonPropertyName("color_primaries")]
        public string ColorPrimaries { get; set; }

        [JsonPropertyName("chroma_location")]
        public string ChromaLocation { get; set; }

        [JsonPropertyName("refs")]
        public long? Refs { get; set; }

        [JsonPropertyName("is_avc")]
        public string IsAvc { get; set; }

        [JsonPropertyName("nal_length_size")]
        public string NalLengthSize { get; set; }

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

        [JsonPropertyName("bits_per_raw_sample")]
        public string BitsPerRawSample { get; set; }

        [JsonPropertyName("nb_frames")]
        public string NbFrames { get; set; }

        [JsonPropertyName("disposition")]
        public Dictionary<string, long> Disposition { get; set; }

        [JsonPropertyName("tags")]
        public StreamTags Tags { get; set; }

        [JsonPropertyName("sample_fmt")]
        public string SampleFmt { get; set; }

        [JsonPropertyName("sample_rate")]
        public string SampleRate { get; set; }

        [JsonPropertyName("channels")]
        public long? Channels { get; set; }

        [JsonPropertyName("channel_layout")]
        public string ChannelLayout { get; set; }

        [JsonPropertyName("bits_per_sample")]
        public long? BitsPerSample { get; set; }

        [JsonPropertyName("max_bit_rate")]
        public string MaxBitRate { get; set; }
    }

    public class StreamTags
    {
        [JsonPropertyName("creation_time")]
        public DateTimeOffset CreationTime { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("handler_name")]
        public string HandlerName { get; set; }
    }
}
