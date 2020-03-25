using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

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
    }
}
