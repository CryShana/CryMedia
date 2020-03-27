using System;
using System.IO;

namespace CryMediaAPI.Audio
{
    /// <summary>
    /// Audio sample containing sample data in PCM format with given bit depth.
    /// </summary>
    public class AudioSample : IDisposable
    {
        int depthBytes = 0;
        int size, offset = 0;

        byte[] sampleBuffer;
        public Memory<byte> RawData { get; }

        /// <summary>
        /// Creates an empty audio sample with given bit depth using the PCM format.
        /// </summary>
        public AudioSample(int channels, int bitDepth = 16)
        {
            if (bitDepth != 16 && bitDepth != 24 && bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");
            if (channels <= 0) throw new InvalidDataException("Channel count has to be bigger than 0!");

            this.depthBytes = bitDepth / 8;
            size = channels * depthBytes;
            sampleBuffer = new byte[size];
            RawData = sampleBuffer.AsMemory();
        }

        /// <summary>
        /// Loads sample data from stream.
        /// </summary>
        /// <param name="str">Stream containing raw sample data in PCM format</param>
        public bool Load(Stream str)
        {
            offset = 0;

            while (offset < size)
            {
                var r = str.Read(sampleBuffer, offset, size - offset);
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
            sampleBuffer = null;
        }
    }
}
