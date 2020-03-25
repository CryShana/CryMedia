using System;
using System.IO;

namespace CryMediaAPI.Audio
{
    /// <summary>
    /// Audio frame containing sample data in RGB24 format.
    /// </summary>
    public class AudioFrame : IDisposable
    {
        int size, offset = 0;

        byte[] frameBuffer;
        public Memory<byte> RawData { get; }


        /// <summary>
        /// Creates an empty audio frame with given dimensions using the RGB24 pixel format.
        /// </summary>
        public AudioFrame()
        {
            //if (w <= 0 || h <= 0) throw new InvalidDataException("Video frame dimensions have to be bigger than 0 pixels!");

            //size = Width * Height * 3;
            frameBuffer = new byte[size];
            RawData = frameBuffer.AsMemory();
        }

        /// <summary>
        /// Loads frame data from stream.
        /// </summary>
        /// <param name="str">Stream containing raw frame data in RGB24 format</param>
        public bool Load(Stream str)
        {
            offset = 0;

            while (offset < size)
            {
                var r = str.Read(frameBuffer, offset, size - offset);
                if (r <= 0) return false;
                offset += r;
            }

            return true;
        }

        /// <summary>
        /// Returns part of memory that contains the pixel data.
        /// </summary>
        /// <param name="x">Starting X coordinate of wanted pixel/s</param>
        /// <param name="y">Starting Y coordinate of wanted pixel/s</param>
        /// <param name="length">Number of pixels to return from the starting pixel</param>
        public Memory<byte> GetPixels(int x, int y, int length = 1) 
        {
            int index = 3;
            return RawData.Slice(index, length * 3);
        }

        public void Dispose()
        {
            frameBuffer = null;
        }
    }
}
