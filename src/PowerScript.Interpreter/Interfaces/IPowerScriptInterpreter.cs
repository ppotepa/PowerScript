namespace PowerScript.Interpreter.Interfaces;

/// <summary>
///     Interface for PowerScript interpreter that executes PowerScript code.
/// </summary>
public interface IPowerScriptInterpreter
{
    /// <summary>
    ///     Executes PowerScript code from a string.
    /// </summary>
    /// <param name="code">The PowerScript code to execute</param>
    /// <returns>The result of execution, or null if no value is returned</returns>
    object? ExecuteCode(string code);

    /// <summary>
    ///     Executes a PowerScript file.
    /// </summary>
    /// <param name="filePath">Path to the PowerScript file</param>
    /// <returns>The result of execution, or null if no value is returned</returns>
    object? ExecuteFile(string filePath);

    /// <summary>
    ///     Links a library file to make its functions available.
    /// </summary>
    /// <param name="libraryPath">Path to the library file</param>
    void LinkLibrary(string libraryPath);
}