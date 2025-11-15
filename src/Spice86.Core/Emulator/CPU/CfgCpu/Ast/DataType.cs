namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents data type.
/// </summary>
public class DataType(BitWidth bitWidth, bool signed) {
    /// <summary>
    /// Gets uint8.
    /// </summary>
    public static DataType UINT8 { get; } = new(BitWidth.BYTE_8, false);
    /// <summary>
    /// Gets int8.
    /// </summary>
    public static DataType INT8 { get; } = new(BitWidth.BYTE_8, true);
    /// <summary>
    /// Gets uint16.
    /// </summary>
    public static DataType UINT16 { get; } = new(BitWidth.WORD_16, false);
    /// <summary>
    /// Gets int16.
    /// </summary>
    public static DataType INT16 { get; } = new(BitWidth.WORD_16, true);
    /// <summary>
    /// Gets uint32.
    /// </summary>
    public static DataType UINT32 { get; } = new(BitWidth.DWORD_32, false);
    /// <summary>
    /// Gets int32.
    /// </summary>
    public static DataType INT32 { get; } = new(BitWidth.DWORD_32, true);
    /// <summary>
    /// Gets bool.
    /// </summary>
    public static DataType BOOL { get; } = new(BitWidth.DWORD_32, false);

    /// <summary>
    /// Gets bit width.
    /// </summary>
    public BitWidth BitWidth { get; } = bitWidth;
    /// <summary>
    /// Gets signed.
    /// </summary>
    public bool Signed { get; } = signed;
}