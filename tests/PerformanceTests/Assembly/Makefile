# Makefile for building performance test binaries
# Requires NASM assembler

ASM = nasm
ASMFLAGS = -f bin

SOURCES = perftest.asm
TARGETS = $(SOURCES:.asm=.com)

.PHONY: all clean

all: $(TARGETS)

%.com: %.asm
	$(ASM) $(ASMFLAGS) -o $@ $<

clean:
	rm -f $(TARGETS)
