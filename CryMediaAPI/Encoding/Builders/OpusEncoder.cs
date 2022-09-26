namespace CryMediaAPI.Encoding.Builders;

public class OpusEncoder : EncoderOptionsBuilder
{
    /// <summary>
    /// Set channel count, leave 'null' to match source
    /// </summary>
    public int? ChannelCount { get; set; } = null;
    /// <summary>
    /// Set sample rate, leave 'null' to match source
    /// </summary>
    public int? SampleRate { get; set; } = null;

    /// <summary>
    /// Set intended application type. (Default: audio)
    /// </summary>
    public Application CodecApplication { get; set; } = Application.Audio;
    /// <summary>
    /// Set encoding algorithm complexity (0-10, 10 gives highest quality but is slowest). (Default: 10)
    /// </summary>
    public int CompressionLevel { get; set; } = 10;

    public override string Format { get; set; } = "ogg";
    public override string Name => "libopus";
    public string CurrentQualitySettings { get; private set; }

    public OpusEncoder()
    {
        SetVBR();
    }

    /// <summary>
    /// Constant bitrate encoding.
    /// </summary>
    /// <param name="bitrate">Target bitrate (ex: '320k', '250k', ...)</param>
    public void SetCBR(string bitrate)
    {
        CurrentQualitySettings = $"-b:a {bitrate} -vbr off";
    }

    /// <summary>
    /// Average bitrate encoding. (Default)
    /// </summary>
    /// <param name="bitrate">Target bitrate (ex: '320k', '250k', ...)</param>
    public void SetVBR(string bitrate = "128k")
    {
        CurrentQualitySettings = $"-b:a {bitrate} -vbr on";
    }

    /// <summary>
    /// Constrained VBR encoding
    /// </summary>
    /// <param name="bitrate">Target bitrate (ex: '320k', '250k', ...)</param>
    public void SetCVBR(string bitrate = "128k")
    {
        CurrentQualitySettings = $"-b:a {bitrate} -vbr constrained";
    }

    public override EncoderOptions Create()
    {
        return new EncoderOptions
        {
            Format = Format,
            EncoderName = Name,
            EncoderArguments = $"{CurrentQualitySettings} " +
                $"-application {CodecApplication.ToString().ToLowerInvariant()} " +
                $"-compression_level {CompressionLevel}" +
                (ChannelCount == null ? "" : $" -ac {ChannelCount}") +
                (SampleRate == null ? "" : $" -ar {SampleRate}")
        };
    }

    public enum Application
    {
        /// <summary>
        /// Favor improved speech intelligibility.
        /// </summary>
        VoIP,
        /// <summary>
        /// Favor faithfulness to the input
        /// </summary>
        Audio,
        /// <summary>
        /// Restrict to only the lowest delay modes. 
        /// </summary>
        LowDelay
    }
}
