namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Emulator.VM.Breakpoint;

/// <summary>
/// Represents breakpoint view model.
/// </summary>
public partial class BreakpointViewModel : ViewModelBase {
    private readonly List<BreakPoint> _breakpoints = new List<BreakPoint>();
    private readonly Action _onReached;
    private readonly EmulatorBreakpointsManager _emulatorBreakpointsManager;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="breakpointsViewModel">The breakpoints view model.</param>
    /// <param name="emulatorBreakpointsManager">The emulator breakpoints manager.</param>
    /// <param name="trigger">The trigger.</param>
    /// <param name="endAddress">The end address.</param>
    /// <param name="type">The type.</param>
    /// <param name="isRemovedOnTrigger">The is removed on trigger.</param>
    /// <param name="onReached">The on reached.</param>
    /// <param name="additionalTriggerCondition">The additional trigger condition.</param>
    /// <param name="comment">The comment.</param>
    public BreakpointViewModel(
        BreakpointsViewModel breakpointsViewModel,
        EmulatorBreakpointsManager emulatorBreakpointsManager,
        long trigger,
        long endAddress,
        BreakPointType type,
        bool isRemovedOnTrigger,
        Action onReached,
        Func<long, bool>? additionalTriggerCondition,
        string comment = "") {
        _emulatorBreakpointsManager = emulatorBreakpointsManager;
        Address = trigger;
        Type = type;
        IsRemovedOnTrigger = isRemovedOnTrigger;
        if (IsRemovedOnTrigger) {
            _onReached = () => {
                breakpointsViewModel.RemoveBreakpointInternal(this);
                onReached();
            };
        } else {
            _onReached = onReached;
        }
        Comment = comment;
        Parameter = $"0x{trigger:X2}";
        EndAddress = endAddress;
        for (long i = Address; i <= EndAddress; i++) {
            AddressBreakPoint breakpoint = CreateBreakpointWithAddressAndCondition(i, additionalTriggerCondition);
            breakpoint.IsEnabled = true;
            _emulatorBreakpointsManager.ToggleBreakPoint(breakpoint, on: breakpoint.IsEnabled);
            _breakpoints.Add(breakpoint);
        }
        _isEnabled = true;
    }

    [ObservableProperty]
    private string _parameter;

    /// <summary>
    /// Gets segmented address.
    /// </summary>
    public SegmentedAddress? SegmentedAddress { get; }

    /// <summary>
    /// The on reached.
    /// </summary>
    public Action OnReached => _onReached;

    /// <summary>
    /// Gets type.
    /// </summary>
    public BreakPointType Type { get; }

    private bool _isEnabled;

    public bool IsEnabled {
        get => _isEnabled;
        set {
            if (value) {
                Enable();
            } else {
                Disable();
            }
            SetProperty(ref _isEnabled, value);
        }
    }

    /// <summary>
    /// Gets is removed on trigger.
    /// </summary>
    public bool IsRemovedOnTrigger { get; }

    /// <summary>
    /// Gets address.
    /// </summary>
    public long Address { get; }

    /// <summary>
    /// Gets end address.
    /// </summary>
    public long EndAddress { get; }

    /// <summary>
    /// Converts toggle.
    /// </summary>
    public void Toggle() {
        if (IsEnabled) {
            Disable();
        } else {
            Enable();
        }
    }

    [ObservableProperty]
    private string? _comment;

    private BreakPoint GetOrCreateBreakpoint(Func<long, bool>? additionalTriggerCondition) {
        AddressBreakPoint breakPoint = CreateBreakpointWithAddressAndCondition(Address, additionalTriggerCondition);
        breakPoint.IsUserBreakpoint = true;
        return breakPoint;
    }

    /// <summary>
    /// Creates breakpoint with address and condition.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="additionalTriggerCondition">The additional trigger condition.</param>
    /// <returns>The result of the operation.</returns>
    protected AddressBreakPoint CreateBreakpointWithAddressAndCondition(long address, Func<long, bool>? additionalTriggerCondition) {
        AddressBreakPoint bp = new AddressBreakPoint(Type, address, _ => _onReached(), IsRemovedOnTrigger, additionalTriggerCondition);
        bp.IsUserBreakpoint = true;
        return bp;
    }

    /// <summary>
    /// Performs the enable operation.
    /// </summary>
    [RelayCommand]
    public void Enable() {
        if (IsEnabled) {
            return;
        }
        foreach (BreakPoint breakpoint in _breakpoints) {
            breakpoint.IsEnabled = true;
        }
        _isEnabled = true;
        OnPropertyChanged(nameof(IsEnabled));
    }

    /// <summary>
    /// Performs the disable operation.
    /// </summary>
    [RelayCommand]
    public void Disable() {
        if (!IsEnabled) {
            return;
        }
        foreach (BreakPoint breakpoint in _breakpoints) {
            breakpoint.IsEnabled = false;
        }
        _isEnabled = false;

        OnPropertyChanged(nameof(IsEnabled));
    }

    /// <summary>
    /// Deletes .
    /// </summary>
    internal void Delete() {
        foreach (BreakPoint breakpoint in _breakpoints) {
            _emulatorBreakpointsManager.RemoveUserBreakpoint(breakpoint);
        }
    }
}