using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.AST.Expressions;

/// <summary>
///     Represents an array creation expression.
///     Syntax: CHAIN
///     <size>
///         Example: FLEX arr = CHAIN 10
/// </summary>
public class ArrayCreationExpression(ValueToken sizeToken) : Expression
{
    public ValueToken SizeToken { get; } = sizeToken;

    public override string ExpressionType { get; set; } = "ArrayCreation";
}