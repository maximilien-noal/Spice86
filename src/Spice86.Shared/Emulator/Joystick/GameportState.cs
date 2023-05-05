namespace Spice86.Shared.Emulator.Joystick;

public class GameportState {
    public void Reinitialize() {
        Joystick1Button1 = false;
        Joystick1Button2 = false;
        Joystick2Button1 = false;
        Joystick2Button2 = false;
        Joystick1XAxis = 127;
        Joystick1YAxis = 127;
        Joystick2XAxis = 127;
        Joystick2YAxis = 127;
    }
    public bool Joystick1Button1
    {
        get => (Value & 0x01) == 0x01;
        set => Value = (byte)((Value & ~0x01) | (value ? 0x01 : 0x00));
    }

    public bool Joystick1Button2
    {
        get => (Value & 0x02) == 0x02;
        set => Value = (byte)((Value & ~0x02) | (value ? 0x02 : 0x00));
    }

    public bool Joystick2Button1
    {
        get => (Value & 0x04) == 0x04;
        set => Value = (byte)((Value & ~0x04) | (value ? 0x04 : 0x00));
    }

    public bool Joystick2Button2
    {
        get => (Value & 0x08) == 0x08;
        set => Value = (byte)((Value & ~0x08) | (value ? 0x08 : 0x00));
    }

    public byte Joystick1XAxis
    {
        get => Value;
        set => Value = value;
    }

    public byte Joystick1YAxis
    {
        get => Value;
        set => Value = value;
    }

    public byte Joystick2XAxis
    {
        get => (byte)((Value >> 4) | 0x0F);
        set => Value = (byte)((Value & 0x0F) | (value << 4));
    }

    public byte Joystick2YAxis
    {
        get => (byte)((Value >> 4) | 0x0F);
        set => Value = (byte)((Value & 0xF0) | value);
    }
    public byte Value { get; set; } = DisconnectedJoystickValue;

    public const byte DisconnectedJoystickValue = 0xFF;
}
