using System.Globalization;

namespace CryMediaAPI.Encoding.Builders
{
    public class VorbisEncoder : EncoderOptionsBuilder
    {
        public override string Format { get; set; } = "ogg";
        public override string Name => "libvorbis";
        public string CurrentQualitySettings { get; private set; }

        public VorbisEncoder()
        {
            SetCQP();
        }

        /// <summary>
        /// Constant bitrate encoding.
        /// </summary>
        /// <param name="bitrate">Target bitrate (ex: '320k', '250k', ...)</param>
        public void SetCBR(string bitrate)
        {
            CurrentQualitySettings = $"-b:a {bitrate}";
        }

        /// <summary>
        /// Constant quality encoding - VBR mode
        /// </summary>
        /// <param name="q">Float number from -1 to 10 (Higher = higher quality)</param>
        public void SetCQP(float q = 3)
        {
            CurrentQualitySettings = $"-q:a {q.ToString("0.00", CultureInfo.InvariantCulture)}";
        }

        public override EncoderOptions Create()
        {
            return new EncoderOptions
            {
                Format = Format,
                EncoderName = Name,
                EncoderArguments = $"{CurrentQualitySettings}"
            };
        }
    }
}
