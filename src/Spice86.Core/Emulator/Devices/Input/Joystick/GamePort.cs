namespace Spice86.Core.Emulator.Devices.Input.Joystick;

using Serilog.Events;

using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Joystick;
using Spice86.Shared.Interfaces;

/// <summary>
/// Represents a game port, with an optionally attached joystick controller.
/// </summary>
public class GamePort : DefaultIOPortHandler {
    /// <summary>
    /// The I/O port number to get or set the joystick status byte.
    /// </summary>
    public const int GetSetJoystickStatus = 0x201;

    private new ILoggerService _loggerService;

    /// <summary>
    /// The attached joystick
    /// </summary>
    public Joystick Joystick { get; private set; } = new();
    
    /// <summary>
    /// The value, in ticks, before the joystick state is considered obsolete for a read.
    /// </summary>
    public const uint Timeout = 100;

    /// <summary>
    /// Gets or sets whether a port write is active.
    /// </summary>
    public bool IsWriteActive { get; set; }

    /// <summary>
    /// The last time, in ticks, when we received a port write that updates the joystick's state.
    /// </summary>
    public long LastWrite { get; set; }

    private readonly Configuration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="GamePort"/>
    /// </summary>
    /// <param name="machine">The emulator machine.</param>
    /// <param name="configuration">The emulator configuration.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public GamePort(Machine machine, Configuration configuration, ILoggerService loggerService) : base(machine, configuration, loggerService) {
        _configuration = configuration;
        _loggerService = loggerService.WithLogLevel(LogEventLevel.Debug);
        if (_configuration.Joystick) {
            Joystick.IsConnected = true;
        }
    }

    /// <inheritdoc />
    public override void InitPortHandlers(IOPortDispatcher ioPortDispatcher) {
        ioPortDispatcher.AddIOPortHandler(GetSetJoystickStatus, this);
    }

    /// <summary>
    /// Sets the dead zone to 100, and IsConnected to <c>true</c>.
    /// <remarks>Used for debugging.</remarks>
    /// </summary>
    public void ConnectDigitalJoystick() {
        Joystick.DeadZone = 100;
        Joystick.IsConnected = true;
    }

    /// <summary>
    /// Sets the X Axis to the given value.
    /// </summary>
    /// <remarks>Used for debugging.</remarks>
    /// <param name="value">The value to copy into XPos</param>
    public void SetX(double value) {
        Joystick.XPos = value;
        Joystick.IsTransformed = false;
    }
    
    /// <summary>
    /// Sets the Y Axis to the given value.
    /// </summary>
    /// <remarks>Used for debugging.</remarks>
    /// <param name="value">The value to copy into YPos</param>
    public void SetY(double value) {
        Joystick.YPos = value;
        Joystick.IsTransformed = false;
    }

    private byte ReadJoystickStatus() {
        if(IsWriteActive && _machine.Timer.NumberOfTicks - LastWrite > Timeout) {
            IsWriteActive = false;
            Joystick.XCount = Joystick.YCount = 0;
            if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                _loggerService.Debug("Joystick X and Y axis values reset by {MethodName}", nameof(ReadJoystickStatus));
            }
        }

        /*  Format of the byte to be returned:       
        **                        | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
        **                        +-------------------------------+
        **                          |   |   |   |   |   |   |   |
        **  Joystick B, Button 2 ---+   |   |   |   |   |   |   +--- Joystick A, X Axis
        **  Joystick B, Button 1 -------+   |   |   |   |   +------- Joystick A, Y Axis
        **  Joystick A, Button 2 -----------+   |   |   +----------- Joystick B, X Axis
        **  Joystick A, Button 1 ---------------+   +--------------- Joystick B, Y Axis
         */
        
        byte ret = 0xFF;
        if (!Joystick.IsConnected) {
            return ret;
        }

        if (Joystick.XCount is not 0) {
            Joystick.XCount--;
        } else {
            ret = (byte)(ret & ~1);
        }

        if (Joystick.YCount is not 0) {
            Joystick.YCount--;
        } else {
            ret = (byte)(ret & ~2);
        }

        if (Joystick.ButtonA) {
            ret = (byte)(ret & ~16);
        }

        if (Joystick.ButtonB) {
            ret = (byte)(ret & ~32);
        }
        LastReadTime = DateTime.Now;
        return ret;
    }

    /// <inheritdoc />
    public override byte ReadByte(int port) {
        LastReadValue = port switch {
            GetSetJoystickStatus => _configuration.Joystick ? ReadJoystickStatus() : base.ReadByte(port),
            _ => base.ReadByte(port),
        };
        return LastReadValue;
    }

    /// <summary>
    /// The value returned by the latest port read.
    /// </summary>
    public byte LastReadValue { get; private set; }
    
    /// <summary>
    /// The timestamp for the last read of the port <see cref="GetSetJoystickStatus"/>.
    /// </summary>
    public DateTimeOffset LastReadTime { get; private set; }

    /// <summary>
    /// The timestamp for the last write of the port <see cref="GetSetJoystickStatus"/>.
    /// </summary>
    public DateTimeOffset LastWriteTime { get; private set; }

    private static uint PercentToCount(double percent) {
        const int range = 64;
        uint scaled = (uint)(percent * range);
        uint shifted = scaled * range;
        return shifted;
    }

    private void TransformDigital() {
        Joystick.XFinal = Joystick.XPos switch {
            > 0.5 => 1.0,
            < -0.5 => -1.0,
            _ => 0.0
        };
        Joystick.YFinal = Joystick.YPos switch {
            > 0.5 => 1.0,
            < -0.5 => -1.0,
            _ => 0.0
        };
    }

    private void TransformInput() {
        if (Joystick.IsTransformed) {
            return;
        }
        Joystick.IsTransformed = true;
        if (Joystick.DeadZone == 100) {
            TransformDigital();
        }
        else {
            TransformInsideSquare();
            Clip();
        }
    }

    private void TransformInsideSquare() {
        double deadzonePercent = Joystick.DeadZone / 100.0;
        double signedDeadZonePercent = 1.0 - deadzonePercent;

        if (Joystick.XPos > deadzonePercent) {
            Joystick.XFinal = (Joystick.XPos - deadzonePercent)/ signedDeadZonePercent;
        } else if ( Joystick.XPos < -deadzonePercent) {
            Joystick.XFinal = (Joystick.XPos + deadzonePercent) / signedDeadZonePercent;
        } else {
            Joystick.XFinal = 0.0;
        }
        if (Joystick.YPos > deadzonePercent) {
            Joystick.YFinal = (Joystick.YPos - deadzonePercent)/ signedDeadZonePercent;
        } else if (Joystick.YPos < -deadzonePercent) {
            Joystick.YFinal = (Joystick.YPos + deadzonePercent) / signedDeadZonePercent;
        } else {
            Joystick.YFinal = 0.0;
        }
    }

    private void Clip() {
        Joystick.XFinal = Math.Clamp(Joystick.XFinal, -1.0, 1.0);
        Joystick.YFinal = Math.Clamp(Joystick.YFinal, -1.0, 1.0);
    }

    private void WriteJoystickStatus() {
        if (!_configuration.Joystick) {
            return;
        }
        
        IsWriteActive = true;
        LastWrite = _machine.Timer.NumberOfTicks;

        if (!Joystick.IsConnected) {
            return;
        }

        TransformInput();
        Joystick.XCount = PercentToCount(Joystick.XFinal);
        Joystick.YCount = PercentToCount(Joystick.YFinal);
    }
    
    /// <inheritdoc />
    public override void WriteByte(int port, byte value) {
        LastWriteTime = DateTime.Now;
        switch (port) {
            case GetSetJoystickStatus:
                WriteJoystickStatus();
                break;
            default:
                base.WriteByte(port, value);
                break;
        }
    }
}