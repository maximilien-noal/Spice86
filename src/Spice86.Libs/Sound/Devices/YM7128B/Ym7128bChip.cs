namespace Spice86.Libs.Sound.Devices.YM7128B;

/*
BSD 2-Clause License

Copyright (c) 2020-2023, Andrea Zoppi
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

/// <summary>
/// Represents ym 7128 b chip.
/// </summary>
internal sealed partial class Ym7128BChip {
    private Ym7128BChipIdeal Ideal { get; } = new();
}

/// <summary>
/// Defines ym 7128 b register values.
/// </summary>
internal enum Ym7128BRegister : byte {
    Gl1 = 0,
    Gl2,
    Gl3,
    Gl4,
    Gl5,
    Gl6,
    Gl7,
    Gl8,
    Gr1,
    Gr2,
    Gr3,
    Gr4,
    Gr5,
    Gr6,
    Gr7,
    Gr8,
    Vm,
    Vc,
    Vl,
    Vr,
    C0,
    C1,
    T0,
    T1,
    T2,
    T3,
    T4,
    T5,
    T6,
    T7,
    T8,
    Count
}

/// <summary>
/// Defines ym 7128 b input channel values.
/// </summary>
internal enum Ym7128BInputChannel : byte {
    Mono = 0,
    Count
}

/// <summary>
/// Defines ym 7128 b output channel values.
/// </summary>
internal enum Ym7128BOutputChannel : byte {
    Left = 0,
    Right,
    Count
}

/// <summary>
/// Represents ym 7128 b datasheet specs.
/// </summary>
internal static class Ym7128BDatasheetSpecs {
    /// <summary>
    /// The clock rate.
    /// </summary>
    internal const int ClockRate = 7_159_090;
    /// <summary>
    /// The write rate.
    /// </summary>
    internal const int WriteRate = ClockRate / 8 / (8 + 1 + 8 + 1);
    /// <summary>
    /// The input rate.
    /// </summary>
    internal const int InputRate = (ClockRate + (304 / 2)) / 304;
    /// <summary>
    /// The oversampling.
    /// </summary>
    internal const int Oversampling = 2;
    /// <summary>
    /// The output rate.
    /// </summary>
    internal const int OutputRate = InputRate * Oversampling;
    /// <summary>
    /// The oversampler length.
    /// </summary>
    internal const int OversamplerLength = 19;
    /// <summary>
    /// The address min.
    /// </summary>
    internal const byte AddressMin = 0;
    /// <summary>
    /// The address max.
    /// </summary>
    internal const byte AddressMax = (byte)(Ym7128BRegister.Count - 1);
    /// <summary>
    /// The buffer length.
    /// </summary>
    internal const int BufferLength = (InputRate / 10) + 1;
    /// <summary>
    /// The tap count.
    /// </summary>
    internal const int TapCount = 9;
    /// <summary>
    /// The tap value bits.
    /// </summary>
    internal const int TapValueBits = 5;
    /// <summary>
    /// The tap value count.
    /// </summary>
    internal const int TapValueCount = 1 << TapValueBits;
    /// <summary>
    /// The tap value mask.
    /// </summary>
    internal const int TapValueMask = TapValueCount - 1;
    /// <summary>
    /// The gain lane count.
    /// </summary>
    internal const int GainLaneCount = 8;
    /// <summary>
    /// The gain data bits.
    /// </summary>
    internal const int GainDataBits = 6;
    /// <summary>
    /// The gain data count.
    /// </summary>
    internal const int GainDataCount = 1 << GainDataBits;
    /// <summary>
    /// The gain data mask.
    /// </summary>
    internal const int GainDataMask = GainDataCount - 1;
    /// <summary>
    /// The gain data sign.
    /// </summary>
    internal const int GainDataSign = 1 << (GainDataBits - 1);
    /// <summary>
    /// The coeff count.
    /// </summary>
    internal const int CoeffCount = 2;
    /// <summary>
    /// The coeff value bits.
    /// </summary>
    internal const int CoeffValueBits = 6;
    /// <summary>
    /// The coeff value count.
    /// </summary>
    internal const int CoeffValueCount = 1 << CoeffValueBits;
    /// <summary>
    /// The coeff value mask.
    /// </summary>
    internal const int CoeffValueMask = CoeffValueCount - 1;
}

/// <summary>
/// Represents ym 7128 b implementation specs.
/// </summary>
internal static class Ym7128BImplementationSpecs {
    /// <summary>
    /// The fixed bits.
    /// </summary>
    internal const int FixedBits = sizeof(short) * 8;
    /// <summary>
    /// The fixed mask.
    /// </summary>
    internal const int FixedMask = (1 << FixedBits) - 1;
    /// <summary>
    /// The fixed decimals.
    /// </summary>
    internal const int FixedDecimals = FixedBits - 1;
    /// <summary>
    /// The fixed rounding.
    /// </summary>
    internal const int FixedRounding = 1 << (FixedDecimals - 1);
    /// <summary>
    /// The fixed max.
    /// </summary>
    internal const int FixedMax = (1 << FixedDecimals) - 1;
    /// <summary>
    /// The fixed min.
    /// </summary>
    internal const int FixedMin = -FixedMax;
    /// <summary>
    /// The signal bits.
    /// </summary>
    internal const int SignalBits = 14;
    /// <summary>
    /// The signal clear bits.
    /// </summary>
    internal const int SignalClearBits = FixedBits - SignalBits;
    /// <summary>
    /// The signal clear mask.
    /// </summary>
    internal const int SignalClearMask = (1 << SignalClearBits) - 1;
    /// <summary>
    /// The signal mask.
    /// </summary>
    internal const int SignalMask = FixedMask - SignalClearMask;
    /// <summary>
    /// The signal decimals.
    /// </summary>
    internal const int SignalDecimals = SignalBits - 1;
    /// <summary>
    /// The operand bits.
    /// </summary>
    internal const int OperandBits = FixedBits;
    /// <summary>
    /// The operand clear bits.
    /// </summary>
    internal const int OperandClearBits = FixedBits - OperandBits;
    /// <summary>
    /// The operand clear mask.
    /// </summary>
    internal const int OperandClearMask = (1 << OperandClearBits) - 1;
    /// <summary>
    /// The operand mask.
    /// </summary>
    internal const int OperandMask = FixedMask - OperandClearMask;
    /// <summary>
    /// The operand decimals.
    /// </summary>
    internal const int OperandDecimals = OperandBits - 1;
    /// <summary>
    /// The gain bits.
    /// </summary>
    internal const int GainBits = 12;
    /// <summary>
    /// The gain clear bits.
    /// </summary>
    internal const int GainClearBits = FixedBits - GainBits;
    /// <summary>
    /// The gain clear mask.
    /// </summary>
    internal const int GainClearMask = (1 << GainClearBits) - 1;
    /// <summary>
    /// The gain mask.
    /// </summary>
    internal const int GainMask = FixedMask - GainClearMask;
    /// <summary>
    /// The gain decimals.
    /// </summary>
    internal const int GainDecimals = GainBits - 1;
    /// <summary>
    /// The gain max.
    /// </summary>
    internal const int GainMax = (1 << (FixedBits - 1)) - 1;
    /// <summary>
    /// The gain min.
    /// </summary>
    internal const int GainMin = -GainMax;
    /// <summary>
    /// The coeff bits.
    /// </summary>
    internal const int CoeffBits = GainBits;
    /// <summary>
    /// The coeff clear bits.
    /// </summary>
    internal const int CoeffClearBits = FixedBits - CoeffBits;
    /// <summary>
    /// The coeff clear mask.
    /// </summary>
    internal const int CoeffClearMask = (1 << CoeffClearBits) - 1;
    /// <summary>
    /// The coeff mask.
    /// </summary>
    internal const int CoeffMask = FixedMask - CoeffClearMask;
    /// <summary>
    /// The coeff decimals.
    /// </summary>
    internal const int CoeffDecimals = CoeffBits - 1;
}

/*
typedef struct YM7128B_OversamplerFixed
{
    YM7128B_Fixed buffer_[YM7128B_Oversampler_Length];
} YM7128B_OversamplerFixed;
*/
/// <summary>
/// Represents ym 7128 b oversampler fixed.
/// </summary>
internal sealed class Ym7128BOversamplerFixed {
    /// <summary>
    /// Gets buffer.
    /// </summary>
    internal short[] Buffer { get; } = new short[Ym7128BDatasheetSpecs.OversamplerLength];
}

