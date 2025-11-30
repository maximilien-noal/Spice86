namespace Spice86.Tests.Dos;

using FluentAssertions;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Shared.Interfaces;

using System.Runtime.CompilerServices;
using System.Text;

using Xunit;

/// <summary>
/// Integration tests for DOS EXEC (INT 21h, AH=4Bh) functionality.
/// Tests verify proper parent-child process execution via the EXEC syscall.
/// </summary>
/// <remarks>
/// These tests are based on the problem statement requirements to test:
/// 1. One executable launching another via proper DOS EXEC
/// 2. Batch file launching a program followed by a TSR
/// 
/// Based on FreeDOS kernel behavior (FDOS/kernel task.c and freecom).
/// </remarks>
public class DosExecIntegrationTests {
    private const int ResultPort = 0x999;    // Port to write test results
    private const int ChildResultPort = 0x998;   // Port for child to write results

    enum TestResult : byte {
        Success = 0x00,
        ChildRan = 0x42,      // Child process ran
        ParentResumed = 0x55, // Parent resumed after child terminated
        Failure = 0xFF
    }

    /// <summary>
    /// Tests that a parent program can execute a child program using INT 21h AH=4Bh (EXEC),
    /// and the child program runs, terminates, and returns control to the parent.
    /// </summary>
    /// <remarks>
    /// This test creates two COM files:
    /// 1. Parent program: Sets up EXEC parameter block and calls INT 21h AH=4Bh to run child
    /// 2. Child program: Writes a marker to an I/O port and terminates with INT 21h AH=4Ch
    /// 
    /// Expected behavior:
    /// 1. Parent starts executing
    /// 2. Parent calls EXEC to run child
    /// 3. Child runs and writes its marker
    /// 4. Child terminates with exit code
    /// 5. Parent regains control and writes its marker
    /// 6. Parent retrieves child's exit code via INT 21h AH=4Dh
    /// 
    /// This directly addresses the bug mentioned in the problem statement:
    /// "an executable launches another executable via proper DOS EXEC - this does not work"
    /// </remarks>
    [Fact]
    public void Exec_ParentLaunchesChild_ChildRunsThenParentResumes() {
        // Create child program that writes a marker and terminates with exit code 0x42
        byte[] childProgram = CreateChildProgram();
        
        // Create parent program that calls EXEC to run child
        byte[] parentProgram = CreateParentProgram();
        
        ExecTestHandler testHandler = RunExecTest(parentProgram, childProgram, "parent.com", "CHILD.COM");

        // Debug: Print all captured writes
        Console.WriteLine($"All writes captured: {testHandler.AllWrites.Count}");
        foreach ((ushort port, byte val) in testHandler.AllWrites) {
            Console.WriteLine($"  Port 0x{port:X4}, Value 0x{val:X2}");
        }
        
        // Verify that child ran (wrote its marker)
        testHandler.ChildResults.Should().Contain((byte)TestResult.ChildRan,
            "child program should have executed and written its marker");

        // Verify that parent resumed after child (wrote its marker)
        testHandler.Results.Should().Contain((byte)TestResult.ParentResumed,
            "parent should have resumed execution after child terminated");

        // Verify overall success
        testHandler.Results.Should().Contain((byte)TestResult.Success,
            "test should complete successfully with correct child exit code");
        testHandler.Results.Should().NotContain((byte)TestResult.Failure);
    }

