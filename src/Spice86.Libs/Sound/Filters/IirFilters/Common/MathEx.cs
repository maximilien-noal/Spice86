namespace Spice86.Libs.Sound.Filters.IirFilters.Common;

using System.Numerics;

/// <summary>
/// Represents math ex.
/// </summary>
internal static class MathEx {
    /// <summary>
    /// The double pi.
    /// </summary>
    internal const double DoublePi = 3.1415926535897932384626433832795028841971;
    /// <summary>
    /// The double pi over two.
    /// </summary>
    internal const double DoublePiOverTwo = 1.5707963267948966192313216916397514420986;
    /// <summary>
    /// The double ln 2.
    /// </summary>
    internal const double DoubleLn2 = 0.69314718055994530941723212145818;
    /// <summary>
    /// The double ln 10.
    /// </summary>
    internal const double DoubleLn10 = 2.3025850929940456840179914546844;

    /// <summary>
    /// Performs the infinity operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    internal static Complex Infinity() {
        return new Complex(double.PositiveInfinity, 0.0);
    }

    /// <summary>
    /// Adds mul.
    /// </summary>
    /// <param name="c">The c.</param>
    /// <param name="v">The v.</param>
    /// <param name="c1">The c 1.</param>
    /// <returns>The result of the operation.</returns>
    internal static Complex AddMul(Complex c, double v, Complex c1) {
        return new Complex(c.Real + (v * c1.Real), c.Imaginary + (v * c1.Imaginary));
    }

    /// <summary>
    /// Performs the asinh operation.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <returns>The result of the operation.</returns>
    internal static double Asinh(double x) {
        return Math.Log(x + Math.Sqrt((x * x) + 1.0));
    }

    /// <summary>
    /// Determines whether nan.
    /// </summary>
    /// <param name="v">The v.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal static bool IsNaN(double v) {
        return double.IsNaN(v);
    }

    /// <summary>
    /// Determines whether nan.
    /// </summary>
    /// <param name="v">The v.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal static bool IsNaN(Complex v) {
        return IsNaN(v.Real) || IsNaN(v.Imaginary);
    }

    /// <summary>
    /// Determines whether infinity.
    /// </summary>
    /// <param name="v">The v.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal static bool IsInfinity(Complex v) {
        return double.IsInfinity(v.Real) || double.IsInfinity(v.Imaginary);
    }
}