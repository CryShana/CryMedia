using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CryMediaAPI.Audio
{
    public class AudioPlayer : IDisposable
    {
        string ffplay;
        Stream input;
        Process ffplayp;
        bool outOpened = false;

        public string Filename { get; }

        /// <summary>
        /// Used for playing audio data
        /// </summary>
        /// <param name="input">Input audio to play (can be left empty if planning on playing frames directly)</param>
        /// <param name="ffplayExecutable">Name or path to the ffplay executable</param> 
        public AudioPlayer(string input = null, string ffplayExecutable = "ffplay")
        {
            ffplay = ffplayExecutable;

            Filename = input;
        }

        /// <summary>
        /// Play audio
        /// </summary>
        public void Play(bool showWindow = false)
        {
            if (string.IsNullOrEmpty(Filename)) throw new InvalidOperationException("No filename was specified!");
            FFmpegWrapper.RunCommand(ffplay, $"-i \"{Filename}\"" + (showWindow ? "" : " -showmode 0"));
        }

        /// <summary>
        /// Play audio in background and return the process associated with it
        /// </summary>
        public Process PlayInBackground(bool showWindow = false)
        {
            if (string.IsNullOrEmpty(Filename)) throw new InvalidOperationException("No filename was specified!");
            FFmpegWrapper.OpenOutput(ffplay, $"-i \"{Filename}\"" + (showWindow ? "" : " -showmode 0"), out Process p);
            return p;
        }

        /// <summary>
        /// Open player for writing frames for playing.
        /// </summary>
        /// <param name="showFFplayOutput">Show FFplay output for debugging purposes.</param>
        public void OpenWrite(bool showFFplayOutput = false)
        {
            if (outOpened) throw new InvalidOperationException("Player is already opened for writing frames!");

            throw new NotImplementedException();

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
        /// <param name="frame">Audio frame to write to player</param>
        public void WriteFrame(AudioFrame frame)
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
