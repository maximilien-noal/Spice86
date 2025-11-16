namespace Spice86.Core.Emulator.OperatingSystem.Devices;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Shared.Interfaces;

using System.IO;

/// <summary>
/// Represents the AuxDevice class.
/// </summary>
public class AuxDevice : CharacterDevice {
    private readonly ILoggerService _loggerService;
    public AuxDevice(ILoggerService loggerService,
        IByteReaderWriter memory, uint baseAddress)
        : base(memory, baseAddress, "AUX") {
        _loggerService = loggerService;
    }

    /// <summary>
    /// The string.
    /// </summary>
    public override string Name => "AUX";

    /// <summary>
    /// The string.
    /// </summary>
    public override string? Alias => "COM1";

    /// <summary>
    /// Gets or sets the ushort.
    /// </summary>
    public override ushort Information { get; }
    /// <summary>
    /// Gets or sets the bool.
    /// </summary>
    public override bool CanRead { get; }
    /// <summary>
    /// Gets or sets the bool.
    /// </summary>
    public override bool CanSeek { get; }
    /// <summary>
    /// Gets or sets the bool.
    /// </summary>
    public override bool CanWrite { get; }
    /// <summary>
    /// Gets or sets the long.
    /// </summary>
    public override long Length { get; }
    /// <summary>
    /// Gets or sets the long.
    /// </summary>
    public override long Position { get; set; }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Flush() {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Flushing {@Device}", this);
        }
        // No-op for aux device
    }

    /// <summary>
    /// int method.
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count) {
        // No-op for aux device
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Reading {@Device}", this);
        }
        return 0;
    }

    /// <summary>
    /// long method.
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin) {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Seeking {@Device}", this);
        }
        return 0;
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void SetLength(long value) {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Setting length {@Device}", this);
        }
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count) {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
            _loggerService.Warning("Writing {@Device}", this);
        }
    }
}