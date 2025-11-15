namespace Spice86.Core.Emulator.VM.CycleBudget;

/// <summary>
/// Defines the contract for i cycles budgeter.
/// </summary>
public interface ICyclesBudgeter {
    int GetNextSliceBudget();
    void UpdateBudget(double elapsedMilliseconds, long cyclesExecuted, bool cpuStateIsRunning);
}