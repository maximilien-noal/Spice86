#!/bin/bash
# Build script for RTC integration tests (Linux/macOS)
# Requires NASM to be installed

echo "Building RTC integration tests..."

nasm -f bin -o cmos_ports.com cmos_ports.asm
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build cmos_ports.com"
    exit 1
fi
echo "  [OK] cmos_ports.com"

nasm -f bin -o bios_int1a.com bios_int1a.asm
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build bios_int1a.com"
    exit 1
fi
echo "  [OK] bios_int1a.com"

nasm -f bin -o dos_int21h.com dos_int21h.asm
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build dos_int21h.com"
    exit 1
fi
echo "  [OK] dos_int21h.com"

echo ""
echo "All tests built successfully!"
