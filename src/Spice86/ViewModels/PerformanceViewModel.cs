namespace Spice86.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Interfaces;
using Spice86.ViewModels.Services;

using System;

/// <summary>
/// Represents performance view model.
/// </summary>
public partial class PerformanceViewModel : ViewModelBase {
    private readonly IPerformanceMeasureReader _cpuPerformanceReader;
    private readonly State _state;

    [ObservableProperty]
    private double _averageInstructionsPerSecond;

    private bool _isPaused;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="pauseHandler">The pause handler.</param>
    /// <param name="uiDispatcher">The ui dispatcher.</param>
    /// <param name="cpuPerfReader">The cpu perf reader.</param>
    public PerformanceViewModel(State state, IPauseHandler pauseHandler,
        IUIDispatcher uiDispatcher, IPerformanceMeasureReader cpuPerfReader) {
        _cpuPerformanceReader = cpuPerfReader;
        pauseHandler.Paused += () => uiDispatcher.Post(() => _isPaused = true);
        pauseHandler.Resumed += () => uiDispatcher.Post(() => _isPaused = false);
        _state = state;
        _isPaused = pauseHandler.IsPaused;
        DispatcherTimerStarter.StartNewDispatcherTimer(TimeSpan.FromSeconds(0.4),
            DispatcherPriority.Background, UpdatePerformanceInfo);
    }

    private void UpdatePerformanceInfo(object? sender, EventArgs e) {
        if (_isPaused) {
            return;
        }

        InstructionsExecuted = _state.Cycles;
        AverageInstructionsPerSecond = _cpuPerformanceReader.AverageValuePerSecond;
        InstructionsPerMillisecond = _cpuPerformanceReader.ValuePerMillisecond;
    }

    [ObservableProperty]
    private double _instructionsPerMillisecond;

    [ObservableProperty]
    private double _instructionsExecuted;
}