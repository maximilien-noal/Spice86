namespace Spice86.Core.Emulator.Devices.Timer;

using Spice86.Core.Emulator.Devices.ExternalInput;

/// <summary>
///     Deterministic time provider backed by the PIC scheduler for reproducible emulation.
/// </summary>
public sealed class DeterministicWallClock : IWallClock {
    private static readonly DateTime _epoch = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly DualPic _pic;

    /// <summary>
    ///     Initializes a new deterministic wall clock.
    /// </summary>
    /// <param name="pic">PIC scheduler providing cycle-based time.</param>
    public DeterministicWallClock(DualPic pic) {
        _pic = pic;
    }

    /// <summary>
    ///     Gets the current UTC timestamp based on PIC scheduler time.
    /// </summary>
    /// <remarks>
    ///     Returns a deterministic timestamp computed from the PIC's full index
    ///     (cycle-based time in milliseconds) added to a fixed epoch.
    /// </remarks>
    public DateTime UtcNow => _epoch.AddMilliseconds(_pic.GetFullIndex());
}
