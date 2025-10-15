using PowerScript.Core.Syntax.Tokens.Identifiers;

namespace PowerScript.Core.AST.Expressions;

/// <summary>
///     Represents an array/collection index access expression.
///     Example: numbers[5], array[i+1], matrix[0][1] (2D)
/// </summary>
public class IndexExpression : Expression
{
    /// <summary>
    ///     The array/collection being indexed.
    ///     Can be an IdentifierExpression for simple arrays, or another IndexExpression for multi-dimensional arrays.
    /// </summary>
    public required Expression ArrayExpression { get; init; }

    /// <summary>
    ///     The index expression (can be a literal, variable, or complex expression)
    /// </summary>
    public required Expression Index { get; init; }

    public override string ExpressionType { get; set; } = "Index";

    public override string ToString()
    {
        return $"{ArrayExpression}[{Index}]";
    }
}