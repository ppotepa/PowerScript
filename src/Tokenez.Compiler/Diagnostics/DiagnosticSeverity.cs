namespace Tokenez.Compiler.Diagnostics;

/// <summary>
///     Represents the severity level of a diagnostic message.
/// </summary>
public enum DiagnosticSeverity
{
    Info, // Informational message
    Warning, // Warning that doesn't prevent compilation
    Error, // Error that prevents successful compilation
    Suggestion // Helpful suggestion for improvement
}