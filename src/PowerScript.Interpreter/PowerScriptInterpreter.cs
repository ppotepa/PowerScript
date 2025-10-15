using PowerScript.Compiler.Interfaces;
using PowerScript.Compiler.Models;
using PowerScript.Interpreter.Interfaces;
using PowerScript.Runtime.Interfaces;
using PowerScript.Runtime.Models;

namespace PowerScript.Interpreter;

/// <summary>
/// New PowerScript interpreter that separates compilation and execution domains.
/// This class orchestrates both the compiler and executor components.
/// </summary>
public class PowerScriptInterpreter : IPowerScriptInterpreter
{
    private readonly IPowerScriptCompilerNew _compiler;
    private readonly IPowerScriptExecutor _executor;
    private readonly Dictionary<string, CompilationResult> _compilationCache = [];

    public PowerScriptInterpreter(
        IPowerScriptCompilerNew compiler,
        IPowerScriptExecutor executor)
    {
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    /// <summary>
    /// Executes PowerScript code from a string.
    /// </summary>
    public object? ExecuteCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty", nameof(code));
        }

        // Check cache first (simple string-based caching)
        string cacheKey = code.GetHashCode().ToString();
        if (!_compilationCache.TryGetValue(cacheKey, out CompilationResult? compilationResult))
        {
            // Compile the code
            compilationResult = _compiler.Compile(code);

            if (compilationResult.IsSuccess)
            {
                _compilationCache[cacheKey] = compilationResult;
            }
        }

        if (!compilationResult.IsSuccess)
        {
            throw new InvalidOperationException($"Compilation failed: {string.Join(", ", compilationResult.Errors)}");
        }

        // Execute the compiled code
        ExecutionResult executionResult = _executor.Execute(compilationResult);

        return !executionResult.IsSuccess
            ? throw new InvalidOperationException($"Execution failed: {string.Join(", ", executionResult.Errors)}")
            : executionResult.ReturnValue;
    }

    /// <summary>
    /// Executes a PowerScript file.
    /// </summary>
    public object? ExecuteFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        // Check cache first (file-based caching)
        string cacheKey = Path.GetFullPath(filePath);
        if (!_compilationCache.TryGetValue(cacheKey, out CompilationResult? compilationResult))
        {
            // Compile the file
            compilationResult = _compiler.CompileFile(filePath);

            if (compilationResult.IsSuccess)
            {
                _compilationCache[cacheKey] = compilationResult;
            }
        }

        if (!compilationResult.IsSuccess)
        {
            throw new InvalidOperationException($"Compilation failed: {string.Join(", ", compilationResult.Errors)}");
        }

        // Execute the compiled code
        ExecutionResult executionResult = _executor.Execute(compilationResult);

        return !executionResult.IsSuccess
            ? throw new InvalidOperationException($"Execution failed: {string.Join(", ", executionResult.Errors)}")
            : executionResult.ReturnValue;
    }

    /// <summary>
    /// Links a library file to make its functions available.
    /// </summary>
    public void LinkLibrary(string libraryPath)
    {
        _executor.LinkLibrary(libraryPath);
    }

    /// <summary>
    /// Compiles code without executing it.
    /// </summary>
    public CompilationResult CompileOnly(string code, string? sourceFile = null)
    {
        return _compiler.Compile(code, sourceFile);
    }

    /// <summary>
    /// Executes previously compiled code.
    /// </summary>
    public ExecutionResult ExecuteOnly(CompilationResult compilationResult)
    {
        return _executor.Execute(compilationResult);
    }

    /// <summary>
    /// Gets the current execution context.
    /// </summary>
    public IExecutionContext GetExecutionContext()
    {
        return _executor.GetExecutionContext();
    }

    /// <summary>
    /// Clears the compilation cache.
    /// </summary>
    public void ClearCache()
    {
        _compilationCache.Clear();
    }
}