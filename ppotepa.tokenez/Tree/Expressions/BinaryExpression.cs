using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Expressions
{
    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public Token Operator { get; set; }
        public Expression Right { get; set; }

        public override string ExpressionType => "BinaryOperation";

        public BinaryExpression(Expression left, Token operatorToken, Expression right)
        {
            StartToken = left.StartToken;
            Left = left;
            Operator = operatorToken;
            Right = right;
        }
    }
}
