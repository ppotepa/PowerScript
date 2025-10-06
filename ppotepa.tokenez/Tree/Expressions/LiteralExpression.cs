using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Expressions
{
    /// <summary>
    /// Represents a literal value expression (constant value like number or string).
    /// Examples: 42, "hello", true
    /// </summary>
    public class LiteralExpression : Expression
    {
        /// <summary>The value token containing the literal</summary>
        public ValueToken Value { get; set; }

        public override string ExpressionType => "Literal";

        public LiteralExpression(ValueToken value)
        {
            StartToken = value;
            Value = value;
        }
    }
}
