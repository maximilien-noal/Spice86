namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.ChebyshevI;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents the LowPass class.
/// </summary>
public class LowPass<TState> : ChebyshevILowPassBase<TState>
    where TState : struct, ISectionState {
    private readonly int _maxOrder;

    protected LowPass(int maxOrder = Constants.DefaultFilterOrder)
        : base(maxOrder) {
        _maxOrder = maxOrder;
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double rippleDb) {
        base.Setup(order, cutoffFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double cutoffFrequency, double rippleDb) {
        base.Setup(order, cutoffFrequency, rippleDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class LowPass(int maxOrder = Constants.DefaultFilterOrder) : LowPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the HighPass class.
/// </summary>
public class HighPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIHighPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double rippleDb) {
        base.Setup(order, cutoffFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double cutoffFrequency, double rippleDb) {
        base.Setup(order, cutoffFrequency, rippleDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class HighPass(int maxOrder = Constants.DefaultFilterOrder) : HighPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the BandPass class.
/// </summary>
public class BandPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIBandPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(order, centerFrequency, widthFrequency, rippleDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class BandPass(int maxOrder = Constants.DefaultFilterOrder) : BandPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the BandStop class.
/// </summary>
public class BandStop<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIBandStopBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double rippleDb) {
        base.Setup(order, centerFrequency, widthFrequency, rippleDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class BandStop(int maxOrder = Constants.DefaultFilterOrder) : BandStop<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the LowShelf class.
/// </summary>
public class LowShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevILowShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(order, cutoffFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(order, cutoffFrequency, gainDb, rippleDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class LowShelf(int maxOrder = Constants.DefaultFilterOrder) : LowShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the HighShelf class.
/// </summary>
public class HighShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIHighShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(order, cutoffFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double cutoffFrequency, double gainDb, double rippleDb) {
        base.Setup(order, cutoffFrequency, gainDb, rippleDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class HighShelf(int maxOrder = Constants.DefaultFilterOrder) : HighShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the BandShelf class.
/// </summary>
public class BandShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIBandShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double gainDb,
        double rippleDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double gainDb,
        double rippleDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double widthFrequency, double gainDb, double rippleDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, gainDb, rippleDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double gainDb, double rippleDb) {
        base.Setup(order, centerFrequency, widthFrequency, gainDb, rippleDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class BandShelf(int maxOrder = Constants.DefaultFilterOrder) : BandShelf<DirectFormIiState>(maxOrder);