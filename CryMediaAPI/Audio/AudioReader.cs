using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CryMediaAPI.BaseClasses;
using CryMediaAPI.Audio.Models;

namespace CryMediaAPI.Audio
{
    public class AudioReader : MediaReader<AudioFrame, AudioWriter>, IDisposable
    {
        string ffmpeg, ffprobe;
        int loadedBitDepth = 16;

        public long CurrentSampleOffset { get; private set; }
        public bool MetadataLoaded { get; private set; }      
        public AudioMetadata Metadata { get; private set; }

        /// <summary>
        /// Used for reading metadata and frames from audio files.
        /// </summary>
        /// <param name="filename">Audio file path</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        /// <param name="ffprobeExecutable">Name or path to the ffprobe executable</param>
        public AudioReader(string filename, string ffmpegExecutable = "ffmpeg", string ffprobeExecutable = "ffprobe")
        {
            if (!File.Exists(filename)) throw new FileNotFoundException($"File '{filename}' not found!");

            Filename = filename;
            this.ffmpeg = ffmpegExecutable;
            this.ffprobe = ffprobeExecutable;
        }

        /// <summary>
        /// Load audio metadata into memory.
        /// </summary>
        public async Task LoadMetadata(bool ignoreStreamErrors = false)
        {
            if (MetadataLoaded) throw new InvalidOperationException("Video metadata is already loaded!");
            var r = FFmpegWrapper.OpenOutput(ffprobe, $"-i \"{Filename}\" -v quiet -print_format json=c=1 -show_format -show_streams");

            try
            {
                var metadata = await JsonSerializer.DeserializeAsync<AudioMetadata>(r);

                try
                {
                    var audioStream = metadata.Streams.Where(x => x.CodecType.ToLower().Trim() == "audio").FirstOrDefault();
                    if (audioStream != null)
                    {
                        metadata.Channels = audioStream.Channels;
                        metadata.Codec = audioStream.CodecName;
                        metadata.CodecLongName = audioStream.CodecLongName;
                        metadata.SampleFormat = audioStream.SampleFmt;

                        metadata.SampleRate = audioStream.SampleRate == null ? -1 : 
                            int.Parse(audioStream.SampleRate);

                        metadata.Duration = audioStream.Duration == null ? 
                            double.Parse(metadata.Format.Duration ?? "-1") : 
                            double.Parse(audioStream.Duration);

                        metadata.BitRate = audioStream.BitRate == null ? -1 : 
                            int.Parse(audioStream.BitRate);

                        metadata.BitDepth = audioStream.BitsPerSample;
                        metadata.PredictedSampleCount = (int)Math.Round(metadata.Duration * metadata.SampleRate);

                        if (metadata.BitDepth == 0)
                        {
                            // try to parse it from format
                            if (metadata.SampleFormat.Contains("64")) metadata.BitDepth = 64;
                            else if (metadata.SampleFormat.Contains("32")) metadata.BitDepth = 32;
                            else if (metadata.SampleFormat.Contains("24")) metadata.BitDepth = 24;
                            else if (metadata.SampleFormat.Contains("16")) metadata.BitDepth = 16;
                            else if (metadata.SampleFormat.Contains("8")) metadata.BitDepth = 8;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // failed to interpret video stream settings
                    if (!ignoreStreamErrors) throw new InvalidDataException("Failed to parse audio stream data! " + ex.Message);
                }

                MetadataLoaded = true;
                Metadata = metadata;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to interpret ffprobe audio metadata output! " + ex.Message);
            }
        }

        /// <summary>
        /// Load the audio and prepare it for reading frames.
        /// </summary>
        /// <param name="bitDepth">frame bit rate in which the audio will be processed (16, 24, 32)</param>
        public void Load(int bitDepth = 16)
        {
            if (bitDepth != 16 && bitDepth != 24 && bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");
            if (OpenedForReading) throw new InvalidOperationException("Audio is already loaded!");
            if (!MetadataLoaded) throw new InvalidOperationException("Please load the audio metadata first!");

            // we will be reading audio in S16LE format (for best accuracy, could use S32LE)
            DataStream = FFmpegWrapper.OpenOutput(ffmpeg, $"-i \"{Filename}\" -f s{bitDepth}le -");
            loadedBitDepth = bitDepth;
            OpenedForReading = true;
        }

        /// <summary>
        /// Loads the next audio frame into memory and returns it. This allocates a new frame.
        /// Returns 'null' when there is no next frame.
        /// </summary>
        /// <returns></returns>
        public override AudioFrame NextFrame() => NextFrame(1024);

        /// <summary>
        /// Loads the next audio frame into memory and returns it. This allocates a new frame.
        /// Returns 'null' when there is no next frame.
        /// </summary>
        /// <param name="samples">Number of samples to read in a frame</param>
        public AudioFrame NextFrame(int samples)
        {
            var frame = new AudioFrame(samples, Metadata.Channels, loadedBitDepth);
            return NextFrame(frame);
        }

        /// <summary>
        /// Loads the next audio frame into memory and returns it. This allocates a new frame.
        /// Returns 'null' when there is no next frame.
        /// </summary>
        /// <param name="frame">Existing frame to be overwritten with new frame data.</param>
        public override AudioFrame NextFrame(AudioFrame frame)
        {
            if (!OpenedForReading) throw new InvalidOperationException("Please load the audio first!");

            var success = frame.Load(DataStream);
            if (success) CurrentSampleOffset += frame.LoadedSamples;
            return success ? frame : null;
        }

        public void Dispose()
        {
            DataStream?.Dispose();
        }
    }
}
