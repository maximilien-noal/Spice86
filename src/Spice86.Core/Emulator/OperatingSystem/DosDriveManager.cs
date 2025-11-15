namespace Spice86.Core.Emulator.OperatingSystem;

using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Interfaces;
using Spice86.Shared.Utils;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

/// <summary>
/// The class responsible for centralizing all the mounted DOS drives.
/// </summary>
public class DosDriveManager : IDictionary<char, VirtualDrive> {
    private readonly SortedDictionary<char, VirtualDrive?> _driveMap = new();
    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="loggerService">The service used to log messages.</param>
    /// <param name="cDriveFolderPath">The host path to be mounted as C:.</param>
    /// <param name="executablePath">The host path to the DOS executable to be launched.</param>
    public DosDriveManager(ILoggerService loggerService, string? cDriveFolderPath, string? executablePath) {
        if (string.IsNullOrWhiteSpace(cDriveFolderPath)) {
            cDriveFolderPath = DosPathResolver.GetExeParentFolder(executablePath);
        }
        _loggerService = loggerService;
        cDriveFolderPath = ConvertUtils.ToSlashFolderPath(cDriveFolderPath);
        _driveMap.Add('A', null);
        _driveMap.Add('B', null);
        _driveMap.Add('C', new VirtualDrive { DriveLetter = 'C', MountedHostDirectory = cDriveFolderPath, CurrentDosDirectory = "" });
        CurrentDrive = _driveMap.ElementAt(2).Value!;
        if (loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            loggerService.Verbose("DOS Drives initialized: {@Drives}", _driveMap.Values);
        }
    }

    /// <summary>
    /// The currently selected drive.
    /// </summary>
    public VirtualDrive CurrentDrive { get; set; }

    internal static readonly ImmutableSortedDictionary<char, byte> DriveLetters = new Dictionary<char, byte>() {
            { 'A', 0 },
            { 'B', 1 },
            { 'C', 2 },
            { 'D', 3 },
            { 'E', 4 },
            { 'F', 5 },
            { 'G', 6 },
            { 'H', 7 },
            { 'I', 8 },
            { 'J', 9 },
            { 'K', 10 },
            { 'L', 11 },
            { 'M', 12 },
            { 'N', 13 },
            { 'O', 14 },
            { 'P', 15 },
            { 'Q', 16 },
            { 'R', 17 },
            { 'S', 18 },
            { 'T', 19 },
            { 'U', 20 },
            { 'V', 21 },
            { 'W', 22 },
            { 'X', 23 },
            { 'Y', 24 },
            { 'Z', 25 }
        }.ToImmutableSortedDictionary();


    /// <summary>
    /// Gets the current DOS drive zero based index.
    /// </summary>
    public byte CurrentDriveIndex => DriveLetters[CurrentDrive.DriveLetter];

    /// <summary>
    /// Determines whether it has drive at index.
    /// </summary>
    /// <param name="zeroBasedIndex">The zero based index.</param>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    internal bool HasDriveAtIndex(ushort zeroBasedIndex) {
        if (zeroBasedIndex > _driveMap.Count - 1) {
            return false;
        }
        return true;
    }

    public byte NumberOfPotentiallyValidDriveLetters {
        get {
            // At least A: and B:
            return (byte)_driveMap.Count;
        }
    }

    /// <summary>
    /// The keys.
    /// </summary>
    public ICollection<char> Keys => ((IDictionary<char, VirtualDrive>)_driveMap).Keys;

    /// <summary>
    /// The values.
    /// </summary>
    public ICollection<VirtualDrive> Values => ((IDictionary<char, VirtualDrive>)_driveMap).Values;

    /// <summary>
    /// The count.
    /// </summary>
    public int Count => ((ICollection<KeyValuePair<char, VirtualDrive>>)_driveMap).Count;

    /// <summary>
    /// The is read only.
    /// </summary>
    public bool IsReadOnly => ((ICollection<KeyValuePair<char, VirtualDrive>>)_driveMap).IsReadOnly;

    public VirtualDrive this[char key] { get => ((IDictionary<char, VirtualDrive>)_driveMap)[key]; set => ((IDictionary<char, VirtualDrive>)_driveMap)[key] = value; }


    /// <summary>
    /// The max drive count.
    /// </summary>
    public const int MaxDriveCount = 26;

    /// <summary>
    /// Adds .
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Add(char key, VirtualDrive value) {
        ((IDictionary<char, VirtualDrive>)_driveMap).Add(key, value);
    }

    /// <summary>
    /// Performs the contains key operation.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public bool ContainsKey(char key) {
        return ((IDictionary<char, VirtualDrive>)_driveMap).ContainsKey(key);
    }

    /// <summary>
    /// Removes .
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public bool Remove(char key) {
        return ((IDictionary<char, VirtualDrive>)_driveMap).Remove(key);
    }

    /// <summary>
    /// Performs the try get value operation.
    /// </summary>
    /// <param name="false">The false.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public bool TryGetValue(char key, [MaybeNullWhen(false)] out VirtualDrive value) {
        return ((IDictionary<char, VirtualDrive>)_driveMap).TryGetValue(key, out value);
    }

    /// <summary>
    /// Adds .
    /// </summary>
    /// <param name="item">The item.</param>
    public void Add(KeyValuePair<char, VirtualDrive> item) {
        ((ICollection<KeyValuePair<char, VirtualDrive>>)_driveMap).Add(item);
    }

    /// <summary>
    /// Performs the clear operation.
    /// </summary>
    public void Clear() {
        ((ICollection<KeyValuePair<char, VirtualDrive>>)_driveMap).Clear();
    }

    /// <summary>
    /// Performs the contains operation.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public bool Contains(KeyValuePair<char, VirtualDrive> item) {
        return ((ICollection<KeyValuePair<char, VirtualDrive>>)_driveMap).Contains(item);
    }

    /// <summary>
    /// Performs the copy to operation.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="arrayIndex">The array index.</param>
    public void CopyTo(KeyValuePair<char, VirtualDrive>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<char, VirtualDrive>>)_driveMap).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes .
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>A boolean value indicating the result.</returns>
    public bool Remove(KeyValuePair<char, VirtualDrive> item) {
        return ((ICollection<KeyValuePair<char, VirtualDrive>>)_driveMap).Remove(item);
    }

    /// <summary>
    /// Gets enumerator.
    /// </summary>
    public IEnumerator<KeyValuePair<char, VirtualDrive>> GetEnumerator() {
        return ((IEnumerable<KeyValuePair<char, VirtualDrive>>)_driveMap).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)_driveMap).GetEnumerator();
    }
}