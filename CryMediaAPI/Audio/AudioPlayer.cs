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
            if (outOpened) throw new InvalidOperationException("Player is already opened for writing frames!");
            if (string.IsNullOrEmpty(Filename)) throw new InvalidOperationException("No filename was specified!");

            FFmpegWrapper.RunCommand(ffplay, $"-i \"{Filename}\"" + (showWindow ? "" : " -showmode 0"));
        }

        /// <summary>
        /// Play audio in background and return the process associated with it
        /// </summary>
        /// <param name="showWindow">Show player window</param>
        /// <param name="runPureBackground">Detach the player from this AudioPlayer control. Player won't be killed on disposing.</param>
        public Process PlayInBackground(bool showWindow = false, bool runPureBackground = false)
        {
            if (!runPureBackground && outOpened) throw new InvalidOperationException("Player is already opened for writing frames!");
            if (string.IsNullOrEmpty(Filename)) throw new InvalidOperationException("No filename was specified!");

            FFmpegWrapper.OpenOutput(ffplay, $"-i \"{Filename}\"" + (showWindow ? "" : " -showmode 0"), out Process p);
            if (!runPureBackground) ffplayp = p;
            return ffplayp;
        }

        /// <summary>
        /// Open player for writing frames for playing.
        /// </summary>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="channels">Number of channels</param>
        /// <param name="bitDepth">Bits per sample (16, 24, 32)</param>
        /// <param name="showFFplayOutput">Show FFplay output for debugging purposes.</param>
        public void OpenWrite(int sampleRate, int channels, int bitDepth, bool showFFplayOutput = false)
        {
            if (bitDepth != 16 && bitDepth != 24 && bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");
            if (outOpened) throw new InvalidOperationException("Player is already opened for writing frames!");
            try
            {
                if (ffplayp != null && !ffplayp.HasExited) ffplayp.Kill();
            }
            catch { }

            input = FFmpegWrapper.OpenInput(ffplay, $"-f s{bitDepth}le -channels {channels} -sample_rate {sampleRate} -i -" 
                + (showFFplayOutput ? "" : " -showmode 0"), 
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
        /// Write audio sample to FFplay to play it.
        /// </summary>
        /// <param name="frame">Audio frame to write to player</param>
        public void WriteSample(AudioSample frame)
        {
            if (!outOpened) throw new InvalidOperationException("Player not opened for writing frames!");

            input.Write(frame.RawData.Span);
        }

        public void Dispose()
        {
            if (outOpened) CloseWrite();
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
