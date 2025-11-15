namespace Spice86.Libs.Sound.Filters.IirFilters.Common;

using Spice86.Libs.Sound.Filters.IirFilters.Common.Layout;

using System.Numerics;

/// <summary>
/// Represents biquad.
/// </summary>
public sealed class Biquad {
    /// <summary>
    /// The a0.
    /// </summary>
    internal double A0 = 1.0;
    /// <summary>
    /// The a1.
    /// </summary>
    internal double A1;
    /// <summary>
    /// The a2.
    /// </summary>
    internal double A2;
    /// <summary>
    /// The b0.
    /// </summary>
    internal double B0 = 1.0;
    /// <summary>
    /// The b1.
    /// </summary>
    internal double B1;
    /// <summary>
    /// The b2.
    /// </summary>
    internal double B2;

    /// <summary>
    /// Performs the response operation.
    /// </summary>
    /// <param name="normalizedFrequency">The normalized frequency.</param>
    /// <returns>The result of the operation.</returns>
    internal Complex Response(double normalizedFrequency) {
        double a0 = GetA0();
        double a1 = GetA1();
        double a2 = GetA2();
        double b0 = GetB0();
        double b1 = GetB1();
        double b2 = GetB2();

        double w = 2.0 * MathEx.DoublePi * normalizedFrequency;
        var czn1 = Complex.FromPolarCoordinates(1.0, -w);
        var czn2 = Complex.FromPolarCoordinates(1.0, -2.0 * w);
        var ct = new Complex(b0 / a0, 0.0);
        Complex cb = Complex.One;
        ct = MathEx.AddMul(ct, b1 / a0, czn1);
        ct = MathEx.AddMul(ct, b2 / a0, czn2);
        cb = MathEx.AddMul(cb, a1 / a0, czn1);
        cb = MathEx.AddMul(cb, a2 / a0, czn2);
        return ct / cb;
    }

    /// <summary>
    /// Gets pole zero pairs.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal IReadOnlyList<PoleZeroPair> GetPoleZeroPairs() {
        return [new BiquadPoleState(this)];
    }

    /// <summary>
    /// Gets a0.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal double GetA0() {
        return A0;
    }

    /// <summary>
    /// Gets a1.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal double GetA1() {
        return A1 * A0;
    }

    /// <summary>
    /// Gets a2.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal double GetA2() {
        return A2 * A0;
    }

    /// <summary>
    /// Gets b0.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal double GetB0() {
        return B0 * A0;
    }

    /// <summary>
    /// Gets b1.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal double GetB1() {
        return B1 * A0;
    }

    /// <summary>
    /// Gets b2.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal double GetB2() {
        return B2 * A0;
    }

    /// <summary>
    /// Sets coefficients.
    /// </summary>
    /// <param name="a0">The a 0.</param>
    /// <param name="a1">The a 1.</param>
    /// <param name="a2">The a 2.</param>
    /// <param name="b0">The b 0.</param>
    /// <param name="b1">The b 1.</param>
    /// <param name="b2">The b 2.</param>
    internal void SetCoefficients(double a0, double a1, double a2, double b0, double b1, double b2) {
        if (double.IsNaN(a0)) {
            throw new ArgumentException("a0 is NaN");
        }

        if (double.IsNaN(a1)) {
            throw new ArgumentException("a1 is NaN");
        }

        if (double.IsNaN(a2)) {
            throw new ArgumentException("a2 is NaN");
        }

        if (double.IsNaN(b0)) {
            throw new ArgumentException("b0 is NaN");
        }

        if (double.IsNaN(b1)) {
            throw new ArgumentException("b1 is NaN");
        }

        if (double.IsNaN(b2)) {
            throw new ArgumentException("b2 is NaN");
        }

        A0 = a0;
        A1 = a1 / a0;
        A2 = a2 / a0;
        B0 = b0 / a0;
        B1 = b1 / a0;
        B2 = b2 / a0;
    }

    /// <summary>
    /// Sets one pole.
    /// </summary>
    /// <param name="pole">The pole.</param>
    /// <param name="zero">The zero.</param>
    internal void SetOnePole(Complex pole, Complex zero) {
        if (pole.Imaginary != 0.0) {
            throw new ArgumentException("Imaginary part of pole is non-zero.");
        }

        if (zero.Imaginary != 0.0) {
            throw new ArgumentException("Imaginary part of zero is non-zero.");
        }

        SetCoefficients(1.0, -pole.Real, 0.0, 1.0, -zero.Real, 0.0);
    }

    /// <summary>
    /// Sets two pole.
    /// </summary>
    /// <param name="pole1">The pole 1.</param>
    /// <param name="zero1">The zero 1.</param>
    /// <param name="pole2">The pole 2.</param>
    /// <param name="zero2">The zero 2.</param>
    internal void SetTwoPole(Complex pole1, Complex zero1, Complex pole2, Complex zero2) {
        const string poleErr = "imaginary parts of both poles need to be 0 or complex conjugate";
        const string zeroErr = "imaginary parts of both zeros need to be 0 or complex conjugate";

        double a1;
        double a2;

        if (pole1.Imaginary != 0.0) {
            if (pole2 != Complex.Conjugate(pole1)) {
                throw new ArgumentException(poleErr);
            }

            a1 = -2.0 * pole1.Real;
            a2 = Complex.Abs(pole1) * Complex.Abs(pole1);
        } else {
            if (pole2.Imaginary != 0.0) {
                throw new ArgumentException(poleErr);
            }

            a1 = -(pole1.Real + pole2.Real);
            a2 = pole1.Real * pole2.Real;
        }

        double b1;
        double b2;

        if (zero1.Imaginary != 0.0) {
            if (zero2 != Complex.Conjugate(zero1)) {
                throw new ArgumentException(zeroErr);
            }

            b1 = -2.0 * zero1.Real;
            b2 = Complex.Abs(zero1) * Complex.Abs(zero1);
        } else {
            if (zero2.Imaginary != 0.0) {
                throw new ArgumentException(zeroErr);
            }

            b1 = -(zero1.Real + zero2.Real);
            b2 = zero1.Real * zero2.Real;
        }

        SetCoefficients(1.0, a1, a2, 1.0, b1, b2);
    }

    /// <summary>
    /// Sets pole zero pair.
    /// </summary>
    /// <param name="pair">The pair.</param>
    internal void SetPoleZeroPair(PoleZeroPair pair) {
        if (pair.IsSinglePole()) {
            SetOnePole(pair.Poles.First, pair.Zeros.First);
        } else {
            SetTwoPole(pair.Poles.First, pair.Zeros.First, pair.Poles.Second, pair.Zeros.Second);
        }
    }

    /// <summary>
    /// Sets pole zero form.
    /// </summary>
    /// <param name="state">The state.</param>
    internal void SetPoleZeroForm(BiquadPoleState state) {
        SetPoleZeroPair(state);
        ApplyScale(state.Gain);
    }

    /// <summary>
    /// Sets identity.
    /// </summary>
    internal void SetIdentity() {
        SetCoefficients(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
    }

    /// <summary>
    /// Performs the apply scale operation.
    /// </summary>
    /// <param name="scale">The scale.</param>
    internal void ApplyScale(double scale) {
        B0 *= scale;
        B1 *= scale;
        B2 *= scale;
    }
}

