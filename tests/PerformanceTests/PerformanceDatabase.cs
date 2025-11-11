namespace Spice86.Tests.PerformanceTests;

using Dapper;
using Microsoft.Data.Sqlite;

/// <summary>
/// Manages the SQLite database for storing performance test results.
/// </summary>
public sealed class PerformanceDatabase : IDisposable {
    private readonly string _connectionString;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceDatabase"/> class.
    /// </summary>
    /// <param name="databasePath">Path to the SQLite database file.</param>
    public PerformanceDatabase(string databasePath) {
        _connectionString = $"Data Source={databasePath}";
        InitializeDatabase();
    }

    /// <summary>
    /// Initializes the database schema if it doesn't exist.
    /// </summary>
    private void InitializeDatabase() {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string createTableSql = @"
            CREATE TABLE IF NOT EXISTS PerformanceTestRuns (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RunTimestamp TEXT NOT NULL,
                GitCommit TEXT,
                MachineName TEXT,
                OsVersion TEXT
            );

            CREATE TABLE IF NOT EXISTS PerformanceTestResults (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RunId INTEGER NOT NULL,
                TestId INTEGER NOT NULL,
                TestName TEXT NOT NULL,
                Cycles INTEGER NOT NULL,
                Result INTEGER NOT NULL,
                Timestamp TEXT NOT NULL,
                FOREIGN KEY (RunId) REFERENCES PerformanceTestRuns(Id)
            );

            CREATE INDEX IF NOT EXISTS idx_run_timestamp 
                ON PerformanceTestRuns(RunTimestamp);
            
            CREATE INDEX IF NOT EXISTS idx_test_results 
                ON PerformanceTestResults(RunId, TestId);
        ";

        connection.Execute(createTableSql);
    }

    /// <summary>
    /// Stores a performance test run and its results in the database.
    /// </summary>
    /// <param name="results">The test results to store.</param>
    /// <param name="gitCommit">Optional git commit hash.</param>
    /// <returns>The run ID.</returns>
    public long StoreTestRun(IEnumerable<PerformanceTestResult> results, string? gitCommit = null) {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try {
            // Insert test run
            string insertRunSql = @"
                INSERT INTO PerformanceTestRuns (RunTimestamp, GitCommit, MachineName, OsVersion)
                VALUES (@Timestamp, @Commit, @Machine, @Os);
                SELECT last_insert_rowid();
            ";

            long runId = connection.ExecuteScalar<long>(insertRunSql, new {
                Timestamp = DateTime.UtcNow.ToString("O"),
                Commit = gitCommit ?? string.Empty,
                Machine = Environment.MachineName,
                Os = Environment.OSVersion.ToString()
            }, transaction);

            // Insert test results
            string insertResultSql = @"
                INSERT INTO PerformanceTestResults (RunId, TestId, TestName, Cycles, Result, Timestamp)
                VALUES (@RunId, @TestId, @TestName, @Cycles, @Result, @Timestamp);
            ";

            foreach (var result in results) {
                connection.Execute(insertResultSql, new {
                    RunId = runId,
                    TestId = result.TestId,
                    TestName = result.TestName,
                    Cycles = result.Cycles,
                    Result = result.Result,
                    Timestamp = result.Timestamp.ToString("O")
                }, transaction);
            }

            transaction.Commit();
            return runId;
        } catch {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Retrieves historical test results for comparison.
    /// </summary>
    /// <param name="testId">The test ID to retrieve results for.</param>
    /// <param name="limit">Maximum number of results to retrieve.</param>
    /// <returns>List of historical test results.</returns>
    public List<HistoricalTestResult> GetHistoricalResults(int testId, int limit = 50) {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string sql = @"
            SELECT r.RunTimestamp, r.GitCommit, t.Cycles, t.Result
            FROM PerformanceTestResults t
            INNER JOIN PerformanceTestRuns r ON t.RunId = r.Id
            WHERE t.TestId = @TestId
            ORDER BY r.RunTimestamp DESC
            LIMIT @Limit;
        ";

        var results = connection.Query(sql, new { TestId = testId, Limit = limit })
            .Select(row => new HistoricalTestResult {
                RunTimestamp = DateTime.Parse((string)row.RunTimestamp),
                GitCommit = (string)row.GitCommit,
                Cycles = (long)row.Cycles,
                Result = (long)row.Result
            })
            .ToList();

        return results;
    }

    /// <summary>
    /// Gets performance statistics for a specific test.
    /// </summary>
    /// <param name="testId">The test ID.</param>
    /// <returns>Statistics for the test.</returns>
    public PerformanceStatistics? GetTestStatistics(int testId) {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string sql = @"
            SELECT 
                COUNT(*) as RunCount,
                AVG(Cycles) as AvgCycles,
                MIN(Cycles) as MinCycles,
                MAX(Cycles) as MaxCycles
            FROM PerformanceTestResults
            WHERE TestId = @TestId;
        ";

        var row = connection.QueryFirstOrDefault(sql, new { TestId = testId });
        
        if (row != null) {
            long runCount = (long)row.RunCount;
            if (runCount > 0) {
                return new PerformanceStatistics {
                    TestId = testId,
                    RunCount = (int)runCount,
                    AverageCycles = (double)row.AvgCycles,
                    MinCycles = (long)row.MinCycles,
                    MaxCycles = (long)row.MaxCycles
                };
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public void Dispose() {
        if (!_disposed) {
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents a historical test result for comparison.
/// </summary>
public record HistoricalTestResult {
    /// <summary>
    /// Gets or sets the run timestamp.
    /// </summary>
    public required DateTime RunTimestamp { get; init; }

    /// <summary>
    /// Gets or sets the git commit hash.
    /// </summary>
    public required string GitCommit { get; init; }

    /// <summary>
    /// Gets or sets the cycle count.
    /// </summary>
    public required long Cycles { get; init; }

    /// <summary>
    /// Gets or sets the result value.
    /// </summary>
    public required long Result { get; init; }
}

/// <summary>
/// Represents performance statistics for a test.
/// </summary>
public record PerformanceStatistics {
    /// <summary>
    /// Gets or sets the test ID.
    /// </summary>
    public required int TestId { get; init; }

    /// <summary>
    /// Gets or sets the number of runs.
    /// </summary>
    public required int RunCount { get; init; }

    /// <summary>
    /// Gets or sets the average cycle count.
    /// </summary>
    public required double AverageCycles { get; init; }

    /// <summary>
    /// Gets or sets the minimum cycle count.
    /// </summary>
    public required long MinCycles { get; init; }

    /// <summary>
    /// Gets or sets the maximum cycle count.
    /// </summary>
    public required long MaxCycles { get; init; }

    /// <summary>
    /// Gets the variance (max - min).
    /// </summary>
    public long Variance => MaxCycles - MinCycles;
}
