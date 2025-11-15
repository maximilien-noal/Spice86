namespace Spice86.ViewModels;

using System;

/// <summary>
/// Defines the contract for i emulator object view model.
/// </summary>
public interface IEmulatorObjectViewModel {
    /// <summary>
    /// Gets or sets is visible.
    /// </summary>
    public bool IsVisible { get; set; }
    public void UpdateValues(object? sender, EventArgs e);
}