    /// <summary>
    /// Simple test to verify basic program execution works before testing EXEC.
    /// </summary>
    [Fact]
    public void SimpleProgram_WritesToPort_AndTerminates() {
        // Simple program: write to port and exit
        byte[] program = new byte[] {
            0xB0, 0x42,             // mov al, 0x42
            0xBA, 0x98, 0x09,       // mov dx, 0x998
            0xEE,                   // out dx, al
            0xB8, 0x00, 0x4C,       // mov ax, 4C00h
            0xCD, 0x21,             // int 21h
            0xF4                    // hlt
        };

        // Write program to a .com file
        string filePath = Path.GetFullPath("simpletest.com");
        File.WriteAllBytes(filePath, program);
        
        try {
            Spice86DependencyInjection spice86DependencyInjection = new Spice86Creator(
                binName: filePath,
                enableCfgCpu: true,
                enablePit: false,
                recordData: false,
                maxCycles: 100000L,
                installInterruptVectors: true,
                enableA20Gate: true
            ).Create();

            ExecTestHandler testHandler = new(
                spice86DependencyInjection.Machine.CpuState,
                NSubstitute.Substitute.For<ILoggerService>(),
                spice86DependencyInjection.Machine.IoPortDispatcher
            );
            
            spice86DependencyInjection.ProgramExecutor.Run();

            // Debug output
            Console.WriteLine($"Simple test - All writes captured: {testHandler.AllWrites.Count}");
            foreach ((ushort port, byte val) in testHandler.AllWrites) {
                Console.WriteLine($"  Port 0x{port:X4}, Value 0x{val:X2}");
            }
            
            // Should have received the marker
            testHandler.ChildResults.Should().Contain((byte)0x42);
        }
        finally {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    /// <summary>
    /// Creates a simple child COM program that writes a marker and terminates.
    /// </summary>
    private static byte[] CreateChildProgram() {
        // Child program:
        //   mov al, 0x42       ; Child marker
        //   mov dx, 0x998      ; ChildResultPort  
        //   out dx, al         ; Write marker
        //   mov ax, 0x4C42     ; Terminate with exit code 0x42
        //   int 21h
        return new byte[] {
            0xB0, 0x42,             // mov al, ChildRan (0x42)
            0xBA, 0x98, 0x09,       // mov dx, 0x998 (ChildResultPort)
            0xEE,                   // out dx, al
            0xB8, 0x42, 0x4C,       // mov ax, 4C42h (terminate with code 0x42)
            0xCD, 0x21,             // int 21h
            0xF4                    // hlt (should never be reached)
        };
    }

    /// <summary>
    /// Creates a parent COM program that calls EXEC to run a child program.
    /// </summary>
    /// <remarks>
    /// This program demonstrates the DOS EXEC (INT 21h, AH=4Bh) functionality.
    /// The parameter block must be filled in at runtime because we need segment addresses.
    /// 
    /// Program layout:
    /// - Code at offset 0x00 (loads at CS:0100)
    /// - Data at offset 0x60:
    ///   - 0x60: Filename "CHILD.COM\0"
    ///   - 0x70: EXEC parameter block (14 bytes)
    ///   - 0x80: Command tail
    ///   - 0x82: FCB1
    ///   - 0x92: FCB2
    /// </remarks>
    private static byte[] CreateParentProgram() {
        // Offsets are relative to CS:0100 (where COM loads)
        // So data at array offset 0x60 = CS:0160
        ushort filenameOff = 0x0160;
        ushort paramBlockOff = 0x0170;
        ushort cmdTailOff = 0x0180;
        ushort fcb1Off = 0x0182;
        ushort fcb2Off = 0x0192;
        
        List<byte> code = new List<byte>();
        
        // Set DS and ES to CS
        code.AddRange(new byte[] { 0x8C, 0xC8 });  // mov ax, cs
        code.AddRange(new byte[] { 0x8E, 0xD8 });  // mov ds, ax
        code.AddRange(new byte[] { 0x8E, 0xC0 });  // mov es, ax
        
        // Set up DS:DX = filename pointer
        code.Add(0xBA);  // mov dx, imm16
        code.Add((byte)(filenameOff & 0xFF));
        code.Add((byte)(filenameOff >> 8));
        
        // Set up ES:BX = parameter block pointer
        code.Add(0xBB);  // mov bx, imm16
        code.Add((byte)(paramBlockOff & 0xFF));
        code.Add((byte)(paramBlockOff >> 8));
        
        // Fill in parameter block at runtime (need CS for segment values)
        // AX already has CS from above
        
        // [BX+0] = 0 (environment segment - inherit from parent)
        code.AddRange(new byte[] { 0xC7, 0x07, 0x00, 0x00 });  // mov word ptr [bx], 0
        
        // [BX+2] = cmdTailOff (command tail offset)
        code.AddRange(new byte[] { 0xC7, 0x47, 0x02 });  // mov word ptr [bx+2], imm16
        code.Add((byte)(cmdTailOff & 0xFF));
        code.Add((byte)(cmdTailOff >> 8));
        
        // [BX+4] = CS (command tail segment)
        code.AddRange(new byte[] { 0x89, 0x47, 0x04 });  // mov [bx+4], ax (AX still has CS)
        
        // [BX+6] = fcb1Off (FCB1 offset)
        code.AddRange(new byte[] { 0xC7, 0x47, 0x06 });  // mov word ptr [bx+6], imm16
        code.Add((byte)(fcb1Off & 0xFF));
        code.Add((byte)(fcb1Off >> 8));
        
        // [BX+8] = CS (FCB1 segment)
        code.AddRange(new byte[] { 0x89, 0x47, 0x08 });  // mov [bx+8], ax
        
        // [BX+0A] = fcb2Off (FCB2 offset)
        code.AddRange(new byte[] { 0xC7, 0x47, 0x0A });  // mov word ptr [bx+0Ah], imm16
        code.Add((byte)(fcb2Off & 0xFF));
        code.Add((byte)(fcb2Off >> 8));
        
        // [BX+0C] = CS (FCB2 segment)
        code.AddRange(new byte[] { 0x89, 0x47, 0x0C });  // mov [bx+0Ch], ax
        
        // Call EXEC: AX = 4B00h
        code.AddRange(new byte[] { 0xB8, 0x00, 0x4B });  // mov ax, 4B00h
        code.AddRange(new byte[] { 0xCD, 0x21 });        // int 21h
        
        // After EXEC, restore DS (EXEC destroys all registers except CS:IP)
        code.AddRange(new byte[] { 0x8C, 0xC8 });  // mov ax, cs
        code.AddRange(new byte[] { 0x8E, 0xD8 });  // mov ds, ax
        
        // Check carry flag - if set, EXEC failed
        code.Add(0x72);  // jc failed
        int jcPos = code.Count;
        code.Add(0x00);  // placeholder
        
        // EXEC succeeded - write parent resumed marker
        code.AddRange(new byte[] { 0xB0, 0x55 });  // mov al, 0x55 (ParentResumed)
        code.AddRange(new byte[] { 0xBA, 0x99, 0x09 });  // mov dx, 0x999 (ResultPort)
        code.Add(0xEE);  // out dx, al
        
        // Get child return code
        code.AddRange(new byte[] { 0xB4, 0x4D });  // mov ah, 4Dh
        code.AddRange(new byte[] { 0xCD, 0x21 });  // int 21h
        
        // Check if exit code is 0x42
        code.AddRange(new byte[] { 0x3C, 0x42 });  // cmp al, 42h
        code.Add(0x75);  // jne failed
        int jnePos = code.Count;
        code.Add(0x00);  // placeholder
        
        // Success!
        code.AddRange(new byte[] { 0xB0, 0x00 });  // mov al, 0x00 (Success)
        code.AddRange(new byte[] { 0xBA, 0x99, 0x09 });  // mov dx, ResultPort
        code.Add(0xEE);  // out dx, al
        code.Add(0xEB);  // jmp exit
        int jmpPos = code.Count;
        code.Add(0x00);  // placeholder
        
        // failed:
        int failedPos = code.Count;
        code.AddRange(new byte[] { 0xB0, 0xFF });  // mov al, 0xFF (Failure)
        code.AddRange(new byte[] { 0xBA, 0x99, 0x09 });  // mov dx, ResultPort
        code.Add(0xEE);  // out dx, al
        
        // exit:
        int exitPos = code.Count;
        code.AddRange(new byte[] { 0xB8, 0x00, 0x4C });  // mov ax, 4C00h
        code.AddRange(new byte[] { 0xCD, 0x21 });        // int 21h
        code.Add(0xF4);  // hlt (should not reach)
        
        // Fix up jumps
        byte[] codeArray = code.ToArray();
        codeArray[jcPos] = (byte)(failedPos - jcPos - 1);
        codeArray[jnePos] = (byte)(failedPos - jnePos - 1);
        codeArray[jmpPos] = (byte)(exitPos - jmpPos - 1);
        
        // Create full program with data section
        byte[] program = new byte[0xA2];  // Enough for code + data
        Array.Copy(codeArray, program, codeArray.Length);
        
        // Filename at offset 0x60: "CHILD.COM\0"
        byte[] filename = Encoding.ASCII.GetBytes("CHILD.COM\0");
        Array.Copy(filename, 0, program, 0x60, filename.Length);
        
        // Parameter block at offset 0x70 (14 bytes) - will be filled at runtime
        // Already zeros which is fine
        
        // Command tail at offset 0x80: length=0, followed by CR
        program[0x80] = 0x00;  // length = 0
        program[0x81] = 0x0D;  // CR
        
        // FCB1 at offset 0x82 and FCB2 at offset 0x92 - already zeros
        
        return program;
    }

    /// <summary>
    /// Runs the EXEC test with a parent and child program.
    /// </summary>
    private ExecTestHandler RunExecTest(byte[] parentProgram, byte[] childProgram,
        string parentFilename, string childFilename,
        [CallerMemberName] string unitTestName = "test") {
        
        // Write both files to current directory (where the emulator's C: is mounted)
        string parentPath = Path.GetFullPath($"{unitTestName}_{parentFilename}");
        string childPath = Path.GetFullPath(childFilename);  // Child must be accessible at its DOS name
        
        File.WriteAllBytes(parentPath, parentProgram);
        File.WriteAllBytes(childPath, childProgram);
        
        try {
            // Setup emulator with DOS initialized
            Spice86DependencyInjection spice86DependencyInjection = new Spice86Creator(
                binName: parentPath,
                enableCfgCpu: true,
                enablePit: false,
                recordData: false,
                maxCycles: 500000L,  // More cycles for EXEC operations
                installInterruptVectors: true,  // Enable DOS
                enableA20Gate: true
            ).Create();

            ExecTestHandler testHandler = new(
                spice86DependencyInjection.Machine.CpuState,
                NSubstitute.Substitute.For<ILoggerService>(),
                spice86DependencyInjection.Machine.IoPortDispatcher
            );
            
            spice86DependencyInjection.ProgramExecutor.Run();

            return testHandler;
        }
        finally {
            // Cleanup files
            try {
                if (File.Exists(parentPath)) File.Delete(parentPath);
                if (File.Exists(childPath)) File.Delete(childPath);
            } catch {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Captures EXEC test results from designated I/O ports.
    /// </summary>
    private class ExecTestHandler : DefaultIOPortHandler {
        public List<byte> Results { get; } = new();
        public List<byte> ChildResults { get; } = new();
        public List<(ushort Port, byte Value)> AllWrites { get; } = new();

        public ExecTestHandler(State state, ILoggerService loggerService,
            IOPortDispatcher ioPortDispatcher) : base(state, true, loggerService) {
            ioPortDispatcher.AddIOPortHandler(ResultPort, this);
            ioPortDispatcher.AddIOPortHandler(ChildResultPort, this);
        }

        public override void WriteByte(ushort port, byte value) {
            AllWrites.Add((port, value));
            if (port == ResultPort) {
                Results.Add(value);
            } else if (port == ChildResultPort) {
                ChildResults.Add(value);
            }
        }
    }
}
