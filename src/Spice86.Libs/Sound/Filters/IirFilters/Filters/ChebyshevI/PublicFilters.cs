namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.ChebyshevI;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents low pass.
/// </summary>
public class LowPass<TState> : ChebyshevILowPassBase<TState>
    where TState : struct, ISectionState {
    private readonly int _maxOrder;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="Constants.DefaultFilterOrder">The constants. default filter order.</param>
    protected LowPass(int maxOrder = Constants.DefaultFilterOrder)
        : base(maxOrder) {
        _maxOrder = maxOrder;
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double rippleDb) {
        base.Setup(order, cutoffFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(double cutoffFrequency, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(int order, double cutoffFrequency, double rippleDb) {
        base.Setup(order, cutoffFrequency, rippleDb);
    }
}

/// <summary>
/// Represents low pass.
/// </summary>
public sealed class LowPass(int maxOrder = Constants.DefaultFilterOrder) : LowPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents high pass.
/// </summary>
public class HighPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIHighPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double rippleDb) {
        base.Setup(order, cutoffFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(double cutoffFrequency, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(int order, double cutoffFrequency, double rippleDb) {
        base.Setup(order, cutoffFrequency, rippleDb);
    }
}

/// <summary>
/// Represents high pass.
/// </summary>
public sealed class HighPass(int maxOrder = Constants.DefaultFilterOrder) : HighPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band pass.
/// </summary>
public class BandPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIBandPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(order, centerFrequency, widthFrequency, rippleDb);
    }
}

/// <summary>
/// Represents band pass.
/// </summary>
public sealed class BandPass(int maxOrder = Constants.DefaultFilterOrder) : BandPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band stop.
/// </summary>
public class BandStop<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIBandStopBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(order, centerFrequency, widthFrequency, rippleDb);
    }
}

/// <summary>
/// Represents band stop.
/// </summary>
public sealed class BandStop(int maxOrder = Constants.DefaultFilterOrder) : BandStop<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents low shelf.
/// </summary>
public class LowShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevILowShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(order, cutoffFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(int order, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(order, cutoffFrequency, gainDb, rippleDb);
    }
}

/// <summary>
/// Represents low shelf.
/// </summary>
public sealed class LowShelf(int maxOrder = Constants.DefaultFilterOrder) : LowShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents high shelf.
/// </summary>
public class HighShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIHighShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(order, cutoffFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(int order, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(order, cutoffFrequency, gainDb, rippleDb);
    }
}

/// <summary>
/// Represents high shelf.
/// </summary>
public sealed class HighShelf(int maxOrder = Constants.DefaultFilterOrder) : HighShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band shelf.
/// </summary>
public class BandShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIBandShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double gainDb,
        double rippleDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double gainDb,
        double rippleDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(double centerFrequency, double widthFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, gainDb, rippleDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double gainDb, double rippleDb) {
        base.Setup(order, centerFrequency, widthFrequency, gainDb, rippleDb);
    }
}

/// <summary>
/// Represents band shelf.
/// </summary>
public sealed class BandShelf(int maxOrder = Constants.DefaultFilterOrder) : BandShelf<DirectFormIiState>(maxOrder);