namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.ChebyshevII;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.Layout;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;
using Spice86.Libs.Sound.Filters.IirFilters.Common.Transforms;

/// <summary>
/// Represents chebyshev ii filter base.
/// </summary>
public abstract class ChebyshevIiFilterBase<TAnalog, TState> : PoleFilterBase<TAnalog, TState>
    where TAnalog : LayoutBase
    where TState : struct, ISectionState {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="maxOrder">The max order.</param>
    /// <param name="maxDigitalPoles">The max digital poles.</param>
    /// <param name="analog">The analog.</param>
    protected ChebyshevIiFilterBase(int maxOrder, int maxDigitalPoles, TAnalog analog)
        : base(maxOrder, maxDigitalPoles, analog) {
        MaxOrder = maxOrder;
    }

    /// <summary>
    /// Gets max order.
    /// </summary>
    protected int MaxOrder { get; }

    /// <summary>
    /// Validates order.
    /// </summary>
    /// <param name="order">The order.</param>
    protected void ValidateOrder(int order) {
        if (order > MaxOrder) {
            throw new ArgumentException(Constants.OrderTooHigh);
        }
    }
}

/// <summary>
/// Represents chebyshev ii low pass base.
/// </summary>
public abstract class ChebyshevIiLowPassBase<TState> : ChebyshevIiFilterBase<AnalogLowPass, TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Performs the chebyshev ii low pass base operation.
    /// </summary>
    /// <param name="maxOrder">The max order.</param>
    protected ChebyshevIiLowPassBase(int maxOrder)
        : base(maxOrder, maxOrder, new AnalogLowPass()) {
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    protected void Setup(int order, double normalizedCutoff, double stopBandDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, stopBandDb);
        _ = new LowPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshev ii high pass base.
/// </summary>
public abstract class ChebyshevIiHighPassBase<TState> : ChebyshevIiFilterBase<AnalogLowPass, TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Performs the chebyshev ii high pass base operation.
    /// </summary>
    /// <param name="maxOrder">The max order.</param>
    protected ChebyshevIiHighPassBase(int maxOrder)
        : base(maxOrder, maxOrder, new AnalogLowPass()) {
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    protected void Setup(int order, double normalizedCutoff, double stopBandDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, stopBandDb);
        _ = new HighPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshev ii band pass base.
/// </summary>
public abstract class ChebyshevIiBandPassBase<TState> : ChebyshevIiFilterBase<AnalogLowPass, TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Performs the chebyshev ii band pass base operation.
    /// </summary>
    /// <param name="maxOrder">The max order.</param>
    protected ChebyshevIiBandPassBase(int maxOrder)
        : base(maxOrder, maxOrder * 2, new AnalogLowPass()) {
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency, double stopBandDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, stopBandDb);
        _ = new BandPassTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshev ii band stop base.
/// </summary>
public abstract class ChebyshevIiBandStopBase<TState> : ChebyshevIiFilterBase<AnalogLowPass, TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Performs the chebyshev ii band stop base operation.
    /// </summary>
    /// <param name="maxOrder">The max order.</param>
    protected ChebyshevIiBandStopBase(int maxOrder)
        : base(maxOrder, maxOrder * 2, new AnalogLowPass()) {
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency, double stopBandDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, stopBandDb);
        _ = new BandStopTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshev ii low shelf base.
/// </summary>
public abstract class ChebyshevIiLowShelfBase<TState> : ChebyshevIiFilterBase<AnalogLowShelf, TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Performs the chebyshev ii low shelf base operation.
    /// </summary>
    /// <param name="maxOrder">The max order.</param>
    protected ChebyshevIiLowShelfBase(int maxOrder)
        : base(maxOrder, maxOrder, new AnalogLowShelf()) {
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    protected void Setup(int order, double normalizedCutoff, double gainDb, double stopBandDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb, stopBandDb);
        _ = new LowPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshev ii high shelf base.
/// </summary>
public abstract class ChebyshevIiHighShelfBase<TState> : ChebyshevIiFilterBase<AnalogLowShelf, TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Performs the chebyshev ii high shelf base operation.
    /// </summary>
    /// <param name="maxOrder">The max order.</param>
    protected ChebyshevIiHighShelfBase(int maxOrder)
        : base(maxOrder, maxOrder, new AnalogLowShelf()) {
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    protected void Setup(int order, double normalizedCutoff, double gainDb, double stopBandDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb, stopBandDb);
        _ = new HighPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshev ii band shelf base.
/// </summary>
public abstract class ChebyshevIiBandShelfBase<TState> : ChebyshevIiFilterBase<AnalogLowShelf, TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Performs the chebyshev ii band shelf base operation.
    /// </summary>
    /// <param name="maxOrder">The max order.</param>
    protected ChebyshevIiBandShelfBase(int maxOrder)
        : base(maxOrder, maxOrder * 2, new AnalogLowShelf()) {
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="stopBandDb">The stop band db.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency, double gainDb, double stopBandDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb, stopBandDb);
        _ = new BandPassTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        DigitalPrototype.SetNormal(centerFrequency < 0.25 ? MathEx.DoublePi : 0.0, 1.0);
        SetLayout(DigitalPrototype);
    }
}