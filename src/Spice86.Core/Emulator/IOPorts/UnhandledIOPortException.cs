﻿namespace Spice86.Core.Emulator.IOPorts;

using Spice86.Core.Emulator.CPU;

using System;

using Spice86.Core.Emulator.Errors;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Utils;

/// <summary>
/// Thrown when an unhandled IO Port is accessed.
/// </summary>
[Serializable]
public class UnhandledIOPortException : UnhandledOperationException {
    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledIOPortException"/> class with the specified machine and IO Port number.
    /// </summary>
    /// <param name="state">The CPU state when the exception occured.</param>
    /// <param name="ioPort">The number of the unhandled IO Port.</param>
    public UnhandledIOPortException(ICpuState state, int ioPort) : base(state, $"Unhandled port {ConvertUtils.ToHex((uint)ioPort)}. This usually means that the hardware behind the port is not emulated or that the port is not routed correctly.") {
    }
}