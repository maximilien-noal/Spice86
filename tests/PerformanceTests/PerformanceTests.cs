namespace Spice86.Tests.PerformanceTests;

using FluentAssertions;
using LibGit2Sharp;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Performance tests for the Spice86 emulator.
/// </summary>
public class PerformanceTests : IDisposable {
    private readonly ITestOutputHelper _output;
    private readonly string _databasePath;
    private PerformanceDatabase? _database;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceTests"/> class.
    /// </summary>
    /// <param name="output">XUnit test output helper.</param>
    public PerformanceTests(ITestOutputHelper output) {
        _output = output;
        _databasePath = Path.Combine(
            Path.GetDirectoryName(typeof(PerformanceTests).Assembly.Location)!,
            "..", "..", "..", "Database", "performance.db");
        
        // Ensure database directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
    }

    /// <summary>
    /// Tests the performance test binary execution.
    /// </summary>
    [Fact]
    public void TestPerformanceBinary() {
        // Find the test binary
        string assemblyDir = Path.GetDirectoryName(typeof(PerformanceTests).Assembly.Location)!;
        string binaryPath = Path.Combine(assemblyDir, "Assembly", "perftest.com");

        _output.WriteLine($"Assembly directory: {assemblyDir}");
        _output.WriteLine($"Binary path: {binaryPath}");
        _output.WriteLine($"Binary exists: {File.Exists(binaryPath)}");

        // Verify binary exists
        File.Exists(binaryPath).Should().BeTrue($"Test binary should exist at {binaryPath}");

        // Run the test
        using var runner = new PerformanceTestRunner(_output, binaryPath);
        var results = runner.RunTest(maxInstructions: 20_000_000, timeoutSeconds: 60);

        // Verify results
        results.Should().NotBeEmpty("Performance tests should produce results");
        results.Should().HaveCount(5, "Should have 5 test results");

        // Validate each test
        foreach (var result in results) {
            _output.WriteLine($"Validating Test {result.TestId}: {result.TestName}");
            result.Cycles.Should().BeGreaterThan(0u, $"Test {result.TestId} should have cycles");
        }

        // Store results in database only on master branch
        _database = new PerformanceDatabase(_databasePath);
        string? gitCommit = GetGitCommit();
        string? currentBranch = GetCurrentBranch();
        
        if (currentBranch == "master" || currentBranch == "main") {
            long runId = _database.StoreTestRun(results, gitCommit);
            _output.WriteLine($"Stored test run with ID: {runId} (branch: {currentBranch})");
        } else {
            _output.WriteLine($"Skipping database storage (not on master/main branch, current: {currentBranch})");
        }

        // Display statistics (always show, even on PRs)
        DisplayStatistics();
    }

    /// <summary>
    /// Tests performance tracking and regression detection.
    /// </summary>
    [Fact]
    public void TestPerformanceTracking() {
        _database = new PerformanceDatabase(_databasePath);

        // Check if we have historical data
        var stats = _database.GetTestStatistics(1);
        if (stats != null && stats.RunCount > 1) {
            _output.WriteLine($"Performance Statistics for Test 1 (Integer Arithmetic):");
            _output.WriteLine($"  Total runs: {stats.RunCount}");
            _output.WriteLine($"  Average cycles: {stats.AverageCycles:F2}");
            _output.WriteLine($"  Min cycles: {stats.MinCycles}");
            _output.WriteLine($"  Max cycles: {stats.MaxCycles}");
            _output.WriteLine($"  Variance: {stats.Variance} ({(stats.Variance / (double)stats.AverageCycles * 100):F2}%)");

            // Check if variance is within acceptable range (e.g., 5%)
            double variancePercent = stats.Variance / stats.AverageCycles * 100;
            _output.WriteLine($"Variance check: {variancePercent:F2}% (should be < 5%)");
            
            // This is informational - small perf variance can happen
            if (variancePercent > 5) {
                _output.WriteLine($"WARNING: Performance variance is high at {variancePercent:F2}%");
            }
        } else {
            _output.WriteLine("Not enough historical data for comparison.");
        }
    }

    /// <summary>
    /// Displays performance statistics for all tests.
    /// </summary>
    private void DisplayStatistics() {
        if (_database == null) {
            return;
        }

        _output.WriteLine("\n=== Performance Statistics ===");
        for (int testId = 1; testId <= 5; testId++) {
            var stats = _database.GetTestStatistics(testId);
            if (stats != null && stats.RunCount > 0) {
                _output.WriteLine($"\nTest {testId}:");
                _output.WriteLine($"  Runs: {stats.RunCount}");
                _output.WriteLine($"  Avg Cycles: {stats.AverageCycles:F2}");
                _output.WriteLine($"  Min/Max: {stats.MinCycles}/{stats.MaxCycles}");
                _output.WriteLine($"  Variance: {stats.Variance} ({(stats.Variance / stats.AverageCycles * 100):F2}%)");

                // Get recent history
                var history = _database.GetHistoricalResults(testId, 5);
                if (history.Count > 1) {
                    _output.WriteLine($"  Recent runs:");
                    foreach (var run in history.Take(3)) {
                        _output.WriteLine($"    {run.RunTimestamp:s}: {run.Cycles} cycles");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the current git commit hash.
    /// </summary>
    /// <returns>Git commit hash or null.</returns>
    private string? GetGitCommit() {
        try {
            string? repoPath = FindGitRepository();
            if (repoPath == null) {
                return null;
            }

            using var repo = new Repository(repoPath);
            return repo.Head?.Tip?.Sha[..8];
        } catch (RepositoryNotFoundException) {
            // Not in a git repository
            return null;
        }
    }

    /// <summary>
    /// Gets the current git branch name.
    /// </summary>
    /// <returns>Branch name or null.</returns>
    private string? GetCurrentBranch() {
        try {
            string? repoPath = FindGitRepository();
            if (repoPath == null) {
                return null;
            }

            using var repo = new Repository(repoPath);
            return repo.Head?.FriendlyName;
        } catch (RepositoryNotFoundException) {
            // Not in a git repository
            return null;
        }
    }

    /// <summary>
    /// Finds the git repository root by walking up the directory tree.
    /// </summary>
    /// <returns>Path to git repository root or null.</returns>
    private string? FindGitRepository() {
        string? dir = Path.GetDirectoryName(typeof(PerformanceTests).Assembly.Location);
        while (dir != null) {
            if (Directory.Exists(Path.Combine(dir, ".git"))) {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }

    /// <inheritdoc/>
    public void Dispose() {
        if (!_disposed) {
            _database?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