/*
typedef struct YM7128B_OversamplerFloat
{
    YM7128B_Float buffer_[YM7128B_Oversampler_Length];
} YM7128B_OversamplerFloat;
*/
/// <summary>
/// Represents ym 7128 b oversampler float.
/// </summary>
internal sealed class Ym7128BOversamplerFloat {
    /// <summary>
    /// Gets buffer.
    /// </summary>
    internal float[] Buffer { get; } = new float[Ym7128BDatasheetSpecs.OversamplerLength];
}

/*
typedef struct YM7128B_ChipIdeal
{
    YM7128B_Register regs_[YM7128B_Reg_Count];
    YM7128B_Float gains_[YM7128B_Reg_T0];
    YM7128B_TapIdeal taps_[YM7128B_Tap_Count];
    YM7128B_Float t0_d_;
    YM7128B_TapIdeal tail_;
    YM7128B_Float* buffer_;
    YM7128B_TapIdeal length_;
    YM7128B_TapIdeal sample_rate_;
} YM7128B_ChipIdeal;
*/
/// <summary>
/// Represents ym 7128 b chip ideal.
/// </summary>
internal sealed class Ym7128BChipIdeal {
    /// <summary>
    /// The buffer.
    /// </summary>
    internal float[]? Buffer;
    /// <summary>
    /// The length.
    /// </summary>
    internal nuint Length;
    /// <summary>
    /// The sample rate.
    /// </summary>
    internal nuint SampleRate;
    /// <summary>
    /// The t0 d.
    /// </summary>
    internal float T0D;
    /// <summary>
    /// The tail.
    /// </summary>
    internal nuint Tail;
    /// <summary>
    /// Gets registers.
    /// </summary>
    internal byte[] Registers { get; } = new byte[(int)Ym7128BRegister.Count];
    /// <summary>
    /// Gets gains.
    /// </summary>
    internal float[] Gains { get; } = new float[(int)Ym7128BRegister.T0];
    /// <summary>
    /// Gets taps.
    /// </summary>
    internal nuint[] Taps { get; } = new nuint[Ym7128BDatasheetSpecs.TapCount];
}

