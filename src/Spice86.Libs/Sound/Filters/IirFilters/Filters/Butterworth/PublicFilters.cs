namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.Butterworth;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents low pass.
/// </summary>
public class LowPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ButterworthLowPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void Setup(double sampleRate, double cutoffFrequency) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency) {
        ValidateOrder(order);
        base.Setup(order, cutoffFrequency / sampleRate);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void SetupN(double cutoffFrequency) {
        base.Setup(_maxOrder, cutoffFrequency);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void SetupN(int order, double cutoffFrequency) {
        ValidateOrder(order);
        base.Setup(order, cutoffFrequency);
    }
}

/// <summary>
/// Represents low pass.
/// </summary>
public sealed class LowPass(int maxOrder = Constants.DefaultFilterOrder) : LowPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents high pass.
/// </summary>
public class HighPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ButterworthHighPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void Setup(double sampleRate, double cutoffFrequency) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency) {
        ValidateOrder(order);
        base.Setup(order, cutoffFrequency / sampleRate);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void SetupN(double cutoffFrequency) {
        base.Setup(_maxOrder, cutoffFrequency);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void SetupN(int order, double cutoffFrequency) {
        ValidateOrder(order);
        base.Setup(order, cutoffFrequency);
    }
}

/// <summary>
/// Represents high pass.
/// </summary>
public sealed class HighPass(int maxOrder = Constants.DefaultFilterOrder) : HighPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band pass.
/// </summary>
public class BandPass<TState>(int maxOrder = Constants.DefaultFilterOrder) : ButterworthBandPassBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency) {
        ValidateOrder(order);
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    public void SetupN(double centerFrequency, double widthFrequency) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency) {
        ValidateOrder(order);
        base.Setup(order, centerFrequency, widthFrequency);
    }
}

/// <summary>
/// Represents band pass.
/// </summary>
public sealed class BandPass(int maxOrder = Constants.DefaultFilterOrder) : BandPass<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band stop.
/// </summary>
public class BandStop<TState>(int maxOrder = Constants.DefaultFilterOrder) : ButterworthBandStopBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency) {
        ValidateOrder(order);
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    public void SetupN(double centerFrequency, double widthFrequency) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency) {
        ValidateOrder(order);
        base.Setup(order, centerFrequency, widthFrequency);
    }
}

/// <summary>
/// Represents band stop.
/// </summary>
public sealed class BandStop(int maxOrder = Constants.DefaultFilterOrder) : BandStop<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents low shelf.
/// </summary>
public class LowShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ButterworthLowShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb) {
        ValidateOrder(order);
        base.Setup(order, cutoffFrequency / sampleRate, gainDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void SetupN(double cutoffFrequency, double gainDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void SetupN(int order, double cutoffFrequency, double gainDb) {
        ValidateOrder(order);
        base.Setup(order, cutoffFrequency, gainDb);
    }
}

/// <summary>
/// Represents low shelf.
/// </summary>
public sealed class LowShelf(int maxOrder = Constants.DefaultFilterOrder) : LowShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents high shelf.
/// </summary>
public class HighShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ButterworthHighShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb) {
        base.Setup(_maxOrder, cutoffFrequency / sampleRate, gainDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void Setup(int order, double sampleRate, double cutoffFrequency, double gainDb) {
        ValidateOrder(order);
        base.Setup(order, cutoffFrequency / sampleRate, gainDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void SetupN(double cutoffFrequency, double gainDb) {
        base.Setup(_maxOrder, cutoffFrequency, gainDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void SetupN(int order, double cutoffFrequency, double gainDb) {
        ValidateOrder(order);
        base.Setup(order, cutoffFrequency, gainDb);
    }
}

/// <summary>
/// Represents high shelf.
/// </summary>
public sealed class HighShelf(int maxOrder = Constants.DefaultFilterOrder) : HighShelf<DirectFormIiState>(maxOrder);

/// <summary>
/// Represents band shelf.
/// </summary>
public class BandShelf<TState>(int maxOrder = Constants.DefaultFilterOrder) : ButterworthBandShelfBase<TState>(maxOrder)
    where TState : struct, ISectionState {
    private readonly int _maxOrder = maxOrder;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void Setup(double sampleRate, double centerFrequency, double widthFrequency, double gainDb) {
        base.Setup(_maxOrder, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void Setup(int order, double sampleRate, double centerFrequency, double widthFrequency, double gainDb) {
        ValidateOrder(order);
        base.Setup(order, centerFrequency / sampleRate, widthFrequency / sampleRate, gainDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void SetupN(double centerFrequency, double widthFrequency, double gainDb) {
        base.Setup(_maxOrder, centerFrequency, widthFrequency, gainDb);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void SetupN(int order, double centerFrequency, double widthFrequency, double gainDb) {
        ValidateOrder(order);
        base.Setup(order, centerFrequency, widthFrequency, gainDb);
    }
}

/// <summary>
/// Represents band shelf.
/// </summary>
public sealed class BandShelf(int maxOrder = Constants.DefaultFilterOrder) : BandShelf<DirectFormIiState>(maxOrder);