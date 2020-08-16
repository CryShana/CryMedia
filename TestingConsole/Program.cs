using System;
using System.IO;
using System.Threading.Tasks;

using CryMediaAPI;
using CryMediaAPI.Audio;
using CryMediaAPI.Encoding.Builders;
using CryMediaAPI.Video;

namespace TestingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) return;

            string input = args[0];
            string output = args[1];

            ConvertVideo(input, output);

            // ReadWriteAudio(input, output);
            // ReadWriteVideo(input, output);

            // ReadPlayAudio(input, output);
            // ReadPlayVideo(input, output);

            // SaveVideoFrame(input);
        }

        static void ReadWriteAudio(string input, string output)
        {
            var audio = new AudioReader(input);
            audio.LoadMetadataAsync().Wait();
            audio.Load();

            using (var writer = new AudioWriter(output, audio.Metadata.Channels, audio.Metadata.SampleRate))
            {
                writer.OpenWrite(true);

                var frame = new AudioFrame(1, audio.Metadata.Channels);
                while (true)
                {
                    // read next sample
                    var f = audio.NextFrame(frame);
                    if (f == null) break;

                    writer.WriteFrame(frame);
                }
            }
        }

        static void ReadWriteVideo(string input, string output)
        {
            var video = new VideoReader(input);
            video.LoadMetadataAsync().Wait();
            video.Load();

            using (var writer = new VideoWriter(File.Create(output),
                video.Metadata.Width, video.Metadata.Height, video.Metadata.AvgFramerate,
                new H264Encoder() { Format = "flv" }.Create()))
            {
                writer.OpenWrite(true);
                //video.CopyTo(writer);

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

        static void ReadPlayVideo(string input, string output)
        {
            var video = new VideoReader(input);
            video.LoadMetadataAsync().Wait();
            video.Load();

            using (var player = new VideoPlayer())
            {
                player.OpenWrite(video.Metadata.Width, video.Metadata.Height, video.Metadata.AvgFramerateText);

                // For simple playing, can just use "CopyTo"
                // video.CopyTo(player);

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

                    try
                    {
                        player.WriteFrame(frame);
                    }
                    catch (IOException) { break; }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        static void ReadPlayAudio(string input, string output)
        {
            var audio = new AudioReader(input);
            audio.LoadMetadataAsync().Wait();
            audio.Load();

            using (var player = new AudioPlayer())
            {
                player.OpenWrite(audio.Metadata.SampleRate, audio.Metadata.Channels, showWindow: false);

                // For simple playing, can just use "CopyTo"
                // audio.CopyTo(player);

                var frame = new AudioFrame(audio.Metadata.Channels);
                while (true)
                {
                    // read next frame
                    var f = audio.NextFrame(frame);
                    if (f == null) break;

                    try
                    {
                        player.WriteFrame(frame);
                    }
                    catch (IOException) { break; }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        static void SaveVideoFrame(string input)
        {
            var video = new VideoReader(input);
            video.LoadMetadataAsync().Wait();
            video.Load(60.5);

            var fr = video.NextFrame();
            fr.Save("test.png");
        }

        static void ConvertVideo(string input, string output)
        {
            var encoder = new VP9Encoder();
            encoder.RowBasedMultithreading = true;
            encoder.SetCQP(31);

            using (var reader = new VideoReader(input))
            {
                reader.LoadMetadata();
                reader.Load();

                using (var writer = new VideoWriter(output,
                    reader.Metadata.Width,
                    reader.Metadata.Height,
                    reader.Metadata.AvgFramerate,
                    encoder.Create()))
                {
                    writer.OpenWrite(true);
                    reader.CopyTo(writer);
                }
            }
        }
    }
}
