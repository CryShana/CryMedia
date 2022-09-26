using System;
using System.IO;
using System.Threading.Tasks;

namespace CryMediaAPI.BaseClasses;

public abstract class MediaReader<Frame, Writer> where Frame : IMediaFrame where Writer : MediaWriter<Frame>
{        
    /// <summary>
    /// Input filename
    /// </summary>
    public virtual string Filename { get; protected set; }

    /// <summary>
    /// Input raw data stream
    /// </summary>
    public virtual Stream DataStream { get; protected set; }

    /// <summary>
    /// Is data stream opened for reading
    /// </summary>
    public virtual bool OpenedForReading { get; protected set; }

    public abstract Frame NextFrame();
    public abstract Frame NextFrame(Frame frame);

    /// <summary>
    /// Copy data directly to writer
    /// </summary>
    /// <param name="writer">Writer that is opened for writing</param>
    public virtual void CopyTo(MediaWriter<Frame> writer)
    {
        if (DataStream == null) throw new InvalidOperationException("Reader is not opened for reading! Have you called Load()?");
        if (!writer.OpenedForWriting) throw new InvalidOperationException("Writer is not opened for writing!");

        DataStream.CopyTo(writer.InputDataStream);
    }

    /// <summary>
    /// Copy data directly to writer
    /// </summary>
    /// <param name="writer">Writer that is opened for writing</param>
    public virtual async Task CopyToAsync(MediaWriter<Frame> writer)
    {
        if (DataStream == null) throw new InvalidOperationException("Reader is not opened for reading! Have you called Load()?");
        if (!writer.OpenedForWriting) throw new InvalidOperationException("Writer is not opened for writing!");          

        await DataStream.CopyToAsync(writer.InputDataStream);
    }
}
