using PowerScript.Core.Syntax.Tokens.Base;

namespace PowerScript.Core.AST.Statements;

/// <summary>
///     Represents a LINK statement that imports .NET namespaces or PowerScript files.
///     Examples:
///       LINK System
///       LINK System.IO
///       LINK "scripts/stdlib/MathLib.ps"
/// </summary>
public class LinkStatement : Statement
{
    /// <summary>The namespace or file path to link</summary>
    public string Target { get; set; } = "";

    /// <summary>True if linking a file path, false if linking a .NET namespace</summary>
    public bool IsFilePath { get; set; }

    public override string StatementType => "LINK";

    public override string ToString()
    {
        var targetDisplay = IsFilePath ? $"\"{Target}\"" : Target;
        return $"LINK {targetDisplay}";
    }
}
