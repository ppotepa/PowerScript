using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Expressions
{
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
}