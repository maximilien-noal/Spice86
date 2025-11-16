namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.Custom;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents the OnePole class.
/// </summary>
public class OnePole<TState>
    where TState : struct, ISectionState {
    private readonly OnePoleInternal<TState> _impl = new();

    /// <summary>
    /// The Coefficients.
    /// </summary>
    public Biquad Coefficients => _impl.Coefficients;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double scale, double pole, double zero) {
        _impl.Setup(scale, pole, zero);
    }

    /// <summary>
    /// Reset method.
    /// </summary>
    public void Reset() {
        _impl.Reset();
    }

    /// <summary>
    /// Filter method.
    /// </summary>
    public double Filter(double sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Filter method.
    /// </summary>
    public float Filter(float sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Filter method.
    /// </summary>
    public double Filter(double sample, ref TState state) {
        return _impl.Filter(sample, ref state);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class OnePole : OnePole<DirectFormIiState>;

/// <summary>
/// Represents the TwoPole class.
/// </summary>
public class TwoPole<TState>
    where TState : struct, ISectionState {
    private readonly TwoPoleInternal<TState> _impl = new();

    /// <summary>
    /// The Coefficients.
    /// </summary>
    public Biquad Coefficients => _impl.Coefficients;

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double scale, double poleRho, double poleTheta, double zeroRho, double zeroTheta) {
        _impl.Setup(scale, poleRho, poleTheta, zeroRho, zeroTheta);
    }

    /// <summary>
    /// Reset method.
    /// </summary>
    public void Reset() {
        _impl.Reset();
    }

    /// <summary>
    /// Filter method.
    /// </summary>
    public double Filter(double sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Filter method.
    /// </summary>
    public float Filter(float sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Filter method.
    /// </summary>
    public double Filter(double sample, ref TState state) {
        return _impl.Filter(sample, ref state);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class TwoPole : TwoPole<DirectFormIiState>;

/// <summary>
/// Represents the SosCascade class.
/// </summary>
public class SosCascade<TState>
    where TState : struct, ISectionState {
    private readonly SosCascadeInternal<TState> _impl;

    protected SosCascade(int stageCount) {
        _impl = new SosCascadeInternal<TState>(stageCount);
    }

    protected SosCascade(double[,] sosCoefficients) {
        int stageCount = sosCoefficients.GetLength(0);
        _impl = new SosCascadeInternal<TState>(stageCount);
        _impl.Setup(sosCoefficients);
    }

    /// <summary>
    /// The StageCount.
    /// </summary>
    public int StageCount => _impl.StageCount;

    /// <summary>
    /// Reset method.
    /// </summary>
    public void Reset() {
        _impl.Reset();
    }

    /// <summary>
    /// Filter method.
    /// </summary>
    public double Filter(double sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Filter method.
    /// </summary>
    public float Filter(float sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(ReadOnlySpan<double> sosCoefficients) {
        _impl.Setup(sosCoefficients);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double[,] sosCoefficients) {
        _impl.Setup(sosCoefficients);
    }

    public double[,] GetCoefficientSnapshot() {
        return _impl.GetCoefficientSnapshot();
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class SosCascade : SosCascade<DirectFormIiState> {
    public SosCascade(int stageCount) : base(stageCount) { }

    public SosCascade(double[,] sosCoefficients) : base(sosCoefficients) { }
}