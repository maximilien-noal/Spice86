namespace Spice86.Shared.Emulator.Joystick;

public class GameportState {
    public GameportState() {
        Value = DisconnectedJoystickValue;
    }
    
    public bool Joystick1Button1 {
        get => GetBit(0x04);
        set => SetBit(0x04, value);
    }

    public bool Joystick1Button2 {
        get => GetBit(0x08);
        set => SetBit(0x08, value);
    }

    public bool Joystick1XAxis {
        get => GetBit(0x01);
        set => SetBit(0x01, value);
    }

    public bool Joystick1YAxis {
        get => GetBit(0x02);
        set => SetBit(0x02, value);
    }
    
    private bool GetBit(int bitPosition) {
        return (Value & (1 << bitPosition)) != 0;
    }

    private void SetBit(int bitPosition, bool value) {
        if (value) {
            Value |= (byte)(1 << bitPosition);
        } else {
            Value &= (byte)~(1 << bitPosition);
        }
    }
    
    public byte Value { get; set; } = DisconnectedJoystickValue;

    public const byte DisconnectedJoystickValue = 0xFF;
}
