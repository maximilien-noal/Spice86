namespace Spice86.Tests.PerformanceTests;

using System.Data;
using System.Data.SQLite;

/// <summary>
/// Manages the SQLite database for storing performance test results.
/// </summary>
public class PerformanceDatabase : IDisposable {
    private readonly string _connectionString;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceDatabase"/> class.
    /// </summary>
    /// <param name="databasePath">Path to the SQLite database file.</param>
    public PerformanceDatabase(string databasePath) {
        _connectionString = $"Data Source={databasePath};Version=3;";
        InitializeDatabase();
    }

    /// <summary>
    /// Initializes the database schema if it doesn't exist.
    /// </summary>
    private void InitializeDatabase() {
        using var connection = new SQLiteConnection(_connectionString);
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

        using var command = new SQLiteCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Stores a performance test run and its results in the database.
    /// </summary>
    /// <param name="results">The test results to store.</param>
    /// <param name="gitCommit">Optional git commit hash.</param>
    /// <returns>The run ID.</returns>
    public long StoreTestRun(IEnumerable<PerformanceTestResult> results, string? gitCommit = null) {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try {
            // Insert test run
            string insertRunSql = @"
                INSERT INTO PerformanceTestRuns (RunTimestamp, GitCommit, MachineName, OsVersion)
                VALUES (@timestamp, @commit, @machine, @os);
                SELECT last_insert_rowid();
            ";

            long runId;
            using (var command = new SQLiteCommand(insertRunSql, connection, transaction)) {
                command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("O"));
                command.Parameters.AddWithValue("@commit", gitCommit ?? string.Empty);
                command.Parameters.AddWithValue("@machine", Environment.MachineName);
                command.Parameters.AddWithValue("@os", Environment.OSVersion.ToString());
                runId = (long)command.ExecuteScalar()!;
            }

            // Insert test results
            string insertResultSql = @"
                INSERT INTO PerformanceTestResults (RunId, TestId, TestName, Cycles, Result, Timestamp)
                VALUES (@runId, @testId, @testName, @cycles, @result, @timestamp);
            ";

            foreach (var result in results) {
                using var command = new SQLiteCommand(insertResultSql, connection, transaction);
                command.Parameters.AddWithValue("@runId", runId);
                command.Parameters.AddWithValue("@testId", result.TestId);
                command.Parameters.AddWithValue("@testName", result.TestName);
                command.Parameters.AddWithValue("@cycles", result.Cycles);
                command.Parameters.AddWithValue("@result", result.Result);
                command.Parameters.AddWithValue("@timestamp", result.Timestamp.ToString("O"));
                command.ExecuteNonQuery();
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
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        string sql = @"
            SELECT r.RunTimestamp, r.GitCommit, t.Cycles, t.Result
            FROM PerformanceTestResults t
            INNER JOIN PerformanceTestRuns r ON t.RunId = r.Id
            WHERE t.TestId = @testId
            ORDER BY r.RunTimestamp DESC
            LIMIT @limit;
        ";

        var results = new List<HistoricalTestResult>();
        using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@testId", testId);
        command.Parameters.AddWithValue("@limit", limit);

        using var reader = command.ExecuteReader();
        while (reader.Read()) {
            results.Add(new HistoricalTestResult {
                RunTimestamp = DateTime.Parse(reader.GetString(0)),
                GitCommit = reader.GetString(1),
                Cycles = reader.GetInt64(2),
                Result = reader.GetInt64(3)
            });
        }

        return results;
    }

    /// <summary>
    /// Gets performance statistics for a specific test.
    /// </summary>
    /// <param name="testId">The test ID.</param>
    /// <returns>Statistics for the test.</returns>
    public PerformanceStatistics? GetTestStatistics(int testId) {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        string sql = @"
            SELECT 
                COUNT(*) as RunCount,
                AVG(Cycles) as AvgCycles,
                MIN(Cycles) as MinCycles,
                MAX(Cycles) as MaxCycles
            FROM PerformanceTestResults
            WHERE TestId = @testId;
        ";

        using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@testId", testId);

        using var reader = command.ExecuteReader();
        if (reader.Read() && reader.GetInt32(0) > 0) {
            return new PerformanceStatistics {
                TestId = testId,
                RunCount = reader.GetInt32(0),
                AverageCycles = reader.GetDouble(1),
                MinCycles = reader.GetInt64(2),
                MaxCycles = reader.GetInt64(3)
            };
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
public class HistoricalTestResult {
    /// <summary>
    /// Gets or sets the run timestamp.
    /// </summary>
    public DateTime RunTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the git commit hash.
    /// </summary>
    public string GitCommit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cycle count.
    /// </summary>
    public long Cycles { get; set; }

    /// <summary>
    /// Gets or sets the result value.
    /// </summary>
    public long Result { get; set; }
}

/// <summary>
/// Represents performance statistics for a test.
/// </summary>
public class PerformanceStatistics {
    /// <summary>
    /// Gets or sets the test ID.
    /// </summary>
    public int TestId { get; set; }

    /// <summary>
    /// Gets or sets the number of runs.
    /// </summary>
    public int RunCount { get; set; }

    /// <summary>
    /// Gets or sets the average cycle count.
    /// </summary>
    public double AverageCycles { get; set; }

    /// <summary>
    /// Gets or sets the minimum cycle count.
    /// </summary>
    public long MinCycles { get; set; }

    /// <summary>
    /// Gets or sets the maximum cycle count.
    /// </summary>
    public long MaxCycles { get; set; }

    /// <summary>
    /// Gets the variance (max - min).
    /// </summary>
    public long Variance => MaxCycles - MinCycles;
}
