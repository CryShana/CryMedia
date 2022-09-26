using System;
using System.Collections.Generic;
using System.Text;

namespace CryMediaAPI.Encoding.Builders;

public class AACEncoder : EncoderOptionsBuilder
{
    /// <summary>
    /// Set channel count, leave 'null' to match source
    /// </summary>
    public int? ChannelCount { get; set; } = null;
    /// <summary>
    /// Set sample rate, leave 'null' to match source
    /// </summary>
    public int? SampleRate { get; set; } = null;

    public override string Format { get; set; } = "m4a";
    public override string Name => "aac";
    public string CurrentQualitySettings { get; private set; }

    public AACEncoder()
    {
        SetCBR();
    }

    /// <summary>
    /// Constant bitrate encoding
    /// </summary>
    /// <param name="bitrate">Target bitrate (ex: '320k', '128k', ...)</param>
    public void SetCBR(string bitrate = "128k")
    {
        CurrentQualitySettings = $"-b:a {bitrate}";
    }

    public override EncoderOptions Create()
    {
        return new EncoderOptions
        {
            Format = Format,
            EncoderName = Name,
            EncoderArguments = $"{CurrentQualitySettings}" +
                (ChannelCount == null ? "" : $" -ac {ChannelCount}") +
                (SampleRate == null ? "" : $" -ar {SampleRate}")
        };
    }
}
