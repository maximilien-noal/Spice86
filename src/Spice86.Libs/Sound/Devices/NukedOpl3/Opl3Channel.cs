// SPDX-FileCopyrightText: 2013-2025 Nuked-OPL3 by nukeykt
// SPDX-License-Identifier: LGPL-2.1

namespace Spice86.Libs.Sound.Devices.NukedOpl3;

// struct _opl3_channel {
//     opl3_slot *slotz[2];/*Don't use "slots" keyword to avoid conflict with Qt applications*/
//     opl3_channel *pair;
//     opl3_chip *chip;
//     int16_t *out[4];
//
// #if OPL_ENABLE_STEREOEXT
//     int32_t leftpan;
//     int32_t rightpan;
// #endif
//
//     uint8_t chtype;
//     uint16_t f_num;
//     uint8_t block;
//     uint8_t fb;
//     uint8_t con;
//     uint8_t alg;
//     uint8_t ksv;
//     uint16_t cha, chb;
//     uint16_t chc, chd;
//     uint8_t ch_num;
// };
/// <summary>
/// Represents opl 3 channel.
/// </summary>
internal sealed class Opl3Channel {
    /// <summary>
    /// Gets slotz.
    /// </summary>
    internal Opl3Operator[] Slotz { get; } = new Opl3Operator[2];
    /// <summary>
    /// Gets or sets pair.
    /// </summary>
    internal Opl3Channel? Pair { get; set; }
    /// <summary>
    /// Gets or sets chip.
    /// </summary>
    internal Opl3Chip? Chip { get; set; }
    /// <summary>
    /// Gets out.
    /// </summary>
    internal ShortSignalSource[] Out { get; } = new ShortSignalSource[4];
#if OPL_ENABLE_STEREOEXT
    /// <summary>
    /// The left pan.
    /// </summary>
    internal int LeftPan;
    /// <summary>
    /// The right pan.
    /// </summary>
    internal int RightPan;
#endif
    /// <summary>
    /// The channel type.
    /// </summary>
    internal ChannelType ChannelType;
    /// <summary>
    /// The f number.
    /// </summary>
    internal ushort FNumber;
    /// <summary>
    /// The block.
    /// </summary>
    internal byte Block;
    /// <summary>
    /// The feedback.
    /// </summary>
    internal byte Feedback;
    /// <summary>
    /// The connection.
    /// </summary>
    internal byte Connection;
    /// <summary>
    /// The algorithm.
    /// </summary>
    internal byte Algorithm;
    /// <summary>
    /// The key scale value.
    /// </summary>
    internal byte KeyScaleValue;
    /// <summary>
    /// The cha.
    /// </summary>
    internal ushort Cha;
    /// <summary>
    /// The chb.
    /// </summary>
    internal ushort Chb;
    /// <summary>
    /// The chc.
    /// </summary>
    internal ushort Chc;
    /// <summary>
    /// The chd.
    /// </summary>
    internal ushort Chd;
    /// <summary>
    /// The channel number.
    /// </summary>
    internal byte ChannelNumber;
}