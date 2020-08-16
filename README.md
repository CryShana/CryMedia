# CryMedia
A cross-platform .NET library that offers a simple interface for reading, writing and playing video and audio files using FFmpeg.

Focuses on the ability to edit raw video/audio data directly using C#.

## Requirements
- **FFmpeg** executable for reading and writing video/audio data
- **FFprobe** executable for reading video/audio metadata
- **FFplay** executable for playing video/audio data

These executables should either be available in the PATH env. variable or should be specified manually.

## Install
```
dotnet add package CryMediaAPI
```

(Requires .NET Core 3.1) For older versions I recommend downloading the code from here and using it directly.
For even older versions, you might have to replace the JSON serializer and adjust the syntax.

## Video Usage
### Reading video metadata
```csharp
// reading video file
var video = new VideoReader("test.mp4");

// load metadata
await video.LoadMetadataAsync();

// example metadata that you can get
var width = video.Metadata.Width;
var height = video.Metadata.Height;
var fps = video.Metadata.AvgFramerate;
var duration = video.Metadata.Duration;
```
### Reading and modifying video frames
```csharp
// open video for reading frames
video.Load();

// read next frame (in RGB24 format)
var frame = video.NextFrame();

// get pixel at (1, 1)
var px = frame.GetPixels(1, 1).Span;

// set pixel to RGB (255, 0, 0)
px[0] = 255;
px[1] = 0;
px[2] = 0;
```
### Writing video frames
```csharp
using (var writer = new VideoWriter("out.mp4", width, height, fps)
{
    // open video for writing
    writer.OpenWrite();

    while (true)
    {
        // read next frame (using VideoReader)
        var f = video.NextFrame(frame);
        if (f == null) break;
       
        // draw a red square (100x100 pixels)
        for (int i = 0; i < 100; i++)
            for (int j = 0; j < 100; j++)
            {
                var px = frame.GetPixels(i, j).Span;
                px[0] = 255;
                px[1] = 0;
                px[2] = 0;
            }

        // write the modified frame
        writer.WriteFrame(frame);
    }
}
```
### Custom Encoding Options
You can customize the encoding options that will be passed to FFmpeg.

```csharp
// EncoderOptions class
var options = new EncoderOptions()
{
    Format = "mp4",
    EncoderName = "libx264",
    EncoderArguments = "-preset veryfast -crf 23"
};

// pass options to VideoWriter
var writer = new VideoWriter("out.mp4", w, h, fps, options);
```
Or use the existing EncoderOptions builders! Or implement your own!
```csharp
var encoder = new H264Encoder();
encoder.EncoderPreset = H264Encoder.Preset.Medium;
encoder.EncoderTune = H264Encoder.Tune.Film;
encoder.EncoderProfile = H264Encoder.Profile.High;
encoder.SetCQP(22);

var options = encoder.Create();
var writer = new VideoWriter("out.mp4", w, h, fps, options);
```
### Playing video files
```csharp
var player = new VideoPlayer("video.mp4");
player.Play();
```
### Playing video frames directly
```csharp
var player = new VideoPlayer();
player.OpenWrite(video.Metadata.Width, video.Metadata.Height, video.Metadata.AvgFramerateText);

// play frame... (usually you would have this in a loop)
player.WriteFrame(frame);

// dispose manually or use 'using' statement
player.Dispose();
```

### Seeking & saving frame as image
```csharp
// seek to 60 seconds and 500ms (60.5sec)
videoReader.Load(60.5);   

var fr = videoReader.NextFrame();

// save frame as PNG
fr.Save("frame.png");

// save frame as WEBP
fr.Save("frame.webp", "libwebp");
```

## Audio Usage
### Reading audio metadata
```csharp
// reading audio file
var audio = new AudioReader("test.mp3");

// load metadata
await audio.LoadMetadataAsync();

// example metadata that you can get
var channels = audio.Metadata.Channels;
var samplerate = audio.Metadata.SampleRate;
var codec = audio.Metadata.Codec;
var duration = audio.Metadata.Duration;
```
### Reading and modifying audio samples (frames)
```csharp
// open audio for reading samples (frames)
audio.Load();
// audio.Load(24); // <-- you can specify bit-depth (16, 24, 32)

// read next number of samples (frame) (in S16LE format - depends on used bit-depth)
var frame = audio.NextFrame();

// get first sample from first channel
var val = frame.GetSample(0, 0).Span;

// set values (default bit-depth is 16 = 2 bytes)
val[0] = 124;
val[1] = 23;
```
### Writing audio samples (frames)
```csharp
using (var writer = new AudioWriter("out.mp3", channels, sample_rate, 16)
{
    // open audio for writing
    writer.OpenWrite();

    while (true)
    {
        // read next number of samples (frame)
        var f = audio.NextFrame(frame);
        if (f == null) break;

        // get and modify first sample in first channel
        var val = frame.GetSample(0, 0).Span;
        val[0] = 122;
        val[1] = 23;

        // write the modified frame
        writer.WriteFrame(frame);
    }
}
```
### Custom encoding options

