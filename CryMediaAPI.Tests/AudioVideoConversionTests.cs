using CryMediaAPI.Audio;
using CryMediaAPI.Encoding.Builders;
using CryMediaAPI.Resources;
using CryMediaAPI.Video;

using System;
using System.IO;
using System.Threading.Tasks;

using Xunit;

namespace CryMediaAPI.Tests
{
    public class AudioVideoConversionTests
    {
        [Fact]
        public async Task ConversionTest()
        {
            var vpath = Res.GetPath(Res.Video_Mp4);
            var apath = Res.GetPath(Res.Audio_Mp3);
            var opath = "out-test-av-1.mp4";

            try
            {
                using var vreader = new VideoReader(vpath);
                await vreader.LoadMetadataAsync();
                vreader.Load();

                using var areader = new AudioReader(apath);
                await areader.LoadMetadataAsync();
                areader.Load();

                // Get video and audio stream metadata
                var vstream = vreader.Metadata.GetFirstVideoStream();
                var astream = areader.Metadata.GetFirstAudioStream();

                // Prepare writer (Converting to H.264 + AAC video)
                using (var writer = new AudioVideoWriter(opath,
                    vstream.Width.Value,
                    vstream.Height.Value,
                    vstream.AvgFrameRateNumber,
                    astream.Channels.Value,
                    astream.SampleRateNumber, 16,
                    new H264Encoder().Create(),
                    new AACEncoder().Create()))
                {

                    // Open for writing (this starts the FFmpeg process)
                    writer.OpenWrite();

                    // Copy raw data directly from stream to stream
                    var t2 = areader.DataStream.CopyToAsync(writer.InputDataStreamAudio);
                    var t1 = vreader.DataStream.CopyToAsync(writer.InputDataStreamVideo);

                    await t1;
                    await t2;
                }

                using var video = new VideoReader(opath);
                await video.LoadMetadataAsync();

                Assert.True(video.Metadata.Streams.Length == 2);

                vstream = video.Metadata.GetFirstVideoStream();
                astream = video.Metadata.GetFirstAudioStream();

                Assert.True(Math.Abs(vstream.AvgFrameRateNumber - vreader.Metadata.AvgFramerate) < 0.1);
                Assert.True(Math.Abs(double.Parse(vstream.Duration) - vreader.Metadata.Duration) < 0.1);
                Assert.True(vstream.Width.Value == vreader.Metadata.Width);
                Assert.True(vstream.Height.Value == vreader.Metadata.Height);

                Assert.True(astream.SampleRateNumber == areader.Metadata.SampleRate);
                Assert.True(Math.Abs(double.Parse(astream.Duration) - areader.Metadata.Duration) < 0.1);
            }
            finally
            {
                if (File.Exists(opath)) File.Delete(opath);
            }
        }

        [Fact]
        public async Task ConversionStreamTest()
        {
            var vpath = Res.GetPath(Res.Video_Mp4);
            var apath = Res.GetPath(Res.Audio_Mp3);
            var opath = "out-test-av-2.mp4";

            try
            {
                using var vreader = new VideoReader(vpath);
                await vreader.LoadMetadataAsync();
                vreader.Load();

                using var areader = new AudioReader(apath);
                await areader.LoadMetadataAsync();
                areader.Load();

                // Get video and audio stream metadata
                var vstream = vreader.Metadata.GetFirstVideoStream();
                var astream = areader.Metadata.GetFirstAudioStream();

                var encoder = new H264Encoder
                {
                    Format = "flv"
                };

                using (var filestream = File.Create(opath))
                {
                    // Prepare writer (Converting to H.264 + AAC video)
                    using (var writer = new AudioVideoWriter(filestream,
                        vstream.Width.Value,
                        vstream.Height.Value,
                        vstream.AvgFrameRateNumber,
                        astream.Channels.Value,
                        astream.SampleRateNumber, 16,
                        encoder.Create(), 
                        new AACEncoder().Create()))
                    {

                        // Open for writing (this starts the FFmpeg process)
                        writer.OpenWrite();

                        // Copy raw data directly from stream to stream
                        var t2 = areader.DataStream.CopyToAsync(writer.InputDataStreamAudio);
                        var t1 = vreader.DataStream.CopyToAsync(writer.InputDataStreamVideo);

                        await t1;
                        await t2;
                    }
                }

                using var video = new VideoReader(opath);
                await video.LoadMetadataAsync();

                Assert.True(video.Metadata.Streams.Length == 2);

                vstream = video.Metadata.GetFirstVideoStream();
                astream = video.Metadata.GetFirstAudioStream();

                Assert.True(Math.Abs(vstream.AvgFrameRateNumber - vreader.Metadata.AvgFramerate) < 0.1);
                Assert.True(Math.Abs(video.Metadata.Duration - vreader.Metadata.Duration) < 0.2);
                Assert.True(vstream.Width.Value == vreader.Metadata.Width);
                Assert.True(vstream.Height.Value == vreader.Metadata.Height);
                Assert.True(astream.SampleRateNumber == areader.Metadata.SampleRate);
            }
            finally
            {
                if (File.Exists(opath)) File.Delete(opath);
            }
        }
    }
}
