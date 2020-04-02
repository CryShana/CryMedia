using Xunit;
using System;
using CryMediaAPI.Audio;
using CryMediaAPI.Resources;
using System.Threading.Tasks;

namespace CryMediaAPI.Tests
{
    public class AudioMetadataTests
    {
        [Fact]
        public async Task LoadMetadataMp3()  
        {
            var audio = new AudioReader(Res.GetPath(Res.Audio_Mp3));

            await audio.LoadMetadata();

            Assert.True(audio.Metadata.Codec == "mp3");
            Assert.True(audio.Metadata.BitRate == 128000);
            Assert.True(audio.Metadata.SampleFormat == "fltp");
            Assert.True(audio.Metadata.SampleRate == 44100);
            Assert.True(audio.Metadata.Channels == 2);
            Assert.True(audio.Metadata.Streams.Length == 1);
            Assert.True(Math.Abs(audio.Metadata.Duration - 1.549187) < 0.01);
        }

        [Fact]
        public async Task LoadMetadataOgg()
        {
            var audio = new AudioReader(Res.GetPath(Res.Audio_Ogg));

            await audio.LoadMetadata();

            Assert.True(audio.Metadata.Codec == "vorbis");
            Assert.True(audio.Metadata.BitRate == 48000);
            Assert.True(audio.Metadata.SampleFormat == "fltp");
            Assert.True(audio.Metadata.SampleRate == 11025);
            Assert.True(audio.Metadata.Channels == 2);
            Assert.True(audio.Metadata.Streams.Length == 1);
            Assert.True(Math.Abs(audio.Metadata.Duration - 1.515102) < 0.01);
        }
    }
}
