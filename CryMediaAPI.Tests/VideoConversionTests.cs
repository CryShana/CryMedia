using CryMediaAPI.Resources;
using CryMediaAPI.Video;

using System;
using System.IO;
using System.Threading.Tasks;

using Xunit;

namespace CryMediaAPI.Tests
{
    public class VideoConversionTests
    {
        [Fact]
        public async Task FFmpegWrapperProgressTest()
        {
            var path = Res.GetPath(Res.Video_Mp4);
            var opath = "out-test.mp4";

            double lastval = -1;

            try
            {
                var video = new VideoReader(path);

                await video.LoadMetadataAsync();
                var dur = video.Metadata.Duration;
                video.Dispose();

                Assert.True(Math.Abs(dur - 5.533333) < 0.01);

                var p = FFmpegWrapper.ExecuteCommand("ffmpeg", $"-i \"{path}\" -c:v libx264 -f mp4 \"{opath}\"");
                var progress = FFmpegWrapper.RegisterProgressTracker(p, dur);
                progress.ProgressChanged += (s, prg) => lastval = prg;
                p.WaitForExit();

                await Task.Delay(300);

                Assert.True(lastval > 50 && lastval <= 100);

                video = new VideoReader(opath);

                await video.LoadMetadataAsync();

                Assert.True(video.Metadata.AvgFramerate == 30);
                Assert.True(video.Metadata.AvgFramerateText == "30/1");
                Assert.True(Math.Abs(video.Metadata.Duration - 5.533333) < 0.01);
                Assert.True(video.Metadata.Width == 560);
                Assert.True(video.Metadata.Height == 320);

                video.Dispose();
            }
            finally
            {
                if (File.Exists(opath)) File.Delete(opath);
            }
        }

        [Fact]
        public async Task ConversionTest()
        {
            var path = Res.GetPath(Res.Video_Mp4);
            var opath = "out-test-2.mp4";

            try
            {
                using var reader = new VideoReader(path);
                await reader.LoadMetadataAsync();

                using (var writer = new VideoWriter(opath,
                    reader.Metadata.Width,
                    reader.Metadata.Height,
                    reader.Metadata.AvgFramerate,
                    FFmpegVideoEncoderOptions.H264))
                {
                    writer.OpenWrite();

                    reader.Load();

                    await reader.CopyToAsync(writer);
                }

                await Task.Delay(200);

                using var video = new VideoReader(opath);
                await video.LoadMetadataAsync();

                Assert.True(video.Metadata.Codec == "h264");
                Assert.True(video.Metadata.AvgFramerate == reader.Metadata.AvgFramerate);
                Assert.True(Math.Abs(video.Metadata.Duration - reader.Metadata.Duration) < 0.01);
                Assert.True(video.Metadata.Width == reader.Metadata.Width);
                Assert.True(video.Metadata.Height == reader.Metadata.Height);
                Assert.True(video.Metadata.BitDepth == reader.Metadata.BitDepth);
                Assert.True(video.Metadata.Streams.Length == 1);  // only video
            }
            finally
            {
                if (File.Exists(opath)) File.Delete(opath);
            }
        }
    }
}
