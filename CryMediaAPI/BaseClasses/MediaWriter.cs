using CryMediaAPI.Encoding;

using System;
using System.Diagnostics;
using System.IO;

namespace CryMediaAPI.BaseClasses
{
    public abstract class MediaWriter<Frame> where Frame : IMediaFrame
    {
        /// <summary>
        /// Output filename
        /// </summary>
        public virtual string Filename { get; protected set; }

        /// <summary>
        /// Input data stream
        /// </summary>
        public virtual Stream InputDataStream { get; protected set; }

        /// <summary>
        /// Is data stream opened for writing
        /// </summary>
        public virtual bool OpenedForWriting { get; protected set; }

        /// <summary>
        /// Writes frame to output. Make sure to call OpenWrite() before calling this.
        /// </summary>
        /// <param name="frame">Frame containing media data</param>
        public virtual void WriteFrame(Frame frame)
        {
            if (!OpenedForWriting) throw new InvalidOperationException("Media needs to be prepared for writing first!");

            InputDataStream.Write(frame.RawData.Span);
        }

        /// <summary>
        /// Converts given input file to output file.
        /// </summary>
        /// <param name="inputFilename">Input video file name/path</param>
        /// <param name="outputFilename">Input video file name/path</param>
        /// <param name="options">Output options</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="inputArguments">Input arguments (such as -f, -v:c, -video_size, -ac, -ar...)</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public static void FileToFile(string inputFilename, string outputFilename, EncoderOptions options, out Process process,
            string inputArguments = "", bool showOutput = false, string ffmpegExecutable = "ffmpeg")
        {
            var output = FFmpegWrapper.ExecuteCommand(ffmpegExecutable, $"{inputArguments} -i \"{inputFilename}\" " +
                $"-c:v {options.EncoderName} {options.EncoderArguments} -f {options.Format} \"{outputFilename}\"", showOutput);

            process = output;
        }

        /// <summary>
        /// Opens output file for writing and returns the input stream.
        /// </summary>
        /// <param name="outputFilename">Output video file name/path</param>
        /// <param name="options">Output options</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="inputArguments">Input arguments (such as -f, -v:c, -video_size, -ac, -ar...)</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public static Stream StreamToFile(string outputFilename, EncoderOptions options, out Process process,
            string inputArguments = "", bool showOutput = false, string ffmpegExecutable = "ffmpeg")
        {
            var input = FFmpegWrapper.OpenInput(ffmpegExecutable, $"{inputArguments} -i - " +
                $"-c:v {options.EncoderName} {options.EncoderArguments} -f {options.Format} \"{outputFilename}\"", out process, showOutput);

            return input;
        }

        /// <summary>
        /// Uses input file and returns the output stream. Make sure to use a streaming format (like flv).
        /// </summary>
        /// <param name="inputFilename">Input video file name/path</param>
        /// <param name="options">Output options</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="inputArguments">Input arguments (such as -f, -v:c, -video_size, -ac, -ar...)</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public static Stream FileToStream(string inputFilename, EncoderOptions options, out Process process,
            string inputArguments = "", bool showOutput = false, string ffmpegExecutable = "ffmpeg")
        {
            var output = FFmpegWrapper.OpenOutput(ffmpegExecutable, $"{inputArguments} -i \"{inputFilename}\" " +
                $"-c:v {options.EncoderName} {options.EncoderArguments} -f {options.Format} -", out process, showOutput);

            return output;
        }

        /// <summary>
        /// Opens output stream for writing and returns both the input and output streams. Make sure to use a streaming format (like flv).
        /// </summary>
        /// <param name="options">Output options</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="inputArguments">Input arguments (such as -f, -v:c, -video_size, -ac, -ar...)</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public static (Stream Input, Stream Output) StreamToStream(EncoderOptions options, out Process process,
            string inputArguments = "", bool showOutput = false, string ffmpegExecutable = "ffmpeg")
        {
            var (input, output) = FFmpegWrapper.Open(ffmpegExecutable, $"{inputArguments} -i - " +
                $"-c:v {options.EncoderName} {options.EncoderArguments} -f {options.Format} -", out process, showOutput);

            return (input, output);
        }
    }
}
