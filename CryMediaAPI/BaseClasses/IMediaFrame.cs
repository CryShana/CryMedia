using System;
using System.IO;

namespace CryMediaAPI.BaseClasses;

public interface IMediaFrame
{
    /// <summary>
    /// Contains raw frame data
    /// </summary>
    public Memory<byte> RawData { get; }

    /// <summary>
    /// Loads raw data into memory
    /// </summary>
    /// <param name="stream">Stream containing raw data</param>
    public bool Load(Stream stream);
}
