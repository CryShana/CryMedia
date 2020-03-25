using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace CryMediaAPI.Video
{
    public class VideoPlayer
    {
        string ffplay;
        public string Filename { get; }

        public VideoPlayer(string input, string ffplayExecutable = "ffplay")
        {
            ffplay = ffplayExecutable;

            Filename = input;
        }

        /// <summary>
        /// Play video
        /// </summary>
        public void Play()
        {
            FFmpegWrapper.RunCommand(ffplay, $"-i \"{Filename}\"");
        }

        /// <summary>
        /// Play video in background and return the process associated with it
        /// </summary>
        public Process PlayInBackground()
        {
            FFmpegWrapper.OpenOutput(ffplay, $"-i \"{Filename}\"", out Process p);
            return p;
        }
    }
}
