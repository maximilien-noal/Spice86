namespace Spice86.Views;

using Avalonia;
using Avalonia.Controls;

using Spice86.ViewModels;

/// <summary>
/// View for the Extended Memory Manager (XMS/HIMEM.SYS) state in the debugger.
/// </summary>
public partial class XmsView : UserControl {
    /// <summary>
    /// Initializes a new instance of the <see cref="XmsView"/> class.
    /// </summary>
    public XmsView() {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;
        this.DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        if (DataContext is IEmulatorObjectViewModel vm) {
            vm.IsVisible = true;
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        if (DataContext is IEmulatorObjectViewModel vm) {
            vm.IsVisible = false;
        }
    }
}
