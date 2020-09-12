using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using CryMediaAPI.BaseClasses;
using CryMediaAPI.Encoding;
using CryMediaAPI.Encoding.Builders;

namespace CryMediaAPI.Audio
{
    public class AudioWriter : MediaWriter<AudioFrame>, IDisposable
    {
        string ffmpeg;
        CancellationTokenSource csc;
        internal Process ffmpegp;

        public Process CurrentFFmpegProcess => ffmpegp;

        public int Channels { get; }
        public int SampleRate { get; }
        public int BitDepth { get; }
        public bool UseFilename { get; }
        public EncoderOptions EncoderOptions { get; }

        public Stream DestinationStream { get; private set; }
        public Stream OutputDataStream { get; private set; }


        /// <summary>
        /// Used for encoding audio samples into a new audio file
        /// </summary>
        /// <param name="filename">Output audio file name/path</param>
        /// <param name="channels">Input number of channels</param>
        /// <param name="sampleRate">Input sample rate</param>
        /// <param name="bitDepth">Input bits per sample</param>
        /// <param name="encoderOptions">Extra FFmpeg encoding options that will be passed to FFmpeg</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public AudioWriter(string filename, int channels, int sampleRate, int bitDepth = 16,
            EncoderOptions encoderOptions = null, string ffmpegExecutable = "ffmpeg")
        {
            if (channels <= 0 || sampleRate <= 0) throw new InvalidDataException("Channels/Sample rate have to be bigger than 0!");
            if (bitDepth != 16 && bitDepth != 24 && bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");
            if (string.IsNullOrEmpty(filename)) throw new NullReferenceException("Filename can't be null or empty!");

            UseFilename = true;
            ffmpeg = ffmpegExecutable;

            Channels = channels;
            BitDepth = bitDepth;
            SampleRate = sampleRate;

            Filename = filename;
            EncoderOptions = encoderOptions ?? new MP3Encoder().Create();
        }

        /// <summary>
        /// Used for encoding audio samples into a stream
        /// </summary>
        /// <param name="destinationStream">Output stream</param>
        /// <param name="channels">Input number of channels</param>
        /// <param name="sampleRate">Input sample rate</param>
        /// <param name="bitDepth">Input bits per sample</param>
        /// <param name="encoderOptions">Extra FFmpeg encoding options that will be passed to FFmpeg</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public AudioWriter(Stream destinationStream, int channels, int sampleRate, int bitDepth = 16,
            EncoderOptions encoderOptions = null, string ffmpegExecutable = "ffmpeg")
        {
            if (channels <= 0 || sampleRate <= 0) throw new InvalidDataException("Channels/Sample rate have to be bigger than 0!");
            if (bitDepth != 16 && bitDepth != 24 && bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");
            UseFilename = false;
            ffmpeg = ffmpegExecutable;

            Channels = channels;
            BitDepth = bitDepth;
            SampleRate = sampleRate;

            DestinationStream = destinationStream ?? throw new NullReferenceException("Stream can't be null!");
            EncoderOptions = encoderOptions ?? new MP3Encoder().Create();
        }

        /// <summary>
        /// Opens output audio file for writing. This will delete any existing file. Call this before writing samples.
        /// </summary>
        /// <param name="showFFmpegOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        public void OpenWrite(bool showFFmpegOutput = false)
        {
            if (OpenedForWriting) throw new InvalidOperationException("File was already opened for writing!");

            var cmd = $"-f s{BitDepth}le -channels {Channels} -sample_rate {SampleRate} -i - " +
                $"-c:a {EncoderOptions.EncoderName} {EncoderOptions.EncoderArguments} -f {EncoderOptions.Format}";

            if (UseFilename)
            {
                if (File.Exists(Filename)) File.Delete(Filename);

                InputDataStream = FFmpegWrapper.OpenInput(ffmpeg, $"{cmd} \"{Filename}\"", out ffmpegp, showFFmpegOutput);
            }
            else
            {
                csc = new CancellationTokenSource();

                // using stream
                (InputDataStream, OutputDataStream) = FFmpegWrapper.Open(ffmpeg, $"{cmd} -", out ffmpegp, showFFmpegOutput);
                _ = OutputDataStream.CopyToAsync(DestinationStream, csc.Token);
            }

            OpenedForWriting = true;
        }

        /// <summary>
        /// Closes output audio file.
        /// </summary>
        public void CloseWrite()
        {
            if (!OpenedForWriting) throw new InvalidOperationException("File is not opened for writing!");

            try
            {
                InputDataStream.Dispose();
                ffmpegp.WaitForExit();
                csc?.Cancel();

                if (!UseFilename) OutputDataStream?.Dispose();

                try
                {
                    if (ffmpegp?.HasExited == false) ffmpegp.Kill();
                }
                catch { }
            }
            finally
            {
                OpenedForWriting = false;
            }
        }

        public void Dispose()
        {
            if (OpenedForWriting) CloseWrite();
        }
    }
}
