using Tokenez.Core.Syntax.Tokens.Identifiers;

namespace Tokenez.Core.AST.Expressions;

/// <summary>
///     Represents an array/collection index access expression.
///     Example: numbers[5], array[i+1]
/// </summary>
public class IndexExpression : Expression
{
    /// <summary>
    ///     The array/collection being indexed
    /// </summary>
    public required IdentifierToken ArrayIdentifier { get; init; }

    /// <summary>
    ///     The index expression (can be a literal, variable, or complex expression)
    /// </summary>
    public required Expression Index { get; init; }

    public override string ExpressionType { get; set; } = "Index";

    public override string ToString()
    {
        return $"{ArrayIdentifier.RawToken.Text}[{Index}]";
    }
}