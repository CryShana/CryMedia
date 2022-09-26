using System.Globalization;

namespace CryMediaAPI.Encoding.Builders;

public class H264Encoder : EncoderOptionsBuilder
{
    /// <summary>
    /// A slower preset will provide better compression (Default: medium)
    /// </summary>
    public Preset EncoderPreset { get; set; } = Preset.Medium;
    /// <summary>
    /// Tune encoder settings based on the content that is being encoded. (Default: Auto)
    /// </summary>
    public Tune EncoderTune { get; set; } = Tune.Auto;
    /// <summary>
    /// Limit encoder output to a specific profile. This affects compatibility with older players and compression efficiency. (Default: Auto)
    /// </summary>
    public Profile EncoderProfile { get; set; } = Profile.Auto;

    public override string Format { get; set; } = "mp4";

    public override string Name => "libx264";

    public string CurrentQualitySettings { get; private set; }

    public H264Encoder()
    {
        SetCQP();
    }

    /// <summary>
    /// Constant quality (CQP/CRF) - Quality-based VBR encoding (Good of achieving best quality, bad for achieving certain bitrate/size)
    /// </summary>
    /// <param name="crf">Float number from 0 to 51 (0=lossless)</param>
    public void SetCQP(float crf = 22)   
    {
        CurrentQualitySettings = "-crf " + crf.ToString("0.00", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// CBR encoding - Set constant bitrate (Good for streaming, inefficient use of bandwidth)
    /// </summary>
    /// <param name="bitrate">Target bitrate (ex: '1M', '1000k', ...)</param>
    /// <param name="bufsize">Decoder buffer size, which determines the variability of the output bitrate. Used as a rate control buffer that will enforce the requested average bitrate across [bufsize] worth of video. Fluctuation within this range is acceptable. This is expected client buffer size. (ex: '1M', '1000k', ...)</param>
    public void SetCBR(string bitrate, string bufsize)
    {
        CurrentQualitySettings = $"-x264-params \"nal-hrd=cbr\" -b:v {bitrate} -minrate {bitrate} -maxrate {bitrate} -bufsize {bufsize}";
    }

    /// <summary>
    /// Constrained quality encoding with a verifier - Set maximum bitrate (Good for streaming where certain frames need less bitrate)
    /// CRF is increased when [max_bitrate] is exceeded.
    /// </summary>
    /// <param name="crf">Float number from 0 to 51 (0=lossless)</param>
    /// <param name="max_bitrate">Max. allowed bitrate (ex: '1M', '1000k', ...)</param>
    /// <param name="bufsize">Decoder buffer size, which determines the variability of the output bitrate. This is expected client buffer size. (ex: '1M', '1000k', ... Should be more than the bitrate)</param>
    /// <param name="crf_max">Prevents lowering CRF beyond this point (-1 = auto)</param>
    public void SetVBV(float crf, string max_bitrate, string bufsize, float crf_max = -1)
    {
        CurrentQualitySettings = $"-crf {crf.ToString("0.00", CultureInfo.InvariantCulture)} -maxrate {max_bitrate} -bufsize {bufsize} -crf_max {crf_max}";
    }

    /// <summary>
    /// Average bitrate encoding (Not recommended as it includes a lot of guessing ahead in time)
    /// </summary>
    /// <param name="avg_bitrate">Average target bitrate (ex: '1M', '1000k', ...)</param>
    public void SetABR(string avg_bitrate)
    {
        CurrentQualitySettings = $"-b:v {avg_bitrate}";
    }

    public override EncoderOptions Create()
    {
        return new EncoderOptions
        {
            Format = Format,
            EncoderName = Name,
            EncoderArguments = $"{CurrentQualitySettings} " +
                $"-preset {EncoderPreset.ToString().ToLowerInvariant()}" +
                (EncoderTune == Tune.Auto ? "" : $" -tune {EncoderTune.ToString().ToLowerInvariant()}") +
                (EncoderProfile == Profile.Auto ? "" : $" -profile:v {EncoderProfile.ToString().ToLowerInvariant()}")          
        };
    }     

    public enum Preset
    {
        UltraFast,
        SuperFast,
        VeryFast,
        Faster,
        Fast,
        Medium,
        Slow,
        Slower,
        VerySlow
    }

    public enum Tune
    {
        Auto,
        /// <summary>
        ///  Use for high quality movie content; lowers deblocking 
        /// </summary>
        Film,
        /// <summary>
        /// Good for cartoons; uses higher deblocking and more reference frames 
        /// </summary>
        Animation,
        /// <summary>
        /// Preserves the grain structure in old, grainy film material 
        /// </summary>
        Grain,
        /// <summary>
        /// Good for slideshow-like content 
        /// </summary>
        StillImage,
        /// <summary>
        /// Allows faster decoding by disabling certain filters 
        /// </summary>
        FastDecode,
        /// <summary>
        /// Good for fast encoding and low-latency streaming 
        /// </summary>
        ZeroLatency
    }

    public enum Profile
    {
        /// <summary>
        /// Automatically pick the appropriate profile
        /// </summary>
        Auto,
        /// <summary>
        /// Maximum compatibility on older devices. Least demanding.
        /// </summary>
        Baseline,
        /// <summary>
        /// Good compatibility even on older devices
        /// </summary>
        Main,
        /// <summary>
        /// Supported by most modern devices
        /// </summary>
        High,
        /// <summary>
        /// Support for 10-bit depth
        /// </summary>
        High10,
        /// <summary>
        /// Support for 4:2:2 chroma subsampling
        /// </summary>
        High442,
        /// <summary>
        /// Support for 4:4:4 chroma subsampling
        /// </summary>
        High444
    }
}
