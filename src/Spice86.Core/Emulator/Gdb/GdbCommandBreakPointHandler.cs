﻿namespace Spice86.Core.Emulator.Gdb;

using Serilog;

using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Core.Utils;
using Spice86.Logging;

using System;
using System.Diagnostics;
using System.IO;

public class GdbCommandBreakpointHandler {
    private static readonly ILogger _logger = new Serilogger().Logger.ForContext<GdbCommandBreakpointHandler>();
    private readonly GdbIo _gdbIo;
    private readonly Machine _machine;
    private volatile bool _resumeEmulatorOnCommandEnd;

    public GdbCommandBreakpointHandler(GdbIo gdbIo, Machine machine) {
        _gdbIo = gdbIo;
        _machine = machine;
    }

    public string AddBreakpoint(string commandContent) {
        BreakPoint? breakPoint = ParseBreakPoint(commandContent);
        _machine.MachineBreakpoints.ToggleBreakPoint(breakPoint, true);
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
            _logger.Debug("Breakpoint added!\n{@BreakPoint}", breakPoint);
        }
        return _gdbIo.GenerateResponse("OK");
    }

    public string ContinueCommand() {
        _resumeEmulatorOnCommandEnd = true;
        _machine.MachineBreakpoints.PauseHandler.RequestResume();

        // Do not send anything to GDB, CPU thread will send something when breakpoint is reached
        return _gdbIo.GenerateResponse("OK");
    }

    public bool ResumeEmulatorOnCommandEnd { get => _resumeEmulatorOnCommandEnd; set => _resumeEmulatorOnCommandEnd = value; }

    public void OnBreakPointReached(BreakPoint breakPoint) {
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
            _logger.Debug("Breakpoint reached!\n{@BreakPoint}", breakPoint);
        }
        _machine.MachineBreakpoints.PauseHandler.RequestPause();
        _resumeEmulatorOnCommandEnd = false;
        try {
            _gdbIo.SendResponse(_gdbIo.GenerateResponse("S05"));
        } catch (IOException e) {
            e.Demystify();
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Error)) {
                _logger.Error(e, "IOException while sending breakpoint info");
            }
        }
    }

    public BreakPoint? ParseBreakPoint(string command) {
        try {
            string[] commandSplit = command.Split(",");
            int type = int.Parse(commandSplit[0]);
            long address = ConvertUtils.ParseHex32(commandSplit[1]);
            // 3rd parameter kind is unused in our case
            BreakPointType? breakPointType = type switch {
                0 => BreakPointType.EXECUTION,
                1 => BreakPointType.EXECUTION,
                2 => BreakPointType.WRITE,
                3 => BreakPointType.READ,
                4 => BreakPointType.ACCESS,
                _ => null
            };
            if (breakPointType == null) {
                if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Error)) {
                    _logger.Error("Cannot parse breakpoint type {@Type} for command {@Command}", type, command);
                }
                return null;
            }
            return new AddressBreakPoint((BreakPointType)breakPointType, address, OnBreakPointReached, false);
        } catch (FormatException nfe) {
            nfe.Demystify();
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Error)) {
                _logger.Error(nfe, "Cannot parse breakpoint {@Command}", command);
            }
            return null;
        }
    }

    public string RemoveBreakpoint(string commandContent) {
        BreakPoint? breakPoint = ParseBreakPoint(commandContent);
        if (breakPoint == null) {
            return _gdbIo.GenerateResponse("");
        }
        _machine.MachineBreakpoints.ToggleBreakPoint(breakPoint, false);
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
            _logger.Debug("Breakpoint removed!\n{@BreakPoint}", breakPoint);
        }
        return _gdbIo.GenerateResponse("OK");
    }

    public string? Step() {
        _resumeEmulatorOnCommandEnd = true;

        // will pause the CPU at the next instruction unconditionally
        BreakPoint stepBreakPoint = new UnconditionalBreakPoint(BreakPointType.EXECUTION, OnBreakPointReached, true);
        _machine.MachineBreakpoints.ToggleBreakPoint(stepBreakPoint, true);
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
            _logger.Debug("Breakpoint added for step!\n{@StepBreakPoint}", stepBreakPoint);
        }

        // Do not send anything to GDB, CPU thread will send something when breakpoint is reached
        return null;
    }
}