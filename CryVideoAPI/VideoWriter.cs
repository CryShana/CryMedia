using System;
using System.IO;

namespace CryVideoAPI
{
    public class VideoWriter : IDisposable
    {
        string ffmpeg;

        Stream input;
        bool outOpened = false;

        public int Width { get; }
        public int Height { get; }
        public double Framerate { get; }
        public FFmpegEncoderOptions EncoderOptions { get; }

        public string Filename { get; }

        /// <summary>
        /// Used for encoding frames into new video files
        /// </summary>
        /// <param name="filename">Output video file name/path</param>
        /// <param name="width">Width of the input video in pixels</param>
        /// <param name="height">Height of the input video in pixels </param>
        /// <param name="framerate">Framerate of the input video in fps</param>
        /// <param name="encoderOptions">Extra FFmpeg encoding options that will be passed to FFmpeg</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public VideoWriter(string filename, int width, int height, double framerate, 
            FFmpegEncoderOptions encoderOptions, string ffmpegExecutable = "ffmpeg")
        {
            if (width <= 0 || height <= 0) throw new InvalidDataException("Video frame dimensions have to be bigger than 0 pixels!");
            if (framerate <= 0) throw new InvalidDataException("Video framerate has to be bigger than 0!");

            ffmpeg = ffmpegExecutable;

            Width = width;
            Height = height;
            Filename = filename;
            Framerate = framerate;
            EncoderOptions = encoderOptions;            
        }

        /// <summary>
        /// Opens output video file for writing. This will delete any existing file.
        /// </summary>
        /// <param name="showFFmpegOutput">Show FFmpeg encoding output for debugging purposes.</param>
        public void OpenForWriting(bool showFFmpegOutput = false)
        {
            if (outOpened) throw new InvalidOperationException("Filename was already opened for writing!");
            if (File.Exists(Filename)) File.Delete(Filename);

            input = FFmpegWrapper.OpenInput(ffmpeg, $"-f rawvideo -video_size {Width}:{Height} -r {Framerate} -pixel_format rgb24 -i - " +
                $"-c:v {EncoderOptions.EncoderName} {EncoderOptions.EncoderArguments} -f {EncoderOptions.Format} \"{Filename}\"", showFFmpegOutput);

            outOpened = true;
        }

        /// <summary>
        /// Encode the video frame
        /// </summary>
        /// <param name="frame">Video frame to encode</param>
        public void WriteFrame(VideoFrame frame)
        {
            if (!outOpened) throw new InvalidOperationException("File needs to be opened for writing first!");

            input.Write(frame.RawData.Span);
        }

        public void Dispose()
        {
            input.Dispose();
        }
    }

    /// <summary>
    /// FFmpeg encoding options to pass to FFmpeg when encoding. Check the online FFmpeg documentation for more info.
    /// </summary>
    public class FFmpegEncoderOptions
    {
        /// <summary>
        /// Container format. (example: 'mp4', 'flv', 'webm')
        /// </summary>
        public string Format { get; set; } = "mp4";

        /// <summary>
        /// Encoder name. (example: 'libx264', 'libx265', 'libvpx')
        /// </summary>
        public string EncoderName { get; set; } = "libx264";

        /// <summary>
        /// Arguments for the encoder. This depends on the used encoder.
        /// </summary>
        public string EncoderArguments { get; set; } = "-preset veryfast -crf 23 -c:a copy";
    }
}
