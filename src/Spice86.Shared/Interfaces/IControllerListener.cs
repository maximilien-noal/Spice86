namespace Spice86.Shared.Interfaces; 

public interface IControllerListener {
    /// <summary>
    /// TODO: Use Silk .NET to get controller data, or the GUI if using the future on-screen controller UI.
    /// TODO: Translate Silk .NET data to a high level internal record.
    /// </summary>
    /// <returns></returns>
    JoystickStatus GetControllerData();
}