namespace Spice86.Tests;

using Spice86.Core.Emulator.VM;
using Spice86.Logging;
using Spice86.Shared.Interfaces;
using Spice86.Core.Emulator.Memory;
using Xunit;
using Serilog;

public class ComprehensiveJitTests {
    static ComprehensiveJitTests() {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Warning()
            .CreateLogger();
    }

    [Fact]
    public void TestAdd_WithJit() => TestOneBin("add");

    [Fact]
    public void TestBcdcnv_WithJit() => TestOneBin("bcdcnv");

    [Fact]
    public void TestBitwise_WithJit() {
        byte[] expected = GetExpected("bitwise");
        expected[0x9F] = 0x12;
        expected[0x9D] = 0x12;
        expected[0x9B] = 0x12;
        expected[0x99] = 0x12;
        TestOneBin("bitwise", expected);
    }

    [Fact]
    public void TestCmpneg_WithJit() => TestOneBin("cmpneg");

    [Fact]
    public void TestControl_WithJit() {
        byte[] expected = GetExpected("control");
        expected[0x1] = 0x78;
        TestOneBin("control", expected);
    }

    [Fact]
    public void TestDatatrnf_WithJit() => TestOneBin("datatrnf");

    [Fact]
    public void TestDiv_WithJit() => TestOneBin("div");

    [Fact]
    public void TestInterrupt_WithJit() => TestOneBin("interrupt");

    [Fact]
    public void TestJump1_WithJit() => TestOneBin("jump1");

    [Fact]
    public void TestJump2_WithJit() => TestOneBin("jump2");

    [Fact]
    public void TestMul_WithJit() {
        byte[] expected = GetExpected("mul");
        expected[0xA2] = 0x2;
        expected[0x9E] = 0x2;
        expected[0x9C] = 0x3;
        expected[0x9A] = 0x3;
        expected[0x98] = 0x2;
        expected[0x96] = 0x2;
        expected[0x92] = 0x2;
        expected[0x73] = 0x2;
        expected[0xAA] = 0x42;
        expected[0xAE] = 0x2;
        expected[0xB0] = 0x3;
        expected[0xB2] = 0x2;
        expected[0xB4] = 0x3;
        expected[0xB6] = 0x42;
        expected[0xBA] = 0x2;
        TestOneBin("mul", expected);
    }

    [Fact]
    public void TestRep_WithJit() => TestOneBin("rep");

    [Fact]
    public void TestRotate_WithJit() => TestOneBin("rotate");

    [Fact]
    public void TestSegpr_WithJit() => TestOneBin("segpr");

    [Fact]
    public void TestShifts_WithJit() {
        byte[] expected = GetExpected("shifts");
        expected[0x6F] = 0x08;
        expected[0x79] = 0x08;
        TestOneBin("shifts", expected);
    }

    [Fact]
    public void TestStrings_WithJit() => TestOneBin("strings");

    [Fact]
    public void TestSub_WithJit() => TestOneBin("sub");

    [Fact]
    public void TestSelfModifyValue_WithJit() {
        byte[] expected = new byte[4];
        expected[0x00] = 0x01;
        expected[0x01] = 0x00;
        expected[0x02] = 0xff;
        expected[0x03] = 0xff;
        TestOneBin("selfmodifyvalue", expected);
    }

    [Fact]
    public void TestSelfModifyInstructions_WithJit() {
        byte[] expected = new byte[6];
        expected[0x00] = 0x03;
        expected[0x01] = 0x00;
        expected[0x02] = 0x02;
        expected[0x03] = 0x00;
        expected[0x04] = 0x01;
        expected[0x05] = 0x00;
        TestOneBin("selfmodifyinstructions", expected);
    }

    private Machine TestOneBin(string binName) {
        byte[] expected = GetExpected(binName);
        return TestOneBin(binName, expected);
    }

    private Machine TestOneBin(string binName, byte[] expected, long maxCycles = 100000L) {
        Spice86DependencyInjection spice86DependencyInjection = new Spice86Creator(
            binName: binName, enableCfgCpu: true, enableJit: true,
            maxCycles: maxCycles, enablePit: false, recordData: false).Create();
        spice86DependencyInjection.ProgramExecutor.Run();
        Machine machine = spice86DependencyInjection.Machine;
        IMemory memory = machine.Memory;
        CompareMemoryWithExpected(memory, expected);
        return machine;
    }

    private static byte[] GetExpected(string binName) {
        string resPath = $"Resources/cpuTests/res/MemoryDumps/{binName}.bin";
        return File.ReadAllBytes(resPath);
    }

    private static void CompareMemoryWithExpected(IMemory memory, byte[] expected) {
        byte[] actual = memory.ReadRam((uint)expected.Length);
        Assert.Equal(expected, actual);
    }
}
