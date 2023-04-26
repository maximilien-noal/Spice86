namespace Spice86.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.ApplicationLifetimes;

using Spice86.ViewModels;

public partial class JoystickWindow : Window {
    public JoystickWindow() {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
    }

    public JoystickWindow(JoystickViewModel joystickViewModel) {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            Owner = desktop.MainWindow;
        }
        DataContext = joystickViewModel;
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}