```csharp
// this is the default encoder
var options = new EncoderOptions()
{
    Format = "mp3",
    EncoderName = "libmp3lame",
    EncoderArguments = "-ar 44100 -ac 2 -b:a 192k"
};

// pass options to AudioWriter
var writer = new AudioWriter("out.mp3", channels, sample_rate, 16, options);
```
Or use the existing EncoderOptions builders! Or implement your own!
```csharp
var encoder = new OpusEncoder();
encoder.CodecApplication = OpusEncoder.Application.Audio;
encoder.CompressionLevel = 10;
encoder.SetVBR("128k");

var options = encoder.Create();
var writer = new AudioWriter("out.ogg", channels, sample_rate, 16, options);
```

### Playing audio files
```csharp
var player = new AudioPlayer("audio.mp3");
player.Play();
```

### Playing audio samples directly
```csharp
var player = new AudioPlayer();
player.OpenWrite(audio.Metadata.SampleRate, audio.Metadata.Channels);

// play frame... (usually you would have this in a loop)
player.WriteFrame(sample);

// dispose manually or use 'using' statement
player.Dispose();
```
### Conversions
You can manually set the EncoderOptions object and use the VideoWriter static methods for conversions:
```csharp
// This will convert the "from.mp4" video to "to.flv" video
VideoWriter.FileToFile("from.mp4", "to.flv", new EncoderOptions
{
    Format = "flv",
    EncoderName = "h264_nvenc",
    EncoderArguments = "-preset slow"
}, out _);

// This will convert the "from.mp4" directly into a stream (example: for streaming video)
var stream = VideoWriter.FileToStream("from.mp4", new EncoderOptions
{
    Format = "flv",
    EncoderName = "h264_nvenc",
    EncoderArguments = "-preset slow"
}, out _);

// You can convert from one stream to another stream
var (input, output) = VideoWriter.StreamToStream(new EncoderOptions
{
    Format = "flv",
    EncoderName = "h264_nvenc",
    EncoderArguments = "-preset slow"
}, out _, "-f mp4");  // <-- you can further describe the input stream if required
```
Or you can use EncoderOption builders. For example VP9:
```csharp
// Converting video to VP9 manually (using constant quality mode and row multithreading)
var encoder = new VP9Encoder();
encoder.RowBasedMultithreading = true;
encoder.SetCQP(31);

// get EncoderOptions from builder
var options = encoder.Create();

// EXAMPLE: using VideoReader + VideoWriter for conversion 
using (var reader = new VideoReader(input))
{
    reader.LoadMetadata();
    reader.Load();

    using (var writer = new VideoWriter(output,
        reader.Metadata.Width,
        reader.Metadata.Height,
        reader.Metadata.AvgFramerate,
        options))
    {
        writer.OpenWrite();
        
        // instead of "CopyTo" can read and write frames separately
        reader.CopyTo(writer);
    }
}
```
### AudioVideoWriter
Made to handle both video and audio readers and writing to a single file or stream.

If you only want to convert files, use the static methods on `VideoWriter` (`FileToFile`, `FileToStream`, ...). Use this if you have a Video and Audio reader and want to join them together.
```csharp
// Load video
var vreader = new VideoReader(input);
vreader.LoadMetadata();
vreader.Load();

// Load audio
var areader = new AudioReader(input);
areader.LoadMetadata();
areader.Load();

// Get video and audio stream metadata (or just access metadata properties directly instead)
var vstream = vreader.Metadata.GetFirstVideoStream();
var astream = areader.Metadata.GetFirstAudioStream();

// Prepare writer (Converting to H.264 + AAC video)
var writer = new AudioVideoWriter(output,
    vstream.Width.Value,
    vstream.Height.Value,
    vstream.AvgFrameRateNumber,
    astream.Channels.Value,
    astream.SampleRateNumber, 16,
    new H264Encoder().Create(),
    new AACEncoder().Create());

// Open for writing (this starts the FFmpeg process)
writer.OpenWrite();

// Attach a progress tracker to the FFmpeg process
var progress = FFmpegWrapper.RegisterProgressTracker(writer.CurrentFFmpegProcess, vreader.Metadata.Duration);
progress.ProgressChanged += (s, p) => Console.WriteLine($"{p:0.00}%");

// Copy raw data directly from stream to stream
// Or you can read/write frames manually here
var t1 = vreader.DataStream.CopyToAsync(writer.InputDataStreamVideo);
var t2 = areader.DataStream.CopyToAsync(writer.InputDataStreamAudio);

Task.WaitAll(t1, t2);
```

## FFmpeg Wrapper
For specialized needs, you can use the FFmpeg wrapper functions directly using the `FFmpegWrapper` static class.

## Development
This is a personal project. I might add more features in the future.

You are free to request any wanted features and I will consider adding them.
