namespace Spice86.Libs.Sound.Filters.IirFilters.Common;

using Spice86.Libs.Sound.Filters.IirFilters.Common.Layout;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents pole filter base.
/// </summary>
public abstract class PoleFilterBase<TAnalog, TState> : Cascade
    where TAnalog : LayoutBase
    where TState : struct, ISectionState {
    private readonly CascadeStages<TState> _stages;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="maxAnalogPoles">The max analog poles.</param>
    /// <param name="maxDigitalPoles">The max digital poles.</param>
    /// <param name="analogPrototype">The analog prototype.</param>
    protected PoleFilterBase(int maxAnalogPoles, int maxDigitalPoles, TAnalog analogPrototype) {
        var analogStorage = new LayoutStorage(maxAnalogPoles);
        var digitalStorage = new LayoutStorage(maxDigitalPoles);

        AnalogPrototype = analogPrototype;
        AnalogPrototype.SetStorage(analogStorage.Base);
        DigitalPrototype = digitalStorage.Base;

        _stages = new CascadeStages<TState>((maxDigitalPoles + 1) / 2);
        SetCascadeStorage(_stages.GetCascadeStorage());
        _stages.Reset();
    }

    /// <summary>
    /// Gets analog prototype.
    /// </summary>
    protected TAnalog AnalogPrototype { get; }

    /// <summary>
    /// Gets digital prototype.
    /// </summary>
    protected LayoutBase DigitalPrototype { get; }

    public void Reset() {
        _stages.Reset();
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double input) {
        return _stages.Filter(input);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The result of the operation.</returns>
    public float Filter(float input) {
        return _stages.FilterSingle(input);
    }
}