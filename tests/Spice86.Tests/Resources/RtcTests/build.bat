@echo off
REM Build script for RTC integration tests (Windows)
REM Requires NASM to be installed and in PATH

echo Building RTC integration tests...

nasm -f bin -o cmos_ports.com cmos_ports.asm
if errorlevel 1 (
    echo ERROR: Failed to build cmos_ports.com
    exit /b 1
)
echo   [OK] cmos_ports.com

nasm -f bin -o bios_int1a.com bios_int1a.asm
if errorlevel 1 (
    echo ERROR: Failed to build bios_int1a.com
    exit /b 1
)
echo   [OK] bios_int1a.com

nasm -f bin -o dos_int21h.com dos_int21h.asm
if errorlevel 1 (
    echo ERROR: Failed to build dos_int21h.com
    exit /b 1
)
echo   [OK] dos_int21h.com

echo.
echo All tests built successfully!
