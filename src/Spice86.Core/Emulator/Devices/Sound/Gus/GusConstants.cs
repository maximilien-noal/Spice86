namespace Spice86.Core.Emulator.Devices.Sound.Gus;

/// <summary>
/// Constants for Gravis UltraSound emulation.
/// Based on the UltraSound SDK documentation and DOSBox Staging implementation.
/// </summary>
public static class GusConstants {
    /// <summary>
    /// Output sample rate of the GUS DAC (44.1 kHz).
    /// </summary>
    public const int OutputSampleRate = 44100;

    /// <summary>
    /// Size of GUS onboard RAM (1 MB).
    /// </summary>
    public const int RamSize = 1024 * 1024;

    /// <summary>
    /// Maximum number of hardware voice channels (32).
    /// </summary>
    public const int MaxVoices = 32;

    /// <summary>
    /// Minimum number of active voices (14).
    /// </summary>
    public const int MinVoices = 14;

    /// <summary>
    /// Default state for voice control registers.
    /// </summary>
    public const byte VoiceDefaultState = 3;

    /// <summary>
    /// Default pan position (center).
    /// </summary>
    public const byte PanDefaultPosition = 7;

    /// <summary>
    /// Number of pan positions (0-15).
    /// </summary>
    public const int PanPositions = 16;

    /// <summary>
    /// Timer 1 default delay in milliseconds.
    /// </summary>
    public const double Timer1DefaultDelay = 0.080;

    /// <summary>
    /// Timer 2 default delay in milliseconds.
    /// </summary>
    public const double Timer2DefaultDelay = 0.320;

    /// <summary>
    /// Volume level scaling factor (0.0235 dB increments).
    /// </summary>
    public const double DeltaDb = 0.002709201;

    /// <summary>
    /// Volume index increment scalar for fractional volume changes.
    /// </summary>
    public const int VolumeIncScalar = 512;

    /// <summary>
    /// Total number of volume levels (4096).
    /// </summary>
    public const int VolumeLevels = 4096;

    /// <summary>
    /// Wave interpolation width (9 bits).
    /// </summary>
    public const int WaveWidth = 1 << 9;

    /// <summary>
    /// Default value for AdLib command register.
    /// </summary>
    public const byte AdlibCommandDefault = 85;

    /// <summary>
    /// Valid IRQ addresses for GUS (indices into hardware selection).
    /// </summary>
    public static readonly byte[] IrqAddresses = [0, 2, 5, 3, 7, 11, 12, 15];

    /// <summary>
    /// Valid DMA addresses for GUS (indices into hardware selection).
    /// </summary>
    public static readonly byte[] DmaAddresses = [0, 1, 3, 5, 6, 7];

    /// <summary>
    /// Environment variable name for GUS configuration.
    /// Format: ULTRASND=port,dma1,dma2,irq1,irq2
    /// </summary>
    public const string UltrasndEnvName = "ULTRASND";

    /// <summary>
    /// Environment variable name for GUS data directory.
    /// </summary>
    public const string UltradirEnvName = "ULTRADIR";

    /// <summary>
    /// Default base port for GUS (240h).
    /// </summary>
    public const ushort DefaultBasePort = 0x240;

    /// <summary>
    /// Default IRQ for GUS (5).
    /// </summary>
    public const byte DefaultIrq = 5;

    /// <summary>
    /// Default DMA channel for GUS (3).
    /// </summary>
    public const byte DefaultDma = 3;

    /// <summary>
    /// Bytes per DMA transfer.
    /// </summary>
    public const int BytesPerDmaTransfer = 8 * 1024;

    /// <summary>
    /// ISA bus throughput in bytes per second (32 MB/s).
    /// </summary>
    public const int IsaBusThroughput = 32 * 1024 * 1024;

    /// <summary>
    /// Default mix control register state (lines disabled, latches enabled).
    /// </summary>
    public const byte MixControlRegisterDefaultState = 0b0000_1011;

    // DMA address manipulation masks for 16-bit mode
    // In 16-bit mode, the address layout uses a different bit arrangement

    /// <summary>
    /// Upper bits mask for 16-bit DMA address (bits 18-19).
    /// </summary>
    public const uint Dma16BitUpperMask = 0b1100_0000_0000_0000;

    /// <summary>
    /// Lower bits mask for 16-bit DMA address (bits 0-16).
    /// </summary>
    public const uint Dma16BitLowerMask = 0b0001_1111_1111_1111;

    /// <summary>
    /// Upper bits mask for 16-bit DMA 20-bit address.
    /// </summary>
    public const uint Dma16BitAddr20UpperMask = 0b1100_0000_0000_0000_0000;

    /// <summary>
    /// Lower bits mask for 16-bit DMA 20-bit address (shifted by 1).
    /// </summary>
    public const uint Dma16BitAddr20LowerMask = 0b0011_1111_1111_1111_1110;

    /// <summary>
    /// Address mask for 8-bit DMA (upper 16 bits of 20-bit address).
    /// </summary>
    public const uint Dma8BitAddressMask = 0b1111_1111_1111_1111_0000;

    /// <summary>
    /// Nibble mask for DMA address (lower 4 bits).
    /// </summary>
    public const uint DmaNibbleMask = 0xf;
}
