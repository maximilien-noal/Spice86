namespace Spice86.ViewModels;

using Avalonia.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.Devices.Input.Joystick;

public partial class JoystickViewModel : ObservableObject {
    private Machine? _machine;

    public JoystickViewModel() {
        if (!Design.IsDesignMode) {
            throw new NotSupportedException("This constructor is not for runtime usage !");
        }
    }
    
    public JoystickViewModel(Machine machine) {
        _machine = machine;
    }

    [RelayCommand]
    public void Up() {
        _machine?.Joystick.WriteByte(Joystick.JoystickPositionAndStatus, unchecked((byte) (Joystick.JoystickPositionAndStatus | 1)));
    }

    [RelayCommand]
    public void Down() {
        _machine?.Joystick.WriteByte(Joystick.JoystickPositionAndStatus, unchecked((byte) (Joystick.JoystickPositionAndStatus | 2)));
    }

    [RelayCommand]
    public void Left() {
        _machine?.Joystick.WriteByte(Joystick.JoystickPositionAndStatus, unchecked((byte) (Joystick.JoystickPositionAndStatus | 4)));
    }

    [RelayCommand]
    public void Right() {
        _machine?.Joystick.WriteByte(Joystick.JoystickPositionAndStatus, unchecked((byte) (Joystick.JoystickPositionAndStatus | 5)));
    }

    [RelayCommand]
    public void Button1() {
        _machine?.Joystick.WriteByte(Joystick.JoystickPositionAndStatus, unchecked((byte) (Joystick.JoystickPositionAndStatus | 6)));
    }
    
    [RelayCommand]
    public void Button2() {
        _machine?.Joystick.WriteByte(Joystick.JoystickPositionAndStatus, unchecked((byte) (Joystick.JoystickPositionAndStatus | 7)));
    }
    
    [RelayCommand]
    public void Button3() {
        _machine?.Joystick.WriteByte(Joystick.JoystickPositionAndStatus, unchecked((byte) (Joystick.JoystickPositionAndStatus | 8)));
    }
    
    [RelayCommand]
    public void Button4() {
        _machine?.Joystick.WriteByte(Joystick.JoystickPositionAndStatus, unchecked((byte) (Joystick.JoystickPositionAndStatus | 9)));
    }
}