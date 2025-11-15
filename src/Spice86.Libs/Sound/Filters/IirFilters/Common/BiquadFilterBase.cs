namespace Spice86.Libs.Sound.Filters.IirFilters.Common;

using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents biquad filter base.
/// </summary>
public abstract class BiquadFilterBase<TState>
    where TState : struct, ISectionState {
    // ReSharper disable once RedundantDefaultMemberInitializer
    private TState _state = default;

    /// <summary>
    /// Gets coefficients.
    /// </summary>
    public Biquad Coefficients { get; } = new();

    /// <summary>
    /// Sets coefficients.
    /// </summary>
    /// <param name="a0">The a 0.</param>
    /// <param name="a1">The a 1.</param>
    /// <param name="a2">The a 2.</param>
    /// <param name="b0">The b 0.</param>
    /// <param name="b1">The b 1.</param>
    /// <param name="b2">The b 2.</param>
    protected void SetCoefficients(double a0, double a1, double a2, double b0, double b1, double b2) {
        Coefficients.SetCoefficients(a0, a1, a2, b0, b1, b2);
    }

    public void Reset() {
        _state.Reset();
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double sample) {
        return _state.Process(sample, Coefficients);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public float Filter(float sample) {
        return (float)Filter((double)sample);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <param name="state">The state.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double sample, ref TState state) {
        return state.Process(sample, Coefficients);
    }
}