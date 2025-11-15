namespace Spice86.Core.Emulator.Memory.ReaderWriter;

/// <summary>
/// Implementation of IReaderWriter over a uint array
/// </summary>
public class UIntArrayReaderWriter : ArrayReaderWriter<uint>, IUIntReaderWriter {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="array">The array.</param>
    public UIntArrayReaderWriter(uint[] array) : base(array) {
    }
}