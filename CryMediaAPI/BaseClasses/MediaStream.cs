using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace CryMediaAPI.BaseClasses
{
    public class MediaStream
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
        public bool IsAudio => CodecType.ToLowerInvariant().Trim() == "audio";
        public bool IsVideo => CodecType.ToLowerInvariant().Trim() == "video";

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

        double? avgfpsnum = null;
        public double AvgFrameRateNumber
        {
            get
            {
                if (avgfpsnum == null)
                {
                    if (AvgFrameRate.Contains('/'))
                    {
                        var parsed = AvgFrameRate.Split('/');
                        avgfpsnum = double.Parse(parsed[0], CultureInfo.InvariantCulture) / double.Parse(parsed[1], CultureInfo.InvariantCulture);
                    }
                    else avgfpsnum = double.Parse(AvgFrameRate, CultureInfo.InvariantCulture);
                }

                return avgfpsnum.Value;
            }
        }


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
        public int SampleRateNumber => string.IsNullOrEmpty(SampleRate) ? -1 : int.Parse(SampleRate);

        [JsonPropertyName("channels")]
        public int? Channels { get; set; }

        [JsonPropertyName("channel_layout")]
        public string ChannelLayout { get; set; }

        [JsonPropertyName("bits_per_sample")]
        public int? BitsPerSample { get; set; }

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
