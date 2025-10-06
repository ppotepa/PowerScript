using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Expressions
{
    public class LiteralExpression : Expression
    {
        public ValueToken Value { get; set; }

        public override string ExpressionType => "Literal";

        public LiteralExpression(ValueToken value)
        {
            StartToken = value;
            Value = value;
        }
    }
}
