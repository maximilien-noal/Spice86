namespace Spice86.ViewModels.Services;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using Spice86.Core.Emulator.VM;

using System.Runtime.InteropServices;

/// <summary>
/// Defines the contract for i exception handler.
/// </summary>
public interface IExceptionHandler {
    void Handle(Exception e);
}

/// <summary>
///     Handles exceptions when the application is running in headless mode.
/// </summary>
/// <remarks>
///     If the application lifetime exists, it will attempt a graceful shutdown.
///     Otherwise, it terminates the application with an exit code.
/// </remarks>
public class HeadlessModeExceptionHandler(IUIDispatcher uiDispatcher) : IExceptionHandler {
    /// <summary>
    /// Handles .
    /// </summary>
    /// <param name="e">The e.</param>
    public void Handle(Exception e) {
        int resultCode = Marshal.GetHRForException(e);
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime lifetime) {
            uiDispatcher.Post(() => lifetime.Shutdown(resultCode));
        } else {
            Environment.Exit(resultCode);
        }
    }
}

/// <summary>
/// Represents main window exception handler.
/// </summary>
public class MainWindowExceptionHandler(IPauseHandler pauseHandler) : IExceptionHandler {
    /// <summary>
    /// Handles .
    /// </summary>
    /// <param name="e">The e.</param>
    public void Handle(Exception e) {
        pauseHandler.RequestPause("Inspect emulator error");
    }
}