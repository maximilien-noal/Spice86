namespace Spice86.Core.Emulator.VM; 

/// <summary>
/// Not an error, but generated by HLT instruction in generated code. Allows the machine to stop running properly
/// </summary>
[Serializable]
public class HaltRequestedException : Exception {
    
}