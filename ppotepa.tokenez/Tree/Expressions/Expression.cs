using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Expressions
{
    public abstract class Expression
    {
        public Token StartToken { get; set; }
        public abstract string ExpressionType { get; }
    }
}
