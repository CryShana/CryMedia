using System;
using System.IO;
using System.Threading.Tasks;
using CryMediaAPI.Video;

namespace CryVideoConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) return;

            string input = args[0];
            string output = args[1];

            var video = new VideoReader(input);
            video.LoadMetadata().Wait();
            video.Load();

            using (var writer = new VideoWriter(output, video.Metadata.Width, video.Metadata.Height, video.Metadata.AvgFramerate))
            {
                writer.OpenForWriting(false);

                var frame = new VideoFrame(video.Metadata.Width, video.Metadata.Height);
                while (true)
                {
                    // read next frame
                    var f = video.NextFrame(frame);
                    if (f == null) break;

                    
                    for (int i = 0; i < 100; i++)
                        for (int j = 0; j < 100; j++)
                        {
                            var px = frame.GetPixels(i, j).Span;
                            px[0] = 255;
                            px[1] = 0;
                            px[2] = 0;
                        }

                    writer.WriteFrame(frame);
                }
            }
        }
    }
}
