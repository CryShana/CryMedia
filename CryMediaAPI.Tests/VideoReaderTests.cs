using Xunit;
using System;
using CryMediaAPI.Video;
using CryMediaAPI.Resources;
using System.Threading.Tasks;

namespace CryMediaAPI.Tests
{
    public class VideoReaderTests
    {
        [Fact]
        public async Task LoadMetadataMp4()  
        {
            var video = new VideoReader(Res.GetPath(Res.Video_Mp4));

            await video.LoadMetadataAsync();

            Assert.True(video.Metadata.Codec == "h264");
            Assert.True(video.Metadata.AvgFramerate == 30);
            Assert.True(video.Metadata.AvgFramerateText == "30/1");
            Assert.True(Math.Abs(video.Metadata.Duration - 5.533333) < 0.01);
            Assert.True(video.Metadata.Width == 560);
            Assert.True(video.Metadata.Height == 320);
            Assert.True(video.Metadata.BitDepth == 8);
            Assert.True(video.Metadata.BitRate == 465641);
            Assert.True(video.Metadata.PixelFormat == "yuv420p");
            Assert.True(video.Metadata.Streams.Length == 2);
        }

        [Fact]
        public async Task LoadMetadataWebm()
        {
            var video = new VideoReader(Res.GetPath(Res.Video_Webm));

            await video.LoadMetadataAsync();

            Assert.True(video.Metadata.Codec == "vp8");
            Assert.True(video.Metadata.AvgFramerate == 30);
            Assert.True(video.Metadata.AvgFramerateText == "30/1");
            Assert.True(Math.Abs(video.Metadata.Duration - 5.568) < 0.01); 
            Assert.True(video.Metadata.Width == 560);
            Assert.True(video.Metadata.Height == 320);
            Assert.True(video.Metadata.PixelFormat == "yuv420p");
            Assert.True(video.Metadata.Streams.Length == 2);  
        }

        [Fact]
        public async Task LoadMetadataFlv()
        {
            var video = new VideoReader(Res.GetPath(Res.Video_Flv));

            await video.LoadMetadataAsync();

            Assert.True(video.Metadata.Codec == "flv1");
            Assert.True(video.Metadata.AvgFramerate == 25);
            Assert.True(video.Metadata.AvgFramerateText == "25/1");
            Assert.True(Math.Abs(video.Metadata.Duration - 5.56) < 0.01);
            Assert.True(video.Metadata.Width == 320); 
            Assert.True(video.Metadata.Height == 240); 
            Assert.True(video.Metadata.BitRate == 800000);
            Assert.True(video.Metadata.PixelFormat == "yuv420p");
            Assert.True(video.Metadata.Streams.Length == 2); 
        }

        [Fact]
        public async Task LoadAtOffset1()
        {
            using var video = new VideoReader(Res.GetPath(Res.Video_Flv));
            var second = 3;

            await video.LoadMetadataAsync();

            var at_frame = (second * video.Metadata.PredictedFrameCount) / video.Metadata.Duration;
            var frames_left = (int)Math.Round(video.Metadata.PredictedFrameCount - at_frame);

            video.Load(second);

            int count = 1;
            var frame = video.NextFrame();
            while (true)
            {
                frame = video.NextFrame(frame);
                if (frame == null) break;
                count++;
            }

            Assert.True(frames_left == count);
        }

        [Fact]
        public async Task LoadAtOffset2()
        {
            using var video = new VideoReader(Res.GetPath(Res.Video_Mp4));
            var second = 4;

            await video.LoadMetadataAsync();

            var at_frame = (second * video.Metadata.PredictedFrameCount) / video.Metadata.Duration;
            var frames_left = (int)Math.Round(video.Metadata.PredictedFrameCount - at_frame);

            video.Load(second);

            int count = 1;
            var frame = video.NextFrame();
            while (true)
            {
                frame = video.NextFrame(frame);
                if (frame == null) break;
                count++;
            }

            Assert.True(frames_left == count);
        }
    }
}
