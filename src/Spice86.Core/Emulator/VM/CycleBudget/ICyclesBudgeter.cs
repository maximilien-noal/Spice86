namespace Spice86.Core.Emulator.VM.CycleBudget;

/// <summary>
/// Defines the contract for ICyclesBudgeter.
/// </summary>
public interface ICyclesBudgeter {
    int GetNextSliceBudget();
    void UpdateBudget(double elapsedMilliseconds, long cyclesExecuted, bool cpuStateIsRunning);
}