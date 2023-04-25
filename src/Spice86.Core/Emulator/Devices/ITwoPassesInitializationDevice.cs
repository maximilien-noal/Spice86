namespace Spice86.Core.Emulator.Devices;

/// <summary>
/// Defines a device for which <see cref="FinishDeviceInitialization"/> must be called right after registration.
/// </summary>
public interface ITwoPassesInitializationDevice {
    /// <summary>
    /// Calls the handler's final init code after it has been registered. Default interface member.
    /// </summary>
    void FinishDeviceInitialization() {
    }
}