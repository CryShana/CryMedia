# CryMedia
A cross-platform .NET library that offers a simple interface for reading, writing and playing video and audio files using FFmpeg.

## Requirements
- **FFmpeg** executable for reading and writing video/audio data
- **FFprobe** executable for reading video/audio metadata
- **FFplay** executable for playing video/audio data

These executables should either be available in the PATH env. variable or should be specified manually.

## Video Usage
### Reading video metadata
```csharp
// reading video file
var video = new VideoReader("test.mp4");

// load metadata
await video.LoadMetadata();

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
    writer.OpenForWriting();

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
// this is the default encoder
var options = new FFmpegEncoderOptions()
{
    Format = "mp4",
    EncoderName = "libx264",
    EncoderArguments = "-preset veryfast -crf 23"
};

// pass options to VideoWriter
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
---
## Audio Usage
### Reading audio metadata
```csharp
// reading audio file
var audio = new AudioReader("test.mp3");

// load metadata
await audio.LoadMetadata();

// example metadata that you can get
var channels = audio.Metadata.Channels;
var samplerate = audio.Metadata.SampleRate;
var codec = audio.Metadata.Codec;
var duration = audio.Metadata.Duration;
```
### Reading and modifying audio samples
```csharp
// open audio for reading samples
audio.Load();
// audio.Load(24); // <-- you can specify bit-depth (16, 24, 32)

// read next frame (in S16LE format - depends on used bit-depth)
var sample = audio.NextSample();

// get value from first channel (index 0)
var value = sample.GetValue(0).Span;

// set values (default bit-depth is 16 = 2 bytes)
px[0] = 124;
px[1] = 23;
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

// play sample... (usually you would have this in a loop)
player.WriteSample(sample);

// dispose manually or use 'using' statement
player.Dispose();
```
###
## Development
This is a personal project. I might add more features in the future.

You are free to request any wanted features and I will consider adding them.
