using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.AST.Expressions;

/// <summary>
///     Represents a literal value expression (constant value like number or string).
///     Examples: 42, "hello", true, 3.14
/// </summary>
public class LiteralExpression : Expression
{
    public LiteralExpression(ValueToken value)
    {
        StartToken = value;
        Value = value;
    }

    public LiteralExpression(DecimalToken value)
    {
        StartToken = value;
        Value = value;
    }

    // Constructor accepting any Token (for StringLiteralToken and other value tokens)
    public LiteralExpression(Token value)
    {
        StartToken = value;
        Value = value;
    }

    /// <summary>The value token containing the literal (ValueToken, DecimalToken, etc.)</summary>
    public Token Value { get; set; }

    public override string ExpressionType { get; set; } = "Literal";
}