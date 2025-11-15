namespace Spice86.Core.Emulator.OperatingSystem.Devices;

using Serilog.Events;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Interfaces;

using System.Diagnostics.CodeAnalysis;
using System.IO;

/// <summary>
/// Represents null device.
/// </summary>
public class NullDevice : VirtualDeviceBase {
    private const string NUL = "NUL";
    private readonly ILoggerService _loggerService;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="loggerService">The logger service.</param>
    /// <param name="memory">The memory.</param>
    /// <param name="baseAddress">The base address.</param>
    public NullDevice(ILoggerService loggerService, IByteReaderWriter memory, uint baseAddress)
        : base(new DosDeviceHeader(memory, baseAddress) {
            Attributes = Enums.DeviceAttributes.CurrentNull,
            Name = NUL,
            NextDevicePointer = new Spice86.Shared.Emulator.Memory.SegmentedAddress(0xFFFF, 0xFFFF)
        }) {
        _loggerService = loggerService;
    }

    /// <summary>
    /// The name.
    /// </summary>
    public override string Name => NUL;
    /// <summary>
    /// The information.
    /// </summary>
    public override ushort Information => 0x8084;
    /// <summary>
    /// The can read.
    /// </summary>
    public override bool CanRead => true;
    /// <summary>
    /// The can seek.
    /// </summary>
    public override bool CanSeek => true;
    /// <summary>
    /// The can write.
    /// </summary>
    public override bool CanWrite => true;
    /// <summary>
    /// The length.
    /// </summary>
    public override long Length => 0;
    /// <summary>
    /// Gets or sets position.
    /// </summary>
    public override long Position { get; set; }

    /// <summary>
    /// Performs the flush operation.
    /// </summary>
    public override void Flush() {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Flushing {@Device}", this);
        }
    }

    public override int Read(byte[] buffer, int offset, int count) {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Reading {@Device}", this);
        }
        return 0;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Seeking {@Device}", this);
        }
        return 0;
    }

    /// <summary>
    /// Sets length.
    /// </summary>
    /// <param name="value">The value.</param>
    public override void SetLength(long value) {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Setting length {@Device}", this);
        }
    }

    /// <summary>
    /// Performs the try read from control channel operation.
    /// </summary>
    /// <param name="true">The true.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public override bool TryReadFromControlChannel(uint address, ushort size,
        [NotNullWhen(true)] out ushort? returnCode) {
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Reading from control channel of {@Device}", this);
        }

        returnCode = null;
        return false;
    }

    /// <summary>
    /// Performs the try write to control channel operation.
    /// </summary>
    /// <param name="true">The true.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public override bool TryWriteToControlChannel(uint address, ushort size,
        [NotNullWhen(true)] out ushort? returnCode) {
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Writing to control channel of {@Device}", this);
        }
        returnCode = null;
        return false;
    }

    public override void Write(byte[] buffer, int offset, int count) {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Writing {@Device}", this);
        }
    }
}