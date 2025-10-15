using Tokenez.Core.AST;
using Tokenez.Parser.Lexer;

namespace Tokenez.Compiler.Models;

/// <summary>
/// Represents the result of a PowerScript compilation process.
/// Contains all artifacts needed for execution without requiring recompilation.
/// </summary>
public class CompilationResult
{
    /// <summary>
    /// The compiled token tree containing the parsed and processed AST.
    /// </summary>
    public TokenTree TokenTree { get; }

    /// <summary>
    /// The root scope containing all statements and declarations.
    /// </summary>
    public Scope RootScope { get; }

    /// <summary>
    /// All function declarations found during compilation.
    /// </summary>
    public IReadOnlyDictionary<string, FunctionDeclaration> Functions { get; }

    /// <summary>
    /// Metadata about the compilation process.
    /// </summary>
    public CompilationMetadata Metadata { get; }

    /// <summary>
    /// Whether the compilation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Compilation errors, if any.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    public CompilationResult(
        TokenTree tokenTree,
        Scope rootScope,
        IReadOnlyDictionary<string, FunctionDeclaration> functions,
        CompilationMetadata metadata,
        bool isSuccess = true,
        IReadOnlyList<string>? errors = null)
    {
        TokenTree = tokenTree ?? throw new ArgumentNullException(nameof(tokenTree));
        RootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
        Functions = functions ?? throw new ArgumentNullException(nameof(functions));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<string>();
    }

    /// <summary>
    /// Creates a failed compilation result.
    /// </summary>
    public static CompilationResult Failed(IReadOnlyList<string> errors, CompilationMetadata? metadata = null)
    {
        return new CompilationResult(
            null!,
            null!,
            new Dictionary<string, FunctionDeclaration>(),
            metadata ?? new CompilationMetadata(DateTime.UtcNow, TimeSpan.Zero, string.Empty),
            false,
            errors);
    }
}