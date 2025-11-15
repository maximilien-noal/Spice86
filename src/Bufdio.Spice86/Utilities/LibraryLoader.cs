namespace Bufdio.Spice86.Utilities;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Represents library loader.
/// </summary>
internal sealed class LibraryLoader : IDisposable {
    private readonly IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="libraryName">The library name.</param>
    public LibraryLoader(string libraryName) {
        ArgumentException.ThrowIfNullOrEmpty(libraryName);
        _handle = NativeLibrary.Load(libraryName);

        Ensure.That<Exception>(_handle != IntPtr.Zero, $"Could not load native library: {libraryName}.");
    }

    /// <summary>
    /// Performs the dispose operation.
    /// </summary>
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if (_disposed) {
            if (disposing && _handle != IntPtr.Zero) {
                NativeLibrary.Free(_handle);
            }
            _disposed = true;
        }
    }
}