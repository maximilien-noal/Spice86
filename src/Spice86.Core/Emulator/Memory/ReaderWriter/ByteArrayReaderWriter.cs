namespace Spice86.Core.Emulator.Memory.ReaderWriter;

/// <summary>
/// Implementation of IReaderWriter over a byte array
/// </summary>
public class ByteArrayReaderWriter : ArrayReaderWriter<byte>, IByteReaderWriter {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="array">The array.</param>
    public ByteArrayReaderWriter(byte[] array) : base(array) {
    }
}