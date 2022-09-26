namespace CryMediaAPI.Encoding;

/// <summary>
/// FFmpeg video encoding options to pass to FFmpeg when encoding. Check the online FFmpeg documentation for more info.
/// </summary>
public class EncoderOptions
{
    /// <summary>
    /// Container format. (example: 'mp4', 'flv', 'webm', 'mp3', 'ogg')
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// Encoder name. (example: 'libx264', 'libx265', 'libvpx', 'libopus', 'libvorbis', 'h264_nvenc')
    /// </summary>
    public string EncoderName { get; set; }

    /// <summary>
    /// Arguments for the encoder. This depends on the used encoder.
    /// </summary>
    public string EncoderArguments { get; set; }
}
