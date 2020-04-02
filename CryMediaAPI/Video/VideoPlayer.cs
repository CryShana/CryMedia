using System;
using System.IO;
using System.Diagnostics;
using CryMediaAPI.BaseClasses;

namespace CryMediaAPI.Video
{
    public class VideoPlayer : MediaWriter<VideoFrame>, IDisposable
    {
        string ffplay;
        Process ffplayp;

        /// <summary>
        /// Used for playing video data
        /// </summary>
        /// <param name="input">Input video to play (can be left empty if planning on playing frames directly)</param>
        /// <param name="ffplayExecutable">Name or path to the ffplay executable</param> 
        public VideoPlayer(string input = null, string ffplayExecutable = "ffplay")
        {
            ffplay = ffplayExecutable;

            Filename = input;
        }

        /// <summary>
        /// Play video
        /// </summary>
        public void Play()
        {
            if (OpenedForWriting) throw new InvalidOperationException("Player is already opened for writing frames!");
            if (string.IsNullOrEmpty(Filename)) throw new InvalidOperationException("No filename was specified!");

            FFmpegWrapper.RunCommand(ffplay, $"-i \"{Filename}\"");
        }

        /// <summary>
        /// Play video in background and return the process associated with it
        /// </summary>
        /// <param name="runPureBackground">Detach the player from this VideoPlayer control. Player won't be killed on disposing.</param>
        public Process PlayInBackground(bool runPureBackground = false)
        {
            if (!runPureBackground && OpenedForWriting) throw new InvalidOperationException("Player is already opened for writing frames!");
            if (string.IsNullOrEmpty(Filename)) throw new InvalidOperationException("No filename was specified!");

            FFmpegWrapper.OpenOutput(ffplay, $"-i \"{Filename}\"", out Process p);
            if (!runPureBackground) ffplayp = p;
            return ffplayp;
        }

        /// <summary>
        /// Open player for writing frames for playing.
        /// </summary>
        /// <param name="width">Video frame width</param>
        /// <param name="height">Video frame height</param>
        /// <param name="framerateFrequency">Video framerate (frequency form)</param>
        /// <param name="showFFplayOutput">Show FFplay output for debugging purposes.</param>
        public void OpenWrite(int width, int height, string framerateFrequency, bool showFFplayOutput = false)
        {
            if (OpenedForWriting) throw new InvalidOperationException("Player is already opened for writing frames!");
            try
            {
                if (ffplayp != null && !ffplayp.HasExited) ffplayp.Kill();
            }
            catch { }

            InputDataStream = FFmpegWrapper.OpenInput(ffplay, $"-f rawvideo -video_size {width}:{height} -framerate {framerateFrequency} -pixel_format rgb24 -i -",
                out ffplayp, showFFplayOutput);

            OpenedForWriting = true;
        }

        /// <summary>
        /// Close player for writing frames.
        /// </summary>
        public void CloseWrite()
        {
            if (!OpenedForWriting) throw new InvalidOperationException("Player is not opened for writing frames!");

            try
            {
                try
                {
                    if (!ffplayp.HasExited) ffplayp.Kill();
                }
                catch { }

                InputDataStream.Dispose();
            }
            finally
            {
                OpenedForWriting = false;
            }
        }

        /// <summary>
        /// Get stream for writing and playing video in custom format.
        /// </summary>
        /// <param name="format">Custom video format</param>
        /// <param name="arguments">Custom FFmpeg arguments for the specified video format</param>
        /// <param name="showFFplayOutput">Show FFplay output for debugging purposes.</param>
        public static Stream GetStreamForWriting(string format, string arguments, out Process ffplayProcess,
            bool showFFplayOutput = false, string ffplayExecutable = "ffplay")
        {
            var str = FFmpegWrapper.OpenInput(ffplayExecutable, $"-f {format} {arguments} -i -",
                out ffplayProcess, showFFplayOutput);

            return str;
        }

        public void Dispose()
        {
            if (OpenedForWriting) CloseWrite();
            else
            {
                try
                {
                    if (ffplayp != null && !ffplayp.HasExited) ffplayp.Kill();
                }
                catch { }
            }
        }
    }
}
