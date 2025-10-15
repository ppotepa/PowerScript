namespace PowerScript.Runtime.Models;

/// <summary>
/// Metadata about an execution process.
/// </summary>
public class ExecutionMetadata
{
    /// <summary>
    /// When the execution started.
    /// </summary>
    public DateTime ExecutionTime { get; }

    /// <summary>
    /// How long the execution took.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Execution statistics.
    /// </summary>
    public ExecutionStatistics Statistics { get; }

    public ExecutionMetadata(
        DateTime executionTime,
        TimeSpan duration,
        ExecutionStatistics? statistics = null)
    {
        ExecutionTime = executionTime;
        Duration = duration;
        Statistics = statistics ?? new ExecutionStatistics();
    }
}

/// <summary>
/// Statistics about an execution process.
/// </summary>
public class ExecutionStatistics
{
    /// <summary>
    /// Number of statements executed.
    /// </summary>
    public int StatementsExecuted { get; init; }

    /// <summary>
    /// Number of function calls made.
    /// </summary>
    public int FunctionCalls { get; init; }

    /// <summary>
    /// Number of variables accessed.
    /// </summary>
    public int VariableAccesses { get; init; }

    /// <summary>
    /// Maximum call stack depth reached.
    /// </summary>
    public int MaxCallStackDepth { get; init; }
}