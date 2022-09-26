namespace CryMediaAPI.Encoding.Builders;

public class VP9Encoder : EncoderOptionsBuilder
{
    /// <summary>  
    /// Encoder quality setting (Default: good)
    /// </summary>
    public Quality EncoderQuality { get; set; } = Quality.Good;
    /// <summary>
    /// Tune encoder settings based on the content that is being encoded.
    /// </summary>
    public Tune EncoderTune { get; set; } = Tune.Default;
    /// <summary>
    /// Quality/Speed ratio modifier (from -8 to 8, also depends on quality setting)
    /// </summary>
    public int? CpuUsed { get; set; } = null;
    /// <summary>
    /// Enable row based multithreading
    /// </summary>
    public bool RowBasedMultithreading { get; set; } = false;


    public override string Format { get; set; } = "webm";

    public override string Name => "libvpx-vp9";

    public string CurrentQualitySettings { get; private set; }

    public VP9Encoder()
    {
        SetCQP();
    }

    /// <summary>
    /// Constrained quality encoding - Set target quality and maximum bitrate
    /// CRF is increased when [max_bitrate] is exceeded.
    /// </summary>
    /// <param name="crf">Number from 0 to 63 (Lower = higher quality)</param>
    public void SetCVBR(int crf, string max_bitrate)      
    {
        CurrentQualitySettings = $"-crf {crf} -b:v {max_bitrate}";
    }

    /// <summary>
    /// Constrained bitrate encoding - Set target, min and max bitrate.
    /// </summary>
    /// <param name="target_bitrate">Target bitrate (ex: '1M', '1000k', ...)</param>
    /// <param name="min_bitrate">Min bitrate (ex: '1M', '1000k', ...)</param>
    /// <param name="max_bitrate">Max bitrate (ex: '1M', '1000k', ...)</param>
    public void SetCVBR(string target_bitrate, string min_bitrate, string max_bitrate)
    {
        CurrentQualitySettings = $"-minrate {min_bitrate} -b:v {target_bitrate} -maxrate {max_bitrate}";
    }

    /// <summary>
    /// ABR encoding
    /// </summary>
    /// <param name="bitrate">Average target bitrate (ex: '1M', '1000k', ...)</param>
    public void SetABR(string bitrate)
    {
        CurrentQualitySettings = $"-b:v {bitrate}";
    }

    /// <summary>
    /// Constant quality encoding
    /// </summary>
    /// <param name="crf">Number from 0 to 63 (Lower = higher quality)</param>
    public void SetCQP(int crf = 31)
    {
        CurrentQualitySettings = $"-crf {crf} -b:v 0";
    }

    /// <summary>
    /// Constant bitrate encoding
    /// </summary>
    /// <param name="bitrate">Average target bitrate (ex: '1M', '1000k', ...)</param>
    public void SetCBR(string bitrate)
    {
        CurrentQualitySettings = $"-minrate {bitrate} -maxrate {bitrate} -b:v {bitrate}";
    }

    public void SetLossless()
    {
        CurrentQualitySettings = $"-lossless 1";
    }


    public override EncoderOptions Create()
    {
        return new EncoderOptions
        {
            Format = Format,
            EncoderName = Name,
            EncoderArguments = $"{CurrentQualitySettings} " +
                $"-tune-content {EncoderTune.ToString().ToLowerInvariant()} " +
                $"-deadline {EncoderQuality.ToString().ToLowerInvariant()}" +
                (CpuUsed == null ? "" : $" -cpu-used {CpuUsed.Value}") +
                (RowBasedMultithreading ? " -row-mt 1" : "")         
        };
    }     

    public enum Tune
    {
        Default,
        /// <summary>
        /// Screen capture content 
        /// </summary>
        Screen,
        /// <summary>
        /// Film content; improves grain retention
        /// </summary>
        Film
    }

    public enum Quality
    {
        /// <summary>
        /// Default and recommended for most applications
        /// </summary>
        Good,
        /// <summary>
        /// Recommended if you have lots of time and want the best compression efficiency.
        /// </summary>
        Best,
        /// <summary>
        /// Recommended for live/fast encoding. 
        /// </summary>
        RealTime
    }
}
