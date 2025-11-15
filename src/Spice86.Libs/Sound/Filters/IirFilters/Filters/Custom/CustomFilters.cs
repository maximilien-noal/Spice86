namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.Custom;

using Spice86.Libs.Sound.Filters.IirFilters.Common;
using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

using System.Numerics;

/// <summary>
/// Represents one pole internal.
/// </summary>
internal sealed class OnePoleInternal<TState> : BiquadFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <param name="pole">The pole.</param>
    /// <param name="zero">The zero.</param>
    public void Setup(double scale, double pole, double zero) {
        Coefficients.SetOnePole(new Complex(pole, 0.0), new Complex(zero, 0.0));
        Coefficients.ApplyScale(scale);
    }
}

/// <summary>
/// Represents two pole internal.
/// </summary>
internal sealed class TwoPoleInternal<TState> : BiquadFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <param name="poleRho">The pole rho.</param>
    /// <param name="poleTheta">The pole theta.</param>
    /// <param name="zeroRho">The zero rho.</param>
    /// <param name="zeroTheta">The zero theta.</param>
    public void Setup(double scale, double poleRho, double poleTheta, double zeroRho, double zeroTheta) {
        var pole = Complex.FromPolarCoordinates(poleRho, poleTheta);
        var zero = Complex.FromPolarCoordinates(zeroRho, zeroTheta);
        Coefficients.SetTwoPole(pole, zero, Complex.Conjugate(pole), Complex.Conjugate(zero));
        Coefficients.ApplyScale(scale);
    }
}

/// <summary>
/// Represents sos cascade internal.
/// </summary>
internal sealed class SosCascadeInternal<TState>
    where TState : struct, ISectionState {
    private readonly CascadeStages<TState> _stages;

    /// <summary>
    /// Performs the sos cascade internal operation.
    /// </summary>
    /// <param name="stageCount">The stage count.</param>
    public SosCascadeInternal(int stageCount) {
        if (stageCount <= 0) {
            throw new ArgumentException("Stage count must be positive.", nameof(stageCount));
        }

        StageCount = stageCount;
        _stages = new CascadeStages<TState>(stageCount);
    }

    /// <summary>
    /// Gets stage count.
    /// </summary>
    public int StageCount { get; }

    public void Reset() {
        _stages.Reset();
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public double Filter(double sample) {
        return _stages.Filter(sample);
    }

    /// <summary>
    /// Performs the filter operation.
    /// </summary>
    /// <param name="sample">The sample.</param>
    /// <returns>The result of the operation.</returns>
    public float Filter(float sample) {
        return _stages.FilterSingle(sample);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sosCoefficients">The sos coefficients.</param>
    public void Setup(ReadOnlySpan<double> sosCoefficients) {
        _stages.Setup(sosCoefficients);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="double">The double.</param>
    /// <param name="sosCoefficients">The sos coefficients.</param>
    public void Setup(double[,] sosCoefficients) {
        _stages.Setup(sosCoefficients);
    }

    /// <summary>
    /// Gets coefficient snapshot.
    /// </summary>
    public double[,] GetCoefficientSnapshot() {
        Biquad[] snapshot = _stages.SnapshotStages();
        double[,] result = new double[StageCount, 6];
        for (int i = 0; i < StageCount; i++) {
            Biquad stage = snapshot[i];
            result[i, 0] = stage.B0;
            result[i, 1] = stage.B1;
            result[i, 2] = stage.B2;
            result[i, 3] = stage.A0;
            result[i, 4] = stage.A1;
            result[i, 5] = stage.A2;
        }

        return result;
    }
}