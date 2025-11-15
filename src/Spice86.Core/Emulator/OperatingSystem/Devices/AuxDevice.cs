namespace Spice86.Core.Emulator.OperatingSystem.Devices;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Shared.Interfaces;

using System.IO;

/// <summary>
/// Represents aux device.
/// </summary>
public class AuxDevice : CharacterDevice {
    private readonly ILoggerService _loggerService;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="loggerService">The logger service.</param>
    /// <param name="memory">The memory.</param>
    /// <param name="baseAddress">The base address.</param>
    public AuxDevice(ILoggerService loggerService,
        IByteReaderWriter memory, uint baseAddress)
        : base(memory, baseAddress, "AUX") {
        _loggerService = loggerService;
    }

    /// <summary>
    /// The name.
    /// </summary>
    public override string Name => "AUX";

    /// <summary>
    /// The alias.
    /// </summary>
    public override string? Alias => "COM1";

    /// <summary>
    /// Gets information.
    /// </summary>
    public override ushort Information { get; }
    /// <summary>
    /// Gets can read.
    /// </summary>
    public override bool CanRead { get; }
    /// <summary>
    /// Gets can seek.
    /// </summary>
    public override bool CanSeek { get; }
    /// <summary>
    /// Gets can write.
    /// </summary>
    public override bool CanWrite { get; }
    /// <summary>
    /// Gets length.
    /// </summary>
    public override long Length { get; }
    /// <summary>
    /// Gets or sets position.
    /// </summary>
    public override long Position { get; set; }

    /// <summary>
    /// Performs the flush operation.
    /// </summary>
    public override void Flush() {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Flushing {@Device}", this);
        }
        // No-op for aux device
    }

    public override int Read(byte[] buffer, int offset, int count) {
        // No-op for aux device
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Reading {@Device}", this);
        }
        return 0;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Seeking {@Device}", this);
        }
        return 0;
    }

    /// <summary>
    /// Sets length.
    /// </summary>
    /// <param name="value">The value.</param>
    public override void SetLength(long value) {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Setting length {@Device}", this);
        }
    }

    public override void Write(byte[] buffer, int offset, int count) {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Writing {@Device}", this);
        }
    }
}