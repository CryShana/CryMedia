using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using CryMediaAPI.BaseClasses;

namespace CryMediaAPI.Audio
{
    public class AudioWriter : MediaWriter<AudioFrame>, IDisposable
    {
        string ffmpeg;
        CancellationTokenSource csc;
        internal Process ffmpegp;

        public int Channels { get; }
        public int SampleRate { get; }
        public int BitDepth { get; }
        public bool UseFilename { get; }
        public FFmpegAudioEncoderOptions EncoderOptions { get; }

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
            FFmpegAudioEncoderOptions encoderOptions = null, string ffmpegExecutable = "ffmpeg")
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
            EncoderOptions = encoderOptions ?? new FFmpegAudioEncoderOptions();
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
            FFmpegAudioEncoderOptions encoderOptions = null, string ffmpegExecutable = "ffmpeg")
        {
            if (channels <= 0 || sampleRate <= 0) throw new InvalidDataException("Channels/Sample rate have to be bigger than 0!");
            if (bitDepth != 16 && bitDepth != 24 && bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");
            if (destinationStream == null) throw new NullReferenceException("Stream can't be null!");

            UseFilename = false;
            ffmpeg = ffmpegExecutable;

            Channels = channels;
            BitDepth = bitDepth;
            SampleRate = sampleRate;

            DestinationStream = destinationStream;
            EncoderOptions = encoderOptions ?? new FFmpegAudioEncoderOptions();
        }

        /// <summary>
        /// Opens output audio file for writing. This will delete any existing file. Call this before writing samples.
        /// </summary>
        /// <param name="showFFmpegOutput">Show FFmpeg encoding output for debugging purposes.</param>
        public void OpenWrite(bool showFFmpegOutput = false)
        {
            if (OpenedForWriting) throw new InvalidOperationException("File was already opened for writing!");
            if (File.Exists(Filename)) File.Delete(Filename);

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
                csc?.Cancel();

                try
                {
                    if (ffmpegp.HasExited) ffmpegp.Kill();
                }
                catch { }

                InputDataStream.Dispose();

                if (!UseFilename) OutputDataStream?.Dispose();
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

        /// <summary>
        /// Converts given input file to output file.
        /// </summary>
        /// <param name="inputFilename">Input audio file name/path</param>
        /// <param name="outputFilename">Input audio file name/path</param>
        /// <param name="options">Output options</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="inputArguments">Input arguments (such as-f, -channels, -sample_rate,...)</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public static void FileToFile(string inputFilename, string outputFilename, FFmpegAudioEncoderOptions options, out Process process,
            string inputArguments = "", bool showOutput = false, string ffmpegExecutable = "ffmpeg")
        {
            var output = FFmpegWrapper.ExecuteCommand(ffmpegExecutable, $"{inputArguments} -i \"{inputFilename}\" " +
                $"-c:a {options.EncoderName} {options.EncoderArguments} -f {options.Format} \"{outputFilename}\"", showOutput);

            process = output;
        }

        /// <summary>
        /// Opens output file for writing and returns the input stream.
        /// </summary>
        /// <param name="outputFilename">Output audio file name/path</param>
        /// <param name="options">Output options</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="inputArguments">Input arguments (such as -f, -channels, -sample_rate,...)</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public static Stream StreamToFile(string outputFilename, FFmpegAudioEncoderOptions options, out Process process,
            string inputArguments = "", bool showOutput = false, string ffmpegExecutable = "ffmpeg")
        {
            var input = FFmpegWrapper.OpenInput(ffmpegExecutable, $"{inputArguments} -i - " +
                $"-c:a {options.EncoderName} {options.EncoderArguments} -f {options.Format} \"{outputFilename}\"", out process, showOutput);

            return input;
        }

        /// <summary>
        /// Uses input file and returns the output stream.
        /// </summary>
        /// <param name="inputFilename">Input audio file name/path</param>
        /// <param name="options">Output options</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="inputArguments">Input arguments (such as -f, -channels, -sample_rate,...)</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public static Stream FileToStream(string inputFilename, FFmpegAudioEncoderOptions options, out Process process,
            string inputArguments = "", string ffmpegExecutable = "ffmpeg")
        {
            var output = FFmpegWrapper.OpenOutput(ffmpegExecutable, $"{inputArguments} -i \"{inputFilename}\" " +
                $"-c:a {options.EncoderName} {options.EncoderArguments} -f {options.Format} -", out process);

            return output;
        }

        /// <summary>
        /// Opens output stream for writing and returns both the input and output streams.
        /// </summary>
        /// <param name="options">Output options</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="inputArguments">Input arguments (such as -f, -channels, -sample_rate,...)</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public static (Stream Input, Stream Output) StreamToStream(FFmpegAudioEncoderOptions options, out Process process,
            string inputArguments = "", string ffmpegExecutable = "ffmpeg")
        {
            var (input, output) = FFmpegWrapper.Open(ffmpegExecutable, $"{inputArguments} -i - " +
                $"-c:a {options.EncoderName} {options.EncoderArguments} -f {options.Format} -", out process);

            return (input, output);
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
