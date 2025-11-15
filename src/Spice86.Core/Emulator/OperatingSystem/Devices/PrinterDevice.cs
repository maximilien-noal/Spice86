namespace Spice86.Core.Emulator.OperatingSystem.Devices;

using Serilog.Events;

using Spice86.Core.Emulator.Memory.ReaderWriter;
using Spice86.Shared.Interfaces;

using System.IO;

/// <summary>
/// Represents printer device.
/// </summary>
public class PrinterDevice : CharacterDevice {
    private const string LPT1 = "LPT1";
    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="loggerService">The logger service.</param>
    /// <param name="memory">The memory.</param>
    /// <param name="baseAddress">The base address.</param>
    public PrinterDevice(ILoggerService loggerService, IByteReaderWriter memory,
        uint baseAddress)
        : base(memory, baseAddress, LPT1) {
        _loggerService = loggerService;
    }

    /// <summary>
    /// The name.
    /// </summary>
    public override string Name => LPT1;

    /// <summary>
    /// The alias.
    /// </summary>
    public override string Alias => "PRN";

    /// <summary>
    /// The information.
    /// </summary>
    public override ushort Information => 0x80A0;

    /// <summary>
    /// The can seek.
    /// </summary>
    public override bool CanSeek => false;

    /// <summary>
    /// The length.
    /// </summary>
    public override long Length => 0;

    /// <summary>
    /// Gets or sets position.
    /// </summary>
    public override long Position { get; set; } = 0;

    /// <summary>
    /// The can read.
    /// </summary>
    public override bool CanRead => false;

    /// <summary>
    /// The can write.
    /// </summary>
    public override bool CanWrite => true;

    public override void Write(byte[] buffer, int offset, int count) {
        string output = System.Text.Encoding.ASCII.GetString(buffer, offset, count);
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Writing to printer: {Output}", output);
        }
    }

    /// <summary>
    /// Performs the flush operation.
    /// </summary>
    public override void Flush() {
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Flushing printer");
        }
    }

    public override int Read(byte[] buffer, int offset, int count) {
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Reading printer");
        }
        return 0;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        if (_loggerService.IsEnabled(LogEventLevel.Information)) {
            _loggerService.Information("Seeking printer");
        }
        return 0;
    }

    /// <summary>
    /// Sets length.
    /// </summary>
    /// <param name="value">The value.</param>
    public override void SetLength(long value) {
        return;
    }
}