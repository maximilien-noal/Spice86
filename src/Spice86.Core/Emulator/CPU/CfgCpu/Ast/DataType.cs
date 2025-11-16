namespace Spice86.Core.Emulator.CPU.CfgCpu.Ast;

using Spice86.Shared.Emulator.Memory;

/// <summary>
/// Represents the DataType class.
/// </summary>
public class DataType(BitWidth bitWidth, bool signed) {
    /// <summary>
    /// DataType method.
    /// </summary>
    public static DataType UINT8 { get; } = new(BitWidth.BYTE_8, false);
    /// <summary>
    /// DataType method.
    /// </summary>
    public static DataType INT8 { get; } = new(BitWidth.BYTE_8, true);
    /// <summary>
    /// DataType method.
    /// </summary>
    public static DataType UINT16 { get; } = new(BitWidth.WORD_16, false);
    /// <summary>
    /// DataType method.
    /// </summary>
    public static DataType INT16 { get; } = new(BitWidth.WORD_16, true);
    /// <summary>
    /// DataType method.
    /// </summary>
    public static DataType UINT32 { get; } = new(BitWidth.DWORD_32, false);
    /// <summary>
    /// DataType method.
    /// </summary>
    public static DataType INT32 { get; } = new(BitWidth.DWORD_32, true);
    /// <summary>
    /// DataType method.
    /// </summary>
    public static DataType BOOL { get; } = new(BitWidth.DWORD_32, false);

    /// <summary>
    /// Gets or sets the BitWidth.
    /// </summary>
    public BitWidth BitWidth { get; } = bitWidth;
    /// <summary>
    /// Gets or sets the Signed.
    /// </summary>
    public bool Signed { get; } = signed;
}