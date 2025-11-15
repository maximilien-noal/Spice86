using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

using Spice86.ViewModels;
using Spice86.ViewModels.Services;

namespace Spice86.Views;

/// <summary>
/// Represents video card view.
/// </summary>
public partial class VideoCardView : UserControl {
    private DispatcherTimer? _timer;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public VideoCardView() {
        InitializeComponent();
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        if (DataContext is IEmulatorObjectViewModel vm) {
            vm.IsVisible = false;
            _timer?.Stop();
            _timer = null;
        }
    }

    /// <summary>
    /// Called when data context changed.
    /// </summary>
    /// <param name="e">The e.</param>
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