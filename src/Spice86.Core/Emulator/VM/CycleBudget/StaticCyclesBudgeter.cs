namespace Spice86.Core.Emulator.VM.CycleBudget;

/// <summary>
/// Represents the StaticCyclesBudgeter class.
/// </summary>
public class StaticCyclesBudgeter(int cycleBudget) : ICyclesBudgeter {
    /// <summary>
    /// GetNextSliceBudget method.
    /// </summary>
    public int GetNextSliceBudget() {
        return cycleBudget;
    }

    /// <summary>
    /// UpdateBudget method.
    /// </summary>
    public void UpdateBudget(double elapsedMilliseconds, long cyclesExecuted, bool cpuStateIsRunning) {
        //nop
    }
}