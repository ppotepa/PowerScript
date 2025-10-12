namespace ppotepa.tokenez.Tree.Diagnostics
{
    /// <summary>
    ///     Represents a diagnostic message (error, warning, suggestion) from the compiler.
    /// </summary>
    public class Diagnostic
    {
        public Diagnostic(DiagnosticSeverity severity, string code, string message, string location = "",
            string suggestion = "")
        {
            Severity = severity;
            Code = code;
            Message = message;
            Location = location;
            Suggestion = suggestion;
        }

        public DiagnosticSeverity Severity { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Location { get; set; }
        public string Suggestion { get; set; }

        public override string ToString()
        {
            var severityPrefix = Severity switch
            {
                DiagnosticSeverity.Error => "❌ ERROR",
                DiagnosticSeverity.Warning => "⚠️  WARNING",
                DiagnosticSeverity.Suggestion => "💡 SUGGESTION",
                DiagnosticSeverity.Info => "ℹ️  INFO",
                _ => "UNKNOWN"
            };

            var result = $"{severityPrefix} {Code}: {Message}";

            if (!string.IsNullOrEmpty(Location)) result += $" at {Location}";

            if (!string.IsNullOrEmpty(Suggestion)) result += $"\n   💡 Suggestion: {Suggestion}";

            return result;
        }
    }
}