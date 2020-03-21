# CryVideo
A .NET library that offers a simple interface for reading and writing video files using FFmpeg.

## Requirements
- **FFmpeg** executable (available in PATH or can specify the path manually)
- **FFprobe** executable for reading video metadata


## Usage
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

## Development
This is a personal project. I might add more features in the future.

You are free to request any wanted features and I will consider adding them.