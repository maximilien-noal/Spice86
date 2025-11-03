namespace Spice86.Shared.Utils;

using Spice86.Shared.Emulator.Memory;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

/// <summary>
/// Utility class for parsing address and value strings. State-independent version.
/// </summary>
public static partial class AddressParser {
    
    [GeneratedRegex(@"^0x[0-9A-Fa-f]+$")]
    public static partial Regex HexUintRegex();
    
    /// <summary>
    /// Regex for segmented address format: A000:0000 or FFFF:FFFF
    /// </summary>
    [GeneratedRegex(@"^([0-9A-Fa-f]{1,4}):([0-9A-Fa-f]{1,4})$")]
    public static partial Regex SegmentedAddressRegex();

    /// <summary>
    /// Tries to parse the address string into a uint address.
    /// </summary>
    /// <param name="value">The user input.</param>
    /// <param name="address">The parsed address. <c>null</c> if we return <c>false</c></param>
    /// <returns>A boolean value indicating success or error, along with the address out variable.</returns>
    public static bool TryParseAddress(string? value, [NotNullWhen(true)] out uint? address) {
        if (string.IsNullOrWhiteSpace(value)) {
            address = null;
            return false;
        }

        string valueTrimmed = value.Trim();
        address = ParseSegmentedAddressAsLinear(valueTrimmed);
        if (address != null) {
            return true;
        }
        
        address = ParseHex(valueTrimmed);
        return address != null;
    }

    /// <summary>
    /// Parse segmented address and return it
    /// </summary>
    public static SegmentedAddress? ParseSegmentedAddress(string value) {
        string trimmedValue = value.Trim();
        Match segmentedMatch = SegmentedAddressRegex().Match(trimmedValue);
        if (segmentedMatch.Success) {
            ushort? segment = ParseHexUshort(segmentedMatch.Groups[1].Value);
            ushort? offset = ParseHexUshort(segmentedMatch.Groups[2].Value);
            if (segment != null && offset != null) {
                return new SegmentedAddress(segment.Value, offset.Value);
            }
        }

        return null;
    }

    /// <summary>
    /// Parse segmented address and return as linear address
    /// </summary>
    public static uint? ParseSegmentedAddressAsLinear(string value) {
        SegmentedAddress? segmented = ParseSegmentedAddress(value);
        return segmented?.Linear;
    }
    
    /// <summary>
    /// Parse hex ushort value
    /// </summary>
    public static ushort? ParseHexUshort(string value) {
        if (ushort.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort result)) {
            return result;
        }
        return null;
    }

    /// <summary>
    /// Check if string is valid hex format
    /// </summary>
    public static bool IsValidHex(string value) {
       return HexUintRegex().Match(value).Success;
    }

    /// <summary>
    /// Parse hex string to uint
    /// </summary>
    public static uint? ParseHex(string value) {
        if (!IsValidHex(value)) {
            return null;
        }

        string hexValue = value;
        if (hexValue.StartsWith("0x") && hexValue.Length is > 2 and <= 10) {
            hexValue = hexValue[2..];
        }
        if (uint.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint res)) {
            return res;
        }

        return null;
    }
    
    /// <summary>
    /// Parse hex string to byte array
    /// </summary>
    public static byte[]? ParseHexAsArray(string? value) {
        if (value == null) {
            return null;
        }
        string valueTrimmed = value.Trim();
        if (!IsValidHex(valueTrimmed)) {
            return null;
        }
        
        if (valueTrimmed.StartsWith("0x")) {
            valueTrimmed = valueTrimmed[2..];
        }

        return ConvertUtils.HexToByteArray(valueTrimmed);
    }
    
    /// <summary>
    /// Validate address format (without State dependency)
    /// </summary>
    public static bool TryValidateAddress(string? value, out string message) {
        if (string.IsNullOrWhiteSpace(value)) {
            message = "Address is required";
            return false;
        }
        if (!IsValidHex(value) && !SegmentedAddressRegex().IsMatch(value)) {
            message = "Invalid address format";
            return false;
        }

        if (!TryParseAddress(value, out uint? _)) {
            message = "Invalid address";
            return false;
        }

        message = string.Empty;
        return true;
    }
}
