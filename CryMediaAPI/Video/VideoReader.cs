using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CryMediaAPI.BaseClasses;
using CryMediaAPI.Video.Models;
using System.Text.RegularExpressions;

namespace CryMediaAPI.Video
{
    public class VideoReader : MediaReader<VideoFrame, MediaWriter<VideoFrame>>, IDisposable
    {
        string ffmpeg, ffprobe;

        public long CurrentFrameOffset { get; private set; }
        public bool LoadedMetadata { get; private set; }
        public VideoMetadata Metadata { get; private set; }


        /// <summary>
        /// Used for reading metadata and frames from video files.
        /// </summary>
        /// <param name="filename">Video file path</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        /// <param name="ffprobeExecutable">Name or path to the ffprobe executable</param>
        public VideoReader(string filename, string ffmpegExecutable = "ffmpeg", string ffprobeExecutable = "ffprobe")
        {
            if (!File.Exists(filename)) throw new FileNotFoundException($"File '{filename}' not found!");

            Filename = filename;
            this.ffmpeg = ffmpegExecutable;
            this.ffprobe = ffprobeExecutable;
        }

        /// <summary>
        /// Load video metadata into memory.
        /// </summary>
        public async Task LoadMetadata(bool ignoreStreamErrors = false)
        {
            if (LoadedMetadata) throw new InvalidOperationException("Video metadata is already loaded!");
            var r = FFmpegWrapper.OpenOutput(ffprobe, $"-i \"{Filename}\" -v quiet -print_format json=c=1 -show_format -show_streams");

            try
            {
                var metadata = await JsonSerializer.DeserializeAsync<VideoMetadata>(r);

                try
                {
                    var videoStream = metadata.Streams.Where(x => x.CodecType.ToLower().Trim() == "video").FirstOrDefault();
                    if (videoStream != null)
                    {
                        metadata.Width = videoStream.Width.Value;
                        metadata.Height = videoStream.Height.Value;
                        metadata.PixelFormat = videoStream.PixFmt;
                        metadata.Codec = videoStream.CodecName;
                        metadata.CodecLongName = videoStream.CodecLongName;

                        metadata.BitRate = videoStream.BitRate == null ? -1 : 
                            int.Parse(videoStream.BitRate);

                        metadata.BitDepth = videoStream.BitsPerRawSample == null ? 
                            tryParseBitDepth(videoStream.PixFmt) : 
                            int.Parse(videoStream.BitsPerRawSample);

                        metadata.Duration = videoStream.Duration == null ? 
                            double.Parse(metadata.Format.Duration ?? "0") : 
                            double.Parse(videoStream.Duration);

                        metadata.SampleAspectRatio = videoStream.SampleAspectRatio;
                        metadata.AvgFramerateText = videoStream.AvgFrameRate;
                        metadata.AvgFramerate = 0.0;

                        if (videoStream.AvgFrameRate.Contains('/'))
                        {
                            var parsed = videoStream.AvgFrameRate.Split('/');
                            metadata.AvgFramerate = double.Parse(parsed[0]) / double.Parse(parsed[1]);
                        }
                        else metadata.AvgFramerate = double.Parse(videoStream.AvgFrameRate);

                        metadata.PredictedFrameCount = (int)(metadata.AvgFramerate * metadata.Duration);
                    }
                }
                catch (Exception ex)
                {
                    // failed to interpret video stream settings
                    if (!ignoreStreamErrors) throw new InvalidDataException("Failed to parse video stream data! " + ex.Message);
                }

                LoadedMetadata = true;
                Metadata = metadata;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to interpret ffprobe video metadata output! " + ex.Message);
            }
        }

        /// <summary>
        /// Load the video and prepare it for reading frames.
        /// </summary>
        public void Load() => Load(0);

        /// <summary>
        /// Load the video for reading frames and seeks to given offset in seconds.
        /// </summary>
        /// <param name="offsetSeconds">Offset in seconds to which to seek to</param>
        public void Load(double offsetSeconds)
        {
            if (OpenedForReading) throw new InvalidOperationException("Video is already loaded!");
            if (!LoadedMetadata) throw new InvalidOperationException("Please load the video metadata first!");
            if (Metadata.Width == 0 || Metadata.Height == 0) throw new InvalidDataException("Loaded metadata contains errors!");

            // we will be reading video in RGB24 format
            DataStream = FFmpegWrapper.OpenOutput(ffmpeg, $"{(offsetSeconds <= 0 ? "" : $"-ss {offsetSeconds:0.00}")} -i \"{Filename}\"" +
                $" -pix_fmt rgb24 -f rawvideo -");
            OpenedForReading = true;
        }

        /// <summary>
        /// Loads the next video frame into memory and returns it. This allocates a new frame.
        /// Returns 'null' when there is no next frame.
        /// </summary>
        public override VideoFrame NextFrame()
        {
            var frame = new VideoFrame(Metadata.Width, Metadata.Height);
            return NextFrame(frame);
        }

        /// <summary>
        /// Loads the next video frame into memory and returns it. This overrides the given frame with no extra allocations. Recommended for performance.
        /// Returns 'null' when there is no next frame.
        /// </summary>
        /// <param name="frame">Existing frame to be overwritten with new frame data.</param>
        public override VideoFrame NextFrame(VideoFrame frame)
        {
            if (!OpenedForReading) throw new InvalidOperationException("Please load the video first!");

            var success = frame.Load(DataStream);
            if (success) CurrentFrameOffset++;
            return success ? frame : null;
        }

        static Regex bitRateSimpleRgx = new Regex(@"\D(\d+?)[bl]e", RegexOptions.Compiled);
        int tryParseBitDepth(string pix_fmt)
        {
            var match = bitRateSimpleRgx.Match(pix_fmt);
            if (match.Success) return int.Parse(match.Groups[1].Value);

            return -1;
        }

        public void Dispose()
        {
            DataStream?.Dispose();
        }
    }
}
