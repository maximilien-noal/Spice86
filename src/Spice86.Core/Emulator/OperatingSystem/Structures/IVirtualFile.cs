namespace Spice86.Core.Emulator.OperatingSystem.Structures;
/// <summary>
/// Defines the contract for i virtual file.
/// </summary>
public interface IVirtualFile {
    /// <summary>
    /// The DOS file name of the file or device.
    /// </summary>
    public string Name { get; set; }
}