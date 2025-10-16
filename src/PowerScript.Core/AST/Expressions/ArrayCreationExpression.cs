using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.AST.Expressions;

/// <summary>
///     Represents an array creation expression.
///     Syntax: CHAIN
///     <size>
///         Example: FLEX arr = CHAIN 10
/// </summary>
public class ArrayCreationExpression(Expression sizeExpression) : Expression
{
    public Expression SizeExpression { get; } = sizeExpression;

    public override string ExpressionType { get; set; } = "ArrayCreation";
}