namespace Spice86.Core.Emulator.CPU.Registers;

/// <summary>
/// Accesses registers bytes values based on a given index that represents a high/low byte.
/// </summary>
public class UInt8HighLowRegistersIndexer : RegistersIndexer<byte> {
    /// <summary>
    /// 3rd bit in register index means to access the high part
    /// </summary>
    private const int Register8IndexHighBitMask = 0b100;

    /// <summary>
    /// Registers allowing access to their high / low parts have indexes from 0 to 3 so 2 bits
    /// </summary>
    private const int Register8IndexHighLowMask = 0b11;

    private readonly UInt8HighRegistersIndexer _uInt8HighRegistersIndexer;
    private readonly UInt8LowRegistersIndexer _uInt8LowRegistersIndexer;


    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="uInt8HighRegistersIndexer">The u int 8 high registers indexer.</param>
    /// <param name="uInt8LowRegistersIndexer">The u int 8 low registers indexer.</param>
    public UInt8HighLowRegistersIndexer(UInt8HighRegistersIndexer uInt8HighRegistersIndexer,
        UInt8LowRegistersIndexer uInt8LowRegistersIndexer) {
        _uInt8HighRegistersIndexer = uInt8HighRegistersIndexer;
        _uInt8LowRegistersIndexer = uInt8LowRegistersIndexer;
    }

    public override byte this[uint index] {
        get {
            uint indexInArray = ComputeRegisterIndexInArray(index);
            if (IsHigh(index)) {
                return _uInt8HighRegistersIndexer[indexInArray];
            }

            return _uInt8LowRegistersIndexer[indexInArray];
        }
        set {
            uint indexInArray = ComputeRegisterIndexInArray(index);
            if (IsHigh(index)) {
                _uInt8HighRegistersIndexer[indexInArray] = value;
            } else {
                _uInt8LowRegistersIndexer[indexInArray] = value;
            }
        }
    }

    /// <summary>
    /// Determines whether high.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    public static bool IsHigh(uint index) {
        return (index & Register8IndexHighBitMask) != 0;
    }

    /// <summary>
    /// Calculates compute register index in array.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The result of the operation.</returns>
    public static uint ComputeRegisterIndexInArray(uint index) {
        return index & Register8IndexHighLowMask;
    }
}