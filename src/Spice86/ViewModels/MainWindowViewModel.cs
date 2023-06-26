﻿namespace Spice86.ViewModels;

using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;

using Avalonia;

using Serilog.Events;

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;

using Spice86.Views;
using Spice86.Core.CLI;
using Spice86.Core.Emulator;
using Spice86.Core.Emulator.Function.Dump;
using Spice86.Shared.Emulator.Keyboard;
using Spice86.Shared.Interfaces;
using Spice86.Wrappers;

using Key = Spice86.Shared.Emulator.Keyboard.Key;

/// <inheritdoc cref="Spice86.Shared.Interfaces.IGui" />
public sealed partial class MainWindowViewModel : ObservableObject, IGui, IDisposable {
    private readonly ILoggerService _loggerService;
    private readonly AvaloniaKeyScanCodeTranslator _avaloniaKeyScanCodeTranslator = new();
    [ObservableProperty]
    private Configuration _configuration = new();
    private bool _disposed;
    private Thread? _emulatorThread;
    private bool _isSettingResolution;
    private DebugWindow? _debugWindow;
    private PaletteWindow? _paletteWindow;
    private PerformanceWindow? _performanceWindow;
    private string _lastExecutableDirectory = string.Empty;
    private JoystickWindow? _joystickWindow;

    private bool _closeAppOnEmulatorExit;

    public bool PauseEmulatorOnStart { get; private set; }

    internal void OnKeyUp(KeyEventArgs e) => KeyUp?.Invoke(this, 
        new((Key) e.Key, 
            false,
            _avaloniaKeyScanCodeTranslator.GetKeyReleasedScancode((Key)e.Key),
            _avaloniaKeyScanCodeTranslator.GetAsciiCode(_avaloniaKeyScanCodeTranslator.GetKeyReleasedScancode((Key)e.Key))));

    private ProgramExecutor? _programExecutor;

    [ObservableProperty]
    private IVideoBufferViewModel? _videoBuffer;
    
    private ManualResetEvent _okayToContinueEvent = new(true);

    internal void OnKeyDown(KeyEventArgs e) => KeyDown?.Invoke(this, 
        new((Key) e.Key, 
            true,
            _avaloniaKeyScanCodeTranslator.GetKeyPressedScancode((Key)e.Key),
            _avaloniaKeyScanCodeTranslator.GetAsciiCode(_avaloniaKeyScanCodeTranslator.GetKeyPressedScancode((Key)e.Key))));

    [ObservableProperty]
    private string _statusMessage = "Emulator: not started.";

    [ObservableProperty]
    private string _asmOverrideStatus = "ASM Overrides: not used.";

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private AvaloniaList<FileInfo> _mostRecentlyUsed = new();

    public event EventHandler<KeyboardEventArgs>? KeyUp;
    public event EventHandler<KeyboardEventArgs>? KeyDown;

    private bool _isMainWindowClosing;

