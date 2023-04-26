namespace Spice86.ViewModels;

using Avalonia.Controls;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.VM;

public class JoystickViewModel : ObservableObject {
    private Machine? _machine;

    public JoystickViewModel() {
        if (!Design.IsDesignMode) {
            throw new NotSupportedException("This constructor is not for runtime usage !");
        }
    }
    
    public JoystickViewModel(Machine machine) {
        _machine = machine;
    }
}