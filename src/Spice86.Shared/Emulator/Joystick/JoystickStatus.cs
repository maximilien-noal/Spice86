namespace Spice86.Shared.Emulator.Joystick;

public readonly record struct JoystickStatus(short AxisX, short AxisY, short DeltaX, short DeltaY, short Button1, short Button2, short Button3, short Button4);