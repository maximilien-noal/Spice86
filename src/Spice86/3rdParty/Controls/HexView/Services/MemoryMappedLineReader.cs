namespace Spice86._3rdParty.Controls.HexView.Services;

using Spice86._3rdParty.Controls.HexView.Models;

public class MemoryMappedLineReader : ILineReader {
    private readonly Memory<byte> _memory;

    public MemoryMappedLineReader(Memory<byte> memory) {
        _memory = memory;
    }

    public byte[] GetLine(long lineNumber, int width) {
        var bytes = new byte[width];
        var offset = lineNumber * width;
        
        for (int i = 0; i < width; i++) {
            long position = offset + i;
            if (position > _memory.Length) {
                break;
            }
            Range range = new((int)position, (int)position);
            bytes[i] = _memory[range].ToArray()[0];
        }

        return bytes;
    }
}