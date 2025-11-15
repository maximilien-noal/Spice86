namespace Spice86.Shared.Utils;

/// <summary>
/// Represents bit mask utils.
/// </summary>
public class BitMaskUtils {
    /// <summary>
    /// Performs the bit mask from bit list operation.
    /// </summary>
    /// <param name="bitPositions">The bit positions.</param>
    /// <returns>The result of the operation.</returns>
    public static uint BitMaskFromBitList(IEnumerable<int> bitPositions) {
        uint mask = 0;
        foreach (int bitPosition in bitPositions) {
            mask |= 1u << bitPosition;
        }
        return mask;
    }
}