// SPDX-FileCopyrightText: 2020-2023, Andrea Zoppi
// SPDX-License-Identifier: BSD 2-Clause License

namespace Spice86.Libs.Sound.Devices.YM7128B;

/// <summary>
/// Represents ym 7128 b chip.
/// </summary>
internal sealed partial class Ym7128BChip {
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    internal Ym7128BChip() {
        ChipIdealCtor(Ideal);
        FixedCtor();
        FloatCtor();
        ShortCtor();
    }

    /// <summary>
    /// Performs the ideal ctor operation.
    /// </summary>
    internal void IdealCtor() {
        ChipIdealCtor(Ideal);
    }

    /// <summary>
    /// Performs the ideal dtor operation.
    /// </summary>
    internal void IdealDtor() {
        ChipIdealDtor(Ideal);
    }

    internal void IdealReset() {
        ChipIdealReset(Ideal);
    }

    /// <summary>
    /// Performs the ideal start operation.
    /// </summary>
    internal void IdealStart() {
        ChipIdealStart(Ideal);
    }

    /// <summary>
    /// Performs the ideal stop operation.
    /// </summary>
    internal void IdealStop() {
        ChipIdealStop(Ideal);
    }

    /// <summary>
    /// Performs the ideal setup operation.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    internal void IdealSetup(nuint sampleRate) {
        ChipIdealSetup(Ideal, sampleRate);
    }

    /// <summary>
    /// Performs the ideal write operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    internal void IdealWrite(byte address, byte value) {
        ChipIdealWrite(Ideal, address, value);
    }

    /// <summary>
    /// Performs the ideal read operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>The result of the operation.</returns>
    internal byte IdealRead(byte address) {
        return ChipIdealRead(Ideal, address);
    }

    /// <summary>
    /// Performs the ideal process operation.
    /// </summary>
    /// <param name="data">The data.</param>
    internal void IdealProcess(Ym7128BChipIdealProcessData data) {
        ChipIdealProcess(Ideal, data);
    }

    internal void FixedResetShim() {
        FixedReset();
    }

    /// <summary>
    /// Performs the fixed start shim operation.
    /// </summary>
    internal void FixedStartShim() {
        FixedStart();
    }

    /// <summary>
    /// Performs the fixed stop shim operation.
    /// </summary>
    internal void FixedStopShim() {
        FixedStop();
    }

    /// <summary>
    /// Performs the fixed process shim operation.
    /// </summary>
    /// <param name="data">The data.</param>
    internal void FixedProcessShim(Ym7128BChipFixedProcessData data) {
        FixedProcess(data);
    }

    /// <summary>
    /// Performs the fixed read shim operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>The result of the operation.</returns>
    internal byte FixedReadShim(byte address) {
        return FixedRead(address);
    }

    /// <summary>
    /// Performs the fixed write shim operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    internal void FixedWriteShim(byte address, byte value) {
        FixedWrite(address, value);
    }

    internal void FloatResetShim() {
        FloatReset();
    }

    /// <summary>
    /// Performs the float start shim operation.
    /// </summary>
    internal void FloatStartShim() {
        FloatStart();
    }

    /// <summary>
    /// Performs the float stop shim operation.
    /// </summary>
    internal void FloatStopShim() {
        FloatStop();
    }

    /// <summary>
    /// Performs the float process shim operation.
    /// </summary>
    /// <param name="data">The data.</param>
    internal void FloatProcessShim(Ym7128BChipFloatProcessData data) {
        FloatProcess(data);
    }

    /// <summary>
    /// Performs the float read shim operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>The result of the operation.</returns>
    internal byte FloatReadShim(byte address) {
        return FloatRead(address);
    }

    /// <summary>
    /// Performs the float write shim operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    internal void FloatWriteShim(byte address, byte value) {
        FloatWrite(address, value);
    }

    internal void ShortResetShim() {
        ShortReset();
    }

    /// <summary>
    /// Performs the short start shim operation.
    /// </summary>
    internal void ShortStartShim() {
        ShortStart();
    }

    /// <summary>
    /// Performs the short stop shim operation.
    /// </summary>
    internal void ShortStopShim() {
        ShortStop();
    }

    /// <summary>
    /// Performs the short setup shim operation.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    internal void ShortSetupShim(nuint sampleRate) {
        ShortSetup(sampleRate);
    }

    /// <summary>
    /// Performs the short process shim operation.
    /// </summary>
    /// <param name="data">The data.</param>
    internal void ShortProcessShim(Ym7128BChipShortProcessData data) {
        ShortProcess(data);
    }

    /// <summary>
    /// Performs the short read shim operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>The result of the operation.</returns>
    internal byte ShortReadShim(byte address) {
        return ShortRead(address);
    }

    /// <summary>
    /// Performs the short write shim operation.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    internal void ShortWriteShim(byte address, byte value) {
        ShortWrite(address, value);
    }

    private static void ChipIdealCtor(Ym7128BChipIdeal chip) {
        ArgumentNullException.ThrowIfNull(chip);
        chip.Buffer = null;
        chip.Length = 0;
        chip.SampleRate = 0;
        chip.T0D = 0;
        chip.Tail = 0;
        Array.Clear(chip.Registers, 0, chip.Registers.Length);
        Array.Clear(chip.Gains, 0, chip.Gains.Length);
        Array.Clear(chip.Taps, 0, chip.Taps.Length);
    }

    private static void ChipIdealDtor(Ym7128BChipIdeal chip) {
        ArgumentNullException.ThrowIfNull(chip);
        chip.Buffer = null;
        chip.Length = 0;
    }

    private static void ChipIdealReset(Ym7128BChipIdeal chip) {
        ArgumentNullException.ThrowIfNull(chip);

        for (byte address = Ym7128BDatasheetSpecs.AddressMin; address <= Ym7128BDatasheetSpecs.AddressMax; address++) {
            ChipIdealWrite(chip, address, 0);
        }
    }

    private static void ChipIdealStart(Ym7128BChipIdeal chip) {
        ArgumentNullException.ThrowIfNull(chip);

        chip.T0D = 0;
        chip.Tail = 0;

        if (chip.Buffer != null) {
            Array.Clear(chip.Buffer, 0, chip.Buffer.Length);
        }
    }

    private static void ChipIdealStop(Ym7128BChipIdeal chip) {
        ArgumentNullException.ThrowIfNull(chip);
    }

    private static void ChipIdealProcess(Ym7128BChipIdeal chip, Ym7128BChipIdealProcessData data) {
        ArgumentNullException.ThrowIfNull(chip);
        ArgumentNullException.ThrowIfNull(data);

        if (chip.Buffer == null || chip.Length == 0) {
            return;
        }

        float input = data.Inputs[(int)Ym7128BInputChannel.Mono];

        nuint t0 = chip.Tail + chip.Taps[0];
        nuint filterHead = t0 >= chip.Length ? t0 - chip.Length : t0;
        float filterT0 = chip.Buffer[(int)filterHead];
        float filterD = chip.T0D;
        chip.T0D = filterT0;
        float filterC0 = Ym7128BHelpers.MulFloat(filterT0, chip.Gains[(int)Ym7128BRegister.C0]);
        float filterC1 = Ym7128BHelpers.MulFloat(filterD, chip.Gains[(int)Ym7128BRegister.C1]);
        float filterSum = Ym7128BHelpers.AddFloat(filterC0, filterC1);
        float filterVc = Ym7128BHelpers.MulFloat(filterSum, chip.Gains[(int)Ym7128BRegister.Vc]);

        float inputVm = Ym7128BHelpers.MulFloat(input, chip.Gains[(int)Ym7128BRegister.Vm]);
        float inputSum = Ym7128BHelpers.AddFloat(inputVm, filterVc);

        chip.Tail = chip.Tail != 0 ? chip.Tail - 1 : chip.Length - 1;
        chip.Buffer[(int)chip.Tail] = inputSum;

        for (byte channel = 0; channel < (byte)Ym7128BOutputChannel.Count; channel++) {
            int gainBase = (int)Ym7128BRegister.Gl1 + (channel * Ym7128BDatasheetSpecs.GainLaneCount);
            float accum = 0;

            for (byte tap = 1; tap < Ym7128BDatasheetSpecs.TapCount; tap++) {
                nuint t = chip.Tail + chip.Taps[tap];
                nuint head = t >= chip.Length ? t - chip.Length : t;
                float buffered = chip.Buffer[(int)head];
                float g = chip.Gains[gainBase + tap - 1];
                float bufferedG = Ym7128BHelpers.MulFloat(buffered, g);
                accum += bufferedG;
            }

            float total = accum;
            float v = chip.Gains[(int)Ym7128BRegister.Vl + channel];
            float totalV = Ym7128BHelpers.MulFloat(total, v);
            float oversampled = Ym7128BHelpers.MulFloat(totalV, 1.0f / Ym7128BDatasheetSpecs.Oversampling);
            data.Outputs[channel] = oversampled;
        }
    }

    private static byte ChipIdealRead(Ym7128BChipIdeal chip, byte address) {
        ArgumentNullException.ThrowIfNull(chip);

        return address switch {
            < (byte)Ym7128BRegister.C0 => (byte)(chip.Registers[address] & Ym7128BDatasheetSpecs.GainDataMask),
            < (byte)Ym7128BRegister.T0 => (byte)(chip.Registers[address] & Ym7128BDatasheetSpecs.CoeffValueMask),
            < (byte)Ym7128BRegister.Count => (byte)(chip.Registers[address] & Ym7128BDatasheetSpecs.TapValueMask),
            _ => 0
        };
    }

    private static void ChipIdealWrite(Ym7128BChipIdeal chip, byte address, byte value) {
        ArgumentNullException.ThrowIfNull(chip);

        switch (address) {
            case < (byte)Ym7128BRegister.C0:
                chip.Registers[address] = (byte)(value & Ym7128BDatasheetSpecs.GainDataMask);
                chip.Gains[address] = Ym7128BHelpers.RegisterToGainFloat(value);
                break;
            case < (byte)Ym7128BRegister.T0:
                chip.Registers[address] = (byte)(value & Ym7128BDatasheetSpecs.CoeffValueMask);
                chip.Gains[address] = Ym7128BHelpers.RegisterToCoeffFloat(value);
                break;
            case < (byte)Ym7128BRegister.Count:
                chip.Registers[address] = (byte)(value & Ym7128BDatasheetSpecs.TapValueMask);
                chip.Taps[address - (byte)Ym7128BRegister.T0] =
                    Ym7128BHelpers.RegisterToTapIdeal(value, chip.SampleRate);
                break;
        }
    }

    private static void ChipIdealSetup(Ym7128BChipIdeal chip, nuint sampleRate) {
        ArgumentNullException.ThrowIfNull(chip);

        if (chip.SampleRate == sampleRate && chip.Buffer != null) {
            return;
        }

        chip.SampleRate = sampleRate;
        chip.Buffer = null;
        chip.Length = 0;

        if (sampleRate < 10) {
            return;
        }

        nuint length = (sampleRate / 10) + 1;
        chip.Length = length;
        chip.Buffer = new float[(int)length];

        for (byte i = 0; i < Ym7128BDatasheetSpecs.TapCount; i++) {
            byte data = chip.Registers[i + (int)Ym7128BRegister.T0];
            chip.Taps[i] = Ym7128BHelpers.RegisterToTapIdeal(data, chip.SampleRate);
        }
    }
}