    public MainWindowViewModel(ILoggerService loggerService) {
        _loggerService = loggerService;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow.Closing += (_, _) => _isMainWindowClosing = true;
        }
    }

    public void PauseEmulationOnStart() {
        Pause();
        PauseEmulatorOnStart = false;
    }
    
    public void HideMouseCursor() {
        Dispatcher.UIThread.Post(() => {
            if (VideoBuffer is null) {
                return;
            }
            VideoBuffer.ShowCursor = false;
        });
    }

    public void ShowMouseCursor() {
        Dispatcher.UIThread.Post(() => {
            if (VideoBuffer is null) {
                return;
            }
            VideoBuffer.ShowCursor = true;
        });
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowPerformanceCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowJoystickCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowDebugWindowCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowColorPaletteCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    [NotifyCanExecuteChangedFor(nameof(DumpEmulatorStateToFileCommand))]
    private bool _isMachineRunning;

    [RelayCommand(CanExecute = nameof(IsMachineRunning))]
    public async Task DumpEmulatorStateToFile() {
        if (_programExecutor is null) {
            return;
        }
        OpenFolderDialog ofd = new OpenFolderDialog() {
            Title = "Dump emulator state to directory...",
            Directory = Configuration.RecordedDataDirectory
        };
        if (!Directory.Exists(Configuration.RecordedDataDirectory)) {
            ofd.Directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
        string? dir = Configuration.RecordedDataDirectory;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            dir = await ofd.ShowAsync(desktop.MainWindow);
        }
        if (string.IsNullOrWhiteSpace(dir)
        && !string.IsNullOrWhiteSpace(Configuration.RecordedDataDirectory)) {
            dir = Configuration.RecordedDataDirectory;
        }
        if (!string.IsNullOrWhiteSpace(dir)) {
            new RecorderDataWriter(dir, _programExecutor.Machine,
                _loggerService)
                    .DumpAll();
        }
    }

    [RelayCommand(CanExecute = nameof(IsMachineRunning))]
    public void ShowJoystick() {
        if (_joystickWindow != null) {
            _joystickWindow.Activate();
        } else if(_programExecutor is not null) {
            _joystickWindow = new JoystickWindow(new JoystickViewModel(_programExecutor.Machine));
            _joystickWindow.Closed += (_, _) => _joystickWindow = null;
            _joystickWindow.Show();
        }

    }

    [RelayCommand(CanExecute = nameof(IsMachineRunning))]
    public void Pause() {
        if (_emulatorThread is null) {
            return;
        }

        _okayToContinueEvent.Reset();
        IsPaused = true;
    }

    [RelayCommand(CanExecute = nameof(IsMachineRunning))]
    public void Play() {
        if (_emulatorThread is null) {
            return;
        }

        _okayToContinueEvent.Set();
        IsPaused = false;
    }

    public void SetConfiguration(string[] args) {
        Configuration = GenerateConfiguration(args);
        SetLogLevel(Configuration.SilencedLogs ? "Silent" : _loggerService.LogLevelSwitch.MinimumLevel.ToString());
        SetMainTitle();
    }

    private void SetMainTitle() {
        MainTitle = $"{nameof(Spice86)} {Configuration.Exe}";
    }

    [ObservableProperty]
    private string? _mainTitle;

    [RelayCommand]
    public async Task DebugExecutable() {
        _closeAppOnEmulatorExit = false;
        await StartNewExecutable();
        PauseEmulatorOnStart = true;
    }

    [RelayCommand]
    public async Task StartExecutable(string? filePath) {
        _closeAppOnEmulatorExit = false;
        await StartNewExecutable(filePath);
    }

    private async Task StartNewExecutable(string? filePath = null) {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            OpenFileDialog ofd = new() {
                Title = "Start Executable...",
                AllowMultiple = false,
                Filters = new(){
                    new FileDialogFilter() {
                        Extensions = {"exe", "com", "EXE", "COM" },
                        Name = "DOS Executables"
                    },
                    new FileDialogFilter() {
                        Extensions = { "*" },
                        Name = "All Files"
                    }
                }
            };
            ofd.Directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            if (Directory.Exists(_lastExecutableDirectory)) {
                ofd.Directory = _lastExecutableDirectory;
            }

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) {
                string[]? files = await ofd.ShowAsync(desktop.MainWindow);
                if (files?.Any() == true) {
                    filePath = files[0];
                    await RestartEmulatorWithNewProgram(filePath);
                }
            } else {
                await RestartEmulatorWithNewProgram(filePath);
            }
        }
    }

    private async Task RestartEmulatorWithNewProgram(string filePath) {
        Configuration.Exe = filePath;
        Configuration.ExeArgs = "";
        Configuration.CDrive = Path.GetDirectoryName(Configuration.Exe);
        Configuration.UseCodeOverride = false;
        Play();
        await Dispatcher.UIThread.InvokeAsync(() => DisposeEmulator(), DispatcherPriority.MaxValue);
        SetMainTitle();
        _okayToContinueEvent = new(true);
        _programExecutor?.Machine.ExitEmulationLoop();
        while (_emulatorThread?.IsAlive == true)
        {
            Dispatcher.UIThread.RunJobs();
        }

        IsMachineRunning = false;
        _closeAppOnEmulatorExit = false;
        RunEmulator();
    }

    private double _timeMultiplier = 1;

    public double TimeMultiplier {
        get => _timeMultiplier;
        set {
            SetProperty(ref _timeMultiplier, value);
            _programExecutor?.Machine.Timer.SetTimeMultiplier(_timeMultiplier);
        }
    }

    [RelayCommand(CanExecute = nameof(IsMachineRunning))]
    public async Task ShowPerformance() {
        if (_performanceWindow != null) {
            _performanceWindow.Activate();
        } else if (_programExecutor is not null) {
            _performanceWindow = new PerformanceWindow() {
                DataContext = new PerformanceViewModel(
                    _programExecutor.Machine, this)
            };
            _performanceWindow.Closed += (_, _) => _performanceWindow = null;
            _performanceWindow.Show();
        } else if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("", "Please start a program first")
                .ShowDialog(desktop.MainWindow);
        }
    }

    [RelayCommand(CanExecute = nameof(IsMachineRunning))]
    public void ShowDebugWindow() {
        if (_debugWindow != null) {
            _debugWindow.Activate();
        } else if(_programExecutor is not null) {
            _debugWindow = new DebugWindow(_programExecutor.Machine);
            _debugWindow.Closed += (_, _) => _debugWindow = null;
            _debugWindow.Show();
        }
    }

    [RelayCommand(CanExecute = nameof(IsMachineRunning))]
    public void ShowColorPalette() {
        if (_paletteWindow != null) {
            _paletteWindow.Activate();
        } else if(_programExecutor is not null) {
            _paletteWindow = new PaletteWindow(new PaletteViewModel(_programExecutor.Machine));
            _paletteWindow.Closed += (_, _) => _paletteWindow = null;
            _paletteWindow.Show();
        }
    }

    [RelayCommand]
    public void ResetTimeMultiplier() {
        TimeMultiplier = Configuration.TimeMultiplier;
    }
    
    public void UpdateScreen() {
        if (_disposed || _isSettingResolution || _isMainWindowClosing) {
            return;
        }
        VideoBuffer?.Draw();
    }

    public int Height { get; private set; }

    public int MouseX { get; set; }

    public int MouseY { get; set; }
    public int Width { get; private set; }

    public bool IsLeftButtonClicked { get; private set; }

    public bool IsRightButtonClicked { get; private set; }
    
    public void OnMainWindowOpened(object? sender, EventArgs e) {
        if(RunEmulator()) {
            _closeAppOnEmulatorExit = true;
        }
    }

    private void AddOrReplaceMostRecentlyUsed(string filePath) {
        if (MostRecentlyUsed.Any(x => x.FullName == filePath)) {
            return;
        }
        MostRecentlyUsed.Insert(0,new FileInfo(filePath));
        if (MostRecentlyUsed.Count > 3) {
            MostRecentlyUsed.RemoveAt(3);
        }
    }

    private bool RunEmulator() {
        if (string.IsNullOrWhiteSpace(Configuration.Exe) ||
            string.IsNullOrWhiteSpace(Configuration.CDrive)) {
            return false;
        }
        AddOrReplaceMostRecentlyUsed(Configuration.Exe);
        _lastExecutableDirectory = Configuration.CDrive;
        StatusMessage = "Emulator starting...";
        if (Configuration is {UseCodeOverrideOption: true, OverrideSupplier: not null}) {
            AsmOverrideStatus = $"ASM code overrides: enabled.";
        } else if(Configuration is {UseCodeOverride: false, OverrideSupplier: not null}) {
            AsmOverrideStatus = $"ASM code overrides: only functions names will be referenced.";
        } else {
            AsmOverrideStatus = $"ASM code overrides: none.";
        }
        RunMachine();
        return true;
    }

    public void OnMouseClick(PointerEventArgs @event, bool click) {
        if (@event.Pointer.IsPrimary) {
            IsLeftButtonClicked = click;
        }

        if (!@event.Pointer.IsPrimary) {
            IsRightButtonClicked = click;
        }
    }

    public void OnMouseMoved(PointerEventArgs @event, Image image) {
        MouseX = (int)@event.GetPosition(image).X;
        MouseY = (int)@event.GetPosition(image).Y;
    }
    
    public void SetResolution(int width, int height) {
        Dispatcher.UIThread.Post(() => {
            _isSettingResolution = true;
            DisposeVideoBuffer();
            Width = width;
            Height = height;
            IVideoBufferViewModel videoBuffer = new VideoBufferViewModel(_videoCard, scale:1, width, height);
            Dispatcher.UIThread.Post(() => {
                    VideoBuffer?.Dispose();
                    VideoBuffer = videoBuffer;
                }
            );
            _isSettingResolution = false;
        }, DispatcherPriority.MaxValue);
    }

    private void DisposeVideoBuffer() {
        VideoBuffer?.Dispose();
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                PlayCommand.Execute(null);
                IsMachineRunning = false;
                DisposeEmulator();
                _performanceWindow?.Close();
                _paletteWindow?.Close();
                _okayToContinueEvent.Set();
                if (_emulatorThread?.IsAlive == true) {
                    _emulatorThread.Join();
                }
                _okayToContinueEvent.Dispose();
            }
            _disposed = true;
        }
    }

    private void DisposeEmulator() {
        DisposeVideoBuffer();
        _programExecutor?.Dispose();
    }

    private static Configuration GenerateConfiguration(string[] args) {
        return CommandLineParser.ParseCommandLine(args);
    }

    private async Task ShowEmulationErrorMessage(Exception e) {
        IMsBoxWindow<ButtonResult> errorMessage = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow("An unhandled exception occured", 
                $"""
                Method name: {e.GetBaseException().TargetSite?.Name},
                Exception message: {e.GetBaseException().Message},
                Stack trace (first line): {e.GetBaseException().StackTrace?.Split(Environment.NewLine).FirstOrDefault()}
                """);
        if (!_disposed && !_isMainWindowClosing && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            await errorMessage.ShowDialog(desktop.MainWindow);
        }
    }

    [ObservableProperty]
    private string _currentLogLevel = "";

    [RelayCommand]
    public void SetLogLevel(string logLevel) {
        if (logLevel == "Silent") {
            CurrentLogLevel = logLevel;
            _loggerService.AreLogsSilenced = true;
            _loggerService.LogLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
        } else {
            _loggerService.AreLogsSilenced = false;
            _loggerService.LogLevelSwitch.MinimumLevel = Enum.Parse<LogEventLevel>(logLevel);
            CurrentLogLevel = logLevel;
        }
    }

    private void RunMachine() {
        _emulatorThread = new Thread(MachineThread) {
            Name = "Emulator"
        };
        _emulatorThread.Start();
    }

    // We use async void, but thankfully this doesn't generate an exception.
    // So this is OK...
    private async void OnEmulatorErrorOccured(Exception e) {
        await Dispatcher.UIThread.InvokeAsync(async () => await ShowEmulationErrorMessage(e));
    }

    private event Action<Exception>? EmulatorErrorOccured;

    [ObservableProperty]
    private bool _showVideo = true;

    private IVideoCard _videoCard = null!;

    private void MachineThread() {
        try {
            if (Debugger.IsAttached) {
                StartProgramExecutor();
            } else {
                try {
                    StartProgramExecutor();
                } catch (Exception e) {
                    e.Demystify();
                    if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                        _loggerService.Error(e, "An error occurred during execution");
                    }
                    EmulatorErrorOccured += OnEmulatorErrorOccured;
                    EmulatorErrorOccured?.Invoke(e);
                    EmulatorErrorOccured -= OnEmulatorErrorOccured;
                }
            }
        }  finally {
            Dispatcher.UIThread.Post(() => IsMachineRunning = false);
            Dispatcher.UIThread.Post(() => StatusMessage = "Emulator: stopped.");
            Dispatcher.UIThread.Post(() => AsmOverrideStatus = "");
        }
    }

    private void StartProgramExecutor() {
        if (!_disposed) {
            _okayToContinueEvent.Set();
        }

        _programExecutor = new ProgramExecutor(_loggerService, this, Configuration);
        TimeMultiplier = Configuration.TimeMultiplier;
        _videoCard = _programExecutor.Machine.VgaCard;
        Dispatcher.UIThread.Post(() => IsMachineRunning = true);
        Dispatcher.UIThread.Post(() => StatusMessage = "Emulator started.");
        Dispatcher.UIThread.Post(() => ShowVideo = true);
        _programExecutor.Run();
        if (_closeAppOnEmulatorExit &&
            Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime {
                MainWindow: MainWindow mainWindow
            }) {
            Dispatcher.UIThread.Post(() => mainWindow.Close());
        }
    }

    /// <inheritdoc />
    public void WaitForContinue() {
        _okayToContinueEvent.WaitOne(Timeout.Infinite);
    }
}