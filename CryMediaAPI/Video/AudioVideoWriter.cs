using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;

using CryMediaAPI.Audio;
using CryMediaAPI.BaseClasses;

namespace CryMediaAPI.Video
{
    public class AudioVideoWriter : MediaWriter<VideoFrame>, IDisposable
    {
        string ffmpeg;
        Socket socket;
        CancellationTokenSource csc;
        internal Process ffmpegp;

        public Process CurrentFFmpegProcess => ffmpegp;
        public NetworkStream InputDataStreamAudio { get; private set; } 

        public Stream DestinationStream { get; }
        public Stream OutputDataStream { get; private set; }

        public string Filename { get; }
        public bool UseFilename { get; }

        public int VideoWidth { get; }
        public int VideoHeight { get; }
        public double VideoFramerate { get; }

        public int AudioChannels { get; }
        public int AudioSampleRate { get; }
        public int AudioBitDepth { get; }

        public FFmpegAudioEncoderOptions AudioEncoderOptions { get; }
        public FFmpegVideoEncoderOptions VideoEncoderOptions { get; }

        /// <summary>
        /// Used for encoding video and audio frames into a single file
        /// </summary>
        /// <param name="filename">Output file name</param>
        /// <param name="video_width">Input video width in pixels</param>
        /// <param name="video_height">Input video height in pixels</param>
        /// <param name="video_framerate">Input video framerate in fps</param>
        /// <param name="audio_channels">Input audio channel count</param>
        /// <param name="audio_sampleRate">Input audio sample rate</param>
        /// <param name="audio_bitDepth">Input audio bits per sample</param>
        /// <param name="videoEncoderOptions">Video encoding options that will be passed to FFmpeg</param>
        /// <param name="audioEncoderOptions">Audio encoding options that will be passed to FFmpeg</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public AudioVideoWriter(string filename, int video_width, int video_height, double video_framerate,
            int audio_channels, int audio_sampleRate, int audio_bitDepth, 
            FFmpegVideoEncoderOptions videoEncoderOptions, 
            FFmpegAudioEncoderOptions audioEncoderOptions, 
            string ffmpegExecutable = "ffmpeg")
        {
            if (video_width <= 0 || video_height <= 0) throw new InvalidDataException("Video frame dimensions have to be bigger than 0 pixels!");
            if (video_framerate <= 0) throw new InvalidDataException("Video framerate has to be bigger than 0!");
            if (string.IsNullOrEmpty(filename)) throw new NullReferenceException("Filename can't be null or empty!");
            if (audio_channels <= 0 || audio_sampleRate <= 0) throw new InvalidDataException("Channels/Sample rate have to be bigger than 0!");
            if (audio_bitDepth != 16 && audio_bitDepth != 24 && audio_bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");

            Filename = filename;
            UseFilename = true;
            
            VideoWidth = video_width;
            VideoHeight = video_height;
            VideoFramerate = video_framerate;
            VideoEncoderOptions = videoEncoderOptions;

            AudioChannels = audio_channels;
            AudioSampleRate = audio_sampleRate;
            AudioBitDepth = audio_bitDepth;
            AudioEncoderOptions = audioEncoderOptions;

            ffmpeg = ffmpegExecutable;
        }

        /// <summary>
        /// Used for encoding video and audio frames into a single stream
        /// </summary>
        /// <param name="outputStream">Output stream</param>
        /// <param name="video_width">Input video width in pixels</param>
        /// <param name="video_height">Input video height in pixels</param>
        /// <param name="video_framerate">Input video framerate in fps</param>
        /// <param name="audio_channels">Input audio channel count</param>
        /// <param name="audio_sampleRate">Input audio sample rate</param>
        /// <param name="audio_bitDepth">Input audio bits per sample</param>
        /// <param name="videoEncoderOptions">Video encoding options that will be passed to FFmpeg</param>
        /// <param name="audioEncoderOptions">Audio encoding options that will be passed to FFmpeg</param>
        /// <param name="ffmpegExecutable">Name or path to the ffmpeg executable</param>
        public AudioVideoWriter(Stream outputStream, int video_width, int video_height, double video_framerate,
            int audio_channels, int audio_sampleRate, int audio_bitDepth,
            FFmpegVideoEncoderOptions videoEncoderOptions,
            FFmpegAudioEncoderOptions audioEncoderOptions,
            string ffmpegExecutable = "ffmpeg")
        {
            if (video_width <= 0 || video_height <= 0) throw new InvalidDataException("Video frame dimensions have to be bigger than 0 pixels!");
            if (video_framerate <= 0) throw new InvalidDataException("Video framerate has to be bigger than 0!");
            if (outputStream == null) throw new NullReferenceException("Stream can't be null!");
            if (audio_channels <= 0 || audio_sampleRate <= 0) throw new InvalidDataException("Channels/Sample rate have to be bigger than 0!");
            if (audio_bitDepth != 16 && audio_bitDepth != 24 && audio_bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");

            DestinationStream = outputStream;
            UseFilename = false;

            VideoWidth = video_width;
            VideoHeight = video_height;
            VideoFramerate = video_framerate;
            VideoEncoderOptions = videoEncoderOptions;

            AudioChannels = audio_channels;
            AudioSampleRate = audio_sampleRate;
            AudioBitDepth = audio_bitDepth;
            AudioEncoderOptions = audioEncoderOptions;

            ffmpeg = ffmpegExecutable;
        }

        /// <summary>
        /// Prepares for writing.
        /// </summary>
        /// <param name="showFFmpegOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        /// <param name="thread_queue_size">Max. number of queued packets when reading from file/stream. 
        /// Should be set to higher when dealing with high rate/low latency streams.</param>
        public void OpenWrite(bool showFFmpegOutput = false, int thread_queue_size = 4096)
        {
            if (OpenedForWriting) throw new InvalidOperationException("File/Stream was already opened for writing!");

            var manual = new ManualResetEvent(false);

            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            socket.Listen(4);
            var port = ((IPEndPoint)socket.LocalEndPoint).Port;
            socket.BeginAccept(r =>
            {
                var s = socket.EndAccept(r);
                InputDataStreamAudio = new NetworkStream(s);
                manual.Set();
            }, null); 

            var cmd = $"-f s{AudioBitDepth}le -channels {AudioChannels} -sample_rate {AudioSampleRate} " +
                $"-thread_queue_size {thread_queue_size} -i \"tcp://{IPAddress.Loopback}:{port}\" " + 
                $"-f rawvideo -video_size {VideoWidth}:{VideoHeight} -r {VideoFramerate} " +
                $"-thread_queue_size {thread_queue_size} -pixel_format rgb24 -i - " +
                $"-map 0 -c:a {AudioEncoderOptions.EncoderName} {AudioEncoderOptions.EncoderArguments} " +
                $"-map 1 -c:v {VideoEncoderOptions.EncoderName} {VideoEncoderOptions.EncoderArguments} " +
                $"-f {VideoEncoderOptions.Format}";

            if (UseFilename)
            {
                if (File.Exists(Filename)) File.Delete(Filename);

                InputDataStream = FFmpegWrapper.OpenInput(ffmpeg, $"{cmd} \"{Filename}\"", out ffmpegp, showFFmpegOutput);
            }
            else
            {
                csc = new CancellationTokenSource();

                // using stream
                (InputDataStream, OutputDataStream) = FFmpegWrapper.Open(ffmpeg, $"{cmd} -", out ffmpegp, showFFmpegOutput);
                _ = OutputDataStream.CopyToAsync(DestinationStream, csc.Token);
            }

            manual.WaitOne();
            OpenedForWriting = true;
        }

        /// <summary>
        /// Closes output video.
        /// </summary>
        public void CloseWrite()
        {
            if (!OpenedForWriting) throw new InvalidOperationException("File is not opened for writing!");

            try
            {
                csc?.Cancel();

                try
                {
                    if (ffmpegp.HasExited) ffmpegp.Kill();
                }
                catch { }

                try
                {
                    InputDataStreamAudio?.Close();
                    InputDataStreamAudio = null;
                    socket.Disconnect(false);
                    socket.Dispose();
                    socket = null;
                }
                catch { }

                InputDataStream.Dispose();
                
                if (!UseFilename) DestinationStream?.Dispose();
            }
            finally
            {
                OpenedForWriting = false;
            }
        }

        /// <summary>
        /// Writes audio frame to output. Make sure to call OpenWrite() before calling this.
        /// </summary>
        /// <param name="frame">Frame containing audio data</param>
        public void WriteFrame(AudioFrame frame)
        {
            if (!OpenedForWriting) throw new InvalidOperationException("Media needs to be prepared for writing first!");

            InputDataStreamAudio.Write(frame.RawData.Span);
        }

        public void Dispose()
        {
            if (OpenedForWriting) CloseWrite();
        }
    }
}
