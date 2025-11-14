using Serilog.Events;
using Spice86.Shared.Interfaces;
using System.Diagnostics;

namespace Spice86.Core.Emulator.Devices.Timer;

/// <summary>
/// Event queue for PIT/PIC-related timing events, inspired by DOSBox Staging implementation.
/// Provides a simpler API specifically designed for device timing callbacks such as:
/// - Typematic keyboard repeat
/// - PS/2 controller delays
/// - Keyboard reset timing
/// </summary>
public sealed class PitPicEventQueue : ITimeMultiplier {
    private readonly ILoggerService _loggerService;
    private readonly Func<long> _nowTicks;
    private readonly long _ticksPerSecond;
    private readonly PriorityQueue<ScheduledEvent, long> _eventQueue = new();
    
    private bool _inEventService = false;
    private long _serviceBaseTicks = 0;
    private double _timeMultiplier = 1.0;

    /// <summary>
    /// Represents a scheduled event in the queue.
    /// </summary>
    public sealed record ScheduledEvent {
        public required string Name { get; init; }
        public required Action Handler { get; init; }
        public required long DueTicks { get; init; }
        public required long StartTicks { get; init; }
        public required double DelayMs { get; init; }
        public required double EffectiveDelayMs { get; init; }
        public bool Canceled { get; set; }
        public override string ToString() => $"{Name} (DueTicks: {DueTicks}, Canceled: {Canceled})";
    }

    /// <summary>
    /// Initializes a new instance of the PitPicEventQueue class.
    /// </summary>
    /// <param name="loggerService">Logger service for diagnostics.</param>
    public PitPicEventQueue(ILoggerService loggerService) {
        _loggerService = loggerService;
        _nowTicks = Stopwatch.GetTimestamp;
        _ticksPerSecond = Stopwatch.Frequency;
    }

    /// <summary>
    /// Sets the time multiplier for all scheduled events.
    /// </summary>
    /// <param name="multiplier">Time multiplier (must be > 0).</param>
    public void SetTimeMultiplier(double multiplier) {
        if (multiplier <= 0) {
            throw new DivideByZeroException(nameof(multiplier));
        }
        _timeMultiplier = multiplier;
    }

    /// <summary>
    /// Schedules an event to fire after the specified delay in milliseconds.
    /// </summary>
    /// <param name="name">Name of the event for debugging.</param>
    /// <param name="delayMs">Delay in milliseconds before the event fires.</param>
    /// <param name="handler">Action to execute when the event fires.</param>
    /// <returns>The scheduled event object.</returns>
    public ScheduledEvent AddEvent(string name, double delayMs, Action handler) {
        long baseTicks;
        if (_inEventService) {
            var now = _nowTicks();
            if (now > _serviceBaseTicks) {
                _serviceBaseTicks = now;
            }
            baseTicks = _serviceBaseTicks;
        } else {
            baseTicks = _nowTicks();
            _serviceBaseTicks = baseTicks;
        }

        double effectiveDelayMs = delayMs / _timeMultiplier;
        long dueTicks = baseTicks + MsToTicks(effectiveDelayMs);

        var ev = new ScheduledEvent {
            Name = name,
            Handler = handler,
            DueTicks = dueTicks,
            StartTicks = baseTicks,
            DelayMs = delayMs,
            EffectiveDelayMs = effectiveDelayMs,
            Canceled = false
        };
        _eventQueue.Enqueue(ev, ev.DueTicks);
        return ev;
    }

    /// <summary>
    /// Cancels a specific event.
    /// </summary>
    /// <param name="event">The event to cancel.</param>
    public void RemoveEvent(ScheduledEvent @event) {
        if (@event != null) {
            @event.Canceled = true;
        }
    }

    /// <summary>
    /// Cancels all events matching the given handler.
    /// </summary>
    /// <param name="handler">The handler to match.</param>
    /// <returns>Number of events canceled.</returns>
    public int RemoveEvents(Action handler) {
        int count = 0;
        foreach ((ScheduledEvent? e, long _) in _eventQueue.UnorderedItems) {
            if (!e.Canceled && e.Handler == handler) {
                e.Canceled = true;
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Processes all due events in the queue.
    /// Should be called regularly from the main emulation loop.
    /// </summary>
    public void RunQueue() {
        long now = _nowTicks();
        _inEventService = true;
        if (_serviceBaseTicks == 0) {
            _serviceBaseTicks = now;
        }

        while (_eventQueue.TryPeek(out ScheduledEvent? ev, out var due)) {
            if (due > now) {
                break; // not yet due
            }

            _eventQueue.Dequeue();
            if (!ev.Canceled) {
                try {
                    _serviceBaseTicks = now;
                    ev.Handler();
                } catch (Exception ex) {
                    if (_loggerService.IsEnabled(LogEventLevel.Warning)) {
                        _loggerService.Warning("PitPic event '{Name}' failed: {Error}", ev.Name, ex.Message);
                    }
                    throw;
                }
                now = _nowTicks();
            }
        }
        _inEventService = false;
    }

    /// <summary>
    /// Checks if there are any pending events in the queue.
    /// </summary>
    /// <returns>True if there are pending events.</returns>
    public bool HasPendingEvents() {
        return _eventQueue.Count > 0;
    }

    /// <summary>
    /// Gets the number of pending events in the queue.
    /// </summary>
    public int PendingEventCount => _eventQueue.Count;

    private long MsToTicks(double ms) => (long)Math.Ceiling(ms * _ticksPerSecond / 1000.0);
}
