namespace Spice86.Libs.Sound.Filters.IirFilters.Common.Layout;

using System.Numerics;

/// <summary>
/// Represents layout base.
/// </summary>
public class LayoutBase {
    private int _maxPoles;
    private double _normalGain = 1.0;
    private double _normalW;
    private int _numPoles;
    private PoleZeroPair[] _pairs = [];

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    internal LayoutBase() {
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="maxPoles">The max poles.</param>
    /// <param name="pairs">The pairs.</param>
    internal LayoutBase(int maxPoles, PoleZeroPair[] pairs) {
        _maxPoles = maxPoles;
        _pairs = pairs;
    }

    internal ref readonly PoleZeroPair this[int pairIndex] => ref GetPair(pairIndex);

    /// <summary>
    /// Sets storage.
    /// </summary>
    /// <param name="other">The other.</param>
    internal void SetStorage(LayoutBase other) {
        _numPoles = 0;
        _maxPoles = other._maxPoles;
        _pairs = other._pairs;
    }

    internal void Reset() {
        _numPoles = 0;
    }

    /// <summary>
    /// Gets num poles.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal int GetNumPoles() {
        return _numPoles;
    }

    /// <summary>
    /// Gets max poles.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal int GetMaxPoles() {
        return _maxPoles;
    }

    /// <summary>
    /// Adds .
    /// </summary>
    /// <param name="pole">The pole.</param>
    /// <param name="zero">The zero.</param>
    internal void Add(Complex pole, Complex zero) {
        if ((_numPoles & 1) != 0) {
            throw new ArgumentException("Can't add 2nd order after a 1st order filter.");
        }

        if (MathEx.IsNaN(pole)) {
            throw new ArgumentException("Pole to add is NaN.");
        }

        if (MathEx.IsNaN(zero)) {
            throw new ArgumentException("Zero to add is NaN.");
        }

        _pairs[_numPoles / 2] = new PoleZeroPair(pole, zero);
        ++_numPoles;
    }

    /// <summary>
    /// Adds pole zero conjugate pairs.
    /// </summary>
    /// <param name="pole">The pole.</param>
    /// <param name="zero">The zero.</param>
    internal void AddPoleZeroConjugatePairs(Complex pole, Complex zero) {
        if ((_numPoles & 1) != 0) {
            throw new ArgumentException("Can't add 2nd order after a 1st order filter.");
        }

        if (MathEx.IsNaN(pole)) {
            throw new ArgumentException("Pole to add is NaN.");
        }

        if (MathEx.IsNaN(zero)) {
            throw new ArgumentException("Zero to add is NaN.");
        }

        _pairs[_numPoles / 2] = new PoleZeroPair(
            pole,
            zero,
            Complex.Conjugate(pole),
            Complex.Conjugate(zero));
        _numPoles += 2;
    }

    /// <summary>
    /// Adds .
    /// </summary>
    /// <param name="poles">The poles.</param>
    /// <param name="zeros">The zeros.</param>
    internal void Add(ComplexPair poles, ComplexPair zeros) {
        if ((_numPoles & 1) != 0) {
            throw new ArgumentException("Can't add 2nd order after a 1st order filter.");
        }

        if (!poles.IsMatchedPair()) {
            throw new ArgumentException("Poles not complex conjugate.");
        }

        if (!zeros.IsMatchedPair()) {
            throw new ArgumentException("Zeros not complex conjugate.");
        }

        _pairs[_numPoles / 2] = new PoleZeroPair(poles.First, zeros.First, poles.Second, zeros.Second);
        _numPoles += 2;
    }

    /// <summary>
    /// Gets pair.
    /// </summary>
    /// <param name="pairIndex">The pair index.</param>
    /// <returns>The result of the operation.</returns>
    internal ref readonly PoleZeroPair GetPair(int pairIndex) {
        if (pairIndex < 0 || pairIndex >= (_numPoles + 1) / 2) {
            throw new ArgumentOutOfRangeException(nameof(pairIndex), "Pair index out of bounds.");
        }

        return ref _pairs[pairIndex];
    }

    /// <summary>
    /// Gets normalw.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal double GetNormalW() {
        return _normalW;
    }

    /// <summary>
    /// Gets normal gain.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal double GetNormalGain() {
        return _normalGain;
    }

    /// <summary>
    /// Sets normal.
    /// </summary>
    /// <param name="w">The w.</param>
    /// <param name="g">The g.</param>
    internal void SetNormal(double w, double g) {
        _normalW = w;
        _normalGain = g;
    }
}

/// <summary>
/// Represents layout storage.
/// </summary>
internal sealed class LayoutStorage {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="maxPoles">The max poles.</param>
    internal LayoutStorage(int maxPoles) {
        var pairs = new PoleZeroPair[(maxPoles + 1) / 2];
        Base = new LayoutBase(maxPoles, pairs);
    }

    /// <summary>
    /// Gets base.
    /// </summary>
    internal LayoutBase Base { get; }
}