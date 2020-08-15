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
        /// Input data stream
        /// </summary>
        public virtual Stream InputDataStream { get; protected set; }

        /// <summary>
        /// Is data stream opened for writing
        /// </summary>
        public virtual bool OpenedForWriting { get; protected set; }

        /// <summary>
        /// Writes frame to output. Make sure to call OpenWrite() before calling this.
        /// </summary>
        /// <param name="frame">Frame containing media data</param>
        public virtual void WriteFrame(Frame frame)
        {
            if (!OpenedForWriting) throw new InvalidOperationException("Media needs to be prepared for writing first!");

            InputDataStream.Write(frame.RawData.Span);
        }
    }
}
