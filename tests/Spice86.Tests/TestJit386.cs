namespace Spice86.Tests;

using Spice86.Core.Emulator.VM;
using Spice86.Logging;
using Spice86.Shared.Interfaces;
using Xunit;
using Serilog;
using System.Text;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Errors;
using Spice86.Core.Emulator.Memory;

public class TestJit386 {
    static TestJit386() {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger();
    }

    [Fact]
    public void Test386WithJitEnabled() {
        //Arrange
        string binName = "test386";
        Spice86DependencyInjection spice86DependencyInjection = new Spice86Creator(
            binName: binName, enableCfgCpu: true, enableJit: true,
            enablePit: false, recordData: false, maxCycles: long.MaxValue,
            failOnUnhandledPort: true).Create();
        Machine machine = spice86DependencyInjection.Machine;
        Test386Handler debugPortsHandler = new Test386Handler(machine.CpuState, new LoggerService(), machine.IoPortDispatcher);

        //Act
        try {
            spice86DependencyInjection.ProgramExecutor.Run();
        } finally {
            Log.Information("Reached POST values {portValues}. Ascii Error is {asciiError}", debugPortsHandler.PostValues, debugPortsHandler.AsciiError);
        }

        //Assert
        Assert.Equal(8, debugPortsHandler.PostValues.Count);
        // FF means test finished normally
        Assert.Equal(0xFF, debugPortsHandler.PostValues.Last());
    }

    private class Test386Handler : DefaultIOPortHandler {
        private const int PostPort = 0x999;
        private const int AsciiOutPort = 0x998;

        public List<ushort> PostValues { get; } = new();
        public string AsciiError { get; private set; } = "";

        public Test386Handler(State state, ILoggerService loggerService,
            IOPortDispatcher ioPortDispatcher) : base(state, true, loggerService) {
            ioPortDispatcher.AddIOPortHandler(PostPort, this);
            ioPortDispatcher.AddIOPortHandler(AsciiOutPort, this);
        }

        public override void WriteByte(ushort port, byte value) {
            if (port == AsciiOutPort) {
                AsciiError += Encoding.ASCII.GetString(new byte[] { value });
            } else if (port == PostPort) {
                if (PostValues.Contains(value)) {
                    throw new UnhandledOperationException(_state, $"POST value {value} already sent. Is test looping?");
                }

                PostValues.Add(value);
            }
        }
    }
}
