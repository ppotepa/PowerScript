using PowerScript.Core.AST.Expressions;

namespace PowerScript.Core.AST.Statements;

/// <summary>
///     Represents a direct call to a .NET framework method.
///     Example: NET.System.Console.WriteLine("Hello")
/// </summary>
public class NetMethodCallStatement(string fullMethodPath, List<Expression>? arguments = null) : Statement
{
    /// <summary>
    ///     The fully qualified .NET method path (e.g., "System.Console.WriteLine")
    /// </summary>
    public string FullMethodPath { get; } = fullMethodPath;

    /// <summary>
    ///     The arguments to pass to the method
    /// </summary>
    public List<Expression> Arguments { get; } = arguments ?? [];

    public override string StatementType => "NET_METHOD_CALL";

    public override string ToString()
    {
        string args = string.Join(", ", Arguments.Select(a => a.ToString()));
        return $"NET.{FullMethodPath}({args})";
    }
}