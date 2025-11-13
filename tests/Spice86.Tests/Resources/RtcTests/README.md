# RTC/CMOS Integration Tests

This directory contains assembly language integration tests for the RTC (Real-Time Clock) and CMOS functionality in Spice86.

## Test Files

### Assembly Source Files (.asm)
- **cmos_ports.asm** - Tests direct CMOS register access via I/O ports 0x70 and 0x71
- **bios_int1a.asm** - Tests BIOS INT 1A time services (functions 00h-05h)
- **dos_int21h.asm** - Tests DOS INT 21H date/time services (functions 2Ah-2Dh)

### Compiled Binaries (.com)
The `.com` files are DOS COM executables compiled from the `.asm` sources. These are committed to the repository so tests can run without requiring an assembler.

## Building the Tests

If you modify the `.asm` files, you need to recompile them. You can use either NASM or MASM.

### Using NASM (Recommended)

Install NASM from https://www.nasm.us/ or via package manager:
```bash
# Windows (via Chocolatey)
choco install nasm

# Linux
sudo apt-get install nasm

# macOS
brew install nasm
```

Then compile:
```bash
nasm -f bin -o cmos_ports.com cmos_ports.asm
nasm -f bin -o bios_int1a.com bios_int1a.asm
nasm -f bin -o dos_int21h.com dos_int21h.asm
```

Or use the build script:
```bash
# Windows
build.bat

# Linux/macOS
chmod +x build.sh
./build.sh
```

### Using MASM

If you're using Microsoft MASM:
```bash
ml /c /Zm cmos_ports.asm
link /tiny cmos_ports.obj
```

## Test Architecture

These integration tests follow the same pattern as the EMS and XMS integration tests:

1. **Assembly programs** write test results to I/O ports:
   - Port 0x999: Result port (0x00 = success, 0xFF = failure)
   - Port 0x998: Details port (test number or diagnostic info)

2. **C# test harness** (`RtcIntegrationTests.cs`):
   - Loads and executes the .com files in the emulator
   - Captures I/O port output via a custom IOPortHandler
   - Validates results using FluentAssertions

3. **Benefits**:
   - Tests run real machine code, not mocked implementations
   - Can be run on actual hardware for comparison
   - Heavily commented ASM makes test intent clear
   - Tests all code paths including error handling

## Test Coverage

### cmos_ports.asm
- ✓ Reads CMOS seconds register (0x00)
- ✓ Reads CMOS minutes register (0x02)
- ✓ Reads CMOS hours register (0x04)
- ✓ Reads CMOS day of month register (0x07)
- ✓ Reads CMOS month register (0x08)
- ✓ Reads CMOS year register (0x09)
- ✓ Reads CMOS century register (0x32)
- ✓ Validates all values are in proper BCD format

### bios_int1a.asm
- ✓ INT 1A Function 00h: Get System Clock Counter
- ✓ INT 1A Function 01h: Set System Clock Counter
- ✓ INT 1A Function 02h: Read RTC Time (validates BCD format)
- ✓ INT 1A Function 03h: Set RTC Time (stub, verifies no crash)
- ✓ INT 1A Function 04h: Read RTC Date (validates BCD format)
- ✓ INT 1A Function 05h: Set RTC Date (stub, verifies no crash)

### dos_int21h.asm
- ✓ INT 21H Function 2Ah: Get DOS Date (validates ranges)
- ✓ INT 21H Function 2Bh: Set DOS Date - valid date
- ✓ INT 21H Function 2Bh: Set DOS Date - invalid year (error handling)
- ✓ INT 21H Function 2Bh: Set DOS Date - invalid month (error handling)
- ✓ INT 21H Function 2Bh: Set DOS Date - invalid day (error handling)
- ✓ INT 21H Function 2Ch: Get DOS Time (validates ranges)
- ✓ INT 21H Function 2Dh: Set DOS Time - valid time
- ✓ INT 21H Function 2Dh: Set DOS Time - invalid hour (error handling)
- ✓ INT 21H Function 2Dh: Set DOS Time - invalid minutes (error handling)
- ✓ INT 21H Function 2Dh: Set DOS Time - invalid seconds (error handling)
- ✓ INT 21H Function 2Dh: Set DOS Time - invalid hundredths (error handling)

## Running on Real Hardware

These tests can be run on actual DOS machines or in DOSBox for comparison:

1. Copy the `.com` files to a DOS system
2. Run each test: `cmos_ports.com`, `bios_int1a.com`, `dos_int21h.com`
3. The programs will execute and terminate
4. Since real hardware doesn't have ports 0x998/0x999, the test will complete silently
5. You can add `int 0x03` breakpoints or modify to print results for manual verification

## Notes

- The compiled `.com` files are committed to the repository for convenience
- ASM source files are heavily commented to serve as documentation
- Tests are designed to be timing-independent where possible
- Some tests (like tick counter) allow small timing variations
- All tests use binary format for portability (no .exe headers, no relocations)
