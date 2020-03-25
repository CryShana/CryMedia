using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CryMediaAPI.Audio.Models;
using System.Collections.Generic;

namespace CryMediaAPI.Audio
{
    public class AudioReader : IDisposable
    {
        Stream audioStream;
        string ffmpeg, ffprobe;
        bool loadedAudio = false;
        bool loadedMetadata = false;

        public string Filename { get; }
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
            if (loadedMetadata) throw new InvalidOperationException("Video metadata is already loaded!");
            var r = FFmpegWrapper.OpenOutput(ffprobe, $"-i \"{Filename}\" -v quiet -print_format json=c=1 -show_format -show_streams");

            try
            {
                var metadata = await JsonSerializer.DeserializeAsync<AudioMetadata>(r);

                try
                {
                    var audioStream = metadata.Streams.Where(x => x.CodecType.ToLower().Trim() == "audio").FirstOrDefault();
                    if (audioStream != null)
                    {
                        metadata.Codec = audioStream.CodecName;
                        metadata.CodecLongName = audioStream.CodecLongName;
                        metadata.SampleFormat = audioStream.SampleFmt;
                        metadata.SampleRate = int.Parse(audioStream.SampleRate);
                        metadata.Duration = double.Parse(audioStream.Duration);
                        metadata.BitRate = int.Parse(audioStream.BitRate);
                        metadata.BitDepth = audioStream.BitsPerSample;
                    }
                }
                catch (Exception ex)
                {
                    // failed to interpret video stream settings
                    if (!ignoreStreamErrors) throw new InvalidDataException("Failed to parse audio stream data! " + ex.Message);
                }

                loadedMetadata = true;
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
        public void Load()
        {
            if (loadedAudio) throw new InvalidOperationException("Audio is already loaded!");
            if (!loadedMetadata) throw new InvalidOperationException("Please load the audio metadata first!");
            //if (Metadata.Width == 0 || Metadata.Height == 0) throw new InvalidDataException("Loaded metadata contains errors!");

            // we will be reading video in RGB24 format
            audioStream = FFmpegWrapper.OpenOutput(ffmpeg, $"-i \"{Filename}\" -pix_fmt rgb24 -f rawvideo -");
            loadedAudio = true;
        }

        /// <summary>
        /// Loads the next audio frame into memory and returns it. This allocates a new frame.
        /// Returns 'null' when there is no next frame.
        /// </summary>
        public AudioFrame NextFrame()
        {
            if (!loadedAudio) throw new InvalidOperationException("Please load the audio first!");

            throw new NotImplementedException();

            var frame = new AudioFrame(0, 0);
            var success = frame.Load(audioStream);
            return success ? frame : null;
        }

        /// <summary>
        /// Loads the next audio frame into memory and returns it. This overrides the given frame with no extra allocations. Recommended for performance.
        /// Returns 'null' when there is no next frame.
        /// </summary>
        /// <param name="frame">Existing frame to be overwritten with new frame data.</param>
        public AudioFrame NextFrame(AudioFrame frame)
        {
            if (!loadedAudio) throw new InvalidOperationException("Please load the audio first!");

            var success = frame.Load(audioStream);
            return success ? frame : null;
        }

        public void Dispose()
        {
            audioStream?.Dispose();
        }
    }
}
