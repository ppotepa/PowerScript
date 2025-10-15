using Tokenez.Compiler.Models;
using Tokenez.Runtime.Models;

namespace Tokenez.Runtime.Interfaces;

/// <summary>
/// Interface for PowerScript execution.
/// Responsible for executing compiled PowerScript artifacts.
/// </summary>
public interface IPowerScriptExecutor
{
    /// <summary>
    /// Executes a compiled PowerScript program.
    /// </summary>
    /// <param name="compilationResult">The compiled PowerScript artifacts to execute</param>
    /// <returns>Execution result containing the return value or errors</returns>
    ExecutionResult Execute(CompilationResult compilationResult);

    /// <summary>
    /// Links a library to be available during execution.
    /// </summary>
    /// <param name="libraryPath">Path to the library file</param>
    void LinkLibrary(string libraryPath);

    /// <summary>
    /// Gets the current execution context (variables, functions, etc.).
    /// </summary>
    /// <returns>Current execution context</returns>
    IExecutionContext GetExecutionContext();
}

/// <summary>
/// Represents the execution context containing runtime state.
/// </summary>
public interface IExecutionContext
{
    /// <summary>
    /// Gets all variables in the current scope.
    /// </summary>
    IReadOnlyDictionary<string, object?> Variables { get; }

    /// <summary>
    /// Gets all registered functions.
    /// </summary>
    IReadOnlyDictionary<string, object> Functions { get; }

    /// <summary>
    /// Gets or sets a variable value.
    /// </summary>
    object? GetVariable(string name);
    void SetVariable(string name, object? value);

    /// <summary>
    /// Checks if a variable exists.
    /// </summary>
    bool HasVariable(string name);

    /// <summary>
    /// Clears all variables and resets the context.
    /// </summary>
    void Reset();
}