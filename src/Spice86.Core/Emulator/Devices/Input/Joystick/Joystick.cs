namespace Spice86.Core.Emulator.Devices.Input.Joystick;

using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Joystick;
using Spice86.Shared.Interfaces;

/// <summary>
/// Joystick implementation. Emulates an unplugged joystick for now.
/// </summary>
public class Joystick : DefaultIOPortHandler {
    public const int GetSetJoystickStatus = 0x201;

    public GameportState GameportState { get; private set; } = new();
    
    public const uint GamePortStateTimeoutInTimerTicks = 100;

    public long TimerTicksOnLastGamePortWrite { get; private set; }

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
        ioPortDispatcher.AddIOPortHandler(GetSetJoystickStatus, this);
    }

    /// <inheritdoc />
    public override byte ReadByte(int port) {
        LastGamePortReadValue = port switch {
            GetSetJoystickStatus => GetGameportStateValue(),
            _ => base.ReadByte(port),
        };
        return LastGamePortReadValue;
    }

    public byte LastGamePortReadValue { get; private set; }

    public byte GetGameportStateValue() {
        long numberOfTicks = _machine.Timer.NumberOfTicks;
        if (numberOfTicks >= TimerTicksOnLastGamePortWrite + GamePortStateTimeoutInTimerTicks) {
            GameportState.Value = 0xFF;
        }
        return GameportState.Value;
    }
    
    /// <inheritdoc />
    public override void WriteByte(int port, byte value) {
        switch (port) {
            case GetSetJoystickStatus:
                GameportState.Value = value;
                TimerTicksOnLastGamePortWrite = _machine.Timer.NumberOfTicks;
                break;
            default:
                base.WriteByte(port, value);
                break;
        }
    }
}