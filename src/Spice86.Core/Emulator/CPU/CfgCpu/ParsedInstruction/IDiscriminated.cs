namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;

using System.Collections.Generic;

public interface IDiscriminated {
    /// <summary>
    /// Value of the discriminator
    /// </summary>
    public IList<byte?> DiscriminatorValue { get; }
}