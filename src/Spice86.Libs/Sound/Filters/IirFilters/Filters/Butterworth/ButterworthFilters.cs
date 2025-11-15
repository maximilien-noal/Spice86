namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.Butterworth;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.Layout;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;
using Spice86.Libs.Sound.Filters.IirFilters.Common.Transforms;

/// <summary>
/// Represents butterworth filter base.
/// </summary>
public abstract class ButterworthFilterBase<TAnalog, TState>(int maxOrder, int maxDigitalPoles, TAnalog analogPrototype)
    : PoleFilterBase<TAnalog, TState>(maxOrder, maxDigitalPoles, analogPrototype)
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
/// Represents butterworth low pass base.
/// </summary>
public abstract class ButterworthLowPassBase<TState>(int maxOrder)
    : ButterworthFilterBase<AnalogLowPass, TState>(maxOrder, maxOrder, new AnalogLowPass())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    protected void Setup(int order, double normalizedCutoff) {
        ValidateOrder(order);
        AnalogPrototype.Design(order);
        _ = new LowPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents butterworth high pass base.
/// </summary>
public abstract class ButterworthHighPassBase<TState>(int maxOrder)
    : ButterworthFilterBase<AnalogLowPass, TState>(maxOrder, maxOrder, new AnalogLowPass())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    protected void Setup(int order, double normalizedCutoff) {
        ValidateOrder(order);
        AnalogPrototype.Design(order);
        _ = new HighPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents butterworth band pass base.
/// </summary>
public abstract class ButterworthBandPassBase<TState>(int maxOrder)
    : ButterworthFilterBase<AnalogLowPass, TState>(maxOrder, maxOrder * 2, new AnalogLowPass())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency) {
        ValidateOrder(order);
        AnalogPrototype.Design(order);
        _ = new BandPassTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents butterworth band stop base.
/// </summary>
public abstract class ButterworthBandStopBase<TState>(int maxOrder)
    : ButterworthFilterBase<AnalogLowPass, TState>(maxOrder, maxOrder * 2, new AnalogLowPass())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency) {
        ValidateOrder(order);
        AnalogPrototype.Design(order);
        _ = new BandStopTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents butterworth low shelf base.
/// </summary>
public abstract class ButterworthLowShelfBase<TState>(int maxOrder)
    : ButterworthFilterBase<AnalogLowShelf, TState>(maxOrder, maxOrder, new AnalogLowShelf())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="gainDb">The gain db.</param>
    protected void Setup(int order, double normalizedCutoff, double gainDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb);
        _ = new LowPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents butterworth high shelf base.
/// </summary>
public abstract class ButterworthHighShelfBase<TState>(int maxOrder)
    : ButterworthFilterBase<AnalogLowShelf, TState>(maxOrder, maxOrder, new AnalogLowShelf())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="normalizedCutoff">The normalized cutoff.</param>
    /// <param name="gainDb">The gain db.</param>
    protected void Setup(int order, double normalizedCutoff, double gainDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb);
        _ = new HighPassTransform(normalizedCutoff, DigitalPrototype, AnalogPrototype);
        SetLayout(DigitalPrototype);
    }
}

/// <summary>
/// Represents butterworth band shelf base.
/// </summary>
public abstract class ButterworthBandShelfBase<TState>(int maxOrder)
    : ButterworthFilterBase<AnalogLowShelf, TState>(maxOrder, maxOrder * 2, new AnalogLowShelf())
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="widthFrequency">The width frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    protected void Setup(int order, double centerFrequency, double widthFrequency, double gainDb) {
        ValidateOrder(order);
        AnalogPrototype.Design(order, gainDb);
        _ = new BandPassTransform(centerFrequency, widthFrequency, DigitalPrototype, AnalogPrototype);
        DigitalPrototype.SetNormal(centerFrequency < 0.25 ? MathEx.DoublePi : 0.0, 1.0);
        SetLayout(DigitalPrototype);
    }
}