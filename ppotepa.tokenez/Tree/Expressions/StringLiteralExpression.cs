using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Expressions
{
    /// <summary>
    /// Represents a string literal expression.
    /// Example: "Hello World"
    /// </summary>
    public class StringLiteralExpression : Expression
    {
        public Token Value { get; set; }

        public StringLiteralExpression(Token value)
        {
            Value = value;
            StartToken = value;
            ExpressionType = "StringLiteral";
        }

        public override string ToString()
        {
            return Value.RawToken.Text;
        }
    }
}