/*
typedef struct YM7128B_ChipIdeal_Process_Data
{
    YM7128B_Float inputs[YM7128B_InputChannel_Count];
    YM7128B_Float outputs[YM7128B_OutputChannel_Count];
} YM7128B_ChipIdeal_Process_Data;
*/
/// <summary>
/// Represents ym 7128 b chip ideal process data.
/// </summary>
internal sealed class Ym7128BChipIdealProcessData {
    /// <summary>
    /// Gets inputs.
    /// </summary>
    internal float[] Inputs { get; } = new float[(int)Ym7128BInputChannel.Count];
    /// <summary>
    /// Gets outputs.
    /// </summary>
    internal float[] Outputs { get; } = new float[(int)Ym7128BOutputChannel.Count];
}

/*
typedef struct YM7128B_ChipFixed
{
    YM7128B_Register regs_[YM7128B_Reg_Count];
    YM7128B_Fixed gains_[YM7128B_Reg_T0];
    YM7128B_Tap taps_[YM7128B_Tap_Count];
    YM7128B_Fixed t0_d_;
    YM7128B_Tap tail_;
    YM7128B_Fixed buffer_[YM7128B_Buffer_Length];
    YM7128B_OversamplerFixed oversampler_[YM7128B_OutputChannel_Count];
} YM7128B_ChipFixed;
*/
/// <summary>
/// Represents ym 7128 b chip fixed.
/// </summary>
internal sealed class Ym7128BChipFixed {
    /// <summary>
    /// The t0 d.
    /// </summary>
    internal short T0D;
    /// <summary>
    /// The tail.
    /// </summary>
    internal ushort Tail;
    /// <summary>
    /// Gets registers.
    /// </summary>
    internal byte[] Registers { get; } = new byte[(int)Ym7128BRegister.Count];
    /// <summary>
    /// Gets gains.
    /// </summary>
    internal short[] Gains { get; } = new short[(int)Ym7128BRegister.T0];
    /// <summary>
    /// Gets taps.
    /// </summary>
    internal ushort[] Taps { get; } = new ushort[Ym7128BDatasheetSpecs.TapCount];
    /// <summary>
    /// Gets buffer.
    /// </summary>
    internal short[] Buffer { get; } = new short[Ym7128BDatasheetSpecs.BufferLength];

    /// <summary>
    /// Gets oversamplers.
    /// </summary>
    internal Ym7128BOversamplerFixed[] Oversamplers { get; } = [
        new(),
        new()
    ];
}

/*
typedef struct YM7128B_ChipFixed_Process_Data
{
    YM7128B_Fixed inputs[YM7128B_InputChannel_Count];
    YM7128B_Fixed outputs[YM7128B_OutputChannel_Count][YM7128B_Oversampling];
} YM7128B_ChipFixed_Process_Data;
*/
/// <summary>
/// Represents ym 7128 b chip fixed process data.
/// </summary>
internal sealed class Ym7128BChipFixedProcessData {
    /// <summary>
    /// Gets inputs.
    /// </summary>
    internal short[] Inputs { get; } = new short[(int)Ym7128BInputChannel.Count];
    /// <summary>
    /// Gets outputs.
    /// </summary>
    internal short[,] Outputs { get; } = new short[(int)Ym7128BOutputChannel.Count, Ym7128BDatasheetSpecs.Oversampling];
}

