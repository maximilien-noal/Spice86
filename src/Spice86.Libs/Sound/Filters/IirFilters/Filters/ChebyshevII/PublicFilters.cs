namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.ChebyshevII;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents low pass.
/// </summary>
public class LowPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiLowPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double stopBandDb) {
        base.Setup(order, cutoffFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(double cutoffFrequency, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(int order, double cutoffFrequency, double stopBandDb) {
        base.Setup(order, cutoffFrequency, stopBandDb);
    }
}

/// <summary>
/// Represents low pass.
/// </summary>
public sealed class LowPass(int maxOrder = Constants.DefaultFilterOrder) : LowPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents high pass.
/// </summary>
public class HighPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiHighPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double stopBandDb) {
        base.Setup(order, cutoffFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(double cutoffFrequency, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(int order, double cutoffFrequency, double stopBandDb) {
        base.Setup(order, cutoffFrequency, stopBandDb);
    }
}

/// <summary>
/// Represents high pass.
/// </summary>
public sealed class HighPass(int maxOrder = Constants.DefaultFilterOrder) : HighPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band pass.
/// </summary>
public class BandPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiBandPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(order, centerFrequency, widthFrequency, stopBandDb);
    }
}

/// <summary>
/// Represents band pass.
/// </summary>
public sealed class BandPass(int maxOrder = Constants.DefaultFilterOrder) : BandPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band stop.
/// </summary>
public class BandStop<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiBandStopBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double stopBandDb) {
        base.Setup(order, centerFrequency, widthFrequency, stopBandDb);
    }
}

/// <summary>
/// Represents band stop.
/// </summary>
public sealed class BandStop(int maxOrder = Constants.DefaultFilterOrder) : BandStop<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents low shelf.
/// </summary>
public class LowShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiLowShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, cutoffFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(int order, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, cutoffFrequency, gainDb, stopBandDb);
    }
}

/// <summary>
/// Represents low shelf.
/// </summary>
public sealed class LowShelf(int maxOrder = Constants.DefaultFilterOrder) : LowShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents high shelf.
/// </summary>
public class HighShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiHighShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, cutoffFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(int order, double cutoffFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, cutoffFrequency, gainDb, stopBandDb);
    }
}

/// <summary>
/// Represents high shelf.
/// </summary>
public sealed class HighShelf(int maxOrder = Constants.DefaultFilterOrder) : HighShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band shelf.
/// </summary>
public class BandShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ChebyshevIiBandShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double gainDb,
        double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double gainDb,
        double stopBandDb) {
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(double centerFrequency, double widthFrequency, double gainDb, double stopBandDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, gainDb, stopBandDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double gainDb, double stopBandDb) {
        base.Setup(order, centerFrequency, widthFrequency, gainDb, stopBandDb);
    }
}

/// <summary>
/// Represents band shelf.
/// </summary>
public sealed class BandShelf(int maxOrder = Constants.DefaultFilterOrder) : BandShelf<DirectFormIiState>(maxOrder);