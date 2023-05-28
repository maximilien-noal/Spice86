namespace Spice86.Shared.Emulator.Joystick;

/// <summary>
/// Represents a joystick
/// </summary>
public class Joystick {
    /// <summary>
    /// Gets or sets whether the X and Y values have been transformed already.
    /// </summary>
    public bool IsTransformed { get; set; }
    
    /// <summary>
    /// Gets or sets whether the Joystick is connected to the game port.
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// The position of the stick on the X axis.
    /// </summary>
    public double XPos { get; set; } = 0.0;
    
    /// <summary>
    /// The position of the stick on the Y axis.
    /// </summary>
    public double YPos { get; set; } = 0.0;

    /// <summary>
    /// The ???
    /// </summary>
    public double XTick { get; set; } = 0.0;
    
    /// <summary>
    /// The ???
    /// </summary>
    public double YTick { get; set; } = 0.0;

    /// <summary>
    /// The transformed value of the stick for the X axis.
    /// </summary>
    public double XFinal { get; set; } = 0.0;
    
    /// <summary>
    /// The transformed value of the stick for the Y axis.
    /// </summary>
    public double YFinal { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the remaining number of times the X axis value must be returned on read.
    /// </summary>
    public uint  XCount { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the remaining number of times the Y axis value must be returned on read.
    /// </summary>
    public uint YCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the threshold below which a X/Y axis is considered to be a movement. Interpreted as percentage.
    /// <remarks>100 means a digital joystick.</remarks>
    /// </summary>
    public int DeadZone { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether button A is down.
    /// </summary>
    public bool ButtonA { get; set; }
    
    /// <summary>
    /// Gets or sets whether button B is down.
    /// </summary>
    public bool ButtonB { get; set; }

    /// <summary>
    /// Gets or sets whether XPos, YPos have been converted to XFinal and YFinal
    /// </summary>
    public bool Transformed { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the joystick state must be returned on read.
    /// <remarks>Cleared when new XPos YPos have been set.</remarks>
    /// </summary>
    public bool IsEnabled { get; set; } = false;
}
