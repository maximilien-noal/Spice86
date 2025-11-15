// SPDX-FileCopyrightText: 2020-2023, Andrea Zoppi
// SPDX-License-Identifier: BSD 2-Clause License

namespace Spice86.Libs.Sound.Devices.YM7128B;

/// <summary>
/// Represents ym 7128 b chip.
/// </summary>
internal sealed partial class Ym7128BChip {
    private Ym7128BChipShort Short { get; } = new();

    /// <summary>
    /// Gets short process data.
    /// </summary>
    internal Ym7128BChipShortProcessData ShortProcessData { get; } = new();

    /// <summary>
    /// Performs the short ctor operation.
    /// </summary>
    internal void ShortCtor() {
        Short.Buffer = null;
        Short.Length = 0;
        Short.SampleRate = 0;
    }

    /// <summary>
    /// Performs the short dtor operation.
    /// </summary>
    internal void ShortDtor() {
        Short.Buffer = null;
        Short.Length = 0;
    }

    internal void ShortReset() {
        for (byte address = Ym7128BDatasheetSpecs.AddressMin; address <= Ym7128BDatasheetSpecs.AddressMax; address++) {
            ShortWrite(address, 0x00);
        }
    }

    /// <summary>
    /// Performs the short start operation.
    /// </summary>
    internal void ShortStart() {
        Short.T0D = 0;
        Short.Tail = 0;
        if (Short.Buffer != null) {
            Array.Clear(Short.Buffer, 0, Short.Buffer.Length);
        }
    }

    /// <summary>
    /// Performs the short stop operation.
    /// </summary>
    internal void ShortStop() {
        // No-op
    }

    /// <summary>
    /// Performs the short setup operation.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    internal void ShortSetup(nuint sampleRate) {
        if (Short.SampleRate == sampleRate && Short.Buffer != null) {
            return;
        }

        Short.SampleRate = sampleRate;
        Short.Buffer = null;
        Short.Length = 0;

        if (sampleRate < 10) {
            return;
        }

        nuint length = (sampleRate / 10) + 1;
        Short.Length = length;
        Short.Buffer = new short[(int)length];

        for (byte i = 0; i < Ym7128BDatasheetSpecs.TapCount; i++) {
            byte data = Short.Registers[i + (int)Ym7128BRegister.T0];
            Short.Taps[i] = Ym7128BHelpers.RegisterToTapIdeal(data, Short.SampleRate);
        }
    }

    /// <summary>
    /// Performs the short process operation.
    /// </summary>
    /// <param name="data">The data.</param>
    internal void ShortProcess(Ym7128BChipShortProcessData data) {
        ArgumentNullException.ThrowIfNull(data);

        if (Short.Buffer == null || Short.Length == 0) {
            return;
        }

        short input = data.Inputs[(int)Ym7128BInputChannel.Mono];

        nuint t0 = Short.Tail + Short.Taps[0];
        nuint filterHead = t0 >= Short.Length ? t0 - Short.Length : t0;
        short filterT0 = Short.Buffer[(int)filterHead];
        short filterD = Short.T0D;
        Short.T0D = filterT0;
        short filterC0 = Ym7128BHelpers.MulShort(filterT0, Short.Gains[(int)Ym7128BRegister.C0]);
        short filterC1 = Ym7128BHelpers.MulShort(filterD, Short.Gains[(int)Ym7128BRegister.C1]);
        short filterSum = Ym7128BHelpers.ClampAddShort(filterC0, filterC1);
        short filterVc = Ym7128BHelpers.MulShort(filterSum, Short.Gains[(int)Ym7128BRegister.Vc]);

        short inputVm = Ym7128BHelpers.MulShort(input, Short.Gains[(int)Ym7128BRegister.Vm]);
        short inputSum = Ym7128BHelpers.ClampAddShort(inputVm, filterVc);

        Short.Tail = Short.Tail != 0 ? Short.Tail - 1 : Short.Length - 1;
        Short.Buffer[(int)Short.Tail] = inputSum;

        for (byte channel = 0; channel < (byte)Ym7128BOutputChannel.Count; channel++) {
            int gainBase = (int)Ym7128BRegister.Gl1 + (channel * Ym7128BDatasheetSpecs.GainLaneCount);
            int accum = 0;

            for (byte tap = 1; tap < Ym7128BDatasheetSpecs.TapCount; tap++) {
                nuint t = Short.Tail + Short.Taps[tap];
                nuint head = t >= Short.Length ? t - Short.Length : t;
                short buffered = Short.Buffer[(int)head];
                short g = Short.Gains[gainBase + tap - 1];
                short bufferedG = Ym7128BHelpers.MulShort(buffered, g);
                accum += bufferedG;
            }

            short total = Ym7128BHelpers.ClampShort(accum);
            short v = Short.Gains[(int)Ym7128BRegister.Vl + channel];
            short totalV = Ym7128BHelpers.MulShort(total, v);
            data.Outputs[channel] = Ym7128BHelpers.ClampShort(totalV);
        }
    }

    /// <summary>
    /// Performs the short read operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>The result of the operation.</returns>
    internal byte ShortRead(byte address) {
        return address switch {
            < (byte)Ym7128BRegister.C0 => (byte)(Short.Registers[address] & Ym7128BDatasheetSpecs.GainDataMask),
            < (byte)Ym7128BRegister.T0 => (byte)(Short.Registers[address] & Ym7128BDatasheetSpecs.CoeffValueMask),
            < (byte)Ym7128BRegister.Count => (byte)(Short.Registers[address] & Ym7128BDatasheetSpecs.TapValueMask),
            _ => 0
        };
    }

    /// <summary>
    /// Performs the short write operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    internal void ShortWrite(byte address, byte value) {
        switch (address) {
            case < (byte)Ym7128BRegister.C0:
                Short.Registers[address] = (byte)(value & Ym7128BDatasheetSpecs.GainDataMask);
                Short.Gains[address] = Ym7128BHelpers.RegisterToGainShort(value);
                break;
            case < (byte)Ym7128BRegister.T0:
                Short.Registers[address] = (byte)(value & Ym7128BDatasheetSpecs.CoeffValueMask);
                Short.Gains[address] = Ym7128BHelpers.RegisterToCoeffShort(value);
                break;
            case < (byte)Ym7128BRegister.Count:
                Short.Registers[address] = (byte)(value & Ym7128BDatasheetSpecs.TapValueMask);
                Short.Taps[address - (byte)Ym7128BRegister.T0] =
                    Ym7128BHelpers.RegisterToTapIdeal(value, Short.SampleRate);
                break;
        }
    }
}