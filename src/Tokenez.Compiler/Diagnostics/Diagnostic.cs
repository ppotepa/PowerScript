namespace Tokenez.Compiler.Diagnostics;

/// <summary>
///     Represents a diagnostic message (error, warning, suggestion) from the compiler.
/// </summary>
public class Diagnostic(
    DiagnosticSeverity severity,
    string code,
    string message,
    string location = "",
    string suggestion = "")
{
    public DiagnosticSeverity Severity { get; set; } = severity;
    public string Code { get; set; } = code;
    public string Message { get; set; } = message;
    public string Location { get; set; } = location;
    public string Suggestion { get; set; } = suggestion;

    public override string ToString()
    {
        string severityPrefix = Severity switch
        {
            DiagnosticSeverity.Error => "❌ ERROR",
            DiagnosticSeverity.Warning => "⚠️  WARNING",
            DiagnosticSeverity.Suggestion => "💡 SUGGESTION",
            DiagnosticSeverity.Info => "ℹ️  INFO",
            _ => "UNKNOWN"
        };

        string result = $"{severityPrefix} {Code}: {Message}";

        if (!string.IsNullOrEmpty(Location))
        {
            result += $" at {Location}";
        }

        if (!string.IsNullOrEmpty(Suggestion))
        {
            result += $"\n   💡 Suggestion: {Suggestion}";
        }

        return result;
    }
}