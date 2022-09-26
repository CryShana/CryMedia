namespace CryMediaAPI.Encoding;

/// <summary>
/// Base class for EncoderOptions builders. Implement this for separate encoders.
/// </summary>
public abstract class EncoderOptionsBuilder
{
    /// <summary>
    /// FFmpeg format name (container format)
    /// </summary>
    public abstract string Format { get; set; }   

    /// <summary>
    /// FFmpeg encoder name
    /// </summary>
    public abstract string Name { get; } 

    /// <summary>
    /// Create an EncoderOptions object from FFmpeg based on the configuration
    /// </summary>
    public abstract EncoderOptions Create();
}
