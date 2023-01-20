using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.VM;

namespace Spice86.Core.Emulator.InterruptHandlers.Dos.Xms;

using Spice86.Shared.Interfaces;

public class XmsVirtualDevice : DefaultIOPortHandler {
    public XmsVirtualDevice(Machine machine, Configuration configuration, ILoggerService loggerService) : base(machine, configuration, loggerService) {
    }

    public override byte ReadByte(int port) => _machine.Memory.A20Gate.IsEnabled ? (byte)0x02 : (byte)0x00;

    public override void WriteByte(int port, byte value) => _machine.Memory.A20Gate.IsEnabled = (value & 0x02) != 0;
}