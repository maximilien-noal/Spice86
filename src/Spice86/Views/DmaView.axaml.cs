namespace Spice86.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

using Spice86.ViewModels;
using Spice86.ViewModels.Services;

/// <summary>
/// View for the DMA system state in the debugger.
/// </summary>
public partial class DmaView : UserControl {
    private DispatcherTimer? _timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DmaView"/> class.
    /// </summary>
    public DmaView() {
        InitializeComponent();
        this.DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        if (DataContext is IEmulatorObjectViewModel vm) {
            vm.IsVisible = false;
            _timer?.Stop();
            _timer = null;
        }
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e) {
        base.OnDataContextChanged(e);
        if (DataContext is IEmulatorObjectViewModel vm) {
            vm.IsVisible = this.IsVisible;
            _timer = DispatcherTimerStarter.StartNewDispatcherTimer(
                TimeSpan.FromMilliseconds(400), DispatcherPriority.Background,
                vm.UpdateValues);
        }
    }
}
