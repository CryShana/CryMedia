using System;
using System.IO;
using System.Diagnostics;
using CryMediaAPI.BaseClasses;

namespace CryMediaAPI.Video
{
    public class VideoWriter : MediaWriter<VideoFrame>, IDisposable
    {
        string ffmpeg;

        internal Process ffmpegp;
        
        public int Width { get; }
        public int Height { get; }
        public double Framerate { get; }
        public FFmpegVideoEncoderOptions EncoderOptions { get; }


        /// <summary>
        /// Used for encoding frames into a new video file
        /// </summary>
        /// <param name="filename">Output video file name/path</param>
        /// <param name="width">Input width of the video in pixels</param>
        /// <param name="height">Input height of the video in pixels </param>
        /// <param name="framerate">Input framerate of the video in fps</param>
        /// <param name="encoderOptions">Extra FFmpeg encoding options that will be passed to FFmpeg</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public VideoWriter(string filename, int width, int height, double framerate,
            FFmpegVideoEncoderOptions encoderOptions = null, string ffmpegExecutable = "ffmpeg")
        {
            if (width <= 0 || height <= 0) throw new InvalidDataException("Video frame dimensions have to be bigger than 0 pixels!");
            if (framerate <= 0) throw new InvalidDataException("Video framerate has to be bigger than 0!");

            ffmpeg = ffmpegExecutable;

            Width = width;
            Height = height;
            Filename = filename;
            Framerate = framerate;
            EncoderOptions = encoderOptions ?? new FFmpegVideoEncoderOptions();
        }

        /// <summary>
        /// Opens output video file for writing. This will delete any existing file. Call this before writing frames.
        /// </summary>
        /// <param name="showFFmpegOutput">Show FFmpeg encoding output for debugging purposes.</param>
        public void OpenWrite(bool showFFmpegOutput = false)
        {
            if (OpenedForWriting) throw new InvalidOperationException("File was already opened for writing!");
            if (File.Exists(Filename)) File.Delete(Filename);

            DataStream = FFmpegWrapper.OpenInput(ffmpeg, $"-f rawvideo -video_size {Width}:{Height} -r {Framerate} -pixel_format rgb24 -i - " +
                $"-c:v {EncoderOptions.EncoderName} {EncoderOptions.EncoderArguments} -f {EncoderOptions.Format} \"{Filename}\"",
                out ffmpegp, showFFmpegOutput);

            OpenedForWriting = true;
        }

        /// <summary>
        /// Closes output video file.
        /// </summary>
        public void CloseWrite()
        {
            if (!OpenedForWriting) throw new InvalidOperationException("File is not opened for writing!");

            try
            {
                try
                {
                    if (ffmpegp.HasExited) ffmpegp.Kill();
                }
                catch { }

                DataStream.Dispose();
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

    /// <summary>
    /// FFmpeg video encoding options to pass to FFmpeg when encoding. Check the online FFmpeg documentation for more info.
    /// </summary>
    public class FFmpegVideoEncoderOptions
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
