namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.Custom;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents one pole.
/// </summary>
public class OnePole<TState>
    where TState : struct, ISectionState {
    private readonly OnePoleInternal<TState> _impl = new();

    /// <summary>
    /// The coefficients.
    /// </summary>
    public Biquad Coefficients => _impl.Coefficients;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <param name="pole">The pole.</param>
    /// <param name="zero">The zero.</param>
    public void Setup(double scale, double pole, double zero) {
        _impl.Setup(scale, pole, zero);
    }

    public void Reset() {
        _impl.Reset();
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public float Filter(float sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <param name="state">The state.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double sample, ref TState state) {
        return _impl.Filter(sample, ref state);
    }
}

/// <summary>
/// Represents one pole.
/// </summary>
public sealed class OnePole : OnePole<DirectFormIiState>;

/// <summary>
/// Represents two pole.
/// </summary>
public class TwoPole<TState>
    where TState : struct, ISectionState {
    private readonly TwoPoleInternal<TState> _impl = new();

    /// <summary>
    /// The coefficients.
    /// </summary>
    public Biquad Coefficients => _impl.Coefficients;

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <param name="poleRho">The pole rho.</param>
    /// <param name="poleTheta">The pole theta.</param>
    /// <param name="zeroRho">The zero rho.</param>
    /// <param name="zeroTheta">The zero theta.</param>
    public void Setup(double scale, double poleRho, double poleTheta, double zeroRho, double zeroTheta) {
        _impl.Setup(scale, poleRho, poleTheta, zeroRho, zeroTheta);
    }

    public void Reset() {
        _impl.Reset();
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public float Filter(float sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <param name="state">The state.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double sample, ref TState state) {
        return _impl.Filter(sample, ref state);
    }
}

/// <summary>
/// Represents two pole.
/// </summary>
public sealed class TwoPole : TwoPole<DirectFormIiState>;

/// <summary>
/// Represents sos cascade.
/// </summary>
public class SosCascade<TState>
    where TState : struct, ISectionState {
    private readonly SosCascadeInternal<TState> _impl;

    /// <summary>
    /// Performs the sos cascade operation.
    /// </summary>
    /// <param name="stageCount">The stage count.</param>
    protected SosCascade(int stageCount) {
        _impl = new SosCascadeInternal<TState>(stageCount);
    }

    /// <summary>
    /// Performs the sos cascade operation.
    /// </summary>
    /// <param name="double">The double.</param>
    /// <param name="sosCoefficients">The sos coefficients.</param>
    protected SosCascade(double[,] sosCoefficients) {
        int stageCount = sosCoefficients.GetLength(0);
        _impl = new SosCascadeInternal<TState>(stageCount);
        _impl.Setup(sosCoefficients);
    }

    /// <summary>
    /// The stage count.
    /// </summary>
    public int StageCount => _impl.StageCount;

    public void Reset() {
        _impl.Reset();
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public float Filter(float sample) {
        return _impl.Filter(sample);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sosCoefficients">The sos coefficients.</param>
    public void Setup(ReadOnlySpan<double> sosCoefficients) {
        _impl.Setup(sosCoefficients);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="double">The double.</param>
    /// <param name="sosCoefficients">The sos coefficients.</param>
    public void Setup(double[,] sosCoefficients) {
        _impl.Setup(sosCoefficients);
    }

    /// <summary>
    /// Gets coefficient snapshot.
    /// </summary>
    public double[,] GetCoefficientSnapshot() {
        return _impl.GetCoefficientSnapshot();
    }
}

/// <summary>
/// Represents sos cascade.
/// </summary>
public sealed class SosCascade : SosCascade<DirectFormIiState> {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="stageCount">The stage count.</param>
    public SosCascade(int stageCount) : base(stageCount) { }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="double">The double.</param>
    /// <param name="sosCoefficients">The sos coefficients.</param>
    public SosCascade(double[,] sosCoefficients) : base(sosCoefficients) { }
}