namespace Spice86.Libs.Sound.Filters.IirFilters.Common.Layout;

using System.Numerics;

/// <summary>
/// Represents complex pair.
/// </summary>
public struct ComplexPair {
    /// <summary>
    /// The first.
    /// </summary>
    internal Complex First;
    /// <summary>
    /// The second.
    /// </summary>
    internal Complex Second;

    /// <summary>
    /// Performs the complex pair operation.
    /// </summary>
    /// <param name="first">The first.</param>
    /// <param name="second">The second.</param>
    internal ComplexPair(Complex first, Complex second) {
        First = first;
        Second = second;
    }

    /// <summary>
    /// Performs the complex pair operation.
    /// </summary>
    /// <param name="single">The single.</param>
    internal ComplexPair(Complex single) {
        double imag = Math.Abs(single.Imaginary);
        if (imag < 1e-12) {
            single = new Complex(single.Real, 0.0);
        }

        if (single.Imaginary != 0.0) {
            throw new ArgumentException("A single complex number needs to be real.", nameof(single));
        }

        First = single;
        Second = Complex.Zero;
    }

    /// <summary>
    /// Determines whether real.
    /// </summary>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal bool IsReal() {
        return Math.Abs(First.Imaginary) < 1e-12 && Math.Abs(Second.Imaginary) < 1e-12;
    }

    /// <summary>
    /// Determines whether matched pair.
    /// </summary>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal bool IsMatchedPair() {
        if (First.Imaginary != 0.0) {
            return Second == Complex.Conjugate(First);
        }

        return Math.Abs(Second.Imaginary) < 1e-12 &&
               Second.Real != 0.0 &&
               First.Real != 0.0;
    }

    /// <summary>
    /// Determines whether nan.
    /// </summary>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal readonly bool IsNaN() {
        return MathEx.IsNaN(First) || MathEx.IsNaN(Second);
    }
}

/// <summary>
/// Represents pole zero pair.
/// </summary>
public struct PoleZeroPair {
    /// <summary>
    /// The poles.
    /// </summary>
    internal ComplexPair Poles;
    /// <summary>
    /// The zeros.
    /// </summary>
    internal ComplexPair Zeros;

    /// <summary>
    /// Performs the pole zero pair operation.
    /// </summary>
    /// <param name="poles">The poles.</param>
    /// <param name="zeros">The zeros.</param>
    internal PoleZeroPair(ComplexPair poles, ComplexPair zeros) {
        Poles = poles;
        Zeros = zeros;
    }

    /// <summary>
    /// Performs the pole zero pair operation.
    /// </summary>
    /// <param name="pole">The pole.</param>
    /// <param name="zero">The zero.</param>
    internal PoleZeroPair(Complex pole, Complex zero) {
        Poles = new ComplexPair(pole);
        Zeros = new ComplexPair(zero);
    }

    /// <summary>
    /// Performs the pole zero pair operation.
    /// </summary>
    /// <param name="p1">The p 1.</param>
    /// <param name="z1">The z 1.</param>
    /// <param name="p2">The p 2.</param>
    /// <param name="z2">The z 2.</param>
    internal PoleZeroPair(Complex p1, Complex z1, Complex p2, Complex z2) {
        Poles = new ComplexPair(p1, p2);
        Zeros = new ComplexPair(z1, z2);
    }

    /// <summary>
    /// Determines whether single pole.
    /// </summary>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal readonly bool IsSinglePole() {
        return Poles.Second == Complex.Zero && Zeros.Second == Complex.Zero;
    }

    /// <summary>
    /// Determines whether nan.
    /// </summary>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal readonly bool IsNaN() {
        return Poles.IsNaN() || Zeros.IsNaN();
    }
}