namespace PowerScript.Runtime.Models;

/// <summary>
/// Represents the result of a PowerScript execution.
/// </summary>
public class ExecutionResult
{
    /// <summary>
    /// Whether the execution was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// The return value from execution, if any.
    /// </summary>
    public object? ReturnValue { get; }

    /// <summary>
    /// Execution errors, if any.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Metadata about the execution process.
    /// </summary>
    public ExecutionMetadata Metadata { get; }

    /// <summary>
    /// Standard output captured during execution.
    /// </summary>
    public string Output { get; }

    public ExecutionResult(
        bool isSuccess,
        object? returnValue = null,
        IReadOnlyList<string>? errors = null,
        ExecutionMetadata? metadata = null,
        string output = "")
    {
        IsSuccess = isSuccess;
        ReturnValue = returnValue;
        Errors = errors ?? Array.Empty<string>();
        Metadata = metadata ?? new ExecutionMetadata(DateTime.UtcNow, TimeSpan.Zero);
        Output = output;
    }

    /// <summary>
    /// Creates a successful execution result.
    /// </summary>
    public static ExecutionResult Success(object? returnValue = null, ExecutionMetadata? metadata = null, string output = "")
    {
        return new ExecutionResult(true, returnValue, null, metadata, output);
    }

    /// <summary>
    /// Creates a failed execution result.
    /// </summary>
    public static ExecutionResult Failed(IReadOnlyList<string> errors, ExecutionMetadata? metadata = null, string output = "")
    {
        return new ExecutionResult(false, null, errors, metadata, output);
    }

    /// <summary>
    /// Creates a failed execution result from a single error.
    /// </summary>
    public static ExecutionResult Failed(string error, ExecutionMetadata? metadata = null, string output = "")
    {
        return Failed(new[] { error }, metadata, output);
    }
}