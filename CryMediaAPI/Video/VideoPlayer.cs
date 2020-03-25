using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CryMediaAPI.Video
{
    public class VideoPlayer : IDisposable
    {
        string ffplay;
        Stream input;
        Process ffplayp;
        bool outOpened = false;

        public string Filename { get; }

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
            if (string.IsNullOrEmpty(Filename)) throw new InvalidOperationException("No filename was specified!");
            FFmpegWrapper.RunCommand(ffplay, $"-i \"{Filename}\"");
        }

        /// <summary>
        /// Play video in background and return the process associated with it
        /// </summary>
        public Process PlayInBackground()
        {
            if (string.IsNullOrEmpty(Filename)) throw new InvalidOperationException("No filename was specified!");
            FFmpegWrapper.OpenOutput(ffplay, $"-i \"{Filename}\"", out Process p);
            return p;
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
            if (outOpened) throw new InvalidOperationException("Player is already opened for writing frames!");

            input = FFmpegWrapper.OpenInput(ffplay, $"-f rawvideo -video_size {width}:{height} -framerate {framerateFrequency} -pixel_format rgb24 -i -",
                out ffplayp, showFFplayOutput);

            outOpened = true;
        }

        /// <summary>
        /// Close player for writing frames.
        /// </summary>
        public void CloseWrite()
        {
            if (!outOpened) throw new InvalidOperationException("Player is not opened for writing frames!");

            try
            {
                try
                {
                    if (!ffplayp.HasExited) ffplayp.Kill();
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
        /// Write frame to FFplay to play it.
        /// </summary>
        /// <param name="frame">Video frame to write to player</param>
        public void WriteFrame(VideoFrame frame)
        {
            if (!outOpened) throw new InvalidOperationException("Player not opened for writing frames!");

            input.Write(frame.RawData.Span);
        }

        public void Dispose()
        {
            if (outOpened) CloseWrite();
        }
    }
}
