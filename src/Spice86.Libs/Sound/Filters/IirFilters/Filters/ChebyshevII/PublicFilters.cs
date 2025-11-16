namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.ChebyshevII;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents the LowPass class.
/// </summary>
public class LowPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiLowPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double stopBandDb) {
        base.Setup(order, cutoffFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double cutoffFrequency, double stopBandDb) {
        base.Setup(order, cutoffFrequency, stopBandDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class LowPass(int maxOrder = Constants.DefaultFilterOrder) : LowPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the HighPass class.
/// </summary>
public class HighPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiHighPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double stopBandDb) {
        base.Setup(order, cutoffFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double cutoffFrequency, double stopBandDb) {
        base.Setup(order, cutoffFrequency, stopBandDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class HighPass(int maxOrder = Constants.DefaultFilterOrder) : HighPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the BandPass class.
/// </summary>
public class BandPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiBandPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(order, centerFrequency, widthFrequency, stopBandDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class BandPass(int maxOrder = Constants.DefaultFilterOrder) : BandPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the BandStop class.
/// </summary>
public class BandStop<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiBandStopBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(order, centerFrequency, widthFrequency, stopBandDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class BandStop(int maxOrder = Constants.DefaultFilterOrder) : BandStop<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the LowShelf class.
/// </summary>
public class LowShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiLowShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, cutoffFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, cutoffFrequency, gainDb, stopBandDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class LowShelf(int maxOrder = Constants.DefaultFilterOrder) : LowShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the HighShelf class.
/// </summary>
public class HighShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiHighShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, cutoffFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, cutoffFrequency, gainDb, stopBandDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class HighShelf(int maxOrder = Constants.DefaultFilterOrder) : HighShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents the BandShelf class.
/// </summary>
public class BandShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiBandShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double gainDb,
        double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double gainDb,
        double stopBandDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double widthFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, gainDb, stopBandDb);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, centerFrequency, widthFrequency, gainDb, stopBandDb);
    }
}

/// <summary>
/// class method.
/// </summary>
public sealed class BandShelf(int maxOrder = Constants.DefaultFilterOrder) : BandShelf<DirectFormIiState>(maxOrder);