namespace ppotepa.tokenez.Tree;

/// <summary>
/// Interface for PowerScript compiler that compiles and executes PowerScript code.
/// </summary>
public interface IPowerScriptCompiler
{
    /// <summary>
    /// Compiles and executes a PowerScript scope.
    /// </summary>
    /// <param name="scope">The scope to execute</param>
    /// <returns>The result of execution</returns>
    object? Execute(Scope scope);

    /// <summary>
    /// Registers a function in the compiler's function table.
    /// </summary>
    /// <param name="functionName">The name of the function</param>
    /// <param name="declaration">The function declaration</param>
    void RegisterFunction(string functionName, FunctionDeclaration declaration);

    /// <summary>
    /// Checks if a function is registered.
    /// </summary>
    /// <param name="functionName">The name of the function</param>
    /// <returns>True if the function is registered, false otherwise</returns>
    bool IsFunctionRegistered(string functionName);
}
