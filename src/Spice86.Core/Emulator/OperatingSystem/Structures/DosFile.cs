namespace Spice86.Core.Emulator.OperatingSystem.Structures;

using Spice86.Core.Emulator.Memory;

/// <summary>
/// Represents a file that has been opened by DOS.
/// </summary>
public class DosFile : VirtualFileBase {
    private readonly int _descriptor;
    private readonly List<MemoryRange> _loadedMemoryRanges = new();
    private readonly Stream _randomAccessStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="DosFile"/> class.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="descriptor">The file descriptor used by DOS.</param>
    /// <param name="randomAccessFile">The stream used for random access to the file.</param>
    public DosFile(string name, int descriptor, Stream randomAccessFile) {
        Name = name;
        _descriptor = descriptor;
        _randomAccessStream = randomAccessFile;
    }

    /// <summary>
    /// Adds a memory range to the list of loaded memory ranges for the file.
    /// </summary>
    /// <param name="memoryRange">The memory range to add.</param>
    public void AddMemoryRange(MemoryRange memoryRange) {
        for (int i = 0; i < _loadedMemoryRanges.Count; i++) {
            MemoryRange loadMemoryRange = _loadedMemoryRanges[i];
            if (loadMemoryRange.StartAddress == memoryRange.StartAddress && loadMemoryRange.EndAddress == memoryRange.EndAddress) {
                // Same, nothing to do
                return;
            }

            if (loadMemoryRange.IsInRange(memoryRange.StartAddress, memoryRange.EndAddress)) {
                // Fuse
                loadMemoryRange.StartAddress = Math.Min(loadMemoryRange.StartAddress, memoryRange.StartAddress);
                loadMemoryRange.EndAddress = Math.Max(loadMemoryRange.EndAddress, memoryRange.EndAddress);
                return;
            }

            if (loadMemoryRange.EndAddress + 1 == memoryRange.StartAddress) {
                // We are the next block, extend
                loadMemoryRange.EndAddress = memoryRange.EndAddress;
                return;
            }
        }

        _loadedMemoryRanges.Add(memoryRange);
    }

    /// <summary>
    /// Gets the file descriptor used by DOS.
    /// </summary>
    public int Descriptor => _descriptor;

    /// <summary>
    /// Gets a list of memory ranges that have been loaded for the file.
    /// </summary>
    public IList<MemoryRange> LoadedMemoryRanges => _loadedMemoryRanges;

    /// <summary>
    /// Gets or sets the bool.
    /// </summary>
    public virtual bool IsOnReadOnlyMedium { get; }

    /// <summary>
    /// Gets or sets the Time.
    /// </summary>
    public ushort Time { get; set; }

    /// <summary>
    /// Gets or sets the Date.
    /// </summary>
    public ushort Date { get; set; }

    /// <summary>
    /// Gets or sets the Flags.
    /// </summary>
    public byte Flags { get; set; }

    /// <summary>
    /// Gets or sets the Drive.
    /// </summary>
    public byte Drive { get; set; } = 0xff; //unset
    /// <summary>
    /// Gets or sets the DeviceInformation.
    /// </summary>
    public ushort DeviceInformation { get; set; }
    /// <summary>
    /// Gets or sets the string.
    /// </summary>
    public override string Name { get; set; }
    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanRead => _randomAccessStream.CanRead;
    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanSeek => _randomAccessStream.CanSeek;
    /// <summary>
    /// The bool.
    /// </summary>
    public override bool CanWrite => _randomAccessStream.CanWrite;
    /// <summary>
    /// The long.
    /// </summary>
    public override long Length => _randomAccessStream.Length;

    /// <summary>
    /// The long.
    /// </summary>
    public override long Position {
        get => _randomAccessStream.Position;
        set => _randomAccessStream.Position = value;
    }
    /// <summary>
    /// Closes the file.
    /// </summary>
    public override void Close() {
        _randomAccessStream.Close();
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Flush() {
        _randomAccessStream.Flush();
    }

    /// <summary>
    /// int method.
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count) {
        return _randomAccessStream.Read(buffer, offset, count);
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void SetLength(long value) {
        _randomAccessStream.SetLength(value);
    }

    /// <summary>
    /// void method.
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count) {
        _randomAccessStream.Write(buffer, offset, count);
    }

    /// <summary>
    /// long method.
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin) {
        return _randomAccessStream.Seek(offset, origin);
    }
}