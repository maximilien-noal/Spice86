namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.ChebyshevI;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.Layout;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;
using Spice86.Libs.Sound.Filters.IirFilters.Common.Transforms;

/// <summary>
/// Represents chebyshevi filter base.
/// </summary>
public abstract class ChebyshevIFilterBase<TAnalog, TState>(int maxOrder, int maxDigitalPoles, TAnalog analog)
    : PoleFilterBase<TAnalog, TState>(maxOrder, maxDigitalPoles, analog)
    where TAnalog : LayoutBase
    where TState : struct, ISectionState {
    /// <summary>
    /// Gets max order.
    /// </summary>
    protected int MaxOrder { get; } = maxOrder;

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
/// Represents chebyshevi low pass base.
/// </summary>
public abstract class ChebyshevILowPassBase<TState>(int maxOrder)
    : ChebyshevIFilterBase<AnalogLowPass, TState>(maxOrder, maxOrder, new AnalogLowPass())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="rippleDb">The ripple db.</param>
    protected void Setup(int order, double normalizedCutoff, double rippleDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, rippleDb);
        _ = new LowPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshevi high pass base.
/// </summary>
public abstract class ChebyshevIHighPassBase<TState>(int maxOrder)
    : ChebyshevIFilterBase<AnalogLowPass, TState>(maxOrder, maxOrder, new AnalogLowPass())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="rippleDb">The ripple db.</param>
    protected void Setup(int order, double normalizedCutoff, double rippleDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, rippleDb);
        _ = new HighPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshevi band pass base.
/// </summary>
public abstract class ChebyshevIBandPassBase<TState>(int maxOrder)
    : ChebyshevIFilterBase<AnalogLowPass, TState>(maxOrder, maxOrder * 2, new AnalogLowPass())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency, double rippleDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, rippleDb);
        _ = new BandPassTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshevi band stop base.
/// </summary>
public abstract class ChebyshevIBandStopBase<TState>(int maxOrder)
    : ChebyshevIFilterBase<AnalogLowPass, TState>(maxOrder, maxOrder * 2, new AnalogLowPass())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="rippleDb">The ripple db.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency, double rippleDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, rippleDb);
        _ = new BandStopTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshevi low shelf base.
/// </summary>
public abstract class ChebyshevILowShelfBase<TState>(int maxOrder)
    : ChebyshevIFilterBase<AnalogLowShelf, TState>(maxOrder, maxOrder, new AnalogLowShelf())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    protected void Setup(int order, double normalizedCutoff, double gainDb, double rippleDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb, rippleDb);
        _ = new LowPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshevi high shelf base.
/// </summary>
public abstract class ChebyshevIHighShelfBase<TState>(int maxOrder)
    : ChebyshevIFilterBase<AnalogLowShelf, TState>(maxOrder, maxOrder, new AnalogLowShelf())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    protected void Setup(int order, double normalizedCutoff, double gainDb, double rippleDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb, rippleDb);
        _ = new HighPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents chebyshevi band shelf base.
/// </summary>
public abstract class ChebyshevIBandShelfBase<TState>(int maxOrder)
    : ChebyshevIFilterBase<AnalogLowShelf, TState>(maxOrder, maxOrder * 2, new AnalogLowShelf())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="rippleDb">The ripple db.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency, double gainDb, double rippleDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb, rippleDb);
        _ = new BandPassTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        DigitalPrototype.SetNormal(centerFrequency < 0.25 ? MathEx.DoublePi : 0.0, 1.0);
        SetLayout(DigitalPrototype);
    }
}