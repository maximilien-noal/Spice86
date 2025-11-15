namespace Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Defines the contract for i section state.
/// </summary>
public interface ISectionState {
    void Reset();

    double Process(double input, Biquad coefficients);
}