/// <summary>
/// Represents biquad pole state.
/// </summary>
internal readonly struct BiquadPoleState {
    /// <summary>
    /// The pair.
    /// </summary>
    internal readonly PoleZeroPair Pair;
    /// <summary>
    /// The gain.
    /// </summary>
    internal readonly double Gain;

    /// <summary>
    /// Performs the biquad pole state operation.
    /// </summary>
    /// <param name="s">The s.</param>
    internal BiquadPoleState(Biquad s) {
        double a0 = s.GetA0();
        double a1 = s.GetA1();
        double a2 = s.GetA2();
        double b0 = s.GetB0();
        double b1 = s.GetB1();
        double b2 = s.GetB2();

        Complex polesFirst;
        Complex polesSecond;
        Complex zerosFirst;
        Complex zerosSecond;

        if (a2 == 0.0 && b2 == 0.0) {
            polesFirst = -a1;
            zerosFirst = -b0 / b1;
            polesSecond = Complex.Zero;
            zerosSecond = Complex.Zero;
        } else {
            var c = Complex.Sqrt(new Complex((a1 * a1) - (4.0 * a0 * a2), 0.0));
            double d = 2.0 * a0;
            polesFirst = -(a1 + c) / d;
            polesSecond = (c - a1) / d;
            if (MathEx.IsNaN(polesFirst) || MathEx.IsNaN(polesSecond)) {
                throw new ArgumentException("poles are NaN");
            }

            var cz = Complex.Sqrt(new Complex((b1 * b1) - (4.0 * b0 * b2), 0.0));
            double dz = 2.0 * b0;
            zerosFirst = -(b1 + cz) / dz;
            zerosSecond = (cz - b1) / dz;
            if (MathEx.IsNaN(zerosFirst) || MathEx.IsNaN(zerosSecond)) {
                throw new ArgumentException("zeros are NaN");
            }
        }

        Pair = new PoleZeroPair(polesFirst, zerosFirst, polesSecond, zerosSecond);
        Gain = b0 / a0;
    }

    /// <summary>
    /// Performs the pole zero pair operation.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>The result of the operation.</returns>
    public static implicit operator PoleZeroPair(BiquadPoleState state) {
        return state.Pair;
    }
}