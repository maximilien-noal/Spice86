namespace Spice86.Views;

using Avalonia.Controls;

/// <summary>
/// The partial.
/// </summary>
public sealed partial class DebugWindow : Window {
    public DebugWindow() {
        InitializeComponent();
    }

    public DebugWindow(WindowBase owner) : this() {
        //Owner property has a protected setter, so we need to set it in the constructor
        Owner = owner;
    }
}