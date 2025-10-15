using PowerScript.Core.AST.Expressions;

namespace PowerScript.Core.AST.Statements;

/// <summary>
///     Represents a PRINT statement that outputs text or expression values to the console.
///     Example: PRINT "Hello World" or PRINT ADD(5, 3)
/// </summary>
public class PrintStatement(Expression expression) : Statement
{
    /// <summary>The expression to print (can be string literal, function call, variable, etc.)</summary>
    public Expression Expression { get; set; } = expression;

    public override string StatementType => "PRINT";

    public override string ToString()
    {
        return $"PRINT {Expression}";
    }
}