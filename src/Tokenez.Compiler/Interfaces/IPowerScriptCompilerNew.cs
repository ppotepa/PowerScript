using Tokenez.Compiler.Models;

namespace Tokenez.Compiler.Interfaces;

/// <summary>
/// Interface for PowerScript compilation (new domain-separated version).
/// Responsible for transforming source code into executable artifacts.
/// </summary>
public interface IPowerScriptCompilerNew
{
    /// <summary>
    /// Compiles PowerScript source code into executable artifacts.
    /// </summary>
    /// <param name="sourceCode">The PowerScript source code to compile</param>
    /// <param name="sourceFile">Optional path to the source file for debugging</param>
    /// <returns>Compilation result containing executable artifacts or errors</returns>
    CompilationResult Compile(string sourceCode, string? sourceFile = null);

    /// <summary>
    /// Compiles PowerScript from a file.
    /// </summary>
    /// <param name="filePath">Path to the PowerScript file</param>
    /// <returns>Compilation result containing executable artifacts or errors</returns>
    CompilationResult CompileFile(string filePath);
}