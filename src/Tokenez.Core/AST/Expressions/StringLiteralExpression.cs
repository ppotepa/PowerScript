using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Core.AST.Expressions;

/// <summary>
///     Represents a string literal expression.
///     Example: "Hello World"
/// </summary>
public class StringLiteralExpression : Expression
{
    public StringLiteralExpression(Token value)
    {
        Value = value;
        StartToken = value;
        ExpressionType = "StringLiteral";
    }

    public Token Value { get; set; }

    public override string ToString()
    {
        return Value.RawToken.Text;
    }
}