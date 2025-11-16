namespace Spice86.ViewModels;

using System;

/// <summary>
/// Defines the contract for IEmulatorObjectViewModel.
/// </summary>
public interface IEmulatorObjectViewModel {
    /// <summary>
    /// Gets or sets the IsVisible.
    /// </summary>
    public bool IsVisible { get; set; }
    /// <summary>
    /// UpdateValues method.
    /// </summary>
    public void UpdateValues(object? sender, EventArgs e);
}