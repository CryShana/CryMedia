using System;
using System.IO;
using System.Diagnostics;

namespace CryMediaAPI.Audio
{
    public class AudioWriter : IDisposable
    {
        string ffmpeg;

        Stream input;
        Process ffmpegp;
        bool outOpened = false;

        public int Channels { get; }
        public int SampleRate { get; }
        public int BitDepth { get; }
        public FFmpegAudioEncoderOptions EncoderOptions { get; }

        public string Filename { get; }

        /// <summary>
        /// Used for encoding samples into a new audio file
        /// </summary>
        /// <param name="filename">Output audio file name/path</param>
        /// <param name="channels">Input number of channels</param>
        /// <param name="sampleRate">Input sample rate</param>
        /// <param name="bitDepth">Input bits per sample</param>
        /// <param name="encoderOptions">Extra FFmpeg encoding options that will be passed to FFmpeg</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public AudioWriter(string filename, int channels, int sampleRate, int bitDepth = 16,
            FFmpegAudioEncoderOptions encoderOptions = null, string ffmpegExecutable = "ffmpeg")
        {
            if (channels <= 0 || sampleRate <= 0) throw new InvalidDataException("Channels/Sample rate have to be bigger than 0!");
            if (bitDepth != 16 && bitDepth != 24 && bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");

            ffmpeg = ffmpegExecutable;

            Channels = channels;
            BitDepth = bitDepth;
            SampleRate = sampleRate;

            Filename = filename;
            EncoderOptions = encoderOptions ?? new FFmpegAudioEncoderOptions();
        }

        /// <summary>
        /// Opens output audio file for writing. This will delete any existing file. Call this before writing samples.
        /// </summary>
        /// <param name="showFFmpegOutput">Show FFmpeg encoding output for debugging purposes.</param>
        public void OpenWrite(bool showFFmpegOutput = false)
        {
            if (outOpened) throw new InvalidOperationException("File was already opened for writing!");
            if (File.Exists(Filename)) File.Delete(Filename);

            input = FFmpegWrapper.OpenInput(ffmpeg, $"-f s{BitDepth}le -channels {Channels} -sample_rate {SampleRate} -i - " +
                $"-c:v {EncoderOptions.EncoderName} {EncoderOptions.EncoderArguments} -f {EncoderOptions.Format} \"{Filename}\"",
                out ffmpegp, showFFmpegOutput);

            outOpened = true;
        }

        /// <summary>
        /// Closes output audio file.
        /// </summary>
        public void CloseWrite()
        {
            if (!outOpened) throw new InvalidOperationException("File is not opened for writing!");

            try
            {
                try
                {
                    if (ffmpegp.HasExited) ffmpegp.Kill();
                }
                catch { }

                input.Dispose();
            }
            finally
            {
                outOpened = false;
            }
        }

        /// <summary>
        /// Encode the audio sample.
        /// </summary>
        /// <param name="sample">Audio sample to encode</param>
        public void WriteSample(AudioSample sample)
        {
            if (!outOpened) throw new InvalidOperationException("File needs to be opened for writing first!");

            input.Write(sample.RawData.Span);
        }

        public void Dispose()
        {
            if (outOpened) CloseWrite();
        }
    }

    /// <summary>
    /// FFmpeg audio encoding options to pass to FFmpeg when encoding. Check the online FFmpeg documentation for more info.
    /// </summary>
    public class FFmpegAudioEncoderOptions  
    {
        /// <summary>
        /// Container format. (example: 'mp3', 'wav', 'flac')
        /// </summary>
        public string Format { get; set; } = "mp3";

        /// <summary>
        /// Encoder name. (example: 'libmp3lame', 'wavpack', 'flac')
        /// </summary>
        public string EncoderName { get; set; } = "libmp3lame";

        /// <summary>
        /// Arguments for the encoder. This depends on the used encoder.
        /// </summary>
        public string EncoderArguments { get; set; } = "-ar 44100 -ac 2 -b:a 192k";
    }
}
