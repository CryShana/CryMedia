using Xunit;
using System;
using CryMediaAPI.Video;
using CryMediaAPI.Resources;
using System.Threading.Tasks;

namespace CryMediaAPI.Tests
{
    public class VideoMetadataTests
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
    }
}