/*
typedef struct YM7128B_ChipFloat
{
    YM7128B_Register regs_[YM7128B_Reg_Count];
    YM7128B_Float gains_[YM7128B_Reg_T0];
    YM7128B_Tap taps_[YM7128B_Tap_Count];
    YM7128B_Float t0_d_;
    YM7128B_Tap tail_;
    YM7128B_Float buffer_[YM7128B_Buffer_Length];
    YM7128B_OversamplerFloat oversampler_[YM7128B_OutputChannel_Count];
} YM7128B_ChipFloat;
*/
/// <summary>
/// Represents ym 7128 b chip float.
/// </summary>
internal sealed class Ym7128BChipFloat {
    /// <summary>
    /// The t0 d.
    /// </summary>
    internal float T0D;
    /// <summary>
    /// The tail.
    /// </summary>
    internal ushort Tail;
    /// <summary>
    /// Gets registers.
    /// </summary>
    internal byte[] Registers { get; } = new byte[(int)Ym7128BRegister.Count];
    /// <summary>
    /// Gets gains.
    /// </summary>
    internal float[] Gains { get; } = new float[(int)Ym7128BRegister.T0];
    /// <summary>
    /// Gets taps.
    /// </summary>
    internal ushort[] Taps { get; } = new ushort[Ym7128BDatasheetSpecs.TapCount];
    /// <summary>
    /// Gets buffer.
    /// </summary>
    internal float[] Buffer { get; } = new float[Ym7128BDatasheetSpecs.BufferLength];

    /// <summary>
    /// Gets oversamplers.
    /// </summary>
    internal Ym7128BOversamplerFloat[] Oversamplers { get; } = [
        new(),
        new()
    ];
}

/*
typedef struct YM7128B_ChipFloat_Process_Data
{
    YM7128B_Float inputs[YM7128B_InputChannel_Count];
    YM7128B_Float outputs[YM7128B_OutputChannel_Count][YM7128B_Oversampling];
} YM7128B_ChipFloat_Process_Data;
*/
/// <summary>
/// Represents ym 7128 b chip float process data.
/// </summary>
internal sealed class Ym7128BChipFloatProcessData {
    /// <summary>
    /// Gets inputs.
    /// </summary>
    internal float[] Inputs { get; } = new float[(int)Ym7128BInputChannel.Count];
    /// <summary>
    /// Gets outputs.
    /// </summary>
    internal float[,] Outputs { get; } = new float[(int)Ym7128BOutputChannel.Count, Ym7128BDatasheetSpecs.Oversampling];
}

/*
typedef struct YM7128B_ChipShort
{
    YM7128B_Register regs_[YM7128B_Reg_Count];
    YM7128B_Fixed gains_[YM7128B_Reg_T0];
    YM7128B_TapIdeal taps_[YM7128B_Tap_Count];
    YM7128B_Fixed t0_d_;
    YM7128B_TapIdeal tail_;
    YM7128B_Fixed* buffer_;
    YM7128B_TapIdeal length_;
    YM7128B_TapIdeal sample_rate_;
} YM7128B_ChipShort;
*/
/// <summary>
/// Represents ym 7128 b chip short.
/// </summary>
internal sealed class Ym7128BChipShort {
    /// <summary>
    /// The buffer.
    /// </summary>
    internal short[]? Buffer;
    /// <summary>
    /// The length.
    /// </summary>
    internal nuint Length;
    /// <summary>
    /// The sample rate.
    /// </summary>
    internal nuint SampleRate;
    /// <summary>
    /// The t0 d.
    /// </summary>
    internal short T0D;
    /// <summary>
    /// The tail.
    /// </summary>
    internal nuint Tail;
    /// <summary>
    /// Gets registers.
    /// </summary>
    internal byte[] Registers { get; } = new byte[(int)Ym7128BRegister.Count];
    /// <summary>
    /// Gets gains.
    /// </summary>
    internal short[] Gains { get; } = new short[(int)Ym7128BRegister.T0];
    /// <summary>
    /// Gets taps.
    /// </summary>
    internal nuint[] Taps { get; } = new nuint[Ym7128BDatasheetSpecs.TapCount];
}

/*
typedef struct YM7128B_ChipShort_Process_Data
{
    YM7128B_Fixed inputs[YM7128B_InputChannel_Count];
    YM7128B_Fixed outputs[YM7128B_OutputChannel_Count];
} YM7128B_ChipShort_Process_Data;
*/
/// <summary>
/// Represents ym 7128 b chip short process data.
/// </summary>
internal sealed class Ym7128BChipShortProcessData {
    /// <summary>
    /// Gets inputs.
    /// </summary>
    internal short[] Inputs { get; } = new short[(int)Ym7128BInputChannel.Count];
    /// <summary>
    /// Gets outputs.
    /// </summary>
    internal short[] Outputs { get; } = new short[(int)Ym7128BOutputChannel.Count];
}