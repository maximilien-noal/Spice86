namespace Spice86.Core.Emulator.OperatingSystem.Devices;

using Serilog.Events;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Shared.Interfaces;

using System.IO;

/// <summary>
/// Represents the PrinterDevice class.
/// </summary>
public class PrinterDevice : CharacterDevice {
    private const string LPT1 = "LPT1";
    private readonly ILoggerService _loggerService;

    public PrinterDevice(ILoggerService loggerService, IByteReaderWriter memory,
        uint baseAddress)
        : base(memory, baseAddress, LPT1) {
        _loggerService = loggerService;
    }

    /// <summary>
    /// The string.
    /// </summary>
    public override string Name => LPT1;

    /// <summary>
    /// The string.
    /// </summary>
    public override string Alias => "PRN";

    /// <summary>
    /// The ushort.
    /// </summary>
    public override ushort Information => 0x80A0;

    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanSeek => false;

    /// <summary>
    /// The long.
    /// </summary>
    public override long Length => 0;

    /// <summary>
    /// Gets or sets the long.
    /// </summary>
    public override long Position { get; set; } = 0;

    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanRead => false;

    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanWrite => true;

    /// <summary>
    /// void method.
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count) {
        string output = System.Text.Encoding.ASCII.GetString(buffer, offset, count);
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Writing to printer: {Output}", output);
        }
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Flush() {
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Flushing printer");
        }
    }

    /// <summary>
    /// int method.
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count) {
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Reading printer");
        }
        return 0;
    }

    /// <summary>
    /// long method.
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin) {
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Seeking printer");
        }
        return 0;
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void SetLength(long value) {
        return;
    }
}