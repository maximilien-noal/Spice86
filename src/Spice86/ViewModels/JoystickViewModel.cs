namespace Spice86.ViewModels;

using Avalonia.Controls;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.Core.Emulator.Devices.Input.Joystick;
using Spice86.Core.Emulator.VM;

/// <summary>
/// Emulates an on screen digital joystick
/// </summary>
public partial class JoystickViewModel : ObservableObject {
    private readonly Machine? _machine;
    private readonly DispatcherTimer? _timer;

    private const int ButtonUpTimeout = 200;

    [ObservableProperty]
    private DateTimeOffset _lastUpdate;

    [ObservableProperty]
    private DateTimeOffset? _lastGamePortReadTime;

    [ObservableProperty]
    private DateTimeOffset? _lastGamePortWriteTime;

    public JoystickViewModel() {
        if (!Design.IsDesignMode) {
            throw new NotSupportedException("This constructor is not for runtime usage !");
        }
    }

    [ObservableProperty]
    private byte? _lastGamePortReadValue;

    [RelayCommand]
    public void UpdateData() {
        LastGamePortReadValue = _machine?.GamePort.LastReadValue;
        LastGamePortReadTime = _machine?.GamePort.LastReadTime;
        LastGamePortWriteTime = _machine?.GamePort.LastWriteTime;
        LastUpdate = DateTimeOffset.Now;
    }

    public JoystickViewModel(Machine machine) {
        _machine = machine;
        _timer = new(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, (_, _) => UpdateData());
        _timer.Start();
        machine.GamePort.ConnectDigitalJoystick();
    }

    [RelayCommand]
    public async Task Up() {
        if (_machine is null) {
            return;
        }
        _machine.GamePort.IsWriteActive = true;
        _machine.GamePort.SetY(1.0);
        UpdateData();
        await Task.Delay(ButtonUpTimeout);
        _machine.GamePort.Joystick.YFinal = 0;
        _machine.GamePort.Joystick.YCount = 0;
        _machine.GamePort.SetY(0.0);
        UpdateData();
    }

    [RelayCommand]
    public async Task Down() {
        if (_machine is null) {
            return;
        }
        _machine.GamePort.IsWriteActive = true;
        _machine.GamePort.SetY(-1.0);
        UpdateData();
        await Task.Delay(ButtonUpTimeout);
        _machine.GamePort.Joystick.YFinal = 0;
        _machine.GamePort.Joystick.YCount = 0;
        _machine.GamePort.SetY(0.0);
        UpdateData();
    }

    [RelayCommand]
    public async Task Left() {
        if (_machine is null) {
            return;
        }
        _machine.GamePort.IsWriteActive = true;
        _machine.GamePort.SetX(1.0);
        UpdateData();
        await Task.Delay(ButtonUpTimeout);
        _machine.GamePort.Joystick.XFinal = 0;
        _machine.GamePort.Joystick.XCount = 0;
        _machine.GamePort.SetX(0.0);
        UpdateData();
    }

    [RelayCommand]
    public async Task Right() {
        if (_machine is null) {
            return;
        }
        _machine.GamePort.IsWriteActive = true;
        _machine.GamePort.SetX(-1.0);
        UpdateData();
        await Task.Delay(ButtonUpTimeout);
        _machine.GamePort.Joystick.XFinal = 0;
        _machine.GamePort.Joystick.XCount = 0;
        _machine.GamePort.SetX(0.0);
        UpdateData();
    }

    [RelayCommand]
    public async Task Button1() {
        if (_machine is null) {
            return;
        }
        _machine.GamePort.Joystick.ButtonA = true;
        UpdateData();
        await Task.Delay(ButtonUpTimeout);
        _machine.GamePort.Joystick.ButtonA = false;
        UpdateData();
    }
    
    [RelayCommand]
    public async Task Button2() {
        if (_machine is null) {
            return;
        }
        _machine.GamePort.Joystick.ButtonB = true;
        UpdateData();
        await Task.Delay(ButtonUpTimeout);
        _machine.GamePort.Joystick.ButtonB = false;
        UpdateData();
    }
}