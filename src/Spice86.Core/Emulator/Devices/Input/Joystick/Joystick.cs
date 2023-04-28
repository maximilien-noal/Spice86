namespace Spice86.Core.Emulator.Devices.Input.Joystick;

using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Interfaces;

/// <summary>
/// Joystick implementation. Emulates an unplugged joystick for now.
/// </summary>
public class Joystick : DefaultIOPortHandler {
    public const int JoystickPositionAndStatus = 0x201;

    public byte JoystickPositionAndStatusValue { get; private set; } = 0xFF;

    /// <summary>
    /// Initializes a new instance of the <see cref="Joystick"/>
    /// </summary>
    /// <param name="machine">The emulator machine.</param>
    /// <param name="configuration">The emulator configuration.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public Joystick(Machine machine, Configuration configuration, ILoggerService loggerService) : base(machine, configuration, loggerService) {
    }

    /// <inheritdoc />
    public override void InitPortHandlers(IOPortDispatcher ioPortDispatcher) {
        ioPortDispatcher.AddIOPortHandler(JoystickPositionAndStatus, this);
    }

    /// <inheritdoc />
    public override byte ReadByte(int port) {
        return port switch {
            JoystickPositionAndStatus => JoystickPositionAndStatusValue,
            _ => base.ReadByte(port),
        };
    }

    /// <inheritdoc />
    public override void WriteByte(int port, byte value) {
        switch (port) {
            case JoystickPositionAndStatus:
                JoystickPositionAndStatusValue = value;
                break;
            default:
                base.WriteByte(port, value);
                break;
        }
    }
}