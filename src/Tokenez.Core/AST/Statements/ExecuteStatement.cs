namespace Tokenez.Core.AST.Statements;

/// <summary>
///     Represents an EXECUTE command to run an external PowerScript file.
///     Syntax: EXECUTE "filename.ps"
/// </summary>
public class ExecuteStatement(string filePath) : Statement
{
    /// <summary>
    ///     Path to the script file to execute
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>Statement type identifier</summary>
    public override string StatementType => "EXECUTE";

    public override string ToString()
    {
        return $"EXECUTE \"{FilePath}\"";
    }
}