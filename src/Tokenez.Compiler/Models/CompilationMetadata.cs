namespace Tokenez.Compiler.Models;

/// <summary>
/// Metadata about a compilation process.
/// </summary>
public class CompilationMetadata
{
    /// <summary>
    /// When the compilation started.
    /// </summary>
    public DateTime CompilationTime { get; }

    /// <summary>
    /// How long the compilation took.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// The source code that was compiled.
    /// </summary>
    public string SourceCode { get; }

    /// <summary>
    /// Optional source file path.
    /// </summary>
    public string? SourceFile { get; }

    /// <summary>
    /// Compilation statistics.
    /// </summary>
    public CompilationStatistics Statistics { get; }

    public CompilationMetadata(
        DateTime compilationTime,
        TimeSpan duration,
        string sourceCode,
        string? sourceFile = null,
        CompilationStatistics? statistics = null)
    {
        CompilationTime = compilationTime;
        Duration = duration;
        SourceCode = sourceCode ?? throw new ArgumentNullException(nameof(sourceCode));
        SourceFile = sourceFile;
        Statistics = statistics ?? new CompilationStatistics();
    }
}

/// <summary>
/// Statistics about a compilation process.
/// </summary>
public class CompilationStatistics
{
    /// <summary>
    /// Number of statements compiled.
    /// </summary>
    public int StatementCount { get; init; }

    /// <summary>
    /// Number of functions declared.
    /// </summary>
    public int FunctionCount { get; init; }

    /// <summary>
    /// Number of variables declared.
    /// </summary>
    public int VariableCount { get; init; }

    /// <summary>
    /// Lines of source code.
    /// </summary>
    public int LineCount { get; init; }
}