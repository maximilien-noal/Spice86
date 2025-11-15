namespace Spice86.Views;

using Avalonia.Controls;

/// <summary>
/// Represents debug window.
/// </summary>
public sealed partial class DebugWindow : Window {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public DebugWindow() {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    public DebugWindow(WindowBase owner) : this() {
        //Owner property has a protected setter, so we need to set it in the constructor
        Owner = owner;
    }
}