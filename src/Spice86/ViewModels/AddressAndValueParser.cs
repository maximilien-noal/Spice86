namespace Spice86.ViewModels;

using Spice86.Core.Emulator.CPU;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Utils;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// Represents address and value parser.
/// </summary>
public partial class AddressAndValueParser {

    [GeneratedRegex(@"^0x[0-9A-Fa-f]+$")]
    public static partial Regex HexUintRegex();

    /// <summary>
    /// A000:0000 is valid
    /// DS:SI is valid
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^([0-9A-Fa-f]{1,4}|[a-zA-Z]{2}):([0-9A-Fa-f]{1,4}|[a-zA-Z]{2})$")]
    public static partial Regex SegmentedAddressRegex();

    /// <summary>
    /// Tries to parse the address string into a uint address.
    /// </summary>
    /// <param name="value">The user input.</param>
    /// <param name="state">The emulated CPU registers and flags.</param>
    /// <param name="address">The parsed address. <c>null</c> if we return <c>false</c></param>
    /// <returns>A boolean value indicating success or error, along with the address out variable.</returns>
    public static bool TryParseAddressString(string? value, State state, [NotNullWhen(true)] out uint? address) {
        if (string.IsNullOrWhiteSpace(value)) {
            address = null;
            return false;
        }

        string valueTrimmed = value.Trim();
        address = ParseSegmentedAddress(valueTrimmed, state)?.Linear;
        if (address != null) {
            return true;
        }

        address = ParseHex(valueTrimmed);
        return address != null;
    }

    /// <summary>
    /// Parses segmented address.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="state">The state.</param>
    public static SegmentedAddress? ParseSegmentedAddress(string value, State? state) {
        string trimmedValue = value.Trim();
        Match segmentedMatch = SegmentedAddressRegex()
            .Match(trimmedValue);
        if (segmentedMatch.Success) {
            ushort? segment = TryParseSegmentOrRegister(
                segmentedMatch.Groups[1].Value,
                state);
            ushort? offset = TryParseSegmentOrRegister(
                segmentedMatch.Groups[2].Value,
                state);
            if (segment != null && offset != null) {
                return new(segment.Value, offset.Value);
            }
        }

        return null;
    }

    /// <summary>
    /// Performs the try parse segment or register operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="state">The state.</param>
    public static ushort? TryParseSegmentOrRegister(string value, State? state) {
        // Try a property of the CPU state first (there is no collision with hex values)
        ushort? res = GetUshortStateProperty(value, state);
        if (res != null) {
            return res;
        }
        // Try to parse hex value
        if (ushort.TryParse(value, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out ushort result)) {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Gets ushort state property.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="state">The state.</param>
    public static ushort? GetUshortStateProperty(string value, State? state) {
        if (state is null) {
            return null;
        }
        PropertyInfo? property = state.GetType().GetProperty(value.ToUpperInvariant());
        if (property != null &&
            property.PropertyType == typeof(ushort) &&
            property.GetValue(state) is ushort propertyValue) {
            return propertyValue;
        }

        return null;
    }

    /// <summary>
    /// Determines whether valid hex.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    public static bool IsValidHex(string value) {
        return HexUintRegex().Match(value).Success;
    }

    /// <summary>
    /// Parses hex.
    /// </summary>
    /// <param name="value">The value.</param>
    public static uint? ParseHex(string value) {
        if (!IsValidHex(value)) {
            return null;
        }

        string hexValue = value;
        if (hexValue.StartsWith("0x") && hexValue.Length is > 2 and <= 10) {
            hexValue = hexValue[2..];
        }
        if (uint.TryParse(hexValue, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out uint res)) {
            return res;
        }

        return null;
    }

    /// <summary>
    /// Parses hex as array.
    /// </summary>
    /// <param name="value">The value.</param>
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
    /// Performs the try validate address operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="state">The state.</param>
    /// <param name="message">The message.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public static bool TryValidateAddress(string? value, State state, out string message) {
        if (string.IsNullOrWhiteSpace(value)) {
            message = "Address is required";
            return false;
        }
        if (!IsValidHex(value) &&
            !SegmentedAddressRegex().IsMatch(value) &&
            GetUshortStateProperty(value, state) == null) {
            message = "Invalid address format";
            return false;
        }

        if (!TryParseAddressString(value, state, out uint? _)) {
            message = "Invalid address";
            return false;
        }

        message = string.Empty;
        return true;
    }
}