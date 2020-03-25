using System;
using System.IO;

namespace CryMediaAPI.Audio
{
    /// <summary>
    /// Audio frame containing sample data in RGB24 format.
    /// </summary>
    public class AudioSample : IDisposable
    {
        int depthBytes = 0;
        int size, offset = 0;

        byte[] frameBuffer;
        public Memory<byte> RawData { get; }

        /// <summary>
        /// Creates an empty audio frame with given dimensions using the RGB24 pixel format.
        /// </summary>
        public AudioSample(int channels, int depthBytes = 2)
        {
            if (channels <= 0 || depthBytes <= 0) throw new InvalidDataException("Channel and depth count has to be bigger than 0!");

            this.depthBytes = depthBytes;
            size = channels * depthBytes;
            frameBuffer = new byte[size];
            RawData = frameBuffer.AsMemory();
        }

        /// <summary>
        /// Loads sample data from stream.
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
        /// Returns part of memory that contains the sample data
        /// </summary>
        /// <param name="channel">Channel index</param>
        public Memory<byte> GetValue(int channel) 
        {
            int index = channel * depthBytes;
            return RawData.Slice(index, depthBytes);
        }

        public void Dispose()
        {
            frameBuffer = null;
        }
    }
}
