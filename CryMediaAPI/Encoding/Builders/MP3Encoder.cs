namespace CryMediaAPI.Encoding.Builders;

public class MP3Encoder : EncoderOptionsBuilder
{
    /// <summary>
    /// Set channel count, leave 'null' to match source
    /// </summary>
    public int? ChannelCount { get; set; } = null;
    /// <summary>
    /// Set sample rate, leave 'null' to match source
    /// </summary>
    public int? SampleRate { get; set; } = null;

    public override string Format { get; set; } = "mp3";
    public override string Name => "libmp3lame";
    public string CurrentQualitySettings { get; private set; }

    public MP3Encoder()
    {
        SetCQP();
    }

    /// <summary>
    /// Constant bitrate encoding. (This mode is considered wasteful, use CQP instead)
    /// </summary>
    /// <param name="bitrate">Target bitrate (ex: '320k', '250k', ...)</param>
    public void SetCBR(string bitrate)
    {
        CurrentQualitySettings = $"-b:a {bitrate}";
    }

    /// <summary>
    /// Average bitrate encoding.
    /// </summary>
    /// <param name="avg_bitrate">Average target bitrate (ex: '320k', '250k', ...)</param>
    public void SetABR(string avg_bitrate)
    {
        CurrentQualitySettings = $"-b:a {avg_bitrate} -abr 1";
    }

    /// <summary>
    /// Constant quality encoding - VBR mode (0 produces around 240kbps, 1 -> 220kbps, 2 -> 190, ...)
    /// </summary>
    /// <param name="qscale">Number from 0 to 9 (Lower = higher quality)</param>
    public void SetCQP(int qscale = 4)
    {
        CurrentQualitySettings = $"-q:a {qscale}";
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
