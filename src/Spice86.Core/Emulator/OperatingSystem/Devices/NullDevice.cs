namespace Spice86.Core.Emulator.OperatingSystem.Devices;

using Serilog.Events;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Interfaces;

using System.Diagnostics.CodeAnalysis;
using System.IO;

/// <summary>
/// Represents the NullDevice class.
/// </summary>
public class NullDevice : VirtualDeviceBase {
    private const string NUL = "NUL";
    private readonly ILoggerService _loggerService;
    public NullDevice(ILoggerService loggerService, IByteReaderWriter memory, uint baseAddress)
        : base(new DosDeviceHeader(memory, baseAddress) {
            Attributes = Enums.DeviceAttributes.CurrentNull,
            Name = NUL,
            NextDevicePointer = new Spice86.Shared.Emulator.Memory.SegmentedAddress(0xFFFF, 0xFFFF)
        }) {
        _loggerService = loggerService;
    }

    /// <summary>
    /// The string.
    /// </summary>
    public override string Name => NUL;
    /// <summary>
    /// The ushort.
    /// </summary>
    public override ushort Information => 0x8084;
    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanRead => true;
    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanSeek => true;
    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanWrite => true;
    /// <summary>
    /// The long.
    /// </summary>
    public override long Length => 0;
    /// <summary>
    /// Gets or sets the long.
    /// </summary>
    public override long Position { get; set; }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Flush() {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Flushing {@Device}", this);
        }
    }

    /// <summary>
    /// int method.
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count) {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Reading {@Device}", this);
        }
        return 0;
    }

    /// <summary>
    /// long method.
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin) {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Seeking {@Device}", this);
        }
        return 0;
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void SetLength(long value) {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Setting length {@Device}", this);
        }
    }

    /// <summary>
    /// bool method.
    /// </summary>
    public override bool TryReadFromControlChannel(uint address, ushort size,
        [NotNullWhen(true)] out ushort? returnCode) {
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Reading from control channel of {@Device}", this);
        }

        returnCode = null;
        return false;
    }

    /// <summary>
    /// bool method.
    /// </summary>
    public override bool TryWriteToControlChannel(uint address, ushort size,
        [NotNullWhen(true)] out ushort? returnCode) {
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Writing to control channel of {@Device}", this);
        }
        returnCode = null;
        return false;
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count) {
        // No-op for null device
        if (_loggerService.IsEnabled(LogEventLevel.Verbose)) {
            _loggerService.Verbose("Writing {@Device}", this);
        }
    }
}