namespace Spice86.ViewModels;

using Avalonia.Controls;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.Devices.Input.Joystick;
using Spice86.Shared.Emulator.Joystick;

public partial class JoystickViewModel : ObservableObject {
    private readonly Machine? _machine;
    private readonly DispatcherTimer? _timer;
    
    [ObservableProperty]
    private byte? _data;

    [ObservableProperty]
    private DateTimeOffset _lastUpdate;

    public JoystickViewModel() {
        if (!Design.IsDesignMode) {
            throw new NotSupportedException("This constructor is not for runtime usage !");
        }
    }

    [RelayCommand]
    public void UpdateData() {
        Data = _machine?.Joystick.GameportState.Value;
        LastUpdate = DateTimeOffset.Now;
    }

    private GameportState _gameportState = new();
    
    public JoystickViewModel(Machine machine) {
        _machine = machine;
        _timer = new(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, (_, _) => UpdateData());
        _timer.Start();
    }

    [RelayCommand]
    public void Up() {
        _gameportState.Joystick1XAxis = 0xFF;
        UpdateData();
        _machine?.Joystick.WriteByte(Joystick.GetSetJoystickStatus, _gameportState.Value);
    }

    [RelayCommand]
    public void Down() {
        _gameportState.Joystick1XAxis = 0;
        UpdateData();
        _machine?.Joystick.WriteByte(Joystick.GetSetJoystickStatus, _gameportState.Value);
    }

    [RelayCommand]
    public void Left() {
        _gameportState.Joystick1YAxis = 0;
        UpdateData();
        _machine?.Joystick.WriteByte(Joystick.GetSetJoystickStatus, _gameportState.Value);
    }

    [RelayCommand]
    public void Right() {
        _gameportState.Joystick1YAxis = 0xFF;
        UpdateData();
        _machine?.Joystick.WriteByte(Joystick.GetSetJoystickStatus, _gameportState.Value);
    }

    [RelayCommand]
    public void Button1() {
        _gameportState.Joystick1Button1 = true;
        UpdateData();
        _machine?.Joystick.WriteByte(Joystick.GetSetJoystickStatus, _gameportState.Value);
    }
    
    [RelayCommand]
    public void Button2() {
        _gameportState.Joystick1Button2 = true;
        UpdateData();
        _machine?.Joystick.WriteByte(Joystick.GetSetJoystickStatus, _gameportState.Value);
    }
}