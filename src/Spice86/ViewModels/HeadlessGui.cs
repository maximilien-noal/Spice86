namespace Spice86.ViewModels;

using Spice86.Shared.Emulator.Keyboard;
using Spice86.Shared.Emulator.Mouse;
using Spice86.Shared.Emulator.Video;
using Spice86.Shared.Interfaces;

/// <inheritdoc cref="Spice86.Shared.Interfaces.IGui" />
public sealed class HeadlessGui : IGui, IDisposable {
    private const double ScreenRefreshHz = 60;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(1000.0 / ScreenRefreshHz);
    private readonly SemaphoreSlim? _drawingSemaphoreSlim = new(1, 1);

    private bool _disposed;

    private Timer? _drawTimer;
    private bool _isAppClosing;
    private bool _isSettingResolution;

    private byte[]? _pixelBuffer;
    private bool _renderingTimerInitialized;

    public HeadlessGui() {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        Console.CancelKeyPress += OnProcessExit;
    }

    /// <summary>
    /// Dispose method.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// ShowMouseCursor method.
    /// </summary>
    public void ShowMouseCursor() {
    }

    /// <summary>
    /// HideMouseCursor method.
    /// </summary>
    public void HideMouseCursor() {
    }

#pragma warning disable CS0067 // Headless GUI never raises these events
    /// <summary>
    /// The EventHandler.
    /// </summary>
    public event EventHandler<KeyboardEventArgs>? KeyUp;
    /// <summary>
    /// The EventHandler.
    /// </summary>
    public event EventHandler<KeyboardEventArgs>? KeyDown;
    /// <summary>
    /// The EventHandler.
    /// </summary>
    public event EventHandler<MouseMoveEventArgs>? MouseMoved;
    /// <summary>
    /// The EventHandler.
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? MouseButtonDown;
    /// <summary>
    /// The EventHandler.
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? MouseButtonUp;
    /// <summary>
    /// The EventHandler.
    /// </summary>
    public event EventHandler<UIRenderEventArgs>? RenderScreen;
    /// <summary>
    /// The Action.
    /// </summary>
    public event Action? UserInterfaceInitialized;
#pragma warning restore CS0067

    /// <summary>
    /// Gets or sets the Width.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets or sets the Height.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets or sets the MouseX.
    /// </summary>
    public double MouseX { get; set; }

    /// <summary>
    /// Gets or sets the MouseY.
    /// </summary>
    public double MouseY { get; set; }

    /// <summary>
    /// SetResolution method.
    /// </summary>
    public void SetResolution(int width, int height) {
        if (width <= 0 || height <= 0) {
            throw new ArgumentOutOfRangeException($"Invalid resolution: {width}x{height}");
        }

        _isSettingResolution = true;
        try {
            if (Width != width || Height != height) {
                Width = width;
                Height = height;
                if (_disposed) {
                    return;
                }

                int bufferSize = width * height * 4;

                _drawingSemaphoreSlim?.Wait();
                try {
                    if (_pixelBuffer == null || _pixelBuffer.Length != bufferSize) {
                        _pixelBuffer = new byte[bufferSize];
                    }

                    Array.Clear(_pixelBuffer, 0, _pixelBuffer.Length);
                } finally {
                    if (!_disposed) {
                        _drawingSemaphoreSlim?.Release();
                    }
                }
            }
        } finally {
            _isSettingResolution = false;
        }

        InitializeRenderingTimer();
    }

    private void OnProcessExit(object? sender, EventArgs e) {
        _isAppClosing = true;
    }

    private void InitializeRenderingTimer() {
        if (_renderingTimerInitialized) {
            return;
        }

        _renderingTimerInitialized = true;
        _drawTimer = new Timer(DrawScreenCallback, null, RefreshInterval, RefreshInterval);
    }

    private void DrawScreenCallback(object? state) {
        DrawScreen();
    }

    private unsafe void DrawScreen() {
        if (_disposed || _isSettingResolution || _isAppClosing || _pixelBuffer is null || RenderScreen is null) {
            return;
        }

        _drawingSemaphoreSlim?.Wait();
        try {
            fixed (byte* bufferPtr = _pixelBuffer) {
                int rowBytes = Width * 4; // 4 bytes per pixel (BGRA)
                int length = rowBytes * Height / 4;

                var uiRenderEventArgs = new UIRenderEventArgs((IntPtr)bufferPtr, length);
                RenderScreen.Invoke(this, uiRenderEventArgs);
            }
        } finally {
            if (!_disposed) {
                _drawingSemaphoreSlim?.Release();
            }
        }
    }

    private void Dispose(bool disposing) {
        if (_disposed) {
            return;
        }

        _disposed = true;
        if (!disposing) {
            return;
        }

        _drawTimer?.Dispose();

        _pixelBuffer = null;
        _drawingSemaphoreSlim?.Dispose();
    }
}