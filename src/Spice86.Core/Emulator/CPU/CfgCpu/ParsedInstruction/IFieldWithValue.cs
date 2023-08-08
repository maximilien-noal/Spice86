namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

public interface IFieldWithValue : IDiscriminated {
    
    /// <summary>
    /// When true value can be used for execution.
    /// When false the value has to be retrieved from the memory location of the field because field value is modified by code.
    /// </summary>
    public bool UseValue { get; set; }

    /// <summary>
    /// Compares the positions and the value of this field with those of another field.
    /// </summary>
    /// <param name="other"></param>
    /// <returns>True if position and value is equls to the other field</returns>
    bool IsValueAndPositionEquals(IFieldWithValue other);
    
    /// <summary>
    /// Length of this field
    /// </summary>
    public int Length { get; }
}