namespace Spice86.Emulator;

using CommandLine;

using Spice86.Emulator.Function;

using System;

/// <summary> Configuration for spice86, that is what to run and how. Set on startup. </summary>
public class Configuration {
    [Option('m', nameof(Mt32RomsPath), Default = false, HelpText = "Zip file or directory containing the MT-32 ROM files")]
    public string? Mt32RomsPath { get; init; }

    [Option('c', nameof(CDrive), Default = null, Required = false, HelpText = "Path to C drive, default is exe parent")]
    public string? CDrive { get; init; }

    [Option('d', nameof(DefaultDumpDirectory), Required = false, HelpText = "Directory to dump data to when not specified otherwise. Workin directory if blank")]
    public string? DefaultDumpDirectory { get; init; } = Environment.CurrentDirectory;


    [Option('e', nameof(Exe), Default = null, Required = true, HelpText = "Path to executable")]
    public string? Exe { get; set; }

    [Option('a', nameof(ExeArgs), Default = null, Required = false, HelpText = "List of parameters to give to the emulated program")]
    public string? ExeArgs { get; init; }


    [Option('x', nameof(ExpectedChecksum), Default = null, Required = false, HelpText = "Hexadecimal string representing the expected checksum of the input program file")]
    public string? ExpectedChecksum { get; init; }
    public byte[] ExpectedChecksumValue {get; set;} = Array.Empty<byte>();


    [Option('f', nameof(FailOnUnhandledPort), Default = false, Required = false, HelpText = "If true, will fail when encountering an unhandled IO port. Useful to check for unimplemented hardware. false by default.")]
    public bool FailOnUnhandledPort { get; init; }

    [Option('g', nameof(GdbPort), Default = null, Required = false, HelpText = "gdb port, if empty gdb server will not be created. If not empty, application will pause until gdb connects")]
    public int? GdbPort { get; init; }

    public bool InstallInterruptVector { get; set; } = true;

    [Option('o', nameof(OverrideSupplierClassName), Default = null, Required = false, HelpText = "Name of a class in the current folder that will generate the initial function informations. See documentation for more information.")]
    public string? OverrideSupplierClassName { get; init; }

    /// <summary>
    /// Instantiated <see cref="OverrideSupplierClassName"/>. Created by <see cref="CLI.CommandLineParser"/>
    /// </summary>
    public IOverrideSupplier? OverrideSupplier { get; set; }

    [Option('p', nameof(ProgramEntryPointSegment), Default = 0x01ED, Required = false, HelpText = "Segment where to load the program. DOS PSP and MCB will be created before it.")]
    public int ProgramEntryPointSegment { get; init; }

    [Option('u', nameof(UseCodeOverride), Default = false, Required = false, HelpText = "<true or false> if false it will use the names provided by overrideSupplierClassName but not the code")]
    public bool UseCodeOverride { get; init; }

    /// <summary>
    /// Only for <see cref="Devices.Timer.Timer"/>
    /// </summary>
    [Option('i', nameof(InstructionsPerSecond), Required = false, HelpText = "<number of instructions that have to be executed by the emulator to consider a second passed> if blank will use time based timer.")]
    public long? InstructionsPerSecond { get; set; }

    [Option('t', nameof(TimeMultiplier), Default = 1, Required = false, HelpText = "<time multiplier> if >1 will go faster, if <1 will go slower.")]
    public double TimeMultiplier { get; init; }
    
    [Option('j', nameof(JumpFile), Default = "spice86dumpJumps.json", Required = false, HelpText = "Json file generated by spice86 with records of addresses of jumps, calls and returns. Provide it not to start from scrach every time.")]
    public string? JumpFile { get; init; }
    [Option('s', nameof(JumpFile), Default = "spice86dumpSymbols.txt", Required = false, HelpText = "Text file with labels and functions. Format is the format expected by ghidra. Provide it not to start from scrach every time.")]
    public string? SymbolsFile { get; init; }
}