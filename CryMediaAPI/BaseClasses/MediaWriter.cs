using System;
using System.IO;

namespace CryMediaAPI.BaseClasses
{
    public abstract class MediaWriter<Frame> where Frame : IMediaFrame
    {
        /// <summary>
        /// Output filename
        /// </summary>
        public virtual string Filename { get; protected set; }

        /// <summary>
        /// Output raw data stream
        /// </summary>
        public virtual Stream DataStream { get; protected set; }

        /// <summary>
        /// Is data stream opened for writing
        /// </summary>
        public virtual bool OpenedForWriting { get; protected set; }

        /// <summary>
        /// Writes frame to output
        /// </summary>
        /// <param name="frame">Frame containing media data</param>
        public virtual void WriteFrame(Frame frame)
        {
            if (!OpenedForWriting) throw new InvalidOperationException("File needs to be opened for writing first!");

            DataStream.Write(frame.RawData.Span);
        }
    